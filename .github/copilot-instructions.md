# Azure DevOps Migration Tools - Copilot Instructions

This document provides GitHub Copilot with information about the Azure DevOps Migration Tools application structure, executables, and key components to enable better code assistance.

## Overview

The Azure DevOps Migration Tools is a comprehensive solution for migrating work items, test cases, and other artifacts between Azure DevOps organizations and Team Foundation Server (TFS) instances. The solution consists of multiple executable assemblies, each serving different purposes and runtime environments.

## Executable Assemblies

### 1. MigrationTools.ConsoleCore

**Path:** `src/MigrationTools.ConsoleCore/Program.cs`

**Purpose:** Modern .NET 8.0 console application for Azure DevOps migrations using REST API clients only. This is the lightweight, cross-platform version of the migration tools.

**Starts With:** `MigrationToolHost.CreateDefaultBuilder(args)` followed by `hostBuilder.RunConsoleAsync()`

**Target Framework:** .NET 8.0

**Assembly Name:** `devopsmigration`

**Dependencies:**
- `MigrationTools.Host` - Common hosting infrastructure
- `MigrationTools.Clients.AzureDevops.Rest` - REST API client for Azure DevOps
- OpenTelemetry packages for monitoring
- Azure Monitor for telemetry export

**Key Features:**
- REST API-based migrations only
- Cross-platform compatibility
- Minimal dependency footprint
- Modern .NET runtime

**Environment Requirements:**
- .NET 8.0 runtime
- Network access to Azure DevOps REST APIs
- Configuration file (typically `configuration-default.json`)

**Copilot Agent Notes:** This is the modern, REST-only version of the migration tool. When suggesting code changes, prefer REST API patterns and modern .NET 8.0 features. This executable doesn't include TFS Object Model capabilities.

---

### 2. MigrationTools.ConsoleFull

**Path:** `src/MigrationTools.ConsoleFull/Program.cs`

**Purpose:** Full-featured .NET Framework 4.7.2 console application that supports both legacy TFS Object Model and modern REST API migrations. This is the most comprehensive version of the migration tools.

**Starts With:** Assembly loading, activity tracking setup, then `MigrationToolHost.CreateDefaultBuilder(args)` followed by `hostBuilder.RunConsoleAsync()`

**Target Framework:** .NET Framework 4.7.2

**Assembly Name:** `devopsmigration`

**Dependencies:**
- `MigrationTools.Host` - Common hosting infrastructure
- `MigrationTools.Clients.TfsObjectModel` - Legacy TFS Object Model client
- `MigrationTools.Clients.AzureDevops.Rest` - REST API client
- `MigrationTools.Clients.FileSystem` - File system operations
- `Microsoft.TeamFoundationServer.ExtendedClient` - TFS integration
- OpenTelemetry packages for monitoring

**Key Features:**
- Full TFS Object Model support
- Dynamic assembly loading for client assemblies
- Comprehensive migration capabilities
- Activity tracking and telemetry

**Special Initialization:**
```csharp
// Dynamically loads client assemblies at startup
string[] clientAssemblies = { 
    "MigrationTools.Clients.TfsObjectModel.dll", 
    "MigrationTools.Clients.FileSystem.dll", 
    "MigrationTools.Clients.AzureDevops.Rest.dll" 
};
```

**Environment Requirements:**
- .NET Framework 4.7.2 runtime
- Windows environment (due to TFS Object Model dependencies)
- TFS/Azure DevOps connectivity
- Various configuration files (`configuration-classic.json`, etc.)

**Copilot Agent Notes:** This is the full-featured version supporting both TFS Object Model and REST API patterns. When working with this application, TFS Object Model code requires Windows and specific TFS assemblies, while REST API code is cross-platform.

---

### 3. MigrationTools.ConsoleDataGenerator

**Path:** `src/MigrationTools.ConsoleDataGenerator/Program.cs`

**Purpose:** Development and documentation utility that generates YAML and Markdown files by reflecting over the migration tools' types and options. Used for auto-generating documentation.

**Starts With:** Standard `Main(string[] args)` with assembly loading and reflection

**Target Framework:** .NET 8.0

**Dependencies:**
- All MigrationTools client assemblies for reflection
- `Microsoft.Extensions.Options` for configuration binding
- `YamlDotNet` for YAML serialization
- `Microsoft.Extensions.Configuration` for configuration management

**Key Features:**
- Assembly reflection to discover types
- Automatic YAML data generation
- Markdown documentation generation
- Jekyll-compatible output format
- Configuration type discovery

**Output Locations:**
- YAML data: `../../../../../docs/_data/`
- Markdown documentation: `../../../../../docs/Reference/Generated/`

**Type Discovery:**
Discovers and documents:
- `IProcessorOptions` implementations
- `IToolOptions` implementations  
- `IFieldMapOptions` implementations
- `IProcessorEnricherOptions` implementations
- `IEndpointOptions` implementations
- `IEndpointEnricherOptions` implementations

**Environment Requirements:**
- .NET 8.0 runtime
- Write access to documentation folders
- `appsettings.json` configuration file

**Copilot Agent Notes:** This is a development utility for documentation generation. When modifying migration tool types, consider whether documentation regeneration is needed. The tool uses reflection extensively, so be aware of type discovery patterns when adding new options or implementations.

---

### 4. MigrationTools.Telemetery

**Path:** `src/MigrationTools.Telemetery/Program.cs`

**Purpose:** Azure Functions application for collecting and processing telemetry data from migration tool usage. Provides insights and monitoring capabilities.

**Starts With:** `HostBuilder().ConfigureFunctionsWebApplication()` pattern with `host.Run()`

**Target Framework:** .NET 8.0

**Runtime:** Azure Functions v4

**Dependencies:**
- `Microsoft.Azure.Functions.Worker` - Azure Functions runtime
- `Microsoft.Azure.Functions.Worker.Extensions.Http` - HTTP trigger support
- `Microsoft.ApplicationInsights.WorkerService` - Application Insights integration
- `OxyPlot` packages for chart generation
- `System.Drawing.Common` for graphics operations

**Key Features:**
- Azure Functions Worker model
- Application Insights telemetry collection
- HTTP trigger functions
- Chart and visualization generation
- ASP.NET Core integration

**Configuration Files:**
- `host.json` - Azure Functions host configuration
- `local.settings.json` - Local development settings

**Environment Requirements:**
- Azure Functions runtime
- Application Insights instance
- Azure hosting environment or local Azure Functions Core Tools

**Copilot Agent Notes:** This is an Azure Functions application using the isolated worker model. When working with this code, use Azure Functions patterns and Application Insights telemetry practices. Consider scalability and stateless function design patterns.

---

## Common Infrastructure

### MigrationToolHost

**Location:** `src/MigrationTools.Host/MigrationToolHost.cs`

**Purpose:** Provides common hosting infrastructure for console applications, including configuration, logging, dependency injection, and CLI command setup.

**Key Services:**
- Serilog logging with file and console outputs
- Configuration from JSON files, environment variables, and command line
- OpenTelemetry integration for monitoring
- Spectre.Console for CLI interface
- Dependency injection container setup

**CLI Commands:**
- `execute` - Runs migration processors from configuration
- `init` - Creates default configuration files or applies templates
- `upgrade` - Upgrades configuration files to newer versions
- `builder` - Interactive configuration file editor (hidden)

**Configuration Sources:**
1. `appsettings.json` (base directory)
2. Configuration file specified via `--config` parameter
3. Environment variables
4. Command line arguments

**Copilot Agent Notes:** The MigrationToolHost provides the foundation for all console applications. When adding new CLI commands or services, follow the established patterns using Spectre.Console and the dependency injection container. Configuration follows the standard .NET configuration hierarchy.

## Architecture Patterns

### TFS Object Model vs REST API

The migration tools support two primary approaches for connecting to Azure DevOps and TFS:

**TFS Object Model Approach:**
- Based on legacy TFS Object Model patterns
- Requires Windows environment and specific TFS assemblies
- Available primarily in MigrationTools.ConsoleFull
- Provides deep integration with TFS/Azure DevOps

**REST API Approach:**
- Uses modern REST API clients
- Cross-platform compatibility
- Available in both console applications but primary in ConsoleCore
- Configuration-driven endpoint and processor registration

### Configuration Templates
Available templates for `init` command:
- `Basic` - Simple migration setup
- `WorkItemTracking` - Work item migration focus
- `PipelineProcessor` - Pipeline migration setup
- `Reference` - Complete feature set with all options

**Copilot Agent Notes:** When suggesting code modifications, consider whether TFS Object Model or REST API patterns are more appropriate. REST API patterns are preferred for new development and cross-platform scenarios, while TFS Object Model may be necessary for specific legacy TFS compatibility requirements.

## Documentation Synchronization Requirements

### Critical: Keep Documentation in Sync with Code Changes

When modifying any option classes or configuration-related classes, the documentation must be updated to reflect the current code state:

**Classes Requiring Documentation Updates:**
- `IProcessorOptions` implementations (processors configuration)
- `IToolOptions` implementations (tools configuration)  
- `IFieldMapOptions` implementations (field mapping configuration)
- `IProcessorEnricherOptions` implementations (processor enrichers configuration)
- `IEndpointOptions` implementations (endpoint configuration)
- `IEndpointEnricherOptions` implementations (endpoint enrichers configuration)

**Required Steps When Editing These Classes:**

1. **Update XML Documentation Comments:** Ensure all public properties have accurate `<summary>` and `<default>` XML documentation comments that reflect current behavior and default values.

2. **Regenerate Documentation:** Run the `MigrationTools.ConsoleDataGenerator` to regenerate YAML data files and Markdown documentation:
   ```bash
   cd src/MigrationTools.ConsoleDataGenerator
   dotnet run
   ```

3. **Verify Configuration Samples:** Check that generated YAML files in `/docs/_data/` contain configuration samples that match the actual class properties:
   - Property names must match exactly
   - Default values must be current
   - All required properties must be present
   - Deprecated properties should be removed

4. **Validate Documentation Pages:** Ensure that any code samples in `/docs/Reference/` markdown files reflect the current class structure and property names.

5. **Cross-Reference Class Data:** Verify that the documentation accurately represents what is actually in the code by comparing:
   - Property types and names in the class vs. documentation
   - Default values in code vs. samples  
   - Required vs. optional properties
   - XML comments vs. generated descriptions

**Documentation Generation Process:**
The `ConsoleDataGenerator` uses reflection to discover types and generates:
- YAML data files in `/docs/_data/` (e.g., `reference.processors.*.yaml`)
- Markdown documentation in `/docs/Reference/Generated/`
- Configuration samples that include all properties with their current default values

**Validation Checklist:**
- [ ] XML documentation comments updated for all modified properties
- [ ] ConsoleDataGenerator executed successfully
- [ ] Generated YAML samples match actual class properties
- [ ] Property types and default values are accurate in documentation
- [ ] No orphaned or outdated property references in samples
- [ ] Markdown documentation reflects current class capabilities

**Why This Matters:**
Users rely on the generated documentation and configuration samples to understand how to configure the migration tools. Outdated documentation leads to configuration errors and user frustration. The reflection-based documentation generation ensures accuracy, but only when the source code contains current and complete XML documentation comments.