# Azure DevOps Migration Tools [![Chocolatey](https://img.shields.io/chocolatey/dt/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/) [![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/azure-devops-migration-tools/releases) ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](http://nkdagility.github.io/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a [Azure DevOps Migration Tools on Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

**[Ask Questions in GitHub Discussions](https://github.com/nkdAgility/azure-devops-migration-tools/discussions)**

## Some Data from the last 30 days (as of 01/02/2023)

| Catagory  | Metric | Notes |
| ------------- | ------------- | ------------- |
| Work Item Revisions | **14m** | A single Work Item may have many revisions that we need to migrate |
| Average Work item Migration Time  | **35s** | Work Item (inlcudes all revisions, links, and attachements for the work item) |
| Git Commit Links  | **480k** |  |
| Attachments | **252.37k**  | Total number of attachments migrated |
| Migration Run Ave  | **14 minutes** | Includes dryruns as well.  |
| Migration Run Total   |  **19bn Seconds** |  |

**INFO: This tool was developed to support the scenarios below, and the edge cases that have been encountered by the 30+ contributors from around the Azure DevOps community. 
You should be comfortable with the TFS/Azure DevOps object model, as well as debugging code in Visual Studio.**
**Community support is available through [GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/discussions); Paid support is available through our [recommended consultants](http://nkdagility.github.io/azure-devops-migration-tools/#support) as well as our contributors and many DevOps consultants around the world.**

## What can you do with this tool?

- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, & `Pipelines` from one `Team Project` to another
- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, & `Pipelines` from one `Organisation` to another
- Bulk edit of `Work Items` across an entire `Project`.

### What versions of Azure DevOps & TFS do you support?

- Work Item Migration Supports all versions of TFS 2013+ and all versions of Azure DevOps (Many tools also work all the way back to TFS 2010 but are not supported)
- You can move from any Tfs/AzureDevOps source to any Tfs/AzureDevOps target.
- Process Template migration only supports XML based Projects

### Typical Uses of this tool

- Merge many projects into a single project
- Split one project into many projects
- Assistance in changing Process Templates
- Bulk edit of Work Items
- Migration of Test Suites & Test Plans
- _new_ Migration of Builds & Pipelines
- Migrate from one Language version of TFS / Azure Devops to another (*new v9.0*)

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://azure.microsoft.com/services/devops/migrate/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move t


## Quick Links

 - [Video Overview](https://youtu.be/RCJsST0xBCE)
 - [Getting Started](./getting-started.md)
 - [Reference](./Reference/index.md)
 - [FAQ](./faq.md)
 - [How To Migrate Things](./HowTo/index.md)


#### Extras

_This is a preview version of both the documentation and the Azure DevOps Migration Tools._

 - [Why](#why-does-this-exist)
 - [FAQ](./faq.md)
 - [Changeset Migration](./changeset-migration.md)
 - [Contributing](#contributing)

#### External Walkthroughs and Reviews

 * [TFS 2017 Migration To Azure DevOps with Azure DevOps Sync Migrator from Mohamed Radwan](http://mohamedradwan.com/2017/09/15/tfs-2017-migration-to-vsts-with-vsts-sync-migrator/)
 * [Options migrating TFS to Azure DevOps from Richard Fennell](https://blogs.blackmarble.co.uk/blogs/rfennell/post/2017/05/10/Options-migrating-TFS-to-VSTS)
 * [Migrating Test artifacts and all other work item types using the Azure DevOps from Gordon Beeming](https://youtu.be/jU6E0k0eXXw)

#### Getting the Tools

There are two ways to get these tools:

* (recommended)[Install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/)
* Download the [latest release from GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and unzip

## Support

1. [Question & Discussion](https://github.com/nkdAgility/azure-devops-migration-tools/discussions) - The first place to look for usage, configuration, and general help. 
1. [Issues on Github](https://github.com/nkdAgility/azure-devops-migration-tools/issues) - If you have identified a bug and have logs then please raise an issue.

### Professional Support

You can get free support from the community above and on social media on a best effort basis if folks are available. If you are *looking for paid support* [naked Agility with Martin Hinshelwood & Co](https://nkdagility.com) has a number of experts, many of whom contribute to this project, that can help. Find out how [we can help you](https://nkdagility.com/technical-consulting-and-coaching/azure-devops-migration-tools-consulting/) with your migration and [book a free consultation](https://nkdagility.com/technical-consulting-and-coaching/azure-devops-migration-tools-consulting/) to discuss how we can make things easier.

We use these tools with our customers, and for fun, to do real world migrations on a daily basis and we can:

 - Consult with your internal folks who need help and guidance in running the tooling.
 - Make changes to the tool to support your needs; all additions are committed to the main repo.
 - Run the migration for you:- you would need to pay for the hours that we would spend baby-sitting the running migrations

 ## Details

These tools are build by naked Agility Limited's DevOps & Agility consultants to do real world migrations on a daily basis. We always work in [Azure DevOps Services](http://dev.azure.com) on https://dev.azure.com/nkdagility/migration-tools/ with code in GitHub and publish as a chocolatey package that pulls from Github Releases.

|                 - | -                                                                                      |
| ----------------: | :------------------------------------------------------------------------------------- |
|     Public Issues | [GitHub Issues](https://github.com/nkdAgility/azure-devops-migration-tools/)           |
| Builds & Releases | [Azure Pipelines](https://dev.azure.com/nkdagility/migration-tools/)                   |
|   Releases Output | [Github Releases](https://github.com/nkdAgility/azure-devops-migration-tools/releases) |
|     Documentation | [Github Pages](http://nkdagility.github.io/azure-devops-migration-tools/)              |

**Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get you started in 30 minutes. This tool is complicated and its not always easy to discover what you need to do.**


## Processors (v1 Architecture)

There are a number processors that can be used to migrate, or process, different sorts of data in different ways. Which one is right for you depends on the situation at hand. 
Most of these processors need to be run in order. If you try to migrate work items before you have migrated Area and Iterations then ***bang*** you need to go back.

| Processors | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [CreateTeamFolders](Reference/v1/Processors/CreateTeamFolders.md) | alpha | Shared Queries | Creates folders in Sared Queries for each Team |
| [ExportProfilePictureFromADContext](Reference/v1/Processors/ExportProfilePictureFromADContext.md) | alpha | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
| [ExportTeamList](Reference/v1/Processors/ExportTeamList.md) | missng XML code comments | missng XML code comments | missng XML code comments |
| [FakeProcessor](Reference/v1/Processors/FakeProcessor.md) | missng XML code comments | missng XML code comments | Note: this is only for internal usage. Don't use this in your configurations. |
| [FixGitCommitLinks](Reference/v1/Processors/FixGitCommitLinks.md) | missng XML code comments | missng XML code comments | missng XML code comments |
| [ImportProfilePictureContext](Reference/v1/Processors/ImportProfilePictureContext.md) | alpha | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
| [TeamMigrationContext](Reference/v1/Processors/TeamMigrationContext.md) | preview | Teams | Migrates Teams and Team Settings: This should be run after `NodeStructuresMigrationConfig` and before all other processors. |
| [TestConfigurationsMigrationContext](Reference/v1/Processors/TestConfigurationsMigrationContext.md) | Beta | Suites & Plans | This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`. |
| [TestPlansAndSuitesMigrationContext](Reference/v1/Processors/TestPlansAndSuitesMigrationContext.md) | Beta | Suites & Plans | Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration |
| [TestVariablesMigrationContext](Reference/v1/Processors/TestVariablesMigrationContext.md) | Beta | Suites & Plans | This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`. |
| [WorkItemDelete](Reference/v1/Processors/WorkItemDelete.md) | ready | WorkItem | The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query. **DANGER:** This is not a recoverable action and should be use with extream caution. |
| [WorkItemMigrationContext](Reference/v1/Processors/WorkItemMigrationContext.md) | ready | Work Items | WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure. |
| [WorkItemPostProcessingContext](Reference/v1/Processors/WorkItemPostProcessingContext.md) | preview | Work Items | Reapply field mappings after a migration. Does not migtate Work Items, only reapplied changes to filed mappings. |
| [WorkItemQueryMigrationContext](Reference/v1/Processors/WorkItemQueryMigrationContext.md) | preview | Shared Queries | This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool. |
| [WorkItemUpdate](Reference/v1/Processors/WorkItemUpdate.md) | missng XML code comments | WorkItem | This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change. |
| [WorkItemUpdateAreasAsTagsContext](Reference/v1/Processors/WorkItemUpdateAreasAsTagsContext.md) | Beta | Work Item | A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags. |


## Processors (v2 Architecture) [ PREVIEW ]

**_These are experimental processors that should replace those above. We are interested in feedback of the new format of the config, as well as the functionality._**

The new processor configuration is designed to allow the Migration Tools to support different Source and targets than just TFS/Azure DevOps, and we moved the Endpoints to the processor to allow both Object Model & REST in different processors.

| Processors | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [AzureDevOpsPipelineProcessor](Reference/v2/Processors/AzureDevOpsPipelineProcessor.md) | Beta | Pipelines | Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines. |
| [ProcessDefinitionProcessor](Reference/v2/Processors/ProcessDefinitionProcessor.md) | Beta | Pipelines | Process definition processor used to keep processes between two orgs in sync |
| [TfsAreaAndIterationProcessor](Reference/v2/Processors/TfsAreaAndIterationProcessor.md) | Beta | Work Items | The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths. |
| [TfsSharedQueryProcessor](Reference/v2/Processors/TfsSharedQueryProcessor.md) | Beta | Queries | The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another. |
| [TfsTeamSettingsProcessor](Reference/v2/Processors/TfsTeamSettingsProcessor.md) | Beta | Teams | Native TFS Processor, does not work with any other Endpoints. |
| [WorkItemTrackingProcessor](Reference/v2/Processors/WorkItemTrackingProcessor.md) | missng XML code comments | missng XML code comments | This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md). |


## Field Maps

By default, when you are moving from source to target the system will map all of the fields that exist in source to the same field in the target. This is the behavior if the **FieldMaps** section is not present in the configuration file.  

However sometimes you want to move data to another field, or use a regex to parse out just the bits that you want. To help we have built a number of mapping tools that should give you the versatility you need.

| FieldMaps | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [FieldBlankMapConfig](Reference/v1/FieldMaps/FieldBlankMapConfig.md) | ready | Work Item | Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue. |
| [FieldClearMapConfig](Reference/v1/FieldMaps/FieldClearMapConfig.md) | ready | Work Item | Allows you to set an already populated field to Null. This will only work with fields that support null. |
| [FieldLiteralMapConfig](Reference/v1/FieldMaps/FieldLiteralMapConfig.md) | ready | Work Item Field | Sets a field on the `target` to b a specific value. |
| [FieldMergeMapConfig](Reference/v1/FieldMaps/FieldMergeMapConfig.md) | ready | Work Item Field | Ever wanted to merge two or three fields? This mapping will let you do just that. |
| [FieldtoFieldMapConfig](Reference/v1/FieldMaps/FieldtoFieldMapConfig.md) | ready | Work Item Field | Just want to map one field to another? This is the one for you. |
| [FieldtoFieldMultiMapConfig](Reference/v1/FieldMaps/FieldtoFieldMultiMapConfig.md) | ready | Work Item Field | Want to setup a bunch of field maps in a single go. Use this shortcut! |
| [FieldtoTagMapConfig](Reference/v1/FieldMaps/FieldtoTagMapConfig.md) | ready | Work Item Field | Want to take a field and convert its value to a tag? Done... |
| [FieldValueMapConfig](Reference/v1/FieldMaps/FieldValueMapConfig.md) | ready | Work Item Field | Need to map not just the field but also values? This is the default value mapper. |
| [FieldValuetoTagMapConfig](Reference/v1/FieldMaps/FieldValuetoTagMapConfig.md) | ready | Work Item Field | Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target. |
| [MultiValueConditionalMapConfig](Reference/v1/FieldMaps/MultiValueConditionalMapConfig.md) | ready | Work Item Field | ??? If you know how to use this please send a PR :) |
| [RegexFieldMapConfig](Reference/v1/FieldMaps/RegexFieldMapConfig.md) | ready | Work Item Field | I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done. |
| [TreeToTagMapConfig](Reference/v1/FieldMaps/TreeToTagMapConfig.md) | ready | Work Item Field | Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path... |


## Code (TFVC)

There are no good tools for migrating TFVC code. All of them suffer from "time-dilation" as one can't control the dates of the Check-ins. While you can use tools like TaskTop, TFS Integration Tools, or others there is no support. We prefer to recommend that you use Git-TFS and migrate from TFVC to Git with full history and branches. If your resulting repository is too large we recommend creating a full clone of TFVC for posterity, and then create a limited branch clone with limited history for work.

## Code (Git)

When moving Git repos between Projects and Accounts you are able to maintain full history. However you will need to have all of the links to 

* **FixGitCommitLinks** - (obsolete) [this is now fixed on the fly if git is migrated first) Allows you to fix the migrated Git commit hooks (and thus external links) to point to the new repository in the target project. If the source and target repository names are the same, this will work out of the box. If the target repository has a different name, you can specify that name via the "TargetRepository" property.

## Build & Releases

When you import a build or release definition into Azure DevOps you need to fill out some of the data to allow that new definition to be saved. Things like connections and other require fields usually don't have matching GUIDS and need manual intervention. For builds & Releases we recommend that you use the built in Export/Import tools provided in the UI to move then to a new Team Project.

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

Events for the Team:

- Weekly Architectural Review - Thursday 2100 GMT

If you want to be added to the community Team then please [fill out this form and request access](http://nkdagility.com/contact)

## FAQ

Check out the [FAQ pages](faq.md)

## Primary Contributors & Consultants

* **Martin Hinshelwood, naked Agility Ltd** - [@MrHinsh](https://github.com/MrHinsh) is the founder of the Azure DevOps Migration Tools is available worldwide to help organisations plan and enact their migration efforts. You can contact him through [naked Agility Ltd.](https://nkdagility.com).
* **Wes MacDonald, LIKE 10 INC.** - [@wesmacdonald](https://github.com/wesmacdonald) is a DevOps Consultant located in Toronto, Canada.  You can reach out to him via [LIKE 10 INC.](http://www.like10.com). 
* **Ove Bastiansen** - [@ovebastiansen](https://github.com/ovebastiansen) is a DevOps consultant living in Oslo, Norway, but working worldwide in todays remote friendly world. You can reach him via his GitHub profile [https://github.com/ovebastiansen](https://github.com/ovebastiansen).
* **Gordon Beeming** - [@DevStarOps](https://github.com/DevStarOps) is a DevOps Specialist in Durban, South Africa working at [Derivco](https://derivco.com). You can reach him via his [profile page](https://devstarops.com/) that links to all social media.
* **Richard Hundhausen** - [@rhundhausen](https://github.com/rhundhausen) is an Azure DevOps consultant living in the United States and working at [Accentient](https://accentient.com)

## Terms

naked Agility Limited & our contributors creates and maintains the "Azure DevOps Migration Tools" project under its [terms of business](https://nkdagility.com/terms/) and allows full access to the source code for customers and the general public. 
