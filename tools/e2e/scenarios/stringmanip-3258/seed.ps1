#Requires -Version 7.0
# stringmanip-3258: StringManipulatorTool's built-in default must remove only what the SOAP API cannot save.
#  letters - Bug whose Title and Found In hold a letter outside the BMP (Osage), a CJK Extension B letter, a BMP emoji and
#            an emoji outside the BMP. SOAP saves all of them, so the target must match the source exactly.
#  nonchar - Bug whose Found In holds U+FFFE, which SOAP rejects. The default removes it, so the item migrates as "AB".
# config.json runs the default (Manipulators empty). config-user.json has its own manipulator, which replaces the
# default: U+FFFE is then not removed, that Bug fails to save, and the run must exit 1.
param([string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
$Src = (Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints.Source.Project
$Tgts = foreach ($c in "config.json", "config-user.json") { (Get-Content (Join-Path $PSScriptRoot $c) -Raw | ConvertFrom-Json).MigrationTools.Endpoints.Target.Project }
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

# Built from code points, so no tool or editor can change the characters.
$text = "A" + [char]::ConvertFromUtf32(0x104CF) + "B " + [char]::ConvertFromUtf32(0x20000) + " W" + [char]0x231A + " E" + [char]::ConvertFromUtf32(0x1F600)
$nonchar = "A" + [char]0xFFFE + "B"

Set-E2EOrg nkdagility-adomt-target
foreach ($t in $Tgts) { New-E2EProject -Name $t -Process $Process | Out-Null }
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
$letters = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "in-scope 3258 $text"; "Microsoft.VSTS.Build.FoundIn" = $text })
$nc = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "in-scope 3258 nonchar"; "Microsoft.VSTS.Build.FoundIn" = $nonchar })
Save-E2EState $PSScriptRoot @{ letters = $letters.id; nonchar = $nc.id; text = $text }
"seeded 3258: letters $($letters.id) nonchar $($nc.id)"
