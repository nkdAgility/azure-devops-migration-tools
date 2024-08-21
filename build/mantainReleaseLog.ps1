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


Update-ReleaseGroups-Minor
Update-ReleaseGroups-MinorSummaries
Update-ReleaseGroups-Major
Update-ReleaseGroups-MajorSummaries 
Get-ChangeLogMarkdown

#==============================================================================

# Function to generate change log markdown
function Generate-ChangeLog {
    param (
        [Parameter(Mandatory = $true)]
        [array]$groupedReleases,
        
        [Parameter(Mandatory = $true)]
        [string]$outputFilePath
    )

    # Initialize an array to hold the markdown lines
    $markdownLines = @("## Change Log")

    # Iterate through each major release
    foreach ($majorRelease in $groupedReleases) {
        $majorVersion = $majorRelease.Major
        $majorSummary = $majorRelease.summary

        # Add major release summary to markdown
        $markdownLines += "- v$majorVersion - $majorSummary"

        # Get minor releases for the major version
        $minorReleases = $majorRelease.Releases

        # Filter out minor releases with a single entry or no summary
        if ($minorReleases.Count -gt 1) {
            foreach ($minorRelease in $minorReleases) {
                $minorVersion = $minorRelease.Minor
                $minorSummary = $minorRelease.summary

                # Add minor release summary to markdown
                $markdownLines += "  - v$majorVersion.$minorVersion - $minorSummary"
            }
        }
    }

    # Save the markdown content to the output file
    $markdownContent = $markdownLines -join "`n"
    Set-Content -Path $outputFilePath -Value $markdownContent

    Write-Host "Change log saved to $outputFilePath"
}

# Define file path for the change log
$changeLogFilePath = "./change-log.md"

# Generate the change log and save it
Generate-ChangeLog -groupedReleases $groupedReleases -outputFilePath $changeLogFilePath

$groupedReleases
