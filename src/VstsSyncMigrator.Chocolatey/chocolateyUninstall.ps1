$toolsLoc = Get-ToolsLocation
$vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"

Uninstall-ChocolateyZipPackage 'vstssyncmigrator' 'vstssyncmigrator-#{GITVERSION.SEMVER}#.zip'

write-host 'VSTS Sync Migration has been uninstalled.'
