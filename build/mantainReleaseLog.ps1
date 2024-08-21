# Load required functions
. ./build/include/Get-ReleaseDescription.ps1
. ./build/include/OpenAI.ps1
. ./build/include/ReleaseMan.ps1

# Define file paths
$releaseFilePath = "./docs/_data/releases.json"
$outputFilePath = "./docs/_data/releases-grouped.json"

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
Get-ChangeLogLightMarkdown
#==============================================================================