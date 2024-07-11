
$releases = gh release list --json name,tagName -L 5009 --exclude-pre-releases | ConvertFrom-Json
 

foreach ($release in $releases) {
    Write-Output "Release: $($release.name)"
    gren changelog --generate --override --tags=v15.0.4..$($release.tagName) --data-source=issues --changelog-filename=./changelogs/CHANGELOG-$($release.tagName)-issues.md
}

$tags = git tag -l
$tags|%{[System.Version]$_}|sort 