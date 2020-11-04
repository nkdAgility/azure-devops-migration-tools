# Azure DevOps Migration Tools [![Chocolatey](https://img.shields.io/chocolatey/dt/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/) [![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/azure-devops-migration-tools/releases) ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](http://nkdagility.github.io/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a [Azure DevOps Migration Tools on Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

![alt text](https://raw.githubusercontent.com/nkdAgility/azure-devops-migration-tools/master/src/MigrationTools.Extension/images/azure-devops-migration-tools-naked-agility-martin-hinshelwood.png)

**WARNING: This tool is not designed for a novice. This tool was developed to support the senarios below, and the edge cases that have been encountered by the 30+ contributers from around the Azure DevOps community. You should be comfortable with the TFS/Azure DevOps object model, as well as debugging code in Visual Studio.**
**Both paid and community support is available through our [recommneded consultants](http://nkdagility.github.io/azure-devops-migration-tools/#support) as well as our contributors and many DevOps consultants around the world.**

<a href="https://stackoverflow.com/questions/tagged/azure-devops-migration-tools" title="Ask Questions on StackOverflow"><img src="http://cdn.sstatic.net/stackexchange/img/logos/so/so-logo.png" width="250"></a>

**Ask Questions on StackOverflow: https://stackoverflow.com/questions/tagged/azure-devops-migration-tools**

## What can you do with this tool?

- Migrate Work Items from one Team Project to another Team Project (*new v8.3*) and Sync changes after a migration
- Merge many Team Projects into a single Team Project
- Split one Team Project into many Team Projects
- Assistance in changing Process Templates
- Bulk edit of Work Items
- Migration of Test Suites & Test Plans
- Migrate from one Language version of TFS / Azure Devops to another (*new v9.0*)

## Change Log

- v10.1 - Changed config design to have only the Name and not FullName of the class. Remove `MigrationTools.Core.Configuration.FieldMap.` and `MigrationTools.Core.Configuration.Processing.` from the config leaving only the Name of the class in ObjectType field.
- v10.0 - Start of the greate refactor over to .NET Core and the REST API as the Object Model has been retired.
- v9.0 - Added support for migration between other language versions of Azure DevOps. Developed for German -> English
- v8.9 - Added 'Collapse Revisions' feature to collapse and attache revisions instead of replaying them
- v8.8 - 'SkipToFinalRevisedWorkItemType' feature added to handle scenario when changing Work Item Type
- v8.7 - Support for inline images using a Personal Access Token added to the Source Project
- v8.6 - Support for fixing links from TFVC Changesets to Git Commits using a mapping file generated from a Git-TFS migration.
- v8.5 - Attachment Max size and linking work items to git repos between projects.
- v8.4 - Support for cross-project linking of work items between projects.
- v8.3 - Support for restarting the migration and syncing at the revision level.
- v8.2 - Merge Git commit Fixing into Work Item migration (requires repos to be migrated first, can be rerun)
- v8.0 - Merge of Work Item, Link, & attachent migrators into one.

## What versions of Azure DevOps & TFS do you support?

- Work Item Migration Supports all versions of TFS 2013+ and all versions of Azure DevOps
- Process Template migration only supports XML based Projects

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://www.visualstudio.com/team-services/migrate-tfs-vsts/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move to Azure DevOps Services.**

## Quick Links

 - [Video Overview](https://youtu.be/ZxDktQae10M)
 - [Getting Started](http://nkdagility.github.io/azure-devops-migration-tools/getting-started.md)
 - [Documentation](http://nkdagility.github.io/azure-devops-migration-tools/)
 - [Help & Support](https://stackoverflow.com/questions/tagged/azure-devops-migration-tools)

## The Technical Details

|-| Technical Overview |
|-------------:|:-------------|
| Azure Pipeline | ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) |
| Coverage | [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=coverage)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Maintainability | [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Security Rating | [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=security_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Vulnerabilities | [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster) |
| Release |[![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](| Release |[![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/azure-devops-migration-tools/releases)|
| Chocolatey |[![Chocolatey](https://img.shields.io/chocolatey/v/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/)|
