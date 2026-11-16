param([switch]$TargetOnly)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$repo = $args[0]
function New-PL($org, $items) { Invoke-E2ERest "_apis/work/processes/lists" -Org $org -Method POST -Body @{ name = "picklist_$([guid]::NewGuid())"; type = "String"; isSuggested = $false; items = $items } }
function New-F($org, $name, $listId) { Invoke-E2ERest "_apis/wit/fields" -Org $org -Method POST -Body @{ name = $name; type = "string"; usage = "workItem"; isPicklist = $true; picklistId = $listId } | Out-Null }
$t = "nkdagility-adomt-target"; Set-E2EOrg $t
$shared = New-PL $t @("X")
New-F $t "amte2eAmt3241SharedA" $shared.id; New-F $t "amte2eAmt3241SharedB" $shared.id
"target shared list $($shared.id)"
if ($TargetOnly) { return }
$s = "nkdagility-adomt-source"; Set-E2EOrg $s
New-F $s "amte2eAmt3241SharedA" (New-PL $s @("X","Y")).id
New-F $s "amte2eAmt3241SharedB" (New-PL $s @("X","Z")).id
"source SharedA [X,Y], SharedB [X,Z]"
