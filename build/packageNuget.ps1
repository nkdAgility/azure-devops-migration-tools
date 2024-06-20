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
Write-Output "Azure DevOps Migration Tools (Nuget) Packaging"
Write-Output "----------------------------------------"
Write-Output "Version: $version"
Write-Output "Output Folder: $outfolder"
#==============================================================================

Copy-Item  -Path ".\src\MigrationTools\bin\**\*.nupkg" -Destination "$outfolder\" -Recurse
