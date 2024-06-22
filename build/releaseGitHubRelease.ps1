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

Write-Output "Release tag: $releaseTag! Version: $version"
Write-Output "Version: $version"
Write-Output "--------------"
switch ($releaseTag)
{
    "Dev"    {
        Write-Output "Running Dev"
        gh release create "$versionText **DELETEME** FAKE RELASE BUILD TEST " $files --generate-notes --prerelease
        }
    "preview"    {
        Write-Output "Running Preview"
        gh release create $versionText $files --generate-notes --prerelease --discussion-category "General"
        }
    "Release"    {
        Write-Output "Running Release"
        gh release create $versionText $files --generate-notes --discussion-category "General"
        }
    default { 
        Write-Output "Unknown Release tag of $releaseTag";
        return -1;
     }
}