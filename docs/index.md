# Azure DevOps Migration Tools [![Chocolatey](https://img.shields.io/chocolatey/dt/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/) [![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/azure-devops-migration-tools/releases) ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](http://nkdagility.github.io/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a [Azure DevOps Migration Tools on Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

<a href="https://stackoverflow.com/questions/tagged/azure-devops-migration-tools" title="Ask Questions on Stack Overflow"><img src="http://cdn.sstatic.net/stackexchange/img/logos/so/so-logo.png" width="250"></a>

**Ask Questions on Stack Overflow: https://stackoverflow.com/questions/tagged/azure-devops-migration-tools**

![alt text](https://raw.githubusercontent.com/nkdAgility/azure-devops-migration-tools/master/src/MigrationTools.Extension/images/azure-devops-migration-tools-naked-agility-martin-hinshelwood.png)

**INFO: This tool was developed to support the scenarios below, and the edge cases that have been encountered by the 30+ contributors from around the Azure DevOps community. You should be comfortable with the TFS/Azure DevOps object model, as well as debugging code in Visual Studio.**
**Community support is available through [GitHub](https://github.com/nkdAgility/azure-devops-migration-tools) and [StackOverflow](https://stackoverflow.com/questions/tagged/azure-devops-migration-tools); Paid support is available through our [recommended consultants](http://nkdagility.github.io/azure-devops-migration-tools/#support) as well as our contributors and many DevOps consultants around the world.**

## What can you do with this tool?

- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, & `Pipelines` from one `Team Project` to another
- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, & `Pipelines` from one `Organisation` to another
- Bulk edit of `Work Items` accross an entire `Project`.

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
- Migrate from one Language version of TFS / Azure Devops to another (*new v9.0*)

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://azure.microsoft.com/services/devops/migrate/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move t


## Quick Links

 - [Video Overview](https://youtu.be/RCJsST0xBCE)
 - [Getting Started](./getting-started.md)
 - [FAQ](./faq.md)
 - _!preview v2_ [How To Migrate Things](./HowTo/index.md)
 - _!preview v2_ [Reference](./Reference/index.md)

#### Extras

_This is a preview version of both the documentation and the Azure DevOps Migration Tools._

 - [Why](#why-does-this-exist)
 - [FAQ](./faq.md)
 - [Changeset Migration](./changeset-migration.md)
 - [Contributing](#contributing)

#### Reference: A Deep Dive

  - _!preview v2_ [Reference](./Reference/index.md)
  - _!preview v2_ [Processors](./Reference/Processors/index.md)
  - _!preview v2_ [ProcessorEnrichers](./Reference/ProcessorEnrichers/index.md)
  - _!preview v2_ [Endpoints](./Reference/Endpoints/index.md)
  - _!preview v2_ [EndpointEnrichers](./Reference/EndpointEnrichers/index.md)

#### External Walkthroughs and Reviews

 * [TFS 2017 Migration To VSTS with VSTS Sync Migrator from Mohamed Radwan](http://mohamedradwan.com/2017/09/15/tfs-2017-migration-to-vsts-with-vsts-sync-migrator/)
 * [Options migrating TFS to VSTS from Richard Fennell](https://blogs.blackmarble.co.uk/blogs/rfennell/post/2017/05/10/Options-migrating-TFS-to-VSTS)
 * [Migrating Test artifacts and all other work item types using the Azure DevOps from Gordon Beeming](https://youtu.be/jU6E0k0eXXw)

#### Getting the Tools

There are two ways to get these tools:

* (recommended)[Install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/)
* Download the [latest release from GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and unzip

#### Getting Support

1. [Question on Stackoverflow](https://stackoverflow.com/questions/tagged/azure-devops-migration-tools) - The first place to look for unsage, configuration, and general help is on Stackoverflow. 
1. [Issues on Gitbub](https://github.com/nkdAgility/azure-devops-migration-tools/issues)


### Processors (v1 Architecture)

There are other processors that can be used to migrate, or process, different sorts of data in different ways. Which one is right for you depends on the situation at hand.

Most of these processors need to be run in order. If you try to migrate work items before you have migrated Area and Iterations then ***bang*** you need to go back.

| Processor                                                                                    | Status  | Target            | Usage                                                                                                      |
| :------------------------------------------------------------------------------------------- | :------ | :---------------- | ---------------------------------------------------------------------------------------------------------- |
| [WorkItemMigrationConfig](./Processors/WorkItemMigrationConfig.md)                           | ready   | Work Items        | Migrates either tip or history of work items with Links & Attachments based on a query with field mappings |
| [TfsTeamSettingsProcessor](./Reference/Processors/TfsTeamSettingsProcessor.md)               | preview | Teams             | Migrates Teams and Team Settings                                                                           |
| [TfsAreaAndIterationProcessor](./Reference/Processors/TfsAreaAndIterationProcessor.md)       | preview | Area & Iterations | Migrates Nodes before run                                                                                  |
| [WorkItemDelete](./Processors/WorkItemDeleteConfig.md)                                       | ready   | Work Items        | Bulk delete of work items **WARNING DANGEROUS**                                                            |
| [WorkItemUpdate](./Processors/WorkItemUpdateConfig.md)                                       | ready   | Work Items        | Bulk update of Work Items based on a query and field mappings                                              |
| [TfsSharedQueryProcessor](./Reference/Processors/TfsSharedQueryProcessor.md)                 | preview | Shared Queries    | Moved Shared Queries best effort                                                                           |
| [TestVariablesMigration](./Processors/TestVariablesMigrationConfig.md)                       | Beta    | Suites & Plans    | Migrates Test Variables                                                                                    |
| [TestConfigurationsMigration](./Processors/TestConfigurationsMigrationConfig.md)             | Beta    | Suites & Plans    | Migrates Test configurations                                                                               |
| [TestPlansAndSuitesMigration](./Processors/TestPlansAndSuitesMigrationConfig.md)             | Beta    | Suites & Plans    | Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration                               |
| [ImportProfilePicture](./Processors/ImportProfilePictureConfig) & ExportProfilePictureFromAD | Beta    | Profiles          | Downloads corporate images and updates TFS/Azure DevOps profiles                                           |
| [WorkItemUpdateAreasAsTags](./Processors/WorkItemUpdateAreasAsTagsConfig)                    | Beta    | Work Items        | Adds tags to work items  to reflect area paths on source system                                            |
| TestRunsMigration                                                                            | Alpha   | Suits & Plans     | Migrates the history of Test Runs                                                                          |

### Processors (v2 Architecture) [ PREVIEW ]

**_These are experimental processors that should replace those above. We are intersted in feedback of the new format of the config, as well as the functionality._**

The new processor configuration is designed to allow the Migration Tools to support diferent Source and targets than just TFS/Azure DevOps, and we moved the Endpoints to the processor to allow both Object Model & REST in different processors.

| Processor                                                                        | Status  | Target         | Usage                                                                                                      |
| :------------------------------------------------------------------------------- | :------ | :------------- | :--------------------------------------------------------------------------------------------------------- |
| [WorkItemTrackingProcessor](./Reference/Processors/WorkItemTrackingProcessor.md) | Alpha   | Work Items     | Migrates either tip or history of work items with Links & Attachments based on a query with field mappings |
| [TfsTeamSettingsProcessor](./Reference/Processors/TfsTeamSettingsProcessor.md)   | preview | Teams          | Migrates Teams and Team Settings                                                                           |
| [TfsSharedQueryProcessor](./Reference/Processors/TfsTeamSettingsProcessor.md)    | preview | Shared Queries | Moved Shared Queries best effort                                                                           |

### Field Maps

By default, when you are moving from source to target the system will map all of the fields that exist in source to the same field in the target. This is the behaviour if the **FieldMaps** section is not present in the configuration file.  

However sometimes you want to move data to another field, or use a regex to parse out just the bits that you want. To help we have built a number of mapping tools that should give you the versatility you need.

* **FieldtoFieldMap** - Just want to map one field to another? This is the one for you.
* **FieldtoFieldMultiMap** - Allows you to create a list of one to one field maps.
* **FieldMergeMap** - Ever wanted to merge two or three fields? This mapping will let you do just that.
* **FieldBlankMap** - Allows you to set an already populated field to empty
* **FieldtoTagMap** - Want to take a field and convert its value to a tag? Done...
* **FieldValueMap** - Need to map not just the field but also values? This is the default value mapper.
* **FieldValuetoTagMap** - Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
* **RegexFieldMap** - I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
* **TreeToTagMap** - Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...

##### Code (TFVC)

There are no good tools for migrating TFVC code. All of them suffer from "time-dilation" as one can't control the dates of the Check-ins. While you can use tools like TaskTop, TFS Integration Tools, or others there is no support. We prefer to recommend that you use Git-TFS and migrate from TFVC to Git with full history and branches. If your resulting repository is too large we recommend creating a full clone of TFVC for posterity, and then create a limited branch clone with limited history for work.

##### Code (Git)

When moving Git repos between Projects and Accounts you are able to maintain full history. However you will need to have all of the links to 

* **FixGitCommitLinks** - (obsolete) [this is now fixed on the fly if git is migrated first) Allows you to fix the migrated Git commit hooks (and thus external links) to point to the new repository in the target project. If the source and target repository names are the same, this will work out of the box. If the target repository has a different name, you can specify that name via the "TargetRepository" property.

##### Build & Releases

When you import a build or release definition into VSTS you need to fill out some of the data to allow that new definition to be saved. Things like connections and other require fields usually don't have matching GUIDS and need manual intervention. For builds & Releases we recommend that you use the built in Export/Import tools provided in the UI to move then to a new Team Project.

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

This project is primarily managed and maintained on Visual Studio Team Services and code checked into MASTER is automatically synched between VSTS and GitHub. There is no hidden published code, but not all branches are published.

If you want to sync your GitHub repository then check out [Open-source with VSTS or TFS and Github for better DevOps
](https://nkdagility.com/open-source-vsts-tfs-github-better-devops/).

Events for the Team:

- Weekly Architectural Review - Thursday 2100 GMT

If you want to be added to the community Team then please [fill out this form and request access](http://nkdagility.com/contact)


## Support

You can get free support from the community here and on social media on a best effort basis if folks are available. If you are looking for paid support there are a number of consultants that contribute to this project and that are experts in this type of work:

These tools are build by naked Agility Limited's DevOps & Agility consultants to do real world migrations on a daily basis. We always work in [Azure DevOps Services](http://dev.azure.com) on https://dev.azure.com/nkdagility/migration-tools/ with code in GitHub and publish as a chocolatey package that pulls from GitGub Releases.

|                 - | -                                                                                      |
| ----------------: | :------------------------------------------------------------------------------------- |
|   Team Work Items | [Azure Boards](https://dev.azure.com/nkdagility/migration-tools/)                      |
|     Public Issues | [GitHub Issues](https://github.com/nkdAgility/azure-devops-migration-tools/)           |
| Builds & Releases | [Azure Pipelines](https://dev.azure.com/nkdagility/migration-tools/)                   |
|   Releases Output | [Github Releases](https://github.com/nkdAgility/azure-devops-migration-tools/releases) |
|     Documentation | [Github Pages](http://nkdagility.github.io/azure-devops-migration-tools/)              |

**Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get you started in 30 minutes. This tool is complicated and its not always easy to discover what you need to do.**


## FAQ

Check out the [FAQ pages](faq.md)

## Terms

naked Agility Limited & our contributors creates and maintains the "Azure DevOps Migration Tools" project under its [terms of business](https://nkdagility.com/terms/) and allows full access to the source code for customers and the general public. 
