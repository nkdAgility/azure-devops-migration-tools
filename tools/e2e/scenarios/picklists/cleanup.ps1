# Removes only what the picklists scenario creates: amte2e-amt3241-* projects and processes, Custom.amte2eAmt3241* fields
# and the picklists those fields used. Remove-E2EObjects.ps1 would also remove other scenarios' objects.
param([string[]]$Organisations = @("nkdagility-adomt-source", "nkdagility-adomt-target"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
foreach ($org in $Organisations) {
    Set-E2EOrg $org
    foreach ($p in @(Get-E2EProjects -Org $org | Where-Object name -like "amte2e-amt3241-*")) {
        Remove-E2EProject $p.name -Org $org; "deleted project $org/$($p.name)"
    }
    $bin = (Invoke-E2ERest "_apis/projects?stateFilter=deleted&`$top=500" -Org $org).value | Where-Object name -like "amte2e-amt3241-*"
    foreach ($p in $bin) { "project $org/$($p.name) is in the recycle bin until Azure DevOps purges it (hard delete is not allowed on Services)" }
    foreach ($pr in @(Get-E2EProcesses -Org $org | Where-Object name -like "amte2e-amt3241-*")) {
        for ($try = 1; ; $try++) {
            try { Remove-E2EProcess $pr.name -Org $org; "deleted process $org/$($pr.name)"; break }
            catch { if ($try -ge 4) { throw }; Start-Sleep -Seconds 15 }
        }
    }
    foreach ($f in @((Invoke-E2ERest "_apis/wit/fields" -Org $org).value | Where-Object referenceName -like "Custom.amte2eAmt3241*")) {
        Remove-E2EField $f.referenceName -Org $org
        $listNote = ""
        if ($f.picklistId) {
            # A just-deleted field can still hold its list for a moment (VS403076).
            for ($try = 1; $try -le 4; $try++) {
                try { Invoke-E2ERest "_apis/work/processes/lists/$($f.picklistId)" -Org $org -Method DELETE | Out-Null; $listNote = " and its picklist"; break }
                catch { $listNote = " (picklist $($f.picklistId) still referenced: $(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message))"; Start-Sleep -Seconds 10 }
            }
        }
        "deleted field $org/$($f.referenceName)$listNote"
    }
}
