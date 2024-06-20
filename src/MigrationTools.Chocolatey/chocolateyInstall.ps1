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

Install-ChocolateyZipPackage 'MigrationTools' 'https://github.com/nkdAgility/azure-devops-migration-tools/releases/download/v0.0.1/MigrationTools-0.0.1.zip' $migrationtoolspath -Checksum 9AB038099B52F7D39938518A2449301F82B9113888A494A00F82F2B769861D96 -ChecksumType SHA256
write-host 'Azure DevOps Migration have been installed. Call `migration` from the command line to see options. You may need to close and reopen the command shell.'
