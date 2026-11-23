# pipelines-3312 outcome checks. Run seed.ps1 first, and build MigrationTools.ConsoleFull.
#   -Case mapped    a definition without repository properties migrates and points at the target repository
#   -Case missing   a definition whose repository is not in the target fails clearly; the other still migrates
#   -Case existing  the existing pipelines scenarios, in amte2e-3312-pipe-*, behave as they do on main
# Each case removes what an earlier run created in its own target project first, so it can be run again.
# Exits 0 when every check passes, 1 otherwise.
param([Parameter(Mandatory)][ValidateSet('mapped', 'missing', 'existing')][string]$Case)
$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
Import-Module (Join-Path $PSScriptRoot '..\..\E2EAzDo.psm1') -Force
$harness = Join-Path $root 'tools\e2e\Invoke-MigrationE2E.ps1'
$failures = [System.Collections.Generic.List[string]]::new()
function Check([bool]$Pass, [string]$What) { if ($Pass) { "PASS $What" } else { "FAIL $What"; $failures.Add($What) } }

function Invoke-Run([string]$Config) {
    $out = & pwsh -NoProfile -File $harness -Config $Config -Filter '.' 2>&1 | ForEach-Object { "$_" }
    $code = $LASTEXITCODE
    $logs = ($out | Select-String '^LOGS: (.+)$' | Select-Object -Last 1).Matches.Groups[1].Value
    $text = if ($logs) { (Get-ChildItem $logs -Recurse -Filter 'migration2*.log' | Get-Content -Raw) -join "`n" } else { $out -join "`n" }
    [pscustomobject]@{ Code = $code; Text = $text }
}
function Get-Definition([string]$Project, [string]$Name) {
    $d = (Invoke-E2ERest "$Project/_apis/build/definitions?name=$Name").value | Select-Object -First 1
    if ($d) { Invoke-E2ERest "$Project/_apis/build/definitions/$($d.id)" }
}
function Remove-Definition([string]$Project, [string]$Name) {
    $d = (Invoke-E2ERest "$Project/_apis/build/definitions?name=$Name").value | Select-Object -First 1
    if ($d) { Invoke-E2ERest "$Project/_apis/build/definitions/$($d.id)" -Method DELETE | Out-Null }
}
function Get-Errors([string]$Text) { @($Text -split "`n" | Where-Object { $_ -match '^\[[0-9:]+ ERR\]' }) }

$tgt = 'amte2e-3312-tgt'
switch ($Case) {
    'mapped' {
        Set-E2EOrg nkdagility-adomt-target
        Remove-Definition $tgt 'amte2e-3312-yaml'
        $run = Invoke-Run (Join-Path $PSScriptRoot 'config-mapped.json')
        $repo = Invoke-E2ERest "$tgt/_apis/git/repositories/amte2e-3312-app"
        $def = Get-Definition $tgt 'amte2e-3312-yaml'
        Check ($run.Code -eq 0) "tool exit code is 0 (was $($run.Code))"
        Check ((Get-Errors $run.Text).Count -eq 0) 'no errors logged'
        Check ($null -ne $def) 'amte2e-3312-yaml exists in the target'
        Check ($def.repository.id -eq $repo.id) "repository id is the target amte2e-3312-app ($($def.repository.id) vs $($repo.id))"
        Check ($def.repository.url -like "*nkdagility-adomt-target*") "repository url is the target's ($($def.repository.url))"
    }
    'missing' {
        Set-E2EOrg nkdagility-adomt-target
        Remove-Definition $tgt 'amte2e-3312-yaml'
        $run = Invoke-Run (Join-Path $PSScriptRoot 'config.json')
        $clear = @(Get-Errors $run.Text | Where-Object { $_ -match 'BuildDefinition "amte2e-3312-missing" cannot be migrated because its repository "amte2e-3312-gone" does not exist in the target project "amte2e-3312-tgt"\. Migrate the repository first, or add a RepositoryNameMaps entry' })
        Check ($run.Code -eq 1) "tool exit code is 1 (was $($run.Code))"
        Check ($clear.Count -eq 1) "one clear error for amte2e-3312-missing (found $($clear.Count))"
        Check ($run.Text -notmatch 'TF401019') 'no TF401019'
        Check ($run.Text -notmatch '(?m)^\s+at \S+\(') 'no stack trace'
        Check ($null -ne (Get-Definition $tgt 'amte2e-3312-yaml')) 'amte2e-3312-yaml was still created'
        Check ($null -eq (Get-Definition $tgt 'amte2e-3312-missing')) 'amte2e-3312-missing is not in the target'
    }
    'existing' {
        $src = 'amte2e-3312-pipe-src'; $ptgt = 'amte2e-3312-pipe-tgt'
        & pwsh -NoProfile -File (Join-Path $PSScriptRoot '..\pipelines\seed.ps1') -Src $src -Tgt $ptgt | Out-Null
        Remove-Item (Join-Path $PSScriptRoot '..\pipelines\state.local.json') -ErrorAction SilentlyContinue
        Set-E2EOrg nkdagility-adomt-target
        Remove-Definition $ptgt 'amte2e-build'
        $rel = (Invoke-E2ERest "$ptgt/_apis/release/definitions?searchText=amte2e-release" -Host_ 'vsrm.dev.azure.com').value | Select-Object -First 1
        if ($rel) { Invoke-E2ERest "$ptgt/_apis/release/definitions/$($rel.id)" -Host_ 'vsrm.dev.azure.com' -Method DELETE | Out-Null }
        $pid_ = (Invoke-E2ERest "_apis/projects/$ptgt").id
        $vg = (Invoke-E2ERest "$ptgt/_apis/distributedtask/variablegroups?groupName=amte2e-vg" -ApiVersion '7.1-preview.2').value | Select-Object -First 1
        if ($vg) { Invoke-E2ERest "_apis/distributedtask/variablegroups/$($vg.id)?projectIds=$pid_" -ApiVersion '7.1-preview.2' -Method DELETE | Out-Null }
        $tmp = Join-Path ([IO.Path]::GetTempPath()) "amte2e-3312-existing"
        New-Item -ItemType Directory -Force $tmp | Out-Null
        $configs = @{}
        foreach ($n in 'config-novariablegroups', 'config') {
            $c = Get-Content (Join-Path $PSScriptRoot "..\pipelines\$n.json") -Raw | ConvertFrom-Json
            $c.MigrationTools.Endpoints.Source.Project = $src; $c.MigrationTools.Endpoints.Target.Project = $ptgt
            $c.MigrationTools.Processors[0].RepositoryNameMaps = @{ $src = $ptgt }
            $configs[$n] = Join-Path $tmp "$n.json"
            $c | ConvertTo-Json -Depth 20 | Set-Content $configs[$n]
        }
        $novg = Invoke-Run $configs['config-novariablegroups']
        $other = @(Get-Errors $novg.Text | Where-Object { $_ -notmatch 'were skipped and 0 were rejected|completed with errors|entered the failed state|The migration run failed' })
        Check ($novg.Code -eq 1) "novariablegroups: tool exit code is 1 (was $($novg.Code))"
        Check ($novg.Text -match "You can't migrate pipelines that uses variablegroups") 'novariablegroups: the variable-group skip is logged'
        Check ($other.Count -eq 0) "novariablegroups: no other errors ($($other -join ' | '))"
        $full = Invoke-Run $configs['config']
        $repo = Invoke-E2ERest "$ptgt/_apis/git/repositories/$ptgt"
        $def = Get-Definition $ptgt 'amte2e-build'
        Check ($full.Code -eq 0) "config: tool exit code is 0 (was $($full.Code))"
        Check ((Get-Errors $full.Text).Count -eq 0) 'config: no errors logged'
        Check ($def.repository.id -eq $repo.id) "config: classic build points at the target repository ($($def.repository.id) vs $($repo.id))"
        Remove-Item $tmp -Recurse -Force
    }
}
if ($failures.Count) { "FAILED: $($failures.Count) check(s)"; exit 1 }
'PASSED'
exit 0
