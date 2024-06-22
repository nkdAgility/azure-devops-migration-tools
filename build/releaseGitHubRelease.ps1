<#
    Azure DevOps Migration Tools (GitHub Release) Release
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version,

    # name of the output folder
    [Parameter(Mandatory=$true)]
    [string]$artifactFolder,

      # name of the releaseTag
      [Parameter(Mandatory=$true)]
      [string]$releaseTag
)
#Publish
Write-Output "Azure DevOps Migration Tools (GitHub Release) Release"
Write-Output "--------------"
$files = Get-ChildItem -Path $artifactFolder -Recurse
foreach ($file in $files) {
    Write-Output $file.FullName
}

Write-Output "Install GitHub CLI if needed"
if (($installedStuff -like "*gh*").Count -eq 0) {
    Write-Output "Installing gh"
    choco install gh --confirm --accept-license -y
} else { Write-Output "Detected gh"}

if ($env:GITHUB_TOKEN -eq $null) {
    Write-Output "Please set the GITHUB_TOKEN environment variable"
    return -1
}

gh auth login --with-token $env:GITHUB_TOKEN


Write-Output "Release tag: $releaseTag"
Write-Output "Version: $version"
Write-Output "--------------"
switch ($releaseTag)
{
    "Local"    {
        Write-Output "Running Local"
        gh release create $version $files --title "$version **DELETEME** FAKE TEST" --generate-notes --prerelease --draft
        }
    "Dev"    {
        Write-Output "Running Dev"
        gh release create $version $files --title "$version **DELETEME** FAKE TEST" --generate-notes --prerelease --draft
        }
    "preview"    {
        Write-Output "Running Preview"
        gh release create $version $files --generate-notes --prerelease
        }
    "Release"    {
        Write-Output "Running Release"
        gh release create $version $files --generate-notes --discussion-category "AnouncementDiscussions"
        }
    default { 
        Write-Output "Unknown Release tag of $releaseTag";
        return -1;
     }
}