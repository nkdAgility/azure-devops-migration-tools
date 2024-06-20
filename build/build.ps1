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
New-Item -Name "output" -ItemType Directory

#==============================================================================
. .\build\versioning.ps1
#==============================================================================
. .\build\compile-and-test.ps1
#==============================================================================
# Azure DevOps Migration Tools (Executable) Packaging
.\build\packageMigrationTools.ps1 -version $versionInfo.SemVer -outfolder ".\output"
#-------------------------------------------
# Azure DevOps Migration Tools (Extension) Packaging
.\build\packageExtension.ps1 -version $versionInfo.SemVer -outfolder ".\output"
#-------------------------------------------
# Azure DevOps Migration Tools (Chocolatey) Packaging
.\build\packageCocolatey.ps1 -version $versionInfo.SemVer -outfolder ".\output"
#-------------------------------------------
# Azure DevOps Migration Tools (Nuget) Packaging
.\build\packageNuget.ps1 -version $versionInfo.SemVer -outfolder ".\output"
#-------------------------------------------
# Azure DevOps Migration Tools (Winget) Packaging
.\build\packageWinget.ps1 -version $versionInfo.SemVer -outfolder ".\output"
#-------------------------------------------
 #==============================================================================
 # Cleanup
 Write-Output "Cleanup"
 Remove-Item -Path ".\output\MigrationTools" -Recurse -Force
 #==============================================================================
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
