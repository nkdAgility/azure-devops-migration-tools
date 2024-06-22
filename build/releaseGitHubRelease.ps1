<#
    Azure DevOps Migration Tools (GitHub Release) Release
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version,

    # name of the output folder
    [Parameter(Mandatory=$true)]
    [string]$outfolder
)
#Publish
Write-Output "Azure DevOps Migration Tools (GitHub Release) Release"
Write-Output "--------------"
$files = Get-ChildItem -Path $outfolder -Recurse
foreach ($file in $files) {
    Write-Output $file.FullName
}

# if ($versionInfo.PreReleaseTag -eq "") {
#    Write-Output "Publishing Release"
#   #gh release create $versionText $files --generate-notes --generate-notes --discussion-category "General"
# } else {
#    Write-Output "Publishing PreRelease"
#   #gh release create $versionText $files --generate-notes --generate-notes --prerelease --discussion-category "General"
# }