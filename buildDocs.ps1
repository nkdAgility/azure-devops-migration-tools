$msBuildExe = 'C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe'
& nuget restore
& "$($msBuildExe)" "MigrationTools.sln" /t:Build /m

& ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\migration.exe" init --config ".\docs\Reference\Generated\configuration.config"
& ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\migration.exe" init --options Full --config ".\docs\Reference\Generated\configuration-Full.config"
& ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\migration.exe" init --options WorkItemTracking --config ".\docs\Reference\Generated\configuration-WorkItemTracking.config"
& ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\migration.exe" init --options Fullv2 --config ".\docs\Reference\Generated\configuration-Fullv2.config"
& ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\migration.exe" init --options WorkItemTrackingv2 --config ".\docs\Reference\Generated\configuration-WorkItemTrackingv2.config"



& ".\src\MigrationTools.ConsoleConfigGenerator\bin\Debug\net472\ConfigGenerator.exe"