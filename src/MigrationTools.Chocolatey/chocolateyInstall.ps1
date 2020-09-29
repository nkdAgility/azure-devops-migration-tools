$toolsLoc = Get-ToolsLocation

$vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"
$migrationtoolspath =Join-Path -Path $toolsLoc -ChildPath "\MigrationTools"

if(test-path $vstssyncmigrationpath) {
  write-host "Cleaning out the contents of $vstssyncmigrationpath"
  Remove-Item "$($vstssyncmigrationpath)\*" -recurse -force -exclude *.json
}

if(test-path $migrationtoolspath) {
  write-host "Cleaning out the contents of $migrationtoolspath"
  Remove-Item "$($migrationtoolspath)\*" -recurse -force -exclude *.json
}

Install-ChocolateyZipPackage 'MigrationTools' 'https://github.com/nkdAgility/azure-devops-migration-tools/releases/download/v#{GITVERSION.SEMVER}#/MigrationTools-#{GITVERSION.SEMVER}#.zip' $migrationtoolspath -Checksum #{Chocolatey.FileHash}# -ChecksumType SHA256
write-host 'Azure DevOps Migration have been installed. Call `migration` from the command line to see options. You may need to close and reopen the command shell.'
