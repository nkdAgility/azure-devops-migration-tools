<#
    Script description.

    Some notes.
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version
)
Write-Output "Azure DevOps Migration Tools (Documentation) Build"
Write-Output "----------------------------------------"
Write-Output "Version: $version"
Write-Output "Output Folder: $outfolder"
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "Install Ruby"
choco install ruby --version=3.1.5.1 --confirm --accept-license -y
Write-Output "REFRESH ENVIRONMENT"
Write-Output "----------------------------------------"
# Refresh environment
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine")
# Make `refreshenv` available right away, by defining the $env:ChocolateyInstall
# variable and importing the Chocolatey profile module.
# Note: Using `. $PROFILE` instead *may* work, but isn't guaranteed to.
$env:ChocolateyInstall = Convert-Path "$((Get-Command choco).Path)\..\.."   
Import-Module "$env:ChocolateyInstall\helpers\chocolateyProfile.psm1"
Update-SessionEnvironment
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "gem update "
gem update --system
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "gem install jekyll bundler"
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
gem install bundler
gem install jekyll
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "bundle install"
bundle install --retry=3 --jobs=4
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
Write-Output "Run Jekyll Build"
bundle install
bundle exec jekyll build
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
#-------------------------------------------