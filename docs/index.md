# VSTS Sync Migration Tools Docs 

VSTS Sync Migration Tools allow you to bulk edit data in Microsoft Team Foundation Server (TFS) and Visual Studio Team Services (VSTS). It has many names depending on what you are trying to achieve. You might call it a migration tool, or a bulk update tool, and both are correct.

## Getting the Tools

There are three ways to use these tools:

- [Install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/) **(recomended)**
- Manually download the [latest release from GitHub](https://github.com/nkdAgility/vsts-sync-migration/releases) and unzip
- Create your own tools by using the [VSTS Bulk Data Editor Engine](https://www.nuget.org/packages/VstsSyncMigrator.Engine/) on nuget

## Overview

These tools are build by naked Agility Limited's DevOps & Agility consultants to do real world migrations on a daily basis. We always work in [Visual Studio Team Services](http://tfs.visualstudio.com) and sync the code to GitHub for visibility. We use Work Item Tracking and a Git Repository inside of TFS to manage the work and Team Build to create our package output. We then have a Release Pipeline in Release Management to publish a [GitHub Release](https://github.com/nkdAgility/vsts-sync-migration/releases) and, if successful, automatically deploy a new version to NuGet and to Chocolatey.

### Main Purpose

- **Bulk Update** - You can bulk update work items and apply processing rules against your server or account. Use the `WorkItemUpdate` class that takes only a target Team Project. The team does a lot of Process Template migrations and we need these tools to fix the data after the migration.
- **Migration** - You can migrate work items, area & iterations, & test data from one Team Project to another. Use the `WorkItemMigrationContext` class that takes both a Source and a Target Team Project

### Field Maps

By default, when you are moving from source to target the system will map all of the fields that exist in source to the same field in the target. This is the behaviour if the **FieldMaps** section is not present in the configuration file.  

However sometimes you want to move data to another field, or use a regex to parse out just the bits that you want. To help we have built a number of mapping tools that should give you the versatility you need.

- **FieldtoFieldMap** - Just want to map one field to another? This is the one for you.
- **FieldMergeMap** - Ever wanted to merge two fields? This mapping will let you do just that.
- **FieldBlankMap** - Allows you to set an already populated field to empty
- **FieldtoTagMap** - Want to take a field and convert its value to a tag? Done...
- **FieldValueMap** - Need to map not just the field but also values? This is the default value mapper.
- **FieldValuetoTagMap** - Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
- **RegexFieldMap** - I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
- **TreeToTagMap** - Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...

### Processors

There are other processors that can be used to migrate, or process, different sorts of data in different ways. Which one is right for you depends on the situation at hand.

#### In-Place Processors

- **WorkItemUpdate** - Need to just update work items in place, use this and only set the Target. All field mappings work...
- **WorkItemDelete** - Woops... Can I just start again? Feed this a query and watch those items vanish ***WARNING***

#### Migrators

Most of these processors need to be run in order. If you try to migrate work items before you have migrated Area and Iterations then ***bang*** you need to go back.

##### Work Items
1. **NodeStructuresMigrationContext** - Moves over the structure of area and iteration paths, you have the option to inject a new root node by setting the `PrefixProjectToNodes` property. 
1. **WorkItemMigrationContext** - **The work horse...** push the tip from one location to another while maintaining context. Make sure that you add a ReflectedWorkItemID and you can restart the service at any time...
1. **AttachementExportMigrationContext** - Exports all work items attachments to the migration machine. This is used in partnership with the **AttachmentImportMigrationContext**   
1. **AttachementImportMigrationContext** - Imports all work items attachments from the migration machine. This is used in partnership with the **AttachementExportMigrationContext**
1. **LinkMigrationContext** - Migrates all the work item links, both between work items and external links.
1. **WorkItemQueryMigrationContext** - Migrate all shared work item queries

##### Test Plans & Suites

1. **TestVeriablesMigrationContext** - Migrates test variables
1. **TestConfigurationsMigrationContext** - Migrate test configurations
1. **TestPlansAndSuitsMigrationContext** - Migrate test plans and suites, this does assume that the actual test cases and shared steps are migrated using the **WorkItemMigrationContext**
1. **TestRunsMigrationContext** [BETA] - Migrates past test run results

##### Misc

The following misc processors do as their names suggest

- **ImportProfilePictureContext** 
- **ExportProfilePictureFromADContext**
- **WorkItemDelete**
- **FixGitCommitLinks**
- **CreateTeamFolders**
- **ExportTeamList**

## Contributing

If you wish to contribute then feel free to fork this repository and submit a pull request. If you would like to join the team please contact.

This project is primarily managed and maintained on Visual Studio Team Services and code checked into MASTER is automatically synched between VSTS and GitHub. There is no hidden published code, but not all branches are published.

If you want to sync your GitHub repository then check out [Open-source with VSTS or TFS and Github for better DevOps
](https://nkdagility.com/open-source-vsts-tfs-github-better-devops/).

## FAQ

Check out the [FAQ pages](faq.md)

## Terms

naked Agility Limited creates and maintains the "Visual Studio Team Services Bulk Data Editor Engine" project under its [terms of business](https://nkdagility.com/company/consulting-terms-of-business/) and allows full access to the source code for customers and the general public. 


