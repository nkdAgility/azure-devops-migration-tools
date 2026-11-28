#Requires -Version 7.0
# stringmanip-3258: StringManipulatorTool's built-in default must remove only what the SOAP API cannot save.
#  letters - Bug whose Title and Found In hold a letter outside the BMP (Osage), a CJK Extension B letter, a BMP emoji, an emoji
#            outside the BMP and an emoji with U+FE0F. SOAP saves all of them, so the target must match the source exactly.
#  nonchar - Bug whose Found In holds U+FFFE, which SOAP rejects. The default removes it, so the item migrates as "XY".
# config.json runs the default (Manipulators empty) and must exit 0. config-user.json has its own no-op manipulator,
# which replaces the default, and migrates only the letters Bug: it must exit 0 with U+FE0F and the emoji kept.
# The defaults are the project names in config.json (Src, Tgt) and config-user.json (TgtUser).
param([string]$Src = "amte2e-3258-src2", [string]$Tgt = "amte2e-3258-tgt2", [string]$TgtUser = "amte2e-3258-tgtu2", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
$Tgts = $Tgt, $TgtUser
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

# Built from code points, so no tool or editor can change the characters.
$text = "A" + [char]::ConvertFromUtf32(0x104CF) + "B " + [char]::ConvertFromUtf32(0x20000) + " W" + [char]0x231A + " E" + [char]::ConvertFromUtf32(0x1F600) + " H" + [char]0x2764 + [char]0xFE0F
$nonchar = "X" + [char]0xFFFE + "Y"

Set-E2EOrg nkdagility-adomt-target
foreach ($t in $Tgts) { New-E2EProject -Name $t -Process $Process | Out-Null }
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
$letters = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "in-scope 3258 $text"; "Microsoft.VSTS.Build.FoundIn" = $text })
$nc = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "in-scope 3258 nonchar"; "Microsoft.VSTS.Build.FoundIn" = $nonchar })
Save-E2EState $PSScriptRoot @{ letters = $letters.id; nonchar = $nc.id; text = $text }
"seeded 3258: letters $($letters.id) nonchar $($nc.id)"
