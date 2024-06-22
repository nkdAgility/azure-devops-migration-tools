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
    [string]$releaseTag,

    # GH_TOKEN
    [Parameter(Mandatory=$true)]
    [string]$GH_TOKEN

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

if ($GH_TOKEN -eq $null) {
    Write-Output "Please set the GH_TOKEN variable"
    return 1;
}

gh auth login --with-token $GH_TOKEN


Write-Output "Release tag: $releaseTag"
Write-Output "Version: $version"
Write-Output "Filed for Upload: $artifactFolder\**"
Write-Output "--------------"
switch ($releaseTag)
{
    "Local"    {
        Write-Output "Running Local"
        gh release create $version "$artifactFolder\**" --title "$version **DELETEME** FAKE TEST" --generate-notes --prerelease --draft
        }
    "Dev"    {
        Write-Output "Running Dev"
        gh release create $version "$artifactFolder\**" --title "$version **DELETEME** FAKE TEST" --generate-notes --prerelease --draft
        }
    "preview"    {
        Write-Output "Running Preview"
        gh release create $version "$artifactFolder\**" --generate-notes --prerelease
        }
    "Release"    {
        Write-Output "Running Release"
        gh release create $version "$artifactFolder\**" --generate-notes --discussion-category "AnouncementDiscussions"
        }
    default { 
        Write-Output "Unknown Release tag of $releaseTag";
        return -1;
     }
}