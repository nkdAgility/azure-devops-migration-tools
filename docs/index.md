# Azure DevOps Migration Tools Docs 

Azure DevOps Migration Tools allow you to bulk edit data in Microsoft Team Foundation Server (TFS) and Azure DevOps Services. It has many names depending on what you are trying to achieve. You might call it a migration tool, or a bulk update tool, and both are correct. It can be used to:


 - Migrate data from TFS to TFS
 - Migrate data from TFS to Azure DevOps Services
 - Migrate data from Azure Devops Services to TFS
 - Migrate data from Azure DevOps to Azure DevOps
 - Bulk update in TFS or Azure DevOps Services

 This is an advanced tool and is not designed to be used by those not intimatly familure with TFS, Azure DevOps Services, and their API's. It supports all version of TFS & Azure DevOps that can be connected to using https://www.nuget.org/packages/Microsoft.TeamFoundationServer.ExtendedClient 

## Change Log

- v8.3 - Support for restarting the migration and syncing at the revision level.
- v8.2 - Merge Git commit Fixing into Work Item migration (requires repos to be migrated first, can be rerun)
- v8.0 - Merge of Work Item, Link, & attachent migrators into one.

## Getting the Tools

The most effective way to get a copy of the tools is to use Chocolatey to install them:

- [Install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/) **(recommended)**
- Manually download the [latest release from GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and unzip

## Overview

These tools are build by naked Agility Limited's DevOps & Agility consultants to do real world migrations on a daily basis. We always work in [Azure Devops Services](http://dev.azure.com) on https://dev.azure.com/nkdagility/migration-tools/ with code in GitHub and publish as a chocolatey package that pulls from GitGub Releases.

| Team Work Items | [Azure Boards](https://dev.azure.com/nkdagility/migration-tools/) |
| Public Issues | [GitHub Issues](https://github.com/nkdAgility/azure-devops-migration-tools/) |
| Builds & Releases | [Azure Pipelines](https://dev.azure.com/nkdagility/migration-tools/) |
| Releases Output | [Github Releases](https://github.com/nkdAgility/azure-devops-migration-tools/releases) |

**Watch the [Video Overview](https://youtu.be/ZxDktQae10M) to get you started in 30 minutes. This tool is complicated and its not always easy to discover what you need to do.**

### Main Purpose

- **Bulk Update** - You can bulk update work items and apply processing rules against your server or account. Use the `WorkItemUpdate` class that takes only a target Team Project. The team does a lot of Process Template migrations and we need these tools to fix the data after the migration.
- **Migration** - You can migrate work items, area & iterations, & test data from one Team Project to another. Use the `WorkItemMigrationContext` class that takes both a Source and a Target Team Project

### Processors

There are other processors that can be used to migrate, or process, different sorts of data in different ways. Which one is right for you depends on the situation at hand.

Most of these processors need to be run in order. If you try to migrate work items before you have migrated Area and Iterations then ***bang*** you need to go back.

|Processor |Staus |Target |Usage |
|---------|---------|---------|---------|
|NodeStructuresMigrationContext | ready | Area & Iteration | Migrates Area and Iteration Paths |
|[WorkItemMigrationContext](./Processors/WorkItemMigrationConfig.md) | ready | Work Items | Migrates either tip or history of work items with Links & Attachments based on a query with field mappings |
|WorkItemRevisionReplayMigrationContext | merged |  Work Items | obsolite - merged into WorkItemMigrationContext |
|LinkMigrationContext | merged | Work Items | obsolite - merged into WorkItemMigrationContext |
|AttachementExportMigrationContext | merged | Work Items | obsolite - merged into WorkItemMigrationContext |
|AttachementImportMigrationContext | merged | Work Items | obsolite - merged into WorkItemMigrationContext |
|HtmlFieldEmbeddedImageMigrationContext | merged | HTML Fields | obsolite - merged into WorkItemMigrationContext |
|GitCommitFixContext | merged | Git links | obsolite - merged into WorkItemMigrationContext |
|WorkItemDelete | ready | Work Items | Bulk delete of work items **WARNING DANGERIOUS** |
|WorkItemUpdate | ready | Work Items | Bulk update of Work Items based on a query and field mappings |
|WorkItemQueryMigrationContext | ready | Queries | Migrates shared queries |
|TestVeriablesMigrationContext | Suits & Plans | Migrates Test Variables |
|TestConfigurationsMigrationContext | Suits & Plans | Migrates Test configurations |
|TestPlansAndSuitesMigrationContext | Suits & Plans | Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigrationContext |
|TestRunsMigrationContext | Alfa | Suits & Plans | Migrates the history of Test Runs |
|ImportProfilePictureContext & ExportProfilePictureFromADContext | Beta | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
|CreateTeamFolders | ? | ? | ? | 
|ExportTeamList | ? | ? | ? | 

### Field Maps

By default, when you are moving from source to target the system will map all of the fields that exist in source to the same field in the target. This is the behaviour if the **FieldMaps** section is not present in the configuration file.  

However sometimes you want to move data to another field, or use a regex to parse out just the bits that you want. To help we have built a number of mapping tools that should give you the versatility you need.

- **FieldtoFieldMap** - Just want to map one field to another? This is the one for you.
- **FieldtoFieldMultiMap** - Allows you to create a list of one to one field maps.
- **FieldMergeMap** - Ever wanted to merge two fields? This mapping will let you do just that.
- **FieldBlankMap** - Allows you to set an already populated field to empty
- **FieldtoTagMap** - Want to take a field and convert its value to a tag? Done...
- **FieldValueMap** - Need to map not just the field but also values? This is the default value mapper.
- **FieldValuetoTagMap** - Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
- **RegexFieldMap** - I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
- **TreeToTagMap** - Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...

##### Code (TFVC)

There are no good tools for migrating TFVC code. All of them suffer from "time-dilation" as one can't control the dates of the Check-ins. While you can use tools like TaskTop, TFS Integration Tools, or others there is no support. We prefer to recommned that you use Git-TFS and migrate from TFVC to Git with full history and branches. If your resulting repository is too large we recommend creating a full clone of TFVC for posterity, and then create a limited branch clone with limited history for work.

##### Code (Git)

When moving Git repos between Projects and Accounts you are able to maintain full history. However you will need to have all of the links to 

- **FixGitCommitLinks** - Allows you to fix the migrated Git commit hooks (and thus external links) to point to the new repository in the target project. If the source and target repository names are the same, this will work out of the box. If the target repository has a different name, you can specify that name via the "TargetRepository" property.

##### Build & Releases

When you import a build or release defenition into VSTS you need to fill out some of the data to allow that new definition to be saved. Things like connections and other require fields usually dont have matching GUIDS and need manual intervention. For builds & Releases we recommend that you use the built in Export/Import tools provided in the UI to move then to a new Team Project.

## Contributing

If you wish to contribute then feel free to fork this repository and submit a pull request. If you would like to join the team please contact.

This project is primarily managed and maintained on Visual Studio Team Services and code checked into MASTER is automatically synched between VSTS and GitHub. There is no hidden published code, but not all branches are published.

If you want to sync your GitHub repository then check out [Open-source with VSTS or TFS and Github for better DevOps
](https://nkdagility.com/open-source-vsts-tfs-github-better-devops/).

## FAQ

Check out the [FAQ pages](faq.md)

## Terms

naked Agility Limited & our contibutors creates and maintains the "VSTS Sync Migration" project under its [terms of business](https://nkdagility.com/company/consulting-terms-of-business/) and allows full access to the source code for customers and the general public. 


