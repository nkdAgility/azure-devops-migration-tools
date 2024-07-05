![azure-devops-migration-tools](https://socialify.git.ci/nkdAgility/azure-devops-migration-tools/image?description=1&descriptionEditable=Azure%20DevOps%20Migration%20Tools%20allow%20you%20to%20migrate%20Work%20Items%2C%20Plans%20%26%20Suits%2C%20%26%20Pipelines%20from%20one%20Azure%20DevOps%20%2F%20TFS%20to%20another!&font=Inter&forks=1&name=1&owner=1&pattern=Signal&stargazers=1&theme=Light)
![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)
![GitHub pre-release](https://img.shields.io/github/v/release/nkdagility/vsts-sync-migration?include_prereleases)

[![Build Status](https://dev.azure.com/nkdagility/AzureDevOps-Tools/_apis/build/status%2FMigrationTools-CIv2?branchName=main)](https://dev.azure.com/nkdagility/AzureDevOps-Tools/_build/latest?definitionId=115&branchName=main)
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/nkdagility/AzureDevOps-Tools/115?compact_message&style=plastic&logo=azuredevops&label=Tests)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=coverage)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=security_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
![Visual Studio Marketplace Rating](https://img.shields.io/visual-studio-marketplace/stars/nkdagility.vsts-sync-migration?logo=visualstudio)
![Chocolatey Downloads](https://img.shields.io/chocolatey/dt/vsts-sync-migrator)

Created and maintained by [Martin Hinshelwood](https://www.linkedin.com/in/martinhinshelwood/) (http://nkdagility.com)

![YouTube Channel Subscribers](https://img.shields.io/youtube/channel/subscribers/UCkYqhFNmhCzkefHsHS652hw)
![YouTube Channel Views](https://img.shields.io/youtube/channel/views/UCkYqhFNmhCzkefHsHS652hw)

# Azure DevOps Migration Tools

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](https://nkdagility.com/docs/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a Winget package a `nkdAgility.AzureDevOpsMigrationTools`.

**Ask Questions on Github: https://github.com/nkdAgility/azure-devops-migration-tools/discussions**

## Some Data from the last 30 days (as of 05/03/2024)

| Category  | Metric | Notes |
| ------------- | ------------- | ------------- |
| Work Items | **1m** | A single Work Item may have many revisions that we need to migrate |
| Work Item Revisions | **23m** | A single Work Item may have many revisions that we need to migrate |
| RelatedLinkCount | **11m** | Each work item may have many links or none. |
| Git Commit Links  | **1.3m** |  |
| Attachments | **1.2m**  | Total number of attachments migrated |
| Test Suits | 52k | total suits migrated | 
| Test Cases Mapped | **1.4m** | Total test cases mapped into Suits |
| Migration Run Ave  | **14 minutes** | Includes dry-runs as well.  |
| Migration Run Total   |  **19bn Seconds** | Thats **316m hours** or **13m days** of run time in the last 30 days. |
| Average Work item Migration Time  | **22s** | Work Item (includes all revisions, links, and attachments for the work item) |


## What can you do with this tool?

- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, `Pipelines`, & `Processes` from one `Team Project` to another
- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, `Pipelines`, & `Processes` from one `Organization` to another
- Bulk edit of `Work Items` across an entire `Project`.

**WARNING: This tool is not designed for a novice. This tool was developed to support the scenarios below, and the edge cases that have been encountered by the 30+ contributors from around the Azure DevOps community. You should be comfortable with the TFS/Azure DevOps object model, as well as debugging code in Visual Studio.**
**Community support is available through [GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/discussions) ; Paid support is available through our [recommended consultants](https://nkdagility.com/docs/azure-devops-migration-tools/#support) as well as our contributors and many DevOps consultants around the world.**

### What versions of Azure DevOps & TFS do you support?

- Work Item Migration Supports all versions of TFS 2013+ and all versions of Azure DevOps
- You can move from any Tfs/AzureDevOps source to any Tfs/AzureDevOps target.
- Process Template migration only supports XML based Projects

### Typical Uses of this tool

- Merge many projects into a single project
- Split one project into many projects
- Assistance in changing Process Templates
- Bulk edit of Work Items
- Migration of Test Suites & Test Plans
- _new_ Migration of Builds & Pipelines
- Migrate from one Language version of TFS / Azure Devops to another (*new v9.0*)1.34
- _new_  Migration of Processes

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://azure.microsoft.com/services/devops/migrate/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move to Azure DevOps Services.**

## Quick Links

 - [Video Overview](https://www.youtube.com/watch?v=RCJsST0xBCE)
 - [Getting Started](https://nkdagility.com/learn/azure-devops-migration-tools/getting-started.html)
 - [Documentation](https://nkdagility.com/learn/azure-devops-migration-tools/)
 - [Questions on Usage](https://github.com/nkdAgility/azure-devops-migration-tools/discussions)

## Installing and running the tools

These tools are available as a portable application and can be installed in a number of ways, including manually from a zip.
For a more detailed getting started guide please see the [documentation](https://nkdagility.com/docs/azure-devops-migration-tools/getting-started.html).

### Option 1: Winget

We use [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/) to host the tools, and you can use the command `winget install nkdAgility.AzureDevOpsMigrationTools` to install them on Windows 10 and Windows 11. 

The tools will be installed to `%Localappdata%\Microsoft\WinGet\Packages\nkdAgility.AzureDevOpsMigrationTools_Microsoft.Winget.Source_XXXXXXXXXX` and a symbolic link to `devopsmigration.exe` that lets you run it from anywhere using `devopsmigration init`.

**NOTE: Do not install using an elevated command prompt!**

### Option 2: Chocolatey

We also deploy to [Chocolatey](https://chocolatey.org/packages/nkdagility.azuredevopsmigrationtools) and you can use the command `choco install vsts-sync-migrator` to install them on Windows Server. 

The tools will be installed to `C:\Tools\MigrationTools\` which should be added to the path. You can run `devopsmigration.exe`

### Option 3: Manual

You can download the [latest release](https://github.com/nkdAgility/azure-devops-migration-tools/releases/latest) and unzip it to a folder of your choice.

## Minimum Permission Requirements

At this time the documented minimum required permissions for running the tools are:

- Account in both the source and target projects with "Project Collection Administrator" rights
- PAT with "full access" for both the Source and the Target

Note: I have been informed by the Azure DevOps product team information that ObjectModel API only works with full scoped PATs, so it won't work with any PAT that has specific scopes. 

### Advanced Unsupported Permission Options

We have seen that the tools may work with less permissions however the following has not been full tested and is not currently supported:

- Project and Team (Read, write, & manage)
- Work Items (Read, Write & Manage)
- Identity (Read & Manage)
- Security (Manage)

If you do try this out then please let us know how you get on!

## Advanced tools

There are additional advanced tooling available on [Azure DevOps Automation Tools](https://github.com/nkdAgility/azure-devops-automation-tools). These are a collection of Powershell scripts that can be used to;

- Generate Migration Tools configurations across many projects on many organisations
- Export Stats on many projects on many organisations
- Publish Custom fields across many projects on many organisations
- Output the fields and other data for many projects on many organisations

These tools are designed to help you manage migration of Work Items at scale.

## Support

1. [Question & Discussion](https://github.com/nkdAgility/azure-devops-migration-tools/discussions) - The first place to look for usage, configuration, and general help. 
1. [Issues on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/issues) - If you have identified a bug and have logs then please raise an issue.

### Professional Support

You can get free support from the community above and on social media on a best effort basis if folks are available. If you are *looking for paid support* [naked Agility with Martin Hinshelwood & Co](https://nkdagility.com) has a number of experts, many of whom contribute to this project, that can help. Find out how [we can help you](https://nkdagility.com/technical-consulting-and-coaching/azure-devops-migration-tools-consulting/) with your migration and [book a free consultation](https://nkdagility.com/technical-consulting-and-coaching/azure-devops-migration-tools-consulting/) to discuss how we can make things easier.

We use these tools with our customers, and for fun, to do real world migrations on a daily basis and we can:

 - Consult with your internal folks who need help and guidance in running the tooling.
 - Make changes to the tool to support your needs; all additions are committed to the main repo.
 - Run the migration for you:- you would need to pay for the hours that we would spend baby-sitting the running migrations

## Change Log

- [v15.0.0](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v15.0.0) - Release v15! Query and CommonEnrichersConfig changes 
- [v14.4.6](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v14.4.6) - Release v14! Migrate to winget and change exe name to `devopsmigration`
- [v13.2.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v13.2.1) - Oct 9, 2023
	- 13.2 - Added [ExportUsersForMapping](https://nkdagility.com/learn/azure-devops-migration-tools/Reference/v1/Processors/TeamMigrationContext/) to export a json file with a list of users ready for a field mapping.
	- 13.1 - Update all NuGet packages to the latest version.
	- 13.0 - Update to .net 7.0 with all dependencies. Focus on documentation improvements to support future updates.
- [v12.8.10](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v12.8.10) - Apr 25, 2023
	- 12.1 - Make embedded images regex lazy
	- 12.1 - Added a stop when there are area or iteration nodes in the source history that are not in the target. This causes missing data. System will now list the areas and iteration that are missing, and then stop. You can decide to add them manually, or add a field mapping.
- [v11.12.23](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v11.12.23) - Jun 6, 2022
	- v11.11 - Refactored revision manager to have more tests and support limiting the number of revisions. CollapseRevisions has been replaced by setting MaxRevisions to 1 and setting AttachRevisionHistory to true; MaxRevisions sets the maximum number of revisions that will be migrated. "First + Last*N = Max". If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated. This is done after all of the existing revisions are created but before anything newer than that target is removed.
	- v11.10 - Added ability to limit the number of revisions migrated with `MaxRevisions` on the `WorkItemMigration` processor. 0 = All, and any other number should migrate the first revision + the latest up to MAX.
	- v11.9 - Dark launch of `Process` migration by @akanieski 
	- v11.9 - Dark launch of `Pipelines` & `Builds` migration by @tomfrenzel
	- v11.8 - As part of moving to the new architecture we moved to default newtonsoft type handling with `$type` properties instead of `ObjectType` ___To Migrate rename "ObjectType" to "$type" in your configuration!___
	- v11.5 - Added more useful logging levels. Replace `"TelemetryEnableTrace": false` with `"LogLevel": "Verbose"` in the config. Verbose will only be logged to the logfile.
	- v11.2.1 - Removed NodeMigrationContext and converted it to an enricher for Work Items. Still needs work, so that it migrates individual nodes, but currently migrates all.
- [v10.2.13](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v10.2.13) - Sep 27, 2020 
	- v10.1 - Changed config design to have only the Name and not FullName of the class. Remove `MigrationTools.Core.Configuration.FieldMap.` and `MigrationTools.Core.Configuration.Processing.` from the config leaving only the Name of the class in the ObjectType field.
	- v10.0 - Start of the great refactor over to .NET Core and the REST API as the Object Model has been retired.
- [v9.3.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v9.3.1) - Sep 7, 2020
- v9.0 - Added support for migration between other language versions of Azure DevOps. Developed for German -> English
- [v8.9.10](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v8.9.10) -Aug 6, 2020
	- v8.9 - Added 'Collapse Revisions' feature to collapse and attache revisions instead of replaying them
	- v8.8 - 'SkipToFinalRevisedWorkItemType' feature added to handle scenario when changing Work Item Type
	- v8.7 - Support for inline images using a Personal Access Token added to the Source Project
	- v8.6 - Support for fixing links from TFVC Changesets to Git Commits using a mapping file generated from a Git-TFS migration.
	- v8.5 - Attachment Max size and linking work items to git repos between projects.
	- v8.4 - Support for cross-project linking of work items between projects.
	- v8.3 - Support for restarting the migration and syncing at the revision level.
	- v8.2 - Merge Git commit Fixing into Work Item migration (requires repos to be migrated first, can be rerun)
	- v8.0 - Merge of Work Item, Link, & attachment migrators into one.
- [v7.5.74](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/7.5.74) Sep 18, 2019
- [v6.3.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/6.3.1) Feb 23, 2017

- 
