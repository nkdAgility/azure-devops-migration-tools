<#
    Script description.

    Some notes.
#>
param (    
    # name of the output folder
    [Parameter(Mandatory=$true)]
    [string]$vsixFile,
    [Parameter(Mandatory=$true)]
    [string]$marketplaceToken
)
Write-Output "Azure DevOps Migration Tools (Extension) Release"
Write-Output "----------------------------------------"
Write-Output "Extension file: $extensionFile"
Write-Output "----------------------------------------"
if (((npm list -g tfx-cli) -join "," ).Contains("empty")) {
    Write-Output "Installing tfx-cli"
    npm i -g tfx-cli
} else { Write-Output "Detected tfx-cli"}
Write-Output "----------------------------------------"
# Login
Write-Output ">>>>> Login"
tfx login --service-url https://marketplace.visualstudio.com --token $marketplaceToken
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
# Build TFS Extension
Write-Output ">>>>> Send TFS Extension"
tfx extension publish --vsix "$vsixFile" --token $marketplaceToken
Write-Output "----------------------------------------"