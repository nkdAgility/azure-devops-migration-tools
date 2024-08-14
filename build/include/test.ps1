
#$Env:OPEN_AI_KEY = ""

. ./build/include/Get-ReleaseDescription.ps1

$asdsadsad = Get-GitChanges -compairFrom main -compairTo "build/update-release-descritpion" -mode diff

$description = Get-PullRequestData -mode diff -OPEN_AI_KEY $Env:OPEN_AI_KEY -compairFrom main -compairTo "build/update-release-descritpion"

Write-Host ""
Write-Host ""
Write-Host ""
Write-Host ""
Write-Host $description