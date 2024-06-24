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
# Write-Output "Install RubyInstallerTeam.RubyWithDevKit.3.2"
# $installedStuff = choco list -i 
# if (($installedStuff -like "*ruby*").Count -eq 0) {
#     Write-Output "Installing ruby2.devkit"
#     choco install ruby2.devkit --confirm --accept-license -y
#  } else { Write-Output "Detected ruby2.devkit"}
choco install ruby --version=3.1.5.1 --confirm --accept-license -y
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
bundle exec jekyll build
Write-Output "----------------------------------------"
Write-Output "----------------------------------------"
#-------------------------------------------