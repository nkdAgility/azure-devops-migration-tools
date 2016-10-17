try {
  $sysDrive = $env:SystemDrive
  $vstssyncmigrationpath =Join-Path -Path "\VSTSSyncMigration" -ChildPath Get-ToolsLocation
  
  Uninstall-ChocolateyZipPackage 'vstssyncmigration' 'vstsbulkeditor-#{GITVERSION.FULLSEMVER}#.zip'

  write-host 'VSTS Sync Migration has been uninstalled.'
  Write-ChocolateySuccess 'vstssyncmigration'
} catch {
  Write-ChocolateyFailure 'vstssyncmigration' $($_.Exception.Message)
  throw 
}
