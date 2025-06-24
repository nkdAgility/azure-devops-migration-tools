#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Converts Jekyll-style markdown links to Hugo rel shortcode format
.DESCRIPTION
    This script scans all markdown files in the docs/content directory and converts
    Jekyll-style links (e.g., /setup/installation.md) to Hugo rel shortcode format
    (e.g., {{ ref "docs/setup/installation" }})
.EXAMPLE
    .\FixHugoLinksAdvanced.ps1
    .\FixHugoLinksAdvanced.ps1 -Verbose
#>

[CmdletBinding()]
param()

# Define the root directory for content
$ContentDir = "docs/content"
$DocsDir = "docs"

# Check if we're in the right directory
if (-not (Test-Path $ContentDir)) {
    Write-Error "Content directory '$ContentDir' not found. Please run this script from the repository root."
    exit 1
}

# Define comprehensive link mappings from Jekyll to Hugo format
$linkMappings = @{
    # Getting started links
    "/getting-started/"                                                           = "docs/get-started/getting-started"
    
    # Setup pages
    "/setup/installation.md"                                                      = "docs/setup/installation"
    "/setup/permissions.md"                                                       = "docs/setup/permissions"
    "/setup/reflectedworkitemid.md"                                               = "docs/setup/reflectedworkitemid"
    
    # General docs references
    "/docs/Reference/index.md"                                                    = "docs/reference"
    "/docs/Reference/v1/Processors/WorkItemMigrationConfig.md"                    = "docs/reference/processors/workitem-tracking-processor"
    "/Reference/v2/ProcessorEnrichers/TfsNodeStructure/"                          = "docs/reference/tools/tfs-node-structure-tool"
    "/Reference/Tools/TfsNodeStructureTool/"                                      = "docs/reference/tools/tfs-node-structure-tool"
    
    # Processor references (Jekyll _reference format to Hugo)
    "_reference/reference.processors.tfsworkitemmigrationprocessor.md"            = "docs/reference/processors/tfs-workitem-migration-processor"
    "_reference/reference.processors.tfsworkitembulkeditprocessor.md"             = "docs/reference/processors/tfs-workitem-bulk-edit-processor"
    "_reference/reference.processors.tfsworkitemdeleteprocessor.md"               = "docs/reference/processors/tfs-workitem-delete-processor"
    "_reference/reference.processors.tfsworkitemoverwriteprocessor.md"            = "docs/reference/processors/tfs-workitem-overwrite-processor"
    "_reference/reference.processors.tfsworkitemoverwriteareasastagsprocessor.md" = "docs/reference/processors/tfs-workitem-overwrite-areas-as-tags-processor"
    "_reference/reference.processors.workitemtrackingprocessor.md"                = "docs/reference/processors/workitem-tracking-processor"
    "_reference/reference.processors.azuredevopspipelineprocessor.md"             = "docs/reference/processors/azure-devops-pipeline-processor"
    "_reference/reference.processors.processdefinitionprocessor.md"               = "docs/reference/processors/process-definition-processor"
    "_reference/reference.processors.tfssharedqueryprocessor.md"                  = "docs/reference/processors/tfs-shared-query-processor"
    "_reference/reference.processors.tfsteamsettingsprocessor.md"                 = "docs/reference/processors/tfs-team-settings-processor"
    "_reference/reference.processors.tfstestconfigurationsmigrationprocessor.md"  = "docs/reference/processors/tfs-test-configurations-migration-processor"
    "_reference/reference.processors.tfstestplansandsuitesmigrationprocessor.md"  = "docs/reference/processors/tfs-test-plans-and-suites-migration-processor"
    "_reference/reference.processors.tfstestvariablesmigrationprocessor.md"       = "docs/reference/processors/tfs-test-variables-migration-processor"
    "_reference/reference.processors.tfsexportprofilepicturefromadprocessor.md"   = "docs/reference/processors/tfs-export-profile-picture-from-a-d-processor"
    "_reference/reference.processors.tfsexportusersformappingprocessor.md"        = "docs/reference/processors/tfs-export-users-for-mapping-processor"
    "_reference/reference.processors.tfsimportprofilepictureprocessor.md"         = "docs/reference/processors/tfs-import-profile-picture-processor"
    "_reference/reference.processors.keepoutboundlinktargetprocessor.md"          = "docs/reference/processors/keep-outbound-link-target-processor"
    "_reference/reference.processors.outboundlinkcheckingprocessor.md"            = "docs/reference/processors/outbound-link-checking-processor"
    
    # Tool references (Jekyll _reference format to Hugo)
    "_reference/reference.tools.tfsgitrepositorytool.md"                          = "docs/reference/tools/tfs-git-repository-tool"
    "_reference/reference.tools.tfschangesetmappingtool.md"                       = "docs/reference/tools/tfs-change-set-mapping-tool"
    "_reference/reference.tools.tfsattachmenttool.md"                             = "docs/reference/tools/tfs-attachment-tool"
    "_reference/reference.tools.tfsembededimagestool.md"                          = "docs/reference/tools/tfs-embeded-images-tool"
    "_reference/reference.tools.tfsnodestructuretool.md"                          = "docs/reference/tools/tfs-node-structure-tool"
    "_reference/reference.tools.tfsrevisionmanagertool.md"                        = "docs/reference/tools/tfs-revision-manager-tool"
    "_reference/reference.tools.tfsteamsettingstool.md"                           = "docs/reference/tools/tfs-team-settings-tool"
    "_reference/reference.tools.tfsusermappingtool.md"                            = "docs/reference/tools/tfs-user-mapping-tool"
    "_reference/reference.tools.tfsvalidaterequiredfieldtool.md"                  = "docs/reference/tools/tfs-validate-required-field-tool"
    "_reference/reference.tools.tfsworkitemembededlinktool.md"                    = "docs/reference/tools/tfs-work-item-embeded-link-tool"
    "_reference/reference.tools.tfsworkitemlinktool.md"                           = "docs/reference/tools/tfs-work-item-link-tool"
    "_reference/reference.tools.workitemtypemappingtool.md"                       = "docs/reference/tools/work-item-type-mapping-tool"
    "_reference/reference.tools.stringmanipulatortool.md"                         = "docs/reference/tools/string-manipulator-tool"
    "_reference/reference.tools.fieldmappingtool.md"                              = "docs/reference/tools/field-mapping-tool"
}

# Function to convert markdown links to Hugo ref shortcode
function ConvertMarkdownLinksToHugo {
    param(
        [string]$filePath,
        [hashtable]$mappings
    )
    
    $content = Get-Content -Path $filePath -Raw
    $changesMade = $false
    
    Write-Verbose "Processing file: $filePath"
    
    # Convert each mapping
    foreach ($oldLink in $mappings.Keys) {
        $newLink = $mappings[$oldLink]
        
        # Pattern to match markdown links with the old format
        $pattern = '\[([^\]]+)\]\(' + [regex]::Escape($oldLink) + '\)'
        $replacement = '[$1]({{< ref "' + $newLink + '" >}})'
        
        if ($content -match $pattern) {
            Write-Verbose "  Converting: $oldLink -> $newLink"
            $content = $content -replace $pattern, $replacement
            $changesMade = $true
        }
    }
    
    # Save the file if changes were made
    if ($changesMade) {
        Set-Content -Path $filePath -Value $content -NoNewline
        Write-Host "Updated: $filePath" -ForegroundColor Green
        return $true
    }
    
    return $false
}

# Function to find and report unmapped links
function FindUnmappedLinks {
    param([string]$contentDir)
    
    Write-Host "Scanning for unmapped links..." -ForegroundColor Yellow
    
    $markdownFiles = Get-ChildItem -Path $contentDir -Filter "*.md" -Recurse
    $unmappedLinks = @()
    
    foreach ($file in $markdownFiles) {
        $content = Get-Content -Path $file.FullName -Raw
        
        # Find all markdown links that start with / (potential internal links)
        $linkMatches = [regex]::Matches($content, '\[([^\]]+)\]\((/[^)]+)\)')
        
        foreach ($match in $linkMatches) {
            $linkText = $match.Groups[1].Value
            $linkUrl = $match.Groups[2].Value
            
            # Skip external links
            if ($linkUrl -match '^https?://') {
                continue
            }
            
            # Check if this link is already mapped
            if (-not $linkMappings.ContainsKey($linkUrl)) {
                $unmappedLinks += @{
                    File     = $file.FullName
                    LinkText = $linkText
                    LinkUrl  = $linkUrl
                    Line     = ($content.Substring(0, $match.Index) -split "`n").Count
                }
            }
        }
    }
    
    if ($unmappedLinks.Count -gt 0) {
        Write-Warning "Found $($unmappedLinks.Count) unmapped links:"
        foreach ($link in $unmappedLinks) {
            Write-Warning "  File: $($link.File)"
            Write-Warning "  Line: $($link.Line)"
            Write-Warning "  Text: $($link.LinkText)"
            Write-Warning "  URL: $($link.LinkUrl)"
            Write-Warning "  ---"
        }
        return $unmappedLinks
    }
    
    Write-Host "No unmapped links found!" -ForegroundColor Green
    return @()
}

# Main execution
Write-Host "Starting Hugo link conversion..." -ForegroundColor Cyan

# Find all markdown files
$markdownFiles = Get-ChildItem -Path $ContentDir -Filter "*.md" -Recurse

Write-Host "Found $($markdownFiles.Count) markdown files to process" -ForegroundColor Yellow

$totalUpdated = 0

# Process each markdown file
foreach ($file in $markdownFiles) {
    if (ConvertMarkdownLinksToHugo -filePath $file.FullName -mappings $linkMappings) {
        $totalUpdated++
    }
}

Write-Host "`nConversion complete!" -ForegroundColor Cyan
Write-Host "Files updated: $totalUpdated" -ForegroundColor Green

# Find any remaining unmapped links
$unmappedLinks = FindUnmappedLinks -contentDir $ContentDir

# Test Hugo build
Write-Host "`nTesting Hugo build..." -ForegroundColor Yellow
try {
    $buildResult = & hugo --source $DocsDir 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Hugo build successful!" -ForegroundColor Green
        Write-Host "Build output:" -ForegroundColor Gray
        Write-Host $buildResult -ForegroundColor Gray
    }
    else {
        Write-Error "Hugo build failed!"
        Write-Host $buildResult -ForegroundColor Red
    }
}
catch {
    Write-Error "Failed to run Hugo: $($_.Exception.Message)"
}

if ($unmappedLinks.Count -gt 0) {
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "1. Review the unmapped links above" -ForegroundColor White
    Write-Host "2. Add mappings to the script for any valid internal links" -ForegroundColor White
    Write-Host "3. Fix or remove any broken links" -ForegroundColor White
    Write-Host "4. Re-run this script to apply new mappings" -ForegroundColor White
}

Write-Host "`nDone! Please review the changes and test your site." -ForegroundColor Cyan
