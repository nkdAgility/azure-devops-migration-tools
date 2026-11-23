# pipelines-3312 outcome checks. Run seed.ps1 first, and build MigrationTools.ConsoleFull.
#   -Case proof     (default, as proof.ps1 runs it after migrating config.json) the migrated definition points at the
#                   target repository, then the missing case
#   -Case mapped    a definition without repository properties migrates and points at the target repository
#   -Case missing   a definition whose repository is not in the target fails clearly; the other still migrates
#   -Case existing  the existing pipelines scenarios, in amte2e-3312-pipe-*, behave as they do on main
# Each case that migrates removes what an earlier run created in its target project first, so it can be run again.
# Exits 0 when every check passes, 1 otherwise.
param(
    [string]$Src = "amte2e-3312-src",
    [string]$Tgt = "amte2e-3312-tgt",
    [ValidateSet('proof', 'mapped', 'missing', 'existing')][string]$Case = 'proof',
    [string]$OutFile
)
$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
Import-Module (Join-Path $PSScriptRoot '..\..\E2EAzDo.psm1') -Force
$harness = Join-Path $root 'tools\e2e\Invoke-MigrationE2E.ps1'
$work = Join-Path ([IO.Path]::GetTempPath()) "amte2e-3312-assert-$PID"
New-Item -ItemType Directory -Force $work | Out-Null
$results = [System.Collections.Generic.List[string]]::new()
$failures = [System.Collections.Generic.List[string]]::new()
function Check([bool]$Pass, [string]$What) {
    $line = if ($Pass) { "PASS $What" } else { "FAIL $What" }
    $line; $results.Add($line); if (-not $Pass) { $failures.Add($What) }
}

function Get-Config([string]$Name, [string]$From, [string]$To, [hashtable]$Extra = @{}) {
    $text = (Get-Content (Join-Path $PSScriptRoot $Name) -Raw).Replace('amte2e-3312-src', $From).Replace('amte2e-3312-tgt', $To)
    $c = $text | ConvertFrom-Json
    foreach ($k in $Extra.Keys) { $c.MigrationTools.Processors[0].$k = $Extra[$k] }
    $path = Join-Path $work $Name
    $c | ConvertTo-Json -Depth 20 | Set-Content $path
    $path
}
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

function Test-Mapped {
    $repo = Invoke-E2ERest "$Tgt/_apis/git/repositories/amte2e-3312-app"
    $def = Get-Definition $Tgt 'amte2e-3312-yaml'
    Check ($null -ne $def) 'amte2e-3312-yaml exists in the target'
    Check ($def.repository.id -eq $repo.id) "repository id is the target amte2e-3312-app ($($def.repository.id) vs $($repo.id))"
    Check ($def.repository.url -like "*nkdagility-adomt-target*") "repository url is the target's ($($def.repository.url))"
}
function Test-Missing {
    Remove-Definition $Tgt 'amte2e-3312-yaml'
    $run = Invoke-Run (Get-Config 'config-both.json' $Src $Tgt)
    $pattern = "BuildDefinition ""amte2e-3312-missing"" cannot be migrated because its repository ""amte2e-3312-gone"" does not exist in the target project ""$([regex]::Escape($Tgt))""\. Migrate the repository first, or add a RepositoryNameMaps entry"
    $clear = @(Get-Errors $run.Text | Where-Object { $_ -match $pattern })
    Check ($run.Code -eq 1) "missing: tool exit code is 1 (was $($run.Code))"
    Check ($clear.Count -eq 1) "missing: one clear error for amte2e-3312-missing (found $($clear.Count))"
    Check ($run.Text -notmatch 'TF401019') 'missing: no TF401019'
    Check ($run.Text -notmatch '(?m)^\s+at \S+\(') 'missing: no stack trace'
    Check ($null -ne (Get-Definition $Tgt 'amte2e-3312-yaml')) 'missing: amte2e-3312-yaml was still created'
    Check ($null -eq (Get-Definition $Tgt 'amte2e-3312-missing')) 'missing: amte2e-3312-missing is not in the target'
}

Set-E2EOrg nkdagility-adomt-target
switch ($Case) {
    'proof' { Test-Mapped; Test-Missing }
    'mapped' {
        Remove-Definition $Tgt 'amte2e-3312-yaml'
        $run = Invoke-Run (Get-Config 'config.json' $Src $Tgt)
        Check ($run.Code -eq 0) "tool exit code is 0 (was $($run.Code))"
        Check ((Get-Errors $run.Text).Count -eq 0) 'no errors logged'
        Test-Mapped
    }
    'missing' { Test-Missing }
    'existing' {
        $psrc = 'amte2e-3312-pipe-src'; $ptgt = 'amte2e-3312-pipe-tgt'
        & pwsh -NoProfile -File (Join-Path $PSScriptRoot '..\pipelines\seed.ps1') -Src $psrc -Tgt $ptgt | Out-Null
        Remove-Item (Join-Path $PSScriptRoot '..\pipelines\state.local.json') -ErrorAction SilentlyContinue
        Set-E2EOrg nkdagility-adomt-target
        Remove-Definition $ptgt 'amte2e-build'
        $rel = (Invoke-E2ERest "$ptgt/_apis/release/definitions?searchText=amte2e-release" -Host_ 'vsrm.dev.azure.com').value | Select-Object -First 1
        if ($rel) { Invoke-E2ERest "$ptgt/_apis/release/definitions/$($rel.id)" -Host_ 'vsrm.dev.azure.com' -Method DELETE | Out-Null }
        $projectId = (Invoke-E2ERest "_apis/projects/$ptgt").id
        $vg = (Invoke-E2ERest "$ptgt/_apis/distributedtask/variablegroups?groupName=amte2e-vg" -ApiVersion '7.1-preview.2').value | Select-Object -First 1
        if ($vg) { Invoke-E2ERest "_apis/distributedtask/variablegroups/$($vg.id)?projectIds=$projectId" -ApiVersion '7.1-preview.2' -Method DELETE | Out-Null }
        $configs = @{}
        foreach ($n in 'config-novariablegroups', 'config') {
            $c = Get-Content (Join-Path $PSScriptRoot "..\pipelines\$n.json") -Raw | ConvertFrom-Json
            $c.MigrationTools.Endpoints.Source.Project = $psrc; $c.MigrationTools.Endpoints.Target.Project = $ptgt
            $c.MigrationTools.Processors[0].RepositoryNameMaps = @{ $psrc = $ptgt }
            $configs[$n] = Join-Path $work "pipelines-$n.json"
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
    }
}
Remove-Item $work -Recurse -Force
if ($OutFile) { $results | Set-Content $OutFile }
if ($failures.Count) { "FAILED: $($failures.Count) check(s)"; exit 1 }
'PASSED'
exit 0
