# Builds scenario configs for the work item processor so each scenario only states what differs.
$script:SrcOrgUrl = "https://dev.azure.com/nkdagility-adomt-source/"
$script:TgtOrgUrl = "https://dev.azure.com/nkdagility-adomt-target/"

function New-E2ETfsEndpoint([string]$OrgUrl, [string]$Project) {
    [ordered]@{
        EndpointType = "TfsTeamProjectEndpoint"; Collection = $OrgUrl; Project = $Project
        ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId"; AllowCrossProjectLinking = $false
        Authentication = @{ AuthenticationMode = "EntraId" }
        LanguageMaps = @{ AreaPath = "Area"; IterationPath = "Iteration" }
    }
}

function New-E2EWorkItemProcessor([hashtable]$Overrides = @{}) {
    $p = [ordered]@{
        ProcessorType = "TfsWorkItemMigrationProcessor"; Enabled = $true; SourceName = "Source"; TargetName = "Target"
        UpdateCreatedDate = $true; UpdateCreatedBy = $true
        WIQLQuery = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.Id]"
        FixHtmlAttachmentLinks = $false; WorkItemCreateRetryLimit = 5; FilterWorkItemsThatAlreadyExistInTarget = $false
        GenerateMigrationComment = $true; MaxGracefulFailures = 0
        SkipRevisionWithInvalidIterationPath = $false; SkipRevisionWithInvalidAreaPath = $false
    }
    foreach ($k in $Overrides.Keys) { $p[$k] = $Overrides[$k] }
    $p
}

# Writes <Dir>/<Name>.json. $Tools is merged over the defaults; $Processors defaults to one work item processor.
function New-E2EConfig {
    param([string]$Dir, [string]$Name = "config", [string]$Src, [string]$Tgt, [hashtable]$Tools = @{}, [object[]]$Processors, [string]$LogLevel = "Information")
    $node = { param($s, $t) @{ Filters = @(); Mappings = @(@{ Match = "^$s([\\]?.*)$"; Replacement = "$t`$1" }) } }
    $common = [ordered]@{
        TfsNodeStructureTool = [ordered]@{ Enabled = $true; Areas = (& $node $Src $Tgt); Iterations = (& $node $Src $Tgt); ShouldCreateMissingRevisionPaths = $true; ReplicateAllExistingNodes = $true }
        TfsRevisionManagerTool = @{ Enabled = $true; ReplayRevisions = $true; MaxRevisions = 0 }
        TfsAttachmentTool = @{ Enabled = $true; ExportBasePath = "c:\temp\amte2e-attachments"; MaxAttachmentSize = 480000000 }
        TfsWorkItemLinkTool = @{ Enabled = $true; FilterIfLinkCountMatches = $true; SaveAfterEachLinkIsAdded = $false }
        TfsUserMappingTool = @{ Enabled = $false }
        TfsTeamSettingsTool = @{ Enabled = $false }
    }
    foreach ($k in $Tools.Keys) { $common[$k] = $Tools[$k] }
    if (-not $Processors) { $Processors = @(New-E2EWorkItemProcessor) }
    $cfg = [ordered]@{
        Serilog = @{ MinimumLevel = $LogLevel }
        MigrationTools = [ordered]@{
            Version = "16.0"
            Endpoints = [ordered]@{ Source = (New-E2ETfsEndpoint $script:SrcOrgUrl $Src); Target = (New-E2ETfsEndpoint $script:TgtOrgUrl $Tgt) }
            CommonTools = $common
            Processors = @($Processors)
        }
    }
    $path = Join-Path $Dir "$Name.json"
    $cfg | ConvertTo-Json -Depth 30 | Set-Content $path
    $path
}

# Runs the harness with full output to a log and returns @{ Exit; Log; Lines }.
function Invoke-E2EScenarioRun {
    param([string]$Config, [string]$LogName)
    $harness = Join-Path $PSScriptRoot "Invoke-MigrationE2E.ps1"
    $log = Join-Path ([IO.Path]::GetTempPath()) "amte2e-$LogName.log"
    & $harness -Config $Config -Filter '.' *> $log
    $code = $LASTEXITCODE
    $lines = Get-Content $log | ForEach-Object { $_ -replace '^\[\d\d:\d\d:\d\d (\w+)\] \[[^\]]+\] ', '[$1] ' }
    [pscustomobject]@{ Exit = $code; Log = $log; Lines = $lines }
}

function Select-E2ELines($Run, [string]$Pattern, [int]$First = 20) { $Run.Lines | Select-String -Pattern $Pattern | Select-Object -First $First | ForEach-Object Line }

Export-ModuleMember -Function *-E2E*
