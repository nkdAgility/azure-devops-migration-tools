#Requires -Version 7.0
# wicore-fresh (#3342): wicore's assertions against the brand-new target project.
param([string]$Src = "amte2e-wicorefresh-src", [string]$Tgt = "amte2e-wicorefresh-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
& (Join-Path $PSScriptRoot "..\wicore\assert.ps1") -Src $Src -Tgt "$Tgt-fresh" -OutFile $OutFile
exit $LASTEXITCODE
