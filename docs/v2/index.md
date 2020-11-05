# Azure DevOps Migration Tools [![Chocolatey](https://img.shields.io/chocolatey/dt/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/) [![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/vsts-sync-migrator/releases) ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](http://nkdagility.github.io/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a [Azure DevOps Migration Tools on Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

**WARNING: This tool is not designed for a novice. This tool was developed to support the scenarios below, and the edge cases that have been encountered by the 30+ contributors from around the Azure DevOps community. You should be comfortable with the TFS/Azure DevOps object model, as well as debugging code in Visual Studio.**

_NOTICE: Both paid and community support is available through our [recommended consultants](#support) as well as our contributors and many DevOps consultants around the world._

## What can you do with this tool?

- Migrate Work Items from one Team Project to another Team Project
- Re-run to Sync Changes since last migration
- Merge many Team Projects into a single Team Project
- Split one Team Project into many Team Projects
- Migration of Test Suites & Test Plans, Teams, 
- Migrate from one Language version of TFS / Azure DevOps to another
- Offline migration for senarios where we cant connect between our Source and Target environments

## Change Log

- v??.? - Release of v2 architecture

## What versions of Azure DevOps & TFS do you support?

* Work Item Migration Supports all versions of TFS 2013+ and all versions of Azure DevOps
* Process Template migration only supports XML based Projects

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://www.visualstudio.com/team-services/migrate-tfs-vsts/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move to Azure DevOps Services.**

## Quick Links

- TBA

### External Walkthroughs and Reviews

* TBA

## Getting the Tools

There are two ways to get these tools:

* (recommended)[Install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/)
* Download the [latest release from GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and unzip

## The State of the v2 Archtecture

We have a lot of work to do to create the new v2 architecture. We now have a .NET Standard WorkItemProcessor with Endpoints for InMemory (for testing), FileSystem, and Tfs all subbed out. 

The model should also work for other data `Teams`, `SharedQueries`, `PlansAndSuits`, 

### The `WorkItemProcessor` has `Endpoints`

`Endpoints` connect to the target system and load and save the data. Because systems likely have different data shapes we also have `EndpointEnrichers` that can be added to `Endpoints` that allow loading and saving of specific data. These might do things like:
- load/save Created fields to WorkItemData
- Fix for Embedded field Attachments
- load/save Attachments to WorkItemData
- load/save Links to WorkItemData
- Fix for Git Commit links

With Endpoints and EndpointEnrichers we should be able to cover Tfs, AzureDevOps, & Filesystem with ease.

### The `WorkItemProcessor` has `ProcessorEnrichers`

`ProcessorEnrichers` allow for us to add new functionality to the processor without having to change the processor.  For example:

- Pause After Each Item
- Append Migration Tools Signature
- Skip to Final Revised Work Item
- Filter work Item that Already Exists in Target

`ProcessorEnrichers`  also have a number of places in the Processor Execution pipeline that they can be called. 

- ProcessorExecutionBegin
- ProcessorExecutionAfterSource
- ProcessorExecutionBeforeProcessWorkItem
- ProcessorExecutionAfterProcessWorkItem
- ProcessorExecutionEnd

This way we can add additional processing where we need it.

### What was added here

- Moved to WorkItemData2 & RevisedItem2 as we needed more changes than the v1 architecture could support
- Enabled the configuration through Options and the loading of the objects for `Processors`, `ProcessorEnrichers`, `Endpoints`, `EndpointEnrichers`. 
- Moved all services setup to the project that holds it using extension methods. e.g. ` services.AddMigrationToolServices();`
- Created new IntegrationTests with logging that can be used to validate autonomously the Processors. Started with `TestTfsToTfsNoEnrichers` to get a migration of just ID, & ReflectedWorkItemId. Still needs actual code in `TfsWorkItemEndpoint` to connect to TFS but it runs, passes, and attaches the log to the test results.

While we still have a long way to go this is a strong move towards v2. It will add object confusion while we build within the context of the existing tool. However, I have marked many of the objects as `[Obsolite("This is v1 *", false)` so that we can distinguish in the confusing areas.

#### Legacy Folders

- `VstsSyncMigrator.Core` - Everything in here must go :)
- `MigrationTools\Engine\` - These will me refactored away and into v2.
- `MigrationTools\Clients\` - Clients model is being abandoned in favour of `Endpoints`
- `MigrationTools.Clients.AzureDevops.ObjectModel\Clients\` - Clients model is being abandoned in favour of `Endpoints`


## Overview

These tools are build by naked Agility Limited's DevOps & Agility consultants to do real world migrations on a daily basis. We always work in [Azure DevOps Services](http://dev.azure.com) on https://dev.azure.com/nkdagility/migration-tools/ with code in GitHub and publish as a chocolatey package that pulls from GitGub Releases.

|-|-|
|-------------:|:-------------|
| Team Work Items | [Azure Boards](https://dev.azure.com/nkdagility/migration-tools/) |
| Public Issues | [GitHub Issues](https://github.com/nkdAgility/azure-devops-migration-tools/) |
| Builds & Releases | [Azure Pipelines](https://dev.azure.com/nkdagility/migration-tools/) |
| Releases Output | [Github Releases](https://github.com/nkdAgility/azure-devops-migration-tools/releases) |
| Documentation | [Github Pages](http://nkdagility.github.io/azure-devops-migration-tools/) |

**Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get you started in 30 minutes. This tool is complicated and its not always easy to discover what you need to do.**

## The Technical Details

|-| Technical Overview |
|-------------:|:-------------|
| Azure Pipeline | ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) |
| Coverage | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=coverage)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Maintainability | [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Security Rating | [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=security_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Vulnerabilities | [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Release | [![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/azure-devops-migration-tools/releases)|
| Chocolatey |[![Chocolatey](https://img.shields.io/chocolatey/v/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/)|


