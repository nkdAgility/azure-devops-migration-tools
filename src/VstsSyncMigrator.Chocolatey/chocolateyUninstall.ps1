$toolsLoc = Get-ToolsLocation
$vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"

Uninstall-ChocolateyZipPackage 'vstssyncmigrator' 'vstsbulkeditor-#{GITVERSION.FULLSEMVER}#.zip'

write-host 'VSTS Sync Migration has been uninstalled.'
