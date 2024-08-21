# Load required functions
. ./build/include/Get-ReleaseDescription.ps1
. ./build/include/OpenAI.ps1
. ./build/include/ReleaseMan.ps1

# Define file paths
$releaseFilePath = "./releases.json"
$outputFilePath = "./releases-grouped.json"

# Step 1: Update releases with the latest data
$updatedReleases = Update-Releases -releaseFilePath $releaseFilePath -limit 10

# Step 2: Add descriptions to all releases
$updatedReleases = Update-ReleaseDescriptions -updatedReleases $updatedReleases -releaseFilePath $releaseFilePath

# Output the total number of releases processed
Write-Host "Total of $($updatedReleases.Count) releases found and processed."

# Step 3: Group releases by major and minor versions
$groupedReleases = Update-ReleaseGroups -releaseFilePath $releaseFilePath -outputFilePath $outputFilePath

# Step 4: Iterate through each major release to create summaries
foreach ($majorRelease in $groupedReleases) {
    Write-Host "Processing Major Version $($majorRelease.Major)..."

    foreach ($minorRelease in $majorRelease.Releases) {
        Write-Host "  Processing Minor Version $($minorRelease.Minor)..."

        # Combine descriptions of all releases in this minor version
        $minorReleaseJson = $minorRelease.Releases | ConvertTo-Json -Depth 4

        # Generate a summary for this minor release using OpenAI
        $prompt = "Provide a summary of the following changes that were introduced in version $($majorRelease.Major).$($minorRelease.Minor). Concentrate on changes that impact users, such as new features, improvements, and bug fixes. use the following json: `n`````n$minorReleaseJson`n````"
        $minorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

        # Add the summary to the minor release
        $minorRelease | Add-Member -MemberType NoteProperty -Name summary -Value $minorSummary

        Write-Host "    Summary for Minor Version $($minorRelease.Minor) added."
    }

    # Combine all minor summaries to create a major summary
    $majorReleaseJson = $majorRelease.Releases | ConvertTo-Json -Depth 4

    # Generate a summary for this major release using OpenAI
    $prompt = "Provide a summary of the following changes that were introduced in the major version $($majorRelease.Major). Concentrate on changes that impact users, such as new features, improvements, and bug fixes. use the following json:  `n`````n$majorReleaseJson`n````"
    $majorSummary = Get-OpenAIResponse -system "Create a release summary" -prompt $prompt -OPEN_AI_KEY $Env:OPEN_AI_KEY

    # Add the summary to the major release
    $majorRelease | Add-Member -MemberType NoteProperty -Name summary -Value $majorSummary

    Write-Host "Summary for Major Version $($majorRelease.Major) added."
}

# Save the updated grouped releases to the output file
$groupedReleasesJson = $groupedReleases | ConvertTo-Json -Depth 4
Set-Content -Path $outputFilePath -Value $groupedReleasesJson

# Output the grouped releases object with summaries
$groupedReleases
