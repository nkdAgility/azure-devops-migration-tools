try {
  Import-Module "$env:ChocolateyInstall\helpers\chocolateyInstaller.psm1" -Force;
  $toolsLoc = Get-ToolsLocation
  $vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"
  
  Uninstall-ChocolateyZipPackage 'vstssyncmigration' 'vstsbulkeditor-#{GITVERSION.FULLSEMVER}#.zip'

  write-host 'VSTS Sync Migration has been uninstalled.'
} catch {
  throw 
}
