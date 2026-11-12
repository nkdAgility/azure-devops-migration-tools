<#
.SYNOPSIS
    Deletes every amte2e-* project and process, and every Custom.amte2e* field, from the test organisations, and reports leftovers.
.DESCRIPTION
    Order matters: projects first (processes in use can't be deleted), then processes, then fields (fields in use by a process
    can't be deleted). Deleted projects go to the organisation's recycle bin; they are then destroyed so the processes they used
    can be deleted too. Nothing outside the amte2e prefix is touched.
#>
# -Prefix "amt<issue>-" removes only that issue's projects and processes, and only its Custom.amte2e<issue>* fields.
param([string[]]$Organisations = @("nkdagility-adomt-source", "nkdagility-adomt-target"), [string]$Prefix = "amte2e-")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "E2EProcess.psm1") -Force

$fieldPrefix = if ($Prefix -match '^amt(\d+)-$') { "Custom.amte2e$($matches[1])" } else { "Custom.amte2e" }
function Get-ScopedFields([string]$Org) { @(Get-E2EFields -Org $Org) | Where-Object { $_.referenceName.StartsWith($fieldPrefix) } }
$leftovers = @()
foreach ($org in $Organisations) {
    Set-E2EOrg $org
    foreach ($p in @(Get-E2EProjects -Org $org -Prefix $Prefix)) {
        try { Remove-E2EProject $p.name -Org $org; "deleted project $org/$($p.name)" } catch { $leftovers += "project $org/$($p.name): $($_.Exception.Message)" }
    }
    # Destroy amte2e projects waiting in the recycle bin so their processes are no longer referenced.
    try {
        $bin = (Invoke-E2ERest "_apis/projects?stateFilter=deleted&`$top=500" -Org $org).value | Where-Object { $_.name.StartsWith($Prefix) }
        foreach ($p in $bin) {
            try { Wait-E2EOperation (Invoke-E2ERest "_apis/projects/$($p.id)?hardDelete=true" -Org $org -Method DELETE -ApiVersion "7.1-preview.4") | Out-Null; "destroyed recycled project $org/$($p.name)" }
            catch { $leftovers += "project $org/$($p.name): soft-deleted, in the recycle bin until Azure DevOps purges it ($(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message))" }
        }
    } catch { "recycle bin not listed for $($org): $($_.Exception.Message)" }
    foreach ($pr in @(Get-E2EProcesses -Org $org -Prefix $Prefix)) {
        # Projects deleted moments ago can still hold the process, so retry before reporting it.
        for ($try = 1; ; $try++) {
            try { Remove-E2EProcess $pr.name -Org $org; "deleted process $org/$($pr.name)"; break }
            catch { if ($try -ge 4) { $leftovers += "process $org/$($pr.name): $(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message)"; break }; Start-Sleep -Seconds 15 }
        }
    }
    foreach ($f in @(Get-ScopedFields $org)) {
        try { Remove-E2EField $f.referenceName -Org $org; "deleted field $org/$($f.referenceName)" } catch { $leftovers += "field $org/$($f.referenceName): $(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message)" }
    }
    $remaining = @(Get-E2EProjects -Org $org -Prefix $Prefix).name + @(Get-E2EProcesses -Org $org -Prefix $Prefix).name + @(Get-ScopedFields $org).referenceName | Where-Object { $_ }
    "remaining in $($org): " + $(if ($remaining) { $remaining -join ", " } else { "none" })
}
if ($leftovers) { "LEFTOVERS:"; $leftovers | ForEach-Object { "  $_" } }
