Write-Output "INSTALL CHOCO APPS"
Write-Output "------------"
# Install Choco Apps
# Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
$installedStuff = choco list -i 
if (($installedStuff -like "*7zip*").Count -eq 0) {
   Write-Output "Installing 7zip"
   choco install 7zip --confirm --accept-license -y
} else { Write-Output "Detected 7zip"}
if (($installedStuff -like "*gh*").Count -eq 0) {
    Write-Output "Installing gh"
    choco install gh --confirm --accept-license -y
} else { Write-Output "Detected gh"}
if (($installedStuff -like "*GitVersion.Portable*").Count -eq 0) {
    Write-Output "Installing GitVersion"
    choco install gitversion.portable --confirm --accept-license -y
} else { Write-Output "Detected GitVersion"}
if (($installedStuff -like "*nodejs*").Count -eq 0) {
    Write-Output "Installing nodejs"
    choco install nodejs --confirm --accept-license -y
} else { Write-Output "Detected nodejs"}

Write-Output "------------"

# Install DotNetApps
Write-Output "INSTALL DotNetApps APPS"
Write-Output "------------"
$installedDotNetStuff = dotnet tool list -g 
if (($installedDotNetStuff -like "*GitVersion.Tool*").Count -eq 0) {
    Write-Output "Installing GitVersion.Tool"
    choco install 7zip --confirm --accept-license -y
 } else { Write-Output "Detected GitVersion.Tool"}
dotnet tool install --global GitVersion.Tool
Write-Output "------------"


# Install NodeJs Apps
if (((npm list -g tfx-cli) -join "," ).Contains("empty")) {
    Write-Output "Installing tfx-cli"
    npm i -g tfx-cli
} else { Write-Output "Detected tfx-cli"}


Write-Output "REFRESH ENVIRONMENT"
Write-Output "------------"
# Refresh environment
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine")
# Make `refreshenv` available right away, by defining the $env:ChocolateyInstall
# variable and importing the Chocolatey profile module.
# Note: Using `. $PROFILE` instead *may* work, but isn't guaranteed to.
$env:ChocolateyInstall = Convert-Path "$((Get-Command choco).Path)\..\.."   
Import-Module "$env:ChocolateyInstall\helpers\chocolateyProfile.psm1"
Update-SessionEnvironment
Write-Output "------------"
