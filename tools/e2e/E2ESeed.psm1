# Seeding helpers shared by scenario seed scripts.
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1")

$script:UserA = "martin@nkdagility.com"
$script:UserB = "nkdbot@nkdagility.com"
function Get-E2EUsers { @{ A = $script:UserA; B = $script:UserB } }

function New-E2ENode {
    param([string]$Project, [ValidateSet("Areas", "Iterations")][string]$Group, [string]$Name, [string]$Under = "", [datetime]$Start, [datetime]$Finish)
    Assert-E2EName $Project
    $body = @{ name = $Name }
    if ($Start) { $body.attributes = @{ startDate = $Start.ToString("o"); finishDate = $Finish.ToString("o") } }
    $path = "$Project/_apis/wit/classificationnodes/$Group"
    if ($Under) { $path += "/" + (($Under -split '\\' | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/') }
    try { Invoke-E2ERest $path -Method POST -Body $body } catch { if ($_.ErrorDetails.Message -notmatch "already in use|ClassificationNodeDuplicateName") { throw } }
}

function Add-E2ELinkOp([string]$Rel, [int]$TargetId, [string]$Comment = "") {
    @{ op = "add"; path = "/relations/-"; value = @{ rel = $Rel; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/wit/workItems/$TargetId"; attributes = @{ comment = $Comment } } }
}

function Add-E2EAttachmentOp([string]$Url, [string]$Comment = "") {
    @{ op = "add"; path = "/relations/-"; value = @{ rel = "AttachedFile"; url = $Url; attributes = @{ comment = $Comment } } }
}

# Writes a state file next to the scenario so assertions can map seeded keys to ids.
function Save-E2EState([string]$Dir, [hashtable]$State) { $State | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $Dir "state.local.json") }
function Read-E2EState([string]$Dir) { Get-Content (Join-Path $Dir "state.local.json") -Raw | ConvertFrom-Json }

# Finds the target work item that reflects a source id.
function Find-E2ETargetItem([string]$Project, [int]$SourceId, [string]$ReflectedField = "Custom.ReflectedWorkItemId", [string]$Org = "nkdagility-adomt-target") {
    $ids = Get-E2EWiql $Project "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '$Project' AND [$ReflectedField] CONTAINS '/_workitems/edit/$SourceId'" $Org
    $match = foreach ($i in $ids) {
        $wi = Invoke-E2ERest "_apis/wit/workitems/$($i.id)?`$expand=relations" -Org $Org
        if ($wi.fields.$ReflectedField -match "/_workitems/edit/$SourceId$") { $wi }
    }
    $match | Select-Object -First 1
}

Export-ModuleMember -Function *-E2E*
