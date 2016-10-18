try {
  $toolsLoc = Get-ToolsLocation
  $vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"
  
  Uninstall-ChocolateyZipPackage 'vsts-sm' 'vstsbulkeditor-#{GITVERSION.FULLSEMVER}#.zip'

  write-host 'VSTS Sync Migration has been uninstalled.'
} catch {
  throw 
}
