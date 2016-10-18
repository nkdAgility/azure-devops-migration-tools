try {
  $toolsLoc = Get-ToolsLocation
  $vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"

  if(test-path $vstssyncmigrationpath) {
    write-host "Cleaning out the contents of $vstssyncmigrationpath"
    Remove-Item "$($vstssyncmigrationpath)\*" -recurse -force
  }
  
  Install-ChocolateyZipPackage 'vstssyncmigration' 'https://github.com/nkdAgility/vsts-sync-migration/releases/download/#{GITVERSION.FULLSEMVER}#/vstsbulkeditor-#{GITVERSION.FULLSEMVER}#.zip' $vstssyncmigrationpath

  write-host 'VSTS Sync Migration has been installed. Call `vstssyncmigration` from the command line to see options. You may need to close and reopen the command shell.'
  Write-ChocolateySuccess 'vstssyncmigration'
} catch {
  Write-ChocolateyFailure 'vstssyncmigration' $($_.Exception.Message)
  throw 
}