cls



$StartTimeBuild = Get-Date;
Write-Output "BUILD Azure DevOps Migration Tools"
Write-Output "======================"
Write-Output "Running from $($MyInvocation.MyCommand.Path)"
 #==============================================================================
.\build\install-prerequsits.ps1
 #==============================================================================

 # Create output folder
if (Get-Item -Path ".\output" -ErrorAction SilentlyContinue) {
    Write-Output "Cleanning up output folder"
    Remove-Item -Path ".\output\" -Recurse -Force
}
$outfolder = New-Item -Name "output" -ItemType Directory

#==============================================================================
. .\build\versioning.ps1
#==============================================================================
. .\build\compile-and-test.ps1
#==============================================================================
$version = $versionInfo.SemVer
if ($version -eq $null) {
    $version = "0.0.2"
}
$NuGetVersion = $versionInfo.NuGetVersion
if ($NuGetVersion -eq $null) {
	$NuGetVersion = "0.0.2"
}
Write-Output "Version: $version"
$stagingfolder = New-Item -Name "output\Staging" -ItemType Directory

# Azure DevOps Migration Tools (Executable) Packaging
.\build\packageExecutable.ps1 -version $version -outfolder $stagingfolder
#-------------------------------------------
# Azure DevOps Migration Tools (Extension) Packaging
.\build\packageExtension.ps1 -SemVer $version -NuGetVersion $NuGetVersion -outfolder $stagingfolder
#-------------------------------------------
# Azure DevOps Migration Tools (Chocolatey) Packaging
.\build\packageChocolatey.ps1 -version $version -outfolder $stagingfolder
#-------------------------------------------
# Azure DevOps Migration Tools (Nuget) Packaging
.\build\packageNuget.ps1 -version $version -outfolder $stagingfolder
#-------------------------------------------
# Azure DevOps Migration Tools (Winget) Packaging
.\build\packageWinget.ps1 -version $version -outfolder $stagingfolder
#-------------------------------------------
#==============================================================================
#==============================================================================
# Publish
#-------------------------------------------
# Azure DevOps Migration Tools (Winget) Packaging

.\build\releaseGitHubRelease.ps1 -version $version -artifactFolder $stagingfolder -releaseTag "Local" -GH_TOKEN $env:GITHUB
#-------------------------------------------

# Publish
#Write-Output "PUBLISH ABBWorkItemClone"
#Write-Output "--------------"
#$files = Get-ChildItem -Path ".\output\*" -Recurse
#if ($versionInfo.PreReleaseTag -eq "") {
#    Write-Output "Publishing Release"
#   #gh release create $versionText $files --generate-notes --generate-notes --discussion-category "General"
#} else {
#    Write-Output "Publishing PreRelease"
#   #gh release create $versionText $files --generate-notes --generate-notes --prerelease --discussion-category "General"
#}

 #==============================================================================
 # Final
 Write-Output "Build ran in $((Get-Date) - $StartTimeBuild)"
 #==============================================================================
