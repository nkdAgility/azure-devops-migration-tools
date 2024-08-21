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

function Update-ReleaseGroups-Minor {
    param (
        [string]$releaseFilePath = "./releases.json",
        [string]$outputFilePath = "./releases-grouped-minor.json"
    )
    
    # Load the releases from releases.json
    $releases = Get-Content -Raw -Path $releaseFilePath | ConvertFrom-Json

    # Load the existing grouped minor releases from the output file if it exists
    if (Test-Path $outputFilePath) {
        $existingGroupedReleases = Get-Content -Raw -Path $outputFilePath | ConvertFrom-Json
    } else {
        $existingGroupedReleases = @()
    }

    # Convert the existing grouped releases to a hashtable for easier updates
    $groupedReleases = @{}
    foreach ($minorRelease in $existingGroupedReleases) {
        $key = "$($minorRelease.Major).$($minorRelease.Minor)"
        $groupedReleases[$key] = @{
            Major = $minorRelease.Major
            Minor = $minorRelease.Minor
            Releases = $minorRelease.Releases
        }
    }

    # Group new releases by major and minor versions
    $releases | ForEach-Object {
        $versionInfo = Parse-Version -version $_.version
        $major = $versionInfo.Major
        $minor = $versionInfo.Minor
        $key = "$major.$minor"
        
        # Ensure minor version exists under the major version
        if (-not $groupedReleases.ContainsKey($key)) {
            $groupedReleases[$key] = @{
                Major = $major
                Minor = $minor
                Releases = @()
            }
        }

        # Check if this release already exists in the minor group
        $existingRelease = $groupedReleases[$key].Releases | Where-Object { $_.tagName -eq $_.tagName }
        if (-not $existingRelease) {
            # Add the release if it does not already exist
            $groupedReleases[$key].Releases += ,$_
        }
    }

    # Convert the grouped releases hashtable back to a list
    $finalGroupedReleases = $groupedReleases.GetEnumerator() | Sort-Object -Property Key | ForEach-Object {
        [PSCustomObject]@{
            Major = $_.Value.Major
            Minor = $_.Value.Minor
            Releases = $_.Value.Releases
        }
    }

    # Save the updated grouped releases to the output file
    $groupedJson = $finalGroupedReleases | ConvertTo-Json -Depth 10
    Set-Content -Path $outputFilePath -Value $groupedJson

    Write-Host "Grouped minor releases have been updated and saved to $outputFilePath"
}




function Update-ReleaseGroups-MinorSummaries {
    param (
        [string]$inputFilePath = "./releases-grouped-minor.json",
        [string]$outputFilePath = "./releases-grouped-minor.json"
    )

    # Load the grouped minor releases
    $groupedReleases = Get-Content -Raw -Path $inputFilePath | ConvertFrom-Json

    # Iterate through each minor release to create summaries
    foreach ($minorRelease in $groupedReleases) {
        Write-Host "Processing Minor Version $($minorRelease.Major).$($minorRelease.Minor)..."

        if (-not $minorRelease.PSObject.Properties['summary']) {
            # Combine descriptions of all sub-releases in this minor version
            $minorReleaseJson = $minorRelease.Releases | ConvertTo-Json -Depth 10

            # Generate a summary for this minor release using OpenAI
            $prompt = "Provide a summary of the following changes for version $($minorRelease.Major).$($minorRelease.Minor). Concentrate on user-impacting changes like new features, improvements, and bug fixes. Create as a short paragraph Use the following json: `n`````n$minorReleaseJson`n````"
            $minorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

            # Add the summary to the minor release
            $minorRelease | Add-Member -MemberType NoteProperty -Name summary -Value $minorSummary -Force

            Write-Host "Summary for Minor Version $($minorRelease.Major).$($minorRelease.Minor) added."
            # Save the updated grouped releases with summaries
            $groupedReleasesJson = $groupedReleases | ConvertTo-Json -Depth 10
            Set-Content -Path $outputFilePath -Value $groupedReleasesJson
        } else {
            Write-Host "Summary for Minor Version $($minorRelease.Major).$($minorRelease.Minor) already exists. Skipping."
        }
    }

    Write-Host "Updated minor release summaries have been saved to $outputFilePath"
}




function Update-ReleaseGroups-Major {
    param (
        [string]$inputFilePath = "./releases-grouped-minor.json",
        [string]$outputFilePath = "./releases-grouped-major.json"
    )

    # Load the grouped minor releases
    $groupedMinorReleases = Get-Content -Raw -Path $inputFilePath | ConvertFrom-Json

    # Load the existing grouped major releases from the output file if it exists
    if (Test-Path $outputFilePath) {
        $existingGroupedMajorReleases = Get-Content -Raw -Path $outputFilePath | ConvertFrom-Json
    } else {
        $existingGroupedMajorReleases = @()
    }

    # Convert the existing grouped major releases to a hashtable for easier updates
    $groupedMajorReleases = @{}
    foreach ($majorRelease in $existingGroupedMajorReleases) {
        $groupedMajorReleases[$majorRelease.Major] = @{
            Major = $majorRelease.Major
            Releases = @($majorRelease.Releases)  # Ensure this is a list
            Summary = $majorRelease.Summary
            HighestMinorTag = $majorRelease.HighestMinorTag
            HighestReleaseTag = $majorRelease.HighestReleaseTag
        }
    }

    # Flag to check if any new minor releases were added
    $newMinorAdded = $false

    # Group by major versions and include minor summaries without the actual releases
    foreach ($minorRelease in $groupedMinorReleases) {
        $major = $minorRelease.Major
        $minor = $minorRelease.Minor
        
        # Ensure major version exists in the grouped releases
        if (-not $groupedMajorReleases.ContainsKey($major)) {
            $groupedMajorReleases[$major] = @{
                Major = $major
                Releases = @()  # Initialize as an empty list
                Summary = $null  # Initially set to null; can be updated later
                HighestMinorTag = $null
                HighestReleaseTag = $null
            }
        }

        # Ensure the minor release is listed under the major version
        $existingMinorGroup = $groupedMajorReleases[$major].Releases | Where-Object { $_.Minor -eq $minor }
        if (-not $existingMinorGroup) {
            $newMinorGroup = [PSCustomObject]@{
                Minor = $minor
                Summary = $minorRelease.Summary
                HighestReleaseTag = ($minorRelease.Releases | Sort-Object -Property { [version]$_.version } -Descending | Select-Object -First 1).tagName
            }
            $groupedMajorReleases[$major].Releases += $newMinorGroup
            $newMinorAdded = $true

            # Update the highest minor tag and highest release tag for the major version
            $highestMinorTag = $groupedMajorReleases[$major].Releases | Sort-Object -Property Minor -Descending | Select-Object -First 1
            $groupedMajorReleases[$major].HighestMinorTag = $highestMinorTag.Minor
            $groupedMajorReleases[$major].HighestReleaseTag = $highestMinorTag.HighestReleaseTag
        }
    }

    # Blank the major summary if new minor releases were added
    if ($newMinorAdded) {
        foreach ($major in $groupedMajorReleases.Keys) {
            $groupedMajorReleases[$major].Summary = $null
        }
    }

    # Convert the grouped major releases hashtable to a list
    $finalGroupedReleases = $groupedMajorReleases.GetEnumerator() | Sort-Object -Property Key | ForEach-Object {
        [PSCustomObject]@{
            Major = $_.Value.Major
            Releases = $_.Value.Releases | Sort-Object -Property Minor
            Summary = $_.Value.Summary
            HighestMinorTag = $_.Value.HighestMinorTag
            HighestReleaseTag = $_.Value.HighestReleaseTag
        }
    }

    # Save the updated grouped major releases to the output file
    $groupedJson = $finalGroupedReleases | ConvertTo-Json -Depth 10
    Set-Content -Path $outputFilePath -Value $groupedJson

    Write-Host "Grouped major releases have been updated and saved to $outputFilePath"
}








function Update-ReleaseGroups-MajorSummaries {
    param (
        [string]$inputFilePath = "./releases-grouped-major.json",
        [string]$outputFilePath = "./releases-grouped-major.json"
    )

    # Load the grouped major releases
    $groupedReleases = Get-Content -Raw -Path $inputFilePath | ConvertFrom-Json

    # Iterate through each major release to create summaries
    foreach ($majorRelease in $groupedReleases) {
        Write-Host "Processing Major Version $($majorRelease.Major)..."

        if (-not $majorRelease.PSObject.Properties['summary']) {
            # Combine summaries of all minor releases in this major version
            $majorReleaseJson = $majorRelease.Releases | ConvertTo-Json -Depth 10

            # Generate a summary for this major release using OpenAI
            $prompt = "Provide a summary of the following changes for major version $($majorRelease.Major). Concentrate on user-impacting changes like new features, improvements, and bug fixes. Use the following json: `n`````n$majorReleaseJson`n````"
            $majorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

            # Add the summary to the major release
            $majorRelease | Add-Member -MemberType NoteProperty -Name summary -Value $majorSummary -Force

            Write-Host "Summary for Major Version $($majorRelease.Major) added."

            # Save the updated grouped releases with summaries
            $groupedReleasesJson = $groupedReleases | ConvertTo-Json -Depth 10
            Set-Content -Path $outputFilePath -Value $groupedReleasesJson

        } else {
            Write-Host "Summary for Major Version $($majorRelease.Major) already exists. Skipping."
        }
    }



    Write-Host "Updated major release summaries have been saved to $outputFilePath"
}
