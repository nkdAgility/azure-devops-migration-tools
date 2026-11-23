# pipelines-3268 outcome checks. Run seed.ps1 first, and build MigrationTools.ConsoleFull.
#   -Case mapped      a release whose deploy job downloads a Build artifact migrates, with the artifact pointing at
#                     the target project and the migrated build definition
#   -Case unmappable  a release whose Build artifact has no target build definition is skipped with a clear error,
#                     and the target never answers with a 400
# Each case removes what an earlier run created in the target project first, so it can be run again.
# Exits 0 when every check passes, 1 otherwise.
param(
    [string]$Src = "amte2e-3268-src",
    [string]$Tgt = "amte2e-3268-tgt",
    [Parameter(Mandatory)][ValidateSet('mapped', 'unmappable')][string]$Case,
    [string]$OutFile
)
$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
Import-Module (Join-Path $PSScriptRoot '..\..\E2EAzDo.psm1') -Force
$harness = Join-Path $root 'tools\e2e\Invoke-MigrationE2E.ps1'
$work = Join-Path ([IO.Path]::GetTempPath()) "amte2e-3268-assert-$PID"
New-Item -ItemType Directory -Force $work | Out-Null
$results = [System.Collections.Generic.List[string]]::new()
$failures = [System.Collections.Generic.List[string]]::new()
function Check([bool]$Pass, [string]$What) {
    $line = if ($Pass) { "PASS $What" } else { "FAIL $What" }
    $line; $results.Add($line); if (-not $Pass) { $failures.Add($What) }
}

function Get-Config([string]$Name) {
    $text = (Get-Content (Join-Path $PSScriptRoot $Name) -Raw).Replace('amte2e-3268-src', $Src).Replace('amte2e-3268-tgt', $Tgt)
    $path = Join-Path $work $Name
    Set-Content $path $text
    $path
}
function Invoke-Run([string]$Config) {
    $out = & pwsh -NoProfile -File $harness -Config $Config -Filter '.' 2>&1 | ForEach-Object { "$_" }
    $code = $LASTEXITCODE
    $logs = ($out | Select-String '^LOGS: (.+)$' | Select-Object -Last 1).Matches.Groups[1].Value
    $text = if ($logs) { (Get-ChildItem $logs -Recurse -Filter 'migration2*.log' | Get-Content -Raw) -join "`n" } else { $out -join "`n" }
    [pscustomobject]@{ Code = $code; Text = $text }
}
function Get-Release([string]$Name) {
    $r = (Invoke-E2ERest "$Tgt/_apis/release/definitions?searchText=$Name&isExactNameMatch=true" -Host_ 'vsrm.dev.azure.com').value | Select-Object -First 1
    if ($r) { Invoke-E2ERest "$Tgt/_apis/release/definitions/$($r.id)" -Host_ 'vsrm.dev.azure.com' }
}
function Remove-Release([string]$Name) {
    $r = (Invoke-E2ERest "$Tgt/_apis/release/definitions?searchText=$Name&isExactNameMatch=true" -Host_ 'vsrm.dev.azure.com').value | Select-Object -First 1
    if ($r) { Invoke-E2ERest "$Tgt/_apis/release/definitions/$($r.id)" -Host_ 'vsrm.dev.azure.com' -Method DELETE | Out-Null }
}
function Get-Errors([string]$Text) { @($Text -split "`n" | Where-Object { $_ -match '^\[[0-9:]+ ERR\]' }) }

Set-E2EOrg nkdagility-adomt-target
switch ($Case) {
    'mapped' {
        Remove-Release 'amte2e-3268-release'
        $run = Invoke-Run (Get-Config 'config.json')
        Check ($run.Code -eq 0) "tool exit code is 0 (was $($run.Code))"
        Check ((Get-Errors $run.Text).Count -eq 0) "no errors logged ($((Get-Errors $run.Text) -join ' | '))"
        $release = Get-Release 'amte2e-3268-release'
        $build = (Invoke-E2ERest "$Tgt/_apis/build/definitions?name=amte2e-3268-build").value | Select-Object -First 1
        $project = Invoke-E2ERest "_apis/projects/$Tgt"
        $artifacts = @($release.artifacts | Where-Object { $_ })
        $artifact = $artifacts | Select-Object -First 1
        Check ($null -ne $release) 'amte2e-3268-release exists in the target'
        Check ($artifacts.Count -eq 1) "the release has its artifact ($($artifacts.Count))"
        Check ($artifact.alias -eq '_amte2e-3268-build') "the alias is kept ($($artifact.alias))"
        Check ($artifact.definitionReference.project.id -eq $project.id) "the artifact points at the target project ($($artifact.definitionReference.project.id) vs $($project.id))"
        Check ($null -ne $build -and $artifact.definitionReference.definition.id -eq "$($build.id)") "the artifact points at the target build definition ($($artifact.definitionReference.definition.id) vs $($build.id))"
    }
    'unmappable' {
        Remove-Release 'amte2e-3268-orphan-release'
        $run = Invoke-Run (Get-Config 'config-unmappable.json')
        $pattern = 'ReleaseDefinition "amte2e-3268-orphan-release" cannot be migrated because its artifact "_amte2e-3268-orphan" \(Build\) cannot be mapped to the target: its build definition "amte2e-3268-orphan" is not in the target project'
        $clear = @(Get-Errors $run.Text | Where-Object { $_ -match $pattern })
        Check ($run.Code -eq 1) "tool exit code is 1 (was $($run.Code))"
        Check ($clear.Count -eq 1) "one clear error naming the release, alias and type (found $($clear.Count))"
        Check ($run.Text -notmatch 'InvalidRequestException|does not matching any linked artifact') 'the target was not sent the release (no 400)'
        Check ($run.Text -match '1 of 1 source ReleaseDefinition\(s\) were skipped and 0 were rejected') 'the release counts as skipped'
        Check ($null -eq (Get-Release 'amte2e-3268-orphan-release')) 'amte2e-3268-orphan-release is not in the target'
    }
}
Remove-Item $work -Recurse -Force
if ($OutFile) { $results | Set-Content $OutFile }
if ($failures.Count) { "FAILED: $($failures.Count) check(s)"; exit 1 }
'PASSED'
exit 0
