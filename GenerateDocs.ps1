#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates documentation by running the MigrationTools.ConsoleDataGenerator application.

.DESCRIPTION
    This script runs the MigrationTools.ConsoleDataGenerator to generate YAML data files 
    by reflecting over the migration tools' types and options.
    
    The generated files are placed in:
    - YAML data: docs/data/classes/

.PARAMETER Configuration
    The build configuration to use (Debug or Release). Default is Debug.

.EXAMPLE
    .\GenerateDocs.ps1
    Runs the documentation generator with default settings.

.EXAMPLE
    .\GenerateDocs.ps1 -Configuration Release
    Runs the documentation generator using Release configuration.

.EXAMPLE
    .\GenerateDocs.ps1 -Verbose
    Runs the documentation generator with verbose output.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get the script directory (repository root)
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectPath = Join-Path $RepoRoot "src" "MigrationTools.ConsoleDataGenerator"

Write-Host "🚀 Azure DevOps Migration Tools - Documentation Generator" -ForegroundColor Cyan
Write-Host "Repository Root: $RepoRoot" -ForegroundColor Gray
Write-Host "Project Path: $ProjectPath" -ForegroundColor Gray
Write-Host ""

# Verify the project exists
if (-not (Test-Path $ProjectPath)) {
    Write-Error "❌ Project not found at: $ProjectPath"
    exit 1
}

# Change to project directory
Push-Location $ProjectPath

try {
    Write-Host "📦 Building MigrationTools.ConsoleDataGenerator..." -ForegroundColor Yellow
    
    # Build the project
    $buildArgs = @(
        "build"
        "--configuration", $Configuration
        "--verbosity", "minimal"
    )
    
    & dotnet @buildArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    
    Write-Host "✅ Build completed successfully" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "📚 Generating documentation..." -ForegroundColor Yellow
    # Run the documentation generator
    $runArgs = @(
        "run"
        "--configuration", $Configuration
        "--no-build"
    )
    
    if ($VerbosePreference -eq 'Continue') {
        $runArgs += "--verbosity", "detailed"
    }
    
    & dotnet @runArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Documentation generation failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    
    Write-Host "✅ Documentation generation completed successfully" -ForegroundColor Green
    Write-Host ""
    
    # Display summary of generated files
    $docsDataPath = Join-Path $RepoRoot "docs" "data" "classes"
    
    if (Test-Path $docsDataPath) {
        $yamlFiles = Get-ChildItem -Path $docsDataPath -Filter "*.yaml" -Recurse | Measure-Object
        Write-Host "📄 Generated $($yamlFiles.Count) YAML data files in docs/data/classes/" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "🎉 Documentation generation completed!" -ForegroundColor Cyan
    Write-Host "Generated YAML files are ready for use in the documentation site." -ForegroundColor Gray
    
}
catch {
    Write-Error "❌ An error occurred: $($_.Exception.Message)"
    exit 1
}
finally {
    # Return to original directory
    Pop-Location
}
