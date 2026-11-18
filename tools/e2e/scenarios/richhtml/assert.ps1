#Requires -Version 7.0
# richhtml assertions: the target Description equals the source Description character for character, once the
# rewrites the migration is meant to make are reversed (attachment url, mention id, organisation and project).
param([string]$OutFile)
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
# Project names come from the scenario config, so the seed, the migration and the assertions always agree.
$Src = $cfg.MigrationTools.Endpoints.Source.Project
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()

$srcWi = Invoke-E2ERest "_apis/wit/workitems/$($s.item)" -Org nkdagility-adomt-source
$source = $srcWi.fields.'System.Description'
# Not a pass/fail check: the service normalises HTML when it saves it (#3273), so this records how much.
"INFO service normalised the Description on save: sent $($s.sent.Length), stored $($source.Length) UTF-16 units"

$wi = Find-E2ETargetItem -Project $Tgt -SourceId $s.item
$other = Find-E2ETargetItem -Project $Tgt -SourceId $s.other
if (-not $wi -or -not $other) { $r += New-E2EResult "items migrated" $false "item $($null -ne $wi), mention target $($null -ne $other)"; exit (Write-E2EResults $r $OutFile) }
$target = $wi.fields.'System.Description'

# The service stores an attachment url with the project id added, so the source url to compare with is read from the source value.
$attachment = 'https://dev\.azure\.com/nkdagility-adomt-(source|target)/([^"/]+/)?_apis/wit/attachments/(?<id>[0-9a-fA-F-]{36})(\?fileName=[^"]*)?'
$sourceImg = [regex]::Match($source, $attachment).Value
$targetImg = [regex]::Match($target, $attachment)
# The target image must be a target attachment that serves the seeded bytes, not just any url.
$imageOk = $false
$imageEvidence = "target img: $($targetImg.Value)"
if ($targetImg.Success -and $targetImg.Groups[1].Value -eq 'target') {
    try { $got = Get-E2EAttachmentBytes $targetImg.Groups['id'].Value -Org nkdagility-adomt-target }
    catch { $got = $null; $imageEvidence += "; download failed: $($_.Exception.Message)" }
    $want = [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==")
    $imageOk = $null -ne $got -and [Linq.Enumerable]::SequenceEqual([byte[]]$got, [byte[]]$want)
}
$r += New-E2EResult "image is a target attachment with the seeded bytes" $imageOk $imageEvidence
$mentionUrl = "https://dev.azure.com/nkdagility-adomt-target/$Tgt/_workitems/edit/$($other.id)/"
$r += New-E2EResult "mention points at the target organisation, project and item" ($target.Contains("href=""$mentionUrl""")) $mentionUrl

$normalised = if ($imageOk) { $target.Replace($targetImg.Value, $sourceImg) } else { $target }
$normalised = $normalised.Replace($mentionUrl, "https://dev.azure.com/nkdagility-adomt-source/$Src/_workitems/edit/$($s.other)/").Replace("#$($other.id)<", "#$($s.other)<")

$same = [string]::Equals($normalised, $source, [StringComparison]::Ordinal)
$evidence = "source $($source.Length), target $($target.Length), normalised $($normalised.Length) UTF-16 units"
if (-not $same) {
    $i = 0; while ($i -lt [Math]::Min($source.Length, $normalised.Length) -and $source[$i] -ceq $normalised[$i]) { $i++ }
    $from = [Math]::Max(0, $i - 30)
    $evidence += "; first difference at $i. source: '$($source.Substring($from, [Math]::Min(80, $source.Length - $from)))' target: '$($normalised.Substring($from, [Math]::Min(80, $normalised.Length - $from)))'"
    Set-Content (Join-Path $PSScriptRoot "source.local.html") $source -NoNewline
    Set-Content (Join-Path $PSScriptRoot "target.local.html") $target -NoNewline
}
$r += New-E2EResult "Description arrives exactly (apart from the intended rewrites)" $same $evidence
exit (Write-E2EResults $r $OutFile)
