$toolsLoc = Get-ToolsLocation
$vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"

if(test-path $vstssyncmigrationpath) {
  write-host "Cleaning out the contents of $vstssyncmigrationpath"
  Remove-Item "$($vstssyncmigrationpath)\*" -recurse -force -exclude *.json
}

Install-ChocolateyZipPackage 'vstssyncmigrator' 'https://github.com/nkdAgility/azure-devops-migration-tools/releases/download/#{GITVERSION.SEMVER}#/vstssyncmigrator-#{GITVERSION.SEMVER}#.zip' $vstssyncmigrationpath -Checksum #{Chocolatey.FileHash}# -ChecksumType SHA256
write-host 'VSTS Sync Migration has been installed. Call `migration` from the command line to see options. You may need to close and reopen the command shell.'
