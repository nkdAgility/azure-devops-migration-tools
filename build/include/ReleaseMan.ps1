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

    # Load the existing grouped releases from the output file if it exists
    if (Test-Path $outputFilePath) {
        $existingGroupedReleases = Get-Content -Raw -Path $outputFilePath | ConvertFrom-Json
    } else {
        $existingGroupedReleases = @()
    }

    # Convert the existing grouped releases to a hashtable for easier updates
    $groupedReleases = @{}
    foreach ($majorRelease in $existingGroupedReleases) {
        $groupedReleases[$majorRelease.Major] = @{
            Major = $majorRelease.Major
            Releases = $majorRelease.Releases
            Summary = $majorRelease.Summary
        }
    }

    # Group new releases by major and minor versions
    $releases | ForEach-Object {
        $versionInfo = Parse-Version -version $_.version
        $major = $versionInfo.Major
        $minor = $versionInfo.Minor
        
        # Check if the major version exists in the grouped releases
        if (-not $groupedReleases.ContainsKey($major)) {
            $groupedReleases[$major] = @{
                Major = $major
                Releases = @()
                Summary = $null # Placeholder for summary
            }
        }

        # Check if the minor version exists under the major version
        $minorGroup = $groupedReleases[$major].Releases | Where-Object { $_.Minor -eq $minor }
        if (-not $minorGroup) {
            $minorGroup = [PSCustomObject]@{
                Minor = $minor
                Releases = @()
                Summary = $null # Placeholder for summary
            }
            $groupedReleases[$major].Releases += $minorGroup
        } else {
            # Preserve the existing summary and description
            $existingSummary = $minorGroup.Summary
            $minorGroup = $groupedReleases[$major].Releases | Where-Object { $_.Minor -eq $minor }
            $minorGroup.Summary = $existingSummary
        }

        # Check if the release already exists in the minor release group
        $existingRelease = $minorGroup.Releases | Where-Object { $_.tagName -eq $_.tagName }
        if ($existingRelease) {
            # If it exists, ensure its description is retained
            if ($existingRelease.description) {
                $_.description = $existingRelease.description
            }
        } else {
            # Add the release to the appropriate minor release group
            $minorGroup.Releases += ,$_
        }
    }

    # Convert the grouped releases hashtable back to a list of PSCustomObjects
    $finalGroupedReleases = $groupedReleases.GetEnumerator() | Sort-Object -Property Key | ForEach-Object {
        [PSCustomObject]@{
            Major = $_.Value.Major
            Releases = ($_.Value.Releases | Sort-Object -Property Minor)
            Summary = $_.Value.Summary
        }
    }

    # Set a higher depth for JSON serialization
    $groupedJson = $finalGroupedReleases | ConvertTo-Json -Depth 10

    # Save the updated JSON to the output file
    Set-Content -Path $outputFilePath -Value $groupedJson

    Write-Host "Grouped releases have been updated and saved to $outputFilePath"

    # Return the grouped releases object
    return $finalGroupedReleases
}





function Generate-ReleaseSummaries {
    param (
        [Parameter(Mandatory = $true)]
        [array]$groupedReleases,
        
        [Parameter(Mandatory = $true)]
        [string]$outputFilePath
    )

    # Function to check if a summary exists
    function Summary-Exists {
        param (
            [Parameter(Mandatory = $false)]
            [object]$summary
        )
        
        # Check if the summary is not null, empty, or missing entirely
        return $null -ne $summary -and -not [string]::IsNullOrEmpty($summary)
    }

    # Iterate through each major release to create summaries
    foreach ($majorRelease in $groupedReleases) {
        Write-Host "Processing Major Version $($majorRelease.Major)..."

        $minorSummaryGenerated = $false

        foreach ($minorRelease in $majorRelease.Releases) {
            Write-Host "  Processing Minor Version $($minorRelease.Minor)..."

            if (-not (Summary-Exists -summary ($minorRelease.PSObject.Properties['summary']?.Value))) {
                # Combine descriptions of all releases in this minor version
                $minorReleaseJson = $minorRelease.Releases | ConvertTo-Json -Depth 10

                # Generate a summary for this minor release using OpenAI
                $prompt = "Provide a summary of the following changes that were introduced in version $($majorRelease.Major).$($minorRelease.Minor). Concentrate on changes that impact users, such as new features, improvements, and bug fixes. Keep it short and do not mention the release or the version number. Should be a single short paragraph. Use the following json: `n`````n$minorReleaseJson`n````"
                $minorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

                # Add the summary to the minor release
                $minorRelease | Add-Member -MemberType NoteProperty -Name summary -Value $minorSummary -Force

                Write-Host "    Summary for Minor Version $($minorRelease.Minor) added."

                # Mark that at least one minor summary was generated
                $minorSummaryGenerated = $true
            } else {
                Write-Host "    Summary for Minor Version $($minorRelease.Minor) already exists. Skipping summary generation."
            }
        }

        # Only generate a major summary if one doesn't exist, or if any minor summaries were newly generated
        if (-not (Summary-Exists -summary ($majorRelease.PSObject.Properties['summary']?.Value)) -or $minorSummaryGenerated) {
            # Combine all minor summaries to create a major summary
            $majorReleaseJson = $majorRelease.Releases | ConvertTo-Json -Depth 10

            # Generate a summary for this major release using OpenAI
            $prompt = "Provide a summary of the following changes that were introduced in the major version $($majorRelease.Major). Concentrate on changes that impact users, such as new features, improvements, and bug fixes. Keep it short and do not mention the release or the version number. Should be a single short paragraph. Use the following json: `n`````n$majorReleaseJson`n````"
            $majorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

            # Add the summary to the major release
            $majorRelease | Add-Member -MemberType NoteProperty -Name summary -Value $majorSummary -Force

            Write-Host "Summary for Major Version $($majorRelease.Major) added."
        } else {
            Write-Host "Summary for Major Version $($majorRelease.Major) already exists and no new minor summaries were added. Skipping summary generation."
        }

        # Save the updated grouped releases to the output file after each major summary is processed
        $groupedReleasesJson = $groupedReleases | ConvertTo-Json -Depth 10
        Set-Content -Path $outputFilePath -Value $groupedReleasesJson
    }

    return $groupedReleases
}
