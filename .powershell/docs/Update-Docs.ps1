# Helpers
. ./.powershell/docs/_includes/IncludesForAll.ps1
$levelSwitch.MinimumLevel = 'Information'

$hugoMarkdownObjects = Get-RecentHugoMarkdownResources -Path ".\docs\content\docs\" 

foreach ($hugoMarkdown in $hugoMarkdownObjects) {

    if ( $hugoMarkdown.FrontMatter.dataFile) {
        $classDataFile = (Join-Path "docs" $hugoMarkdown.FrontMatter.dataFile)
        if (Test-Path -Path $classDataFile) {
            $classData = Get-Content -Path $classDataFile -Raw | ConvertFrom-Yaml
            Write-InformationLog $classData.description
            if ($classData.description -ne "missing XML code comments") {
                $newDescription = $classData.description
            }            
        }
    }

    If ($newDescription -eq $null) {
        Write-ErrorLog "No description found for $($hugoMarkdown.FilePath)"
        continue
    }


    Update-Field -FieldName 'description' -FieldValue $newDescription -frontMatter $hugoMarkdown.frontMatter -addAfter "title"
    Save-HugoMarkdown -HugoMarkdown $hugoMarkdown -Path $hugoMarkdown.FilePath
   

    

}