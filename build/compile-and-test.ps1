$dotnetversion = where dotnet | dotnet --version

Write-InfoLog "Run Compile & Test"
Write-InfoLog "---------------"
dotnet restore
dotnet build /p:Version=$($versionInfo.AssemblySemVer) /p:FileVersion=$($versionInfo.AssemblySemVer) /p:InformationalVersion=$($versionInfo.InformationalVersion)

dotnet test --collect "Code coverage" --no-build --filter "(TestCategory=L0|TestCategory=L1)" --logger trx --results-directory .\output\testresults
dotnet test --collect "Code coverage" --no-build --filter "(TestCategory=L2|TestCategory=L3)" --logger trx --results-directory .\output\testresults
dotnet test --collect "Code coverage" --no-build --filter "(TestCategory!=L0&TestCategory!=L1&TestCategory!=L2&TestCategory!=L3)" --logger trx --results-directory .\output\testresults

Write-InfoLog "---------------"