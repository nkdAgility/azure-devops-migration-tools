# Azure DevOps Migration Tools [![Chocolatey](https://img.shields.io/chocolatey/dt/vsts-sync-migrator.svg)](https://chocolatey.org/packages/vsts-sync-migrator/) [![GitHub release](https://img.shields.io/github/release/nkdAgility/vsts-sync-migration.svg)](https://github.com/nkdAgility/vsts-sync-migrator/releases) ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](http://nkdagility.github.io/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a [Azure DevOps Migration Tools on Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

**WARNING: This tool is not designed for a novice. This tool was developed to support the scenarios below, and the edge cases that have been encountered by the 30+ contributors from around the Azure DevOps community. You should be comfortable with the TFS/Azure DevOps object model, as well as debugging code in Visual Studio.**

_NOTICE: Both paid and community support is available through our [recommended consultants](#support) as well as our contributors and many DevOps consultants around the world._

## What can you do with this tool?

- Migrate Work Items from one Team Project to another Team Project (*new v8.3*) and Sync changes after a migration
- Merge many Team Projects into a single Team Project
- Split one Team Project into many Team Projects
- Assistance in changing Process Templates
- Bulk edit of Work Items
- Migration of Test Suites & Test Plans
- Migrate from one Language version of TFS / Azure DevOps to another (*new v9.0*)

## Change Log

- v11.2.1 - Removed NodeMogrationContext and converted it to an enricher for Work Items. Still needs work, so that it migrates individual nodes, but currently migrates all.
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
- v8.0 - Merge of Work Item, Link, & attachment migrators into one.

## What versions of Azure DevOps & TFS do you support?

* Work Item Migration Supports all versions of TFS 2013+ and all versions of Azure DevOps
* Process Template migration only supports XML based Projects

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://www.visualstudio.com/team-services/migrate-tfs-vsts/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move to Azure DevOps Services.**

## Quick Links

 * _{new}_ [Video Overview](https://youtu.be/RCJsST0xBCE)
 * [Getting Started](./getting-started.md)
 * [Documentation](http://nkdagility.github.io/azure-devops-migration-tools/)
 * _{NEW}_ [Changeset Migration](./changeset-migration.md)
 * [Contributing](#contributing)
 * [Why](#why-does-this-exist)
 * [FAQ](./faq.md)
 * [Support](#support)

### External Walkthroughs and Reviews

 * [TFS 2017 Migration To VSTS with VSTS Sync Migrator from Mohamed Radwan](http://mohamedradwan.com/2017/09/15/tfs-2017-migration-to-vsts-with-vsts-sync-migrator/)
 * [Options migrating TFS to VSTS from Richard Fennell](https://blogs.blackmarble.co.uk/blogs/rfennell/post/2017/05/10/Options-migrating-TFS-to-VSTS)
 * [Migrating Test artifacts and all other work item types using the Azure DevOps from Gordon Beeming](https://youtu.be/jU6E0k0eXXw)

## Getting the Tools

There are two ways to get these tools:

* (recommended)[Install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/)
* Download the [latest release from GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and unzip

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

### Processors

There are other processors that can be used to migrate, or process, different sorts of data in different ways. Which one is right for you depends on the situation at hand.

Most of these processors need to be run in order. If you try to migrate work items before you have migrated Area and Iterations then ***bang*** you need to go back.

|Processor | Status |Target |Usage |
|---------|---------|---------|---------|
| [WorkItemMigration](./Processors/WorkItemMigrationConfig.md) | ready | Work Items | Migrates either tip or history of work items with Links & Attachments based on a query with field mappings |
| [TeamMigration](./Processors/TeamMigrationConfig.md) | beta | Teams | Migrates Teams and Team Settings |
| [WorkItemDelete](./Processors/WorkItemDeleteConfig.md) | ready | Work Items | Bulk delete of work items **WARNING DANGEROUS** |
| [WorkItemUpdate](./Processors/WorkItemUpdateConfig.md) | ready | Work Items | Bulk update of Work Items based on a query and field mappings |
|[WorkItemQueryMigration](./Processors/WorkItemQueryMigrationConfig.md) | ready | Queries | Migrates shared queries |
|[TestVeriablesMigration](./Processors/TestVeriablesMigrationConfig.md) | Beta | Suits & Plans | Migrates Test Variables |
|[TestConfigurationsMigration](./Processors/TestConfigurationsMigrationConfig.md) | Beta  | Suits & Plans | Migrates Test configurations |
|[TestPlansAndSuitesMigration](./Processors/TestPlansAndSuitesMigrationConfig.md) | Beta  | Suits & Plans | Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration |
|TestRunsMigration | Alfa | Suits & Plans | Migrates the history of Test Runs |
|ImportProfilePicture & ExportProfilePictureFromAD | Beta | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
|CreateTeamFolders | ? | ? | ? | 
|ExportTeamList | ? | ? | ? | 
|[NodeStructuresMigration](./Processors/NodeStructuresMigrationConfig.md) | merged | Area & Iteration | obsolite - merged into WorkItemMigration |
|AttachementExportMigration | merged | Work Items | obsolite - merged into WorkItemMigration |
|AttachementImportMigration | merged | Work Items | obsolite - merged into WorkItemMigration |
|LinkMigration | merged | Work Items | obsolite - merged into WorkItemMigration |
|HtmlFieldEmbeddedImageMigration | merged | HTML Fields | obsolite - merged into WorkItemMigration |
|WorkItemRevisionReplayMigration | merged |  Work Items | obsolite - merged into WorkItemMigration |
|GitCommitFix | merged | Git links | obsolite - merged into WorkItemMigration |

### Field Maps

By default, when you are moving from source to target the system will map all of the fields that exist in source to the same field in the target. This is the behaviour if the **FieldMaps** section is not present in the configuration file.  

However sometimes you want to move data to another field, or use a regex to parse out just the bits that you want. To help we have built a number of mapping tools that should give you the versatility you need.

* **FieldtoFieldMap** - Just want to map one field to another? This is the one for you.
* **FieldtoFieldMultiMap** - Allows you to create a list of one to one field maps.
* **FieldMergeMap** - Ever wanted to merge two fields? This mapping will let you do just that.
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

* **FixGitCommitLinks** - (obsolite) [this is now fixed on the fly if git is migrated first) Allows you to fix the migrated Git commit hooks (and thus external links) to point to the new repository in the target project. If the source and target repository names are the same, this will work out of the box. If the target repository has a different name, you can specify that name via the "TargetRepository" property.

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

## Support

You can get free support from the community here and on social media on a best effort basis if folks are available. If you are looking for paid support there are a number of consultants that contribute to this project and that are experts in this type of work:

* **Martin Hinshelwood, naked Agility Ltd** - [@MrHinsh](https://github.com/MrHinsh) is the founder of the Azure DevOps Migration Tools is available worldwide to help organisations plan and enact their migration efforts. You can contact him through [naked Agility Ltd.](https://nkdagility.com).
* **Wes MacDonald, LIKE 10 INC.** - [@wesmacdonald](https://github.com/wesmacdonald) is a DevOps Consultant located in Toronto, Canada.  You can reach out to him via [LIKE 10 INC.](http://www.like10.com).
* **Ove Bastiansen** - [@ovebastiansen](https://github.com/ovebastiansen) is a DevOps Consultant located in Norway. You can reach out to him via his [GitHub Profile](https://github.com/ovebastiansen). 
* **Gordon Beeming** - [@Gordon-Beeming](https://github.com/Gordon-Beeming) is a DevOps Consultant located in South Africa. You can reach out to him via [profile page](https://beeming.dev/). 

## FAQ

Check out the [FAQ pages](faq.md)

## Terms

naked Agility Limited & our contributors creates and maintains the "Azure DevOps Migration Tools" project under its [terms of business](https://nkdagility.com/terms/) and allows full access to the source code for customers and the general public. 

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


