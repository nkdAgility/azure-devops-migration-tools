
#$Env:OPEN_AI_KEY = ""

. ./build/include/Get-ReleaseDescription.ps1

$description = Get-ReleaseDescription2 -mode log -OPEN_AI_KEY $Env:OPEN_AI_KEY 

Write-Host ""
Write-Host ""
Write-Host ""
Write-Host ""
Write-Host $description