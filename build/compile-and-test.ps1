$dotnetversion = where dotnet | dotnet --version

dotnet restore
dotnet build /p:Version=$($versionInfo.AssemblySemVer) /p:FileVersion=$($versionInfo.AssemblySemVer) /p:InformationalVersion=$($versionInfo.InformationalVersion)
dotnet test --collect "Code coverage" --no-build --filter "(TestCategory=L0|TestCategory=L1)"
dotnet test --collect "Code coverage" --no-build --filter "(TestCategory=L2|TestCategory=L3)"
dotnet test --collect "Code coverage" --no-build --filter "(TestCategory!=L0&TestCategory!=L1&TestCategory!=L2&TestCategory!=L3)" 
