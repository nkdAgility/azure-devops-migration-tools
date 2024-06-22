<#
    Azure DevOps Migration Tools (GitHub Release) Release
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version,

    # name of the output folder
    [Parameter(Mandatory=$true)]
    [string]$artifactFolder,

    # name of the releaseTag
    [Parameter(Mandatory=$true)]
    [string]$releaseTag,

    # GH_TOKEN
    [Parameter(Mandatory=$true)]
    [string]$GH_TOKEN

)
#Publish
Write-Output "Azure DevOps Migration Tools (GitHub Release) Release"
Write-Output "--------------"
$files = Get-ChildItem -Path $artifactFolder -Recurse
foreach ($file in $files) {
    Write-Output $file.FullName
}

Write-Output "Install GitHub CLI if needed"
if (($installedStuff -like "*gh*").Count -eq 0) {
    Write-Output "Installing gh"
    choco install gh --confirm --accept-license -y
} else { Write-Output "Detected gh"}

if ($GH_TOKEN -eq $null) {
    Write-Output "Please set the GH_TOKEN variable"
    return 2;
}

Write-Output "Logging in to GitHub with $GH_TOKEN"
Write-Output "--------------"
$env:GH_TOKEN = $GH_TOKEN
"$GH_TOKEN" | gh auth login --with-token 
Write-Output "--------------"
Write-Output "GitHub Login Status"
Write-Output "--------------"
gh auth status
Write-Output "--------------"
Write-Output "Release tag: $releaseTag"
Write-Output "Version: $version"
Write-Output "Filed for Upload: $artifactFolder\*"
Write-Output "--------------"

$result = gh release view $version --repo "nkdAgility/azure-devops-migration-tools"
if ($result -ne $null) {
    Write-Output "Release $version already exists add assets only"
} else {
    switch ($releaseTag)
    {
        "Local"    {
            Write-Output "Creating Local Release"
            gh release create $version --title "$version **DELETEME** FAKE TEST" --generate-notes --prerelease --draft --repo "nkdAgility/azure-devops-migration-tools"
        }
        "Dev"    {
            Write-Output "Creating Dev Release"
            gh release create $version --title "$version **DELETEME** FAKE TEST" --generate-notes --prerelease --draft --repo "nkdAgility/azure-devops-migration-tools"
            }
        "preview"    {
            Write-Output "Creating Previwe Release"
            gh release create $version --generate-notes --prerelease --repo "nkdAgility/azure-devops-migration-tools"
            }
        "Release"    {
            Write-Output "Creating Production Release"
            gh release create $version --generate-notes --discussion-category "AnouncementDiscussions" --repo "nkdAgility/azure-devops-migration-tools"
            }
        default { 
            Write-Output "Unknown Release tag of $releaseTag";
            return 3;
         }
    }
}
Write-Output "--------------"
Write-Output "Add Assets to $version"
foreach ($file in $files) {
    Write-Output "Upload $version $file.FullName"
    gh release upload $version $file.FullName
}

Write-Output "--------------"