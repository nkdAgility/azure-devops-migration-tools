# images: #3200 (placeholder work items destroyed), #3203 (no doubled ?fileName).
# One item whose Description embeds two attachment images: a normal url and one with the doubled ?fileName query.
param([string]$Src = "amte2e-images-src", [string]$Tgt = "amte2e-images-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

# 1x1 PNG. The attachment url returned by the API already ends with ?fileName=<name>.
$png = [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==")
$a = New-E2EAttachment "a.png" $png
$b = New-E2EAttachment "b.png" $png
$html = "<div>normal <img src=""$($a.url)""></div><div>doubled <img src=""$($b.url)?fileName=b.png""></div>"
$item = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "images item"; "System.Description" = $html })
Set-E2EWorkItem -Id $item.id -Fields ([ordered]@{ "System.History" = "history with image <img src=""$($a.url)"">" }) | Out-Null

Save-E2EState $PSScriptRoot @{ item = $item.id; a = $a.url; b = $b.url }
"seeded images: item $($item.id)"
