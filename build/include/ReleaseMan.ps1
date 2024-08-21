function Update-Releases {
    param (
        [string]$releaseFilePath = "./releases.json",
        [int]$limit = 10
    )

    # Load existing releases from file
    if (Test-Path $releaseFilePath) {
        $existingReleases = Get-Content -Raw -Path $releaseFilePath | ConvertFrom-Json
    } else {
        $existingReleases = @()
    }

    # Retrieve the latest GitHub releases excluding pre-releases
    $newReleases = gh release list --exclude-pre-releases --json name,tagName,publishedAt --limit $limit | ConvertFrom-Json

    # Filter out any new releases that already exist in the existingReleases
    $existingTagNames = $existingReleases.tagName
    $newReleasesFiltered = $newReleases | Where-Object { $existingTagNames -notcontains $_.tagName }

    if ($newReleasesFiltered.Count -eq 0) {
        Write-Host "No new releases found."
        return $existingReleases
    } else {
        Write-Host "Found $($newReleasesFiltered.Count) new releases"
        
        # Combine existing releases with the new, filtered releases
        $combinedReleases = $existingReleases + $newReleasesFiltered

        # Add a normalized 'version' property to each release if it doesn't already exist
        foreach ($release in $combinedReleases) {
            # Check if the 'version' property already exists
            if (-not $release.PSObject.Properties['version']) {
                $normalizedTagName = $release.tagName
                if ($normalizedTagName -like 'v*') {
                    $normalizedTagName = $normalizedTagName.Substring(1)  # Remove 'v' prefix
                }
                $release | Add-Member -MemberType NoteProperty -Name version -Value $normalizedTagName
            }
        }

        # Sort the combined releases by the 'version' property, treating it as a [version] object
        $combinedReleases = $combinedReleases | Sort-Object -Property {[version]$_.version} -Descending
        
        Write-Host "Updating $releaseFilePath"
        
        # Update the releases.json file with the combined releases
        $updatedJson = $combinedReleases | ConvertTo-Json -Depth 2
        Set-Content -Path $releaseFilePath -Value $updatedJson

        return $combinedReleases
    }
}

function Add-ReleaseDescription {
    param (
        [object]$release,
        [object]$nextRelease,
        [string]$releaseFilePath,
        [object]$updatedReleases
    )

    # Determine tags for comparison
    $compairFromTag = $nextRelease.tagName
    $compairToTag = $release.tagName 

    # Get the diff between the current and next release
    $diff = Get-GitChanges -compairFrom $compairFromTag -compairTo $compairToTag -mode diff

    # Generate a summary using the OpenAI model
    $prompt = "Provide a brief summary of the following git diff on a single line and no more than one paragraph. Concentrate on changes that will impact users like configuration, options, etc., but do not mention that there is a significant impact; just describe the changes. `n$diff"
    $description = Get-OpenAIResponse -system "Create a release description" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

    # Add the description to the release
    $release | Add-Member -MemberType NoteProperty -Name description -Value $description

    # Save the updated releases with the new description back to the file
    $updatedJson = $updatedReleases | ConvertTo-Json -Depth 2
    Set-Content -Path $releaseFilePath -Value $updatedJson

    Write-Host "Saved release: $($release.tagName)"
}

function Update-ReleaseDescriptions {
    param (
        [object]$updatedReleases,
        [string]$releaseFilePath
    )

    # Iterate over the combined releases to add descriptions if missing
    for ($i = 0; $i -lt $updatedReleases.Count; $i++) {
        $release = $updatedReleases[$i]

        # Skip if the release already has a description
        if ($release.PSObject.Properties['description']) {
            Write-Host "Skipping release: $($release.tagName)"
            continue
        }

        # Determine the next release to compare against, skip if it's the last release
        if ($i -lt ($updatedReleases.Count - 1)) {
            $nextRelease = $updatedReleases[$i + 1]

            # Call the function to add description and save the release
            Add-ReleaseDescription -release $release -nextRelease $nextRelease -releaseFilePath $releaseFilePath -updatedReleases $updatedReleases
        }
    }

    # Return the updated release list
    return $updatedReleases
}


# Function to parse the version and extract major and minor parts
function Parse-Version {
    param (
        [string]$version
    )
    $versionParts = $version -split '\.'
    $majorVersion = [int]$versionParts[0]
    $minorVersion = [int]$versionParts[1]
    
    return @{
        Major = $majorVersion
        Minor = $minorVersion
    }
}

# Function to update and return the release groups
function Update-ReleaseGroups {
    param (
        [string]$releaseFilePath,
        [string]$outputFilePath = "./releases-grouped.json"
    )
    
    # Load the original releases JSON file
    $releases = Get-Content -Raw -Path $releaseFilePath | ConvertFrom-Json

    # Group releases by major and minor versions
    $groupedReleases = @{}

    $releases | ForEach-Object {
        $versionInfo = Parse-Version -version $_.version
        $major = $versionInfo.Major
        $minor = $versionInfo.Minor
        
        # Check if the major version exists in the grouped releases
        if (-not $groupedReleases.ContainsKey($major)) {
            $groupedReleases[$major] = @{
                Major = $major
                Releases = @()
            }
        }

        # Check if the minor version exists under the major version
        $minorGroup = $groupedReleases[$major].Releases | Where-Object { $_.Minor -eq $minor }
        if (-not $minorGroup) {
            $minorGroup = [PSCustomObject]@{
                Minor = $minor
                Releases = @()
            }
            $groupedReleases[$major].Releases += $minorGroup
        }

        # Add the release to the appropriate minor release group
        $minorGroup.Releases += ,$_
    }

    # Convert the grouped releases to a list of PSCustomObjects
    $finalGroupedReleases = $groupedReleases.GetEnumerator() | Sort-Object -Property Key | ForEach-Object {
        [PSCustomObject]@{
            Major = $_.Value.Major
            Releases = ($_.Value.Releases | Sort-Object -Property Minor)
        }
    }

    # Set a higher depth for JSON serialization
    $groupedJson = $finalGroupedReleases | ConvertTo-Json -Depth 10

    # Save the JSON to the output file
    Set-Content -Path $outputFilePath -Value $groupedJson

    Write-Host "Grouped releases have been saved to $outputFilePath"

    # Return the grouped releases object
    return $finalGroupedReleases
}