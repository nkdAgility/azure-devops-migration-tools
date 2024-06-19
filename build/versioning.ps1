.\build\logging.ps1
Write-InfoLog "Detect Version"
Write-InfoLog "--------------"

$IsLocal = $False
Write-InfoLog "Azure DevOps Build ID: $($Env:Build_BuildId)"
Write-InfoLog "GitHub Run ID: $($github.run_id)"
$IsGitHubAction = $False
$IsAzureDevOps = $False
$IsLocal = $True
if ($Env:Build_BuildId -ne $Null)
{
    $IsAzureDevOps = $True
}
if ($github.run_id -ne $Null)
{
    $IsGitHubAction = $True
}
if ($IsAzureDevOps -and $IsGitHubAction)
{
    $IsLocal = $False
}
Write-InfoLog "IsGitHubAction: $IsGitHubAction"
Write-InfoLog "IsAzureDevOps: $IsAzureDevOps"
Write-InfoLog "IsLocal: $IsLocal"

# Get Version Numbers
$versionInfoJson = dotnet-gitversion
If ($IsLocal) {
    $versionInfoJson = $versionInfoJson | foreach {$_.replace("Preview","Local")}
    $versionInfoJson = $versionInfoJson | foreach {$_.replace("preview","Local")}
}
$versionInfo = $versionInfoJson | ConvertFrom-Json
$versionInfo | ConvertTo-Json |  Set-Content -Path ".\output\GitVersion.json"
Write-InfoLog "FullSemVer: $($versionInfo.FullSemVer)"
Write-InfoLog "SemVer: $($versionInfo.SemVer)"
Write-InfoLog "PreReleaseTag: $($versionInfo.PreReleaseTag)"
Write-InfoLog "InformationalVersion: $($versionInfo.InformationalVersion)"
Write-InfoLog "--------------"
If ($IsAzureDevOps)
{
    $versionInfo.PSObject.Properties | ForEach-Object {
        Write-Output "##vso[task.setvariable variable=$_.Name;isOutput=true;issecret=false;]$_.Value"
        Write-Output "##vso[task.setvariable variable=GitVersion_$_.Name;isOutput=true;issecret=false;]$_.Value"
      }
}