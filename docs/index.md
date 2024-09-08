---
title: Azure DevOps Migration Tools
layout: page
pageType: index
template: index-template.md
toc: false
pageStatus: published
discussionId: 
---

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](https://nkdagility.com/learn/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a Winget package a `nkdAgility.AzureDevOpsMigrationTools`.

**[Ask Questions on Github](https://github.com/nkdAgility/azure-devops-migration-tools/discussions)**

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

For the most part we support moving data between `((Azure DevOps Server | Team Foundation Server | Azure DevOps Services) <=> (Azure DevOps Server | Team Foundation Server | Azure DevOps Services))` for any version greater than 2013. 

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

- [Installation](installation.md)
- [Permissions](permissions.md)
- [Getting Started](getting-started.md)
- [Configuration Reference](./Reference/)
- [FAQ](faq.md)
- [Support](support.md)
- [How To Migrate Things](./HowTo/index.md)
- [Community Support](https://github.com/nkdAgility/azure-devops-migration-tools/discussions)
- [Commercial Support](https://nkdagility.com/capabilities/azure-devops-migration-services/)

The documentation for the preview is on [Preview](https://nkdagility.com/docs/azure-devops-migration-tools/preview/)]

#### External Walkthroughs and Reviews

 * [TFS 2017 Migration To Azure DevOps with Azure DevOps Sync Migrator from Mohamed Radwan](http://mohamedradwan.com/2017/09/15/tfs-2017-migration-to-vsts-with-vsts-sync-migrator/)
 * [Options migrating TFS to Azure DevOps from Richard Fennell](https://blogs.blackmarble.co.uk/blogs/rfennell/post/2017/05/10/Options-migrating-TFS-to-VSTS)
 * [Migrating Test artifacts and all other work item types using the Azure DevOps from Gordon Beeming](https://youtu.be/jU6E0k0eXXw)


 ## Metrics (v16+ experimental)

These metrics come directly from Custo Metrics in Application Insights and are updated every 10 minutes. They are experimental and may not be accurate yet... once I get more data Ill tweek both the collection and rendering.

| Metric          | Category   | Type       | All                                                                                                                     | v16                                                                                                                      | Notes             |
|-----------------|------------|------------|-------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|-------------------|
| WorkItemMetrics | Work Items | Count    | ![Work Items Total](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemTotals%3Fcode%3Dgithub%26version%3D&label=%20)  | ![Work Items v16](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemTotals%3Fcode%3Dgithub%26version%3D16.0&label=%20) | This counts the number of work items processed |
| WorkItemMetrics | Work Items | Ave  | ![Work Items Avg](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemProcessingDuration%3Fcode%3Dgithub%26version%3D&label=%20) | ![Work Items Avg v16](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemProcessingDuration%3Fcode%3Dgithub%26version%3D16.0&label=%20) | the average amount of time a work item takes to process |
| WorkItemMetrics | Revisions  | Count    | ![Revisions Total](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemRevisionsTotal%3Fcode%3Dgithub&label=%20)  | ![Revisions v16](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemRevisionsTotal%3Fcode%3Dgithub%26version%3D16.0&label=%20) | the total number of revisions processed |
| WorkItemMetrics | Revisions  | Ave  | ![Revisions Avg](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemRevisions%3Fcode%3Dgithub&label=%20)  | ![Revisions Avg v16](https://img.shields.io/endpoint?url=https%3A%2F%2Fmigrationtoolstelemetery.azurewebsites.net%2Fapi%2FGetShieldIoWorkItemMetrics_WorkItemRevisions%3Fcode%3Dgithub%26version%3D16.0&label=%20)  | the average number of revisions per work item |

![Work Items in last 30 days](https://migrationtoolstelemetery.azurewebsites.net/api/GetGraphWorkItemMetrics_WorkItems?code=github)


This tool uses OpenTelemetery to collect metrics and logs, and Application Insights to store and analyse them. Exceptions are also sent to [Elmah.io](https://elmah.io) for analysis and improvement.

### Some Data from the last 30 days (v15- as of 06/04/2023)

| Catagory  | Metric | Notes |
| ------------- | ------------- | ------------- |
| Work Item Revisions | **14m** | A single Work Item may have many revisions that we need to migrate |
| Average Work item Migration Time  | **35s** | Work Item (inlcudes all revisions, links, and attachements for the work item) |
| RelatedLinkCount | **5m** | Each work item may have many links or none. |
| Git Commit Links  | **480k** |  |
| Attachments | **252.37k**  | Total number of attachments migrated |
| Test Suits | 52k | total suits migrated | 
| Test Cases Mapped | **800k** | Total test cases mapped into Suits |
| Migration Run Ave  | **14 minutes** | Includes dry-runs as well.  |
| Migration Run Total   |  **19bn Seconds** | Thats **316m hours** or **13m days** of run time in the last 30 days. |


## Processors 

{% include content-collection-table.html collection = "reference" typeName = "Processors" %}

## Field Maps

By default, when you are moving from source to target the system will map all of the fields that exist in source to the same field in the target. This is the behavior if the **FieldMaps** section is not present in the configuration file.  

However sometimes you want to move data to another field, or use a regex to parse out just the bits that you want. To help we have built a number of mapping tools that should give you the versatility you need.

{% include content-collection-table.html collection = "reference" typeName = "FieldMaps" %}


## Why does this exist
The main migration tool for TFS has always been the TFS Integration Tools which is sadly no longer supported. Indeed it only loosely supported versions of TFS after 2010 and had a lot of bugs. It was very difficult to configure and run. This tool hopes to solve some of that by providing support for TFS 2015 and Visual Studio Team Services (VSTS).

It solves:

 * Supports all currently supported version of TFS
 * Supports Azure DevOps Services 
 * Migrates work items from one instance of TFS or Azure DevOps to another
 * Bulk edits fields in place for both TFS and Azure DevOps Services
 * Being able to migrate Test Plans an Suits from one Team Project to another
 * Being able to migrate Teams from one Team Project to another

## Contributing

If you wish to contribute then feel free to fork this repository and submit a pull request. If you would like to join the team please contact.

This project is primarily managed and maintained on Visual Studio Team Services and code checked into MASTER is automatically synched between VAzure DevOps and GitHub. There is no hidden published code, but not all branches are published.

If you want to sync your GitHub repository then check out [Open-source with Azure DevOps or TFS and Github for better DevOps
](https://nkdagility.com/open-source-vsts-tfs-github-better-devops/).


## Primary Contributors & Consultants

* **Martin Hinshelwood, naked Agility Ltd** - [@MrHinsh](https://github.com/MrHinsh) is the founder of the Azure DevOps Migration Tools is available worldwide to help organisations plan and enact their migration efforts. You can contact him through [naked Agility Ltd.](https://nkdagility.com). 
* **Wes MacDonald, LIKE 10 INC.** - [@wesmacdonald](https://github.com/wesmacdonald) is a DevOps Consultant located in Toronto, Canada.  You can reach out to him via [LIKE 10 INC.](http://www.like10.com). 
* **Ove Bastiansen** - [@ovebastiansen](https://github.com/ovebastiansen) is a DevOps consultant living in Oslo, Norway, but working worldwide in todays remote friendly world. You can reach him via his GitHub profile [https://github.com/ovebastiansen](https://github.com/ovebastiansen).
* **Gordon Beeming** - [@DevStarOps](https://github.com/DevStarOps) is a DevOps Specialist in Durban, South Africa working at [Derivco](https://derivco.com). You can reach him via his [profile page](https://devstarops.com/) that links to all social media.
* **Richard Hundhausen** - [@rhundhausen](https://github.com/rhundhausen) is an Azure DevOps consultant living in the United States and working at [Accentient](https://accentient.com)

## Terms

naked Agility Limited & our contributors creates and maintains the "Azure DevOps Migration Tools" project under its [terms of business](https://nkdagility.com/terms/) and allows full access to the source code for customers and the general public. 
