<#
    Script description.

    Some notes.
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version,
    
    # name of the output folder
    [Parameter(Mandatory=$true)]
    [string]$outfolder
)
Write-Output "Azure DevOps Migration Tools (Winget) Packaging"
Write-Output "----------------------------------------"
Write-Output "Version: $version"
Write-Output "Output Folder: $outfolder"
#==============================================================================
New-Item -Path $outfolder -Name "\WinGet\" -ItemType Directory
Copy-Item  -Path ".\src\MigrationTools.WinGet\**" -Destination "$outfolder\WinGet" -Recurse
