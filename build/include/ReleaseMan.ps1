function Update-Releases {
    param (
        [string]$releaseFilePath = "./docs/_data/releases.json",
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
        [string]$releaseFilePath = "./docs/_data/releases.json",
        [string]$outputFilePath = "./docs/_data/releases-grouped-minor.json"
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
            LatestVersion = $minorRelease.LatestVersion
            LatestTagName = $minorRelease.LatestTagName
            Summary = $minorRelease.Summary
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
                LatestVersion = $_.version
                LatestTagName = $_.tagName
                Summary = $null
            }
        }

        # Check if this release already exists in the minor group
        $existingRelease = $groupedReleases[$key].Releases | Where-Object { $_.tagName -eq $_.tagName }
        if (-not $existingRelease) {
            # Add the release if it does not already exist
            $groupedReleases[$key].Releases += ,$_
        }

        # Order the releases by version and update the LatestVersion and LatestTagName
        $orderedReleases = $groupedReleases[$key].Releases | Sort-Object -Property { [version]$_.version } -Descending
        $latestRelease = $orderedReleases | Select-Object -First 1
        $groupedReleases[$key].LatestVersion = $latestRelease.version
        $groupedReleases[$key].LatestTagName = $latestRelease.tagName
    }

    # Convert the grouped releases hashtable back to a list
    $finalGroupedReleases = $groupedReleases.GetEnumerator() | Sort-Object -Property Key | ForEach-Object {
        [PSCustomObject]@{
            Major = $_.Value.Major
            Minor = $_.Value.Minor
            Releases = $_.Value.Releases
            LatestVersion = $_.Value.LatestVersion
            LatestTagName = $_.Value.LatestTagName
            Summary = $_.Value.Summary
        }
    }

    # Save the updated grouped releases to the output file
    $groupedJson = $finalGroupedReleases | ConvertTo-Json -Depth 10
    Set-Content -Path $outputFilePath -Value $groupedJson

    Write-Host "Grouped minor releases have been updated and saved to $outputFilePath"
}







function Update-ReleaseGroups-MinorSummaries {
    param (
        [string]$inputFilePath = "./docs/_data/releases-grouped-minor.json",
        [string]$outputFilePath = "./docs/_data/releases-grouped-minor.json"
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
            $prompt = "Provide a summary of the following changes for version $($minorRelease.Major).$($minorRelease.Minor). Concentrate on user-impacting changes like new features, improvements, and bug fixes. This shoudl be a short paragraph that explains the changes, and should not include the version number. Use the following json: `n`````n$minorReleaseJson`n````"
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
        [string]$inputFilePath = "./docs/_data/releases-grouped-minor.json",
        [string]$outputFilePath = "./docs/_data/releases-grouped-major.json"
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
            Releases = $majorRelease.Releases
            LatestVersion = $majorRelease.LatestVersion
            LatestTagName = $majorRelease.LatestTagName
            LatestMinor = $majorRelease.LatestMinor
            Summary = $majorRelease.Summary
        }
    }

    # Flag to check if any new minor releases were added
    $newMinorAdded = $false

    # Group by major versions and include minor summaries without the actual releases
    foreach ($minorRelease in $groupedMinorReleases) {
        $major = $minorRelease.Major
        $minor = $minorRelease.Minor
        $minorVersion = "$($major).$($minor)"

        # Ensure major version exists in the grouped releases
        if (-not $groupedMajorReleases.ContainsKey($major)) {
            $groupedMajorReleases[$major] = @{
                Major = $major
                Releases = @()  # Initialize as an empty list
                LatestVersion = $null
                LatestTagName = $null
                LatestMinor = $null
                Summary = $null  # Initially set to null; can be updated later
            }
        }

        # Ensure the minor release is listed under the major version
        $existingMinorGroup = $groupedMajorReleases[$major].Releases | Where-Object { $_.MinorVersion -eq $minorVersion }
        if (-not $existingMinorGroup) {
            $newMinorGroup = [PSCustomObject]@{
                MinorVersion = $minorVersion
                LatestTagName = $minorRelease.LatestTagName
                Summary = $minorRelease.Summary
            }
            $groupedMajorReleases[$major].Releases += $newMinorGroup
            $newMinorAdded = $true
        }

        # Update LatestVersion, LatestTagName, and LatestMinor for the major version based on the newest minor
        $latestMinorRelease = $groupedMajorReleases[$major].Releases | Sort-Object -Property { [version]$_.MinorVersion } -Descending | Select-Object -First 1
        $groupedMajorReleases[$major].LatestVersion = $minorRelease.LatestVersion
        $groupedMajorReleases[$major].LatestTagName = $minorRelease.LatestTagName
        $groupedMajorReleases[$major].LatestMinor = $latestMinorRelease.MinorVersion
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
            Releases = $_.Value.Releases | Sort-Object -Property MinorVersion
            LatestVersion = $_.Value.LatestVersion
            LatestTagName = $_.Value.LatestTagName
            LatestMinor = $_.Value.LatestMinor
            Summary = $_.Value.Summary
        }
    }

    # Save the updated grouped major releases to the output file
    $groupedJson = $finalGroupedReleases | ConvertTo-Json -Depth 10
    Set-Content -Path $outputFilePath -Value $groupedJson

    Write-Host "Grouped major releases have been updated and saved to $outputFilePath"
}











function Update-ReleaseGroups-MajorSummaries {
    param (
        [string]$inputFilePath = "./docs/_data/releases-grouped-major.json",
        [string]$outputFilePath = "./docs/_data/releases-grouped-major.json"
    )

    # Load the grouped major releases
    $groupedReleases = Get-Content -Raw -Path $inputFilePath | ConvertFrom-Json

    # Iterate through each major release to create summaries
    foreach ($majorRelease in $groupedReleases) {
        Write-Host "Processing Major Version $($majorRelease.Major)..."

        # Check if the summary for this major release is missing or empty
        if (-not $majorRelease.PSObject.Properties['Summary'] -or [string]::IsNullOrEmpty($majorRelease.Summary)) {
            
            # Combine summaries of all minor releases in this major version
            $majorReleaseJson = $majorRelease.Releases | ConvertTo-Json -Depth 10

            # Generate a summary for this major release using OpenAI
            $prompt = "Provide a summary of the following changes for major version $($majorRelease.Major). Concentrate on user-impacting changes like new features, improvements, and bug fixes. Do not use phrases from this prompt. This shoudl be a short paragraph that explains the changes, and should not include the version number. Use the following json: `n`````n$majorReleaseJson`n````"
            $majorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

            # Add or update the summary in the major release
            $majorRelease.Summary = $majorSummary

            Write-Host "Summary for Major Version $($majorRelease.Major) added."

            # Save the updated grouped releases with summaries after each major release is processed
            $groupedReleasesJson = $groupedReleases | ConvertTo-Json -Depth 10
            Set-Content -Path $outputFilePath -Value $groupedReleasesJson

        } else {
            Write-Host "Summary for Major Version $($majorRelease.Major) already exists. Skipping."
        }
    }

    Write-Host "Updated major release summaries have been saved to $outputFilePath"
}



function Get-ChangeLogMarkdown {
    param (
        [string]$inputFilePath = "./docs/_data/releases-grouped-major.json",
        [string]$outputFilePath = "./docs/change-log.md"
    )

    # Load the grouped major releases
    $groupedReleases = Get-Content -Raw -Path $inputFilePath | ConvertFrom-Json

    # Initialize an array to hold the markdown lines, starting with the header
    $markdownLines = @(
        "---",
        "title: Change Log",
        "layout: page",
        "template: default",
        "pageType: index",
        "toc: true",
        "pageStatus: published",
        "discussionId: ",
        "redirect_from: /change-log.html",
        "---",
        "",
        "## Change Log",
        ""
    )

    # Reverse the order of the grouped major releases
    $groupedReleases = $groupedReleases | Sort-Object -Property Major -Descending

    # Iterate through each major release
    foreach ($majorRelease in $groupedReleases) {
        $majorReleaseMajor = $majorRelease.Major
        $majorReleaseLatestTagName = $majorRelease.LatestTagName
        $majorReleaseSummary = $majorRelease.Summary

        # Generate the major release markdown
        $majorLine = "- [v$majorReleaseMajor.0](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/$majorReleaseLatestTagName) - $majorReleaseSummary"
        $markdownLines += $majorLine

        # Reverse the order of the grouped major releases
        $minorReleases = $majorRelease.Releases | Sort-Object -Property MinorVersion -Descending

        # Iterate through the minor releases under this major release
        foreach ($minorRelease in $minorReleases) {
            $minorLatestTagName = $minorRelease.LatestTagName
            $minorSummary = $minorRelease.Summary
            $minorReleaseMinor = $minorRelease.Minor

            # Generate the minor release markdown
            $minorLine = "  - [$minorLatestTagName](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/$minorLatestTagName) - $minorSummary"
            $markdownLines += $minorLine
        }
    }

    # Combine the markdown lines into a single string
    $markdownContent = $markdownLines -join "`n"

    # Save the markdown content to the output file
    Set-Content -Path $outputFilePath -Value $markdownContent

    Write-Host "Change log markdown has been generated and saved to $outputFilePath"
}

function Get-ChangeLogLightMarkdown {
    param (
        [string]$inputFilePath = "./docs/_data/releases-grouped-major.json",
        [string]$outputFilePath = "./change-log.md"
    )

    $markdownLines = @()
    # Load the grouped major releases
    $groupedReleases = Get-Content -Raw -Path $inputFilePath | ConvertFrom-Json

    # Reverse the order of the grouped major releases
    $groupedReleases = $groupedReleases | Sort-Object -Property Major -Descending

    # Iterate through each major release
    foreach ($majorRelease in $groupedReleases) {
        $majorReleaseMajor = $majorRelease.Major
        $majorReleaseLatestTagName = $majorRelease.LatestTagName
        $majorReleaseSummary = $majorRelease.Summary

        # Generate the major release markdown
        $majorLine = "- [$majorReleaseLatestTagName](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/$majorReleaseLatestTagName) - $majorReleaseSummary"
        $markdownLines += $majorLine

    }

    # Combine the markdown lines into a single string
    $markdownContent = $markdownLines -join "`n"

    # Save the markdown content to the output file
    Set-Content -Path $outputFilePath -Value $markdownContent

    Write-Host "Change log markdown has been generated and saved to $outputFilePath"
}