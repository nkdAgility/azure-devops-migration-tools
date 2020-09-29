Uninstall-ChocolateyZipPackage 'vstssyncmigrator' 'vstssyncmigrator-#{GITVERSION.SEMVER}#.zip'
Uninstall-ChocolateyZipPackage 'MigrationTools' 'MigrationTools-#{GITVERSION.SEMVER}#.zip'

write-host 'Azure DevOps Migration Tools has been uninstalled.'
