#Requires -Version 7.0
# richhtml: #3273. A Description with entities, emoji, supplementary-plane letters, links, a work item mention and an
# embedded image must arrive exactly, apart from the rewrites the migration is meant to make (attachment url,
# mention id, organisation and project). The body is sent as ASCII-escaped JSON so no character depends on encoding.
param([string]$Src = "amte2e-3271-html-src", [string]$Tgt = "amte2e-3271-html-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

function New-Item3273([string]$Title, [string]$Html) {
    $patch = @(
        @{ op = "add"; path = "/fields/System.Title"; value = $Title },
        @{ op = "add"; path = "/fields/System.Description"; value = $Html })
    $body = ConvertTo-Json @($patch) -Depth 5 -EscapeHandling EscapeNonAscii
    Invoke-E2ERest "$Src/_apis/wit/workitems/`$User%20Story?suppressNotifications=true" -Method POST -Body $body -ContentType "application/json-patch+json"
}

$png = [Convert]::FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==")
$img = New-E2EAttachment "pixel.png" $png
$other = New-Item3273 "in-scope mention target" "<div>plain</div>"

function U([int]$cp) { [char]::ConvertFromUtf32($cp) }
$html = @(
    "<div>Entities: &amp; &lt;b&gt; &quot;quoted&quot; &#39;apos&#39; a&nbsp;b &copy; $(U(0x2014)) $(U(0x201C))smart$(U(0x201D))</div>",
    "<div>Emoji: $(U(0x1F600)) $(U(0x1F44D))$(U(0x1F3FD)) $(U(0x1F468))$(U(0x200D))$(U(0x1F469))$(U(0x200D))$(U(0x1F467)) BMP $(U(0x231A)) Osage $(U(0x104CF)) CJK-B $(U(0x20000)) math $(U(0x1D538))</div>",
    "<div><a href=""https://example.com/a?x=1&amp;y=2"">external link</a></div>",
    "<div><a href=""https://dev.azure.com/nkdagility-adomt-source/$Src/_workitems/edit/$($other.id)/"" data-vss-mention=""version:1.0"">#$($other.id)</a> mention</div>",
    "<div><img src=""$($img.url)""></div>",
    "<p style=""color:red"">styled</p><ul><li>one</li><li>two</li></ul><br>"
) -join ""
$item = New-Item3273 "in-scope rich description" $html

Save-E2EState $PSScriptRoot @{ item = $item.id; other = $other.id; img = $img.url; sent = $html }
"seeded richhtml: item $($item.id) (Description sent $($html.Length) UTF-16 units, stored $($item.fields.'System.Description'.Length)) mention target $($other.id)"
