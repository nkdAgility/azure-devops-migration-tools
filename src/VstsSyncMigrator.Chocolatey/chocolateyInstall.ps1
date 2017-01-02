$toolsLoc = Get-ToolsLocation
$vstssyncmigrationpath =Join-Path -Path $toolsLoc -ChildPath "\VSTSSyncMigration"

if(test-path $vstssyncmigrationpath) {
  write-host "Cleaning out the contents of $vstssyncmigrationpath"
  Remove-Item "$($vstssyncmigrationpath)\*" -recurse -force
}

Install-ChocolateyZipPackage 'vstssyncmigrator' 'https://github.com/nkdAgility/vsts-sync-migration/releases/download/#{GITVERSION.FULLSEMVER}#/vstssyncmigrator-#{GITVERSION.FULLSEMVER}#.zip' $vstssyncmigrationpath -Checksum #{Chocolatey.FileHash}# -ChecksumType SHA256
write-host 'VSTS Sync Migration has been installed. Call `vstssyncmigrator` from the command line to see options. You may need to close and reopen the command shell.'
