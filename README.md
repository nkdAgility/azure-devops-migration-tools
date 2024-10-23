![Azure DevOps Migration Tools from Naked Agility with Martin Hinshelwood](https://github.com/user-attachments/assets/997cc49f-cbe9-4f22-a8e1-49b529d0dff0)
![GitHub release](https://img.shields.io/github/v/release/nkdAgility/azure-devops-migration-tools)
![GitHub pre-release](https://img.shields.io/github/v/release/nkdAgility/azure-devops-migration-tools?include_prereleases)

[![Build Status](https://dev.azure.com/nkdagility/AzureDevOps-Tools/_apis/build/status%2FMigrationTools-CIv2?branchName=main)](https://dev.azure.com/nkdagility/AzureDevOps-Tools/_build/latest?definitionId=115&branchName=main)
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/nkdagility/AzureDevOps-Tools/115?compact_message&style=plastic&logo=azuredevops&label=Tests)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=coverage)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=security_rating)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=vsts-sync-migrator%3Amaster&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=vsts-sync-migrator%3Amaster)
![Visual Studio Marketplace Rating](https://img.shields.io/visual-studio-marketplace/stars/nkdagility.vsts-sync-migration?logo=visualstudio)
![Chocolatey Downloads](https://img.shields.io/chocolatey/dt/vsts-sync-migrator)
[![Elmah.io](https://img.shields.io/badge/sponsored_by-elmah_io-0da58e)](https://elmah.io)

Created and maintained by [Martin Hinshelwood](https://www.linkedin.com/in/martinhinshelwood/) (http://nkdagility.com)

![YouTube Channel Subscribers](https://img.shields.io/youtube/channel/subscribers/UCkYqhFNmhCzkefHsHS652hw)
![YouTube Channel Views](https://img.shields.io/youtube/channel/views/UCkYqhFNmhCzkefHsHS652hw)

# Azure DevOps Migration Tools

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](https://nkdagility.com/learn/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a Winget package a `nkdAgility.AzureDevOpsMigrationTools`.

**Ask Questions on Github: https://github.com/nkdAgility/azure-devops-migration-tools/discussions**

## Compatability

These tools run on Windows and support connecting to Team Foundation Server 2013+, Azure DevOps Server, & Azure DevOps Services. They support both hosted and on-premise instances and can move data between any two.

- Supports all versions of TFS 2013+ and all versions of Azure DevOps.
- You can migrate from any TFS/Azure DevOps source to any TFS/Azure DevOps target.

## What do you get?

- *Move* Work Items, Test Plans & Suits, and Pipelines between projects, collections, and even organizations.
- *Merge* multiple projects into a single project even from different organizations.
- *Split* one project into several projects even between projects, collections, and even organizations.
- *Change* Process process from Agile to Scrum or any other template.
- *Bulk edit* Work Items.

## What does this tool do?

For the most part we support moving data between ((Azure DevOps Server | Team Foundation Server | Azure DevOps Services) <=> (Azure DevOps Server | Team Foundation Server | Azure DevOps Services)) for any version greater than 2013. 

- `Work Items` (including links and attachments) with custom mappings for fields and types
	- Copy Work Items between locations with history
	- Bulk Edit in place of Work Items (Great for cleaning up data, process template changes)
	- Optionaly includes `Teams`, `Shared Queries`
- `Test Plans & Suites` 
	- Copy Test Plans & Suites between locations
	- Includes `Configurations`, `Shared Steps`, `Shared Parameters`
- `Pipelines`
	- Copy Pipelines between locations
	- excludes XAML & Classic Builds & Release
- `Processes`
	- Copy Processes between locations

**Note**: 'Locations' includes `Projects`, `Collections`, `Organizations`

**Important:** This tool is intended for experienced users familiar with TFS/Azure DevOps object models and debugging in Visual Studio. It was developed by 100+ contributors from the Azure DevOps community to handle various scenarios and edge cases. _Not all cases are supported_.

**Support Options:** Community support is available on [GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/discussions). For paid support, consider our [Azure DevOps Migration Services](https://nkdagility.com/capabilities/azure-devops-migration-services/).

## Quick Links

- [Documenation](https://nkdagility.com/docs/azure-devops-migration-tools/)
- [Installation](https://nkdagility.com/learn/azure-devops-migration-tools/installation/)
- [Permissions](https://nkdagility.com/learn/azure-devops-migration-tools/permissions/)
- [Getting Started](https://nkdagility.com/learn/azure-devops-migration-tools/getting-started/)
- [Configuration Reference](https://nkdagility.com/learn/azure-devops-migration-tools/Reference/)
- [Community Support](https://github.com/nkdAgility/azure-devops-migration-tools/discussions)
- [Commercial Support](https://nkdagility.com/capabilities/azure-devops-migration-services/)
- [Change Log](https://nkdagility.com/learn/azure-devops-migration-tools/change-log/)

The documentation for the preview is on [Preview](https://nkdagility.com/docs/azure-devops-migration-tools/preview/)]

## Metrics

These metrics come directly from Custo Metrics in Application Insights and are updated every 10 minutes. They are experimental and may not be accurate.


| Metric          | Category   | Type       | All (last 30 days)                                                                                                                     | Notes             |
|-----------------|------------|------------|-------------------------------------------------------------------------------------------------------------------------|-------------------|
| WorkItemMetrics | WorkItems  | Count      | ![Work Items Total](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemTotals%3Fcode%3Dgithub%26version%3D&label=%20)  | This counts the number of work items processed |
|                 |            | Ave        | ![Work Items Avg](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemProcessingDuration%3Fcode%3Dgithub%26version%3D&label=%20) | The average amount of time a work item takes to process |
|                 | Revisions  | Count      | ![Revisions Total](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemRevisionsTotal%3Fcode%3Dgithub&label=%20)  | The total number of revisions processed |
|                 |            | Ave        | ![Revisions Avg](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemRevisions%3Fcode%3Dgithub&label=%20)  | The average number of revisions per work item |

![Work Items in last 30 days](https://migrationtoolstelemetery.azurewebsites.net/api/GetGraphWorkItemMetrics_WorkItems?code=github)


This tool uses OpenTelemetery to collect metrics and logs, and Application Insights to store and analyse them. Exceptions are also sent to [Elmah.io](https://elmah.io) for analysis and improvement.

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
