# VSTS Bulk Data Editor Engine Docs

Visual Studio Team Services Bulk Data Editor Engine allows you to bulk edit data in Microsoft Team Foundation Server (TFS) and Visual Studio Team Services (VSTS). It has many names depending on what you are trying to achive. You might call it a migration tool, or a bulk update tool, and both are correct. 

## Overview

### Main Purpose

- **Bulk Update** - You can builk update work items and apply processing rules against your server or account. Use the `WorkItemUpdate` class that takes only a target Team Project. 
- **Migration** - You can migrate work items, area & iterations, & test data from one Team Project to another. Use the `WorkItemMigrationContext` calss that takes both a Source and a Target Team Project

### Field Mapps

There are for field mapping systems available:

- FieldValueMap
- FieldToTagFieldMap
- FieldToTagFieldMap
- RegexFieldMap

### Processors

There are other processors that can be used to migrate, or process, different sorts of data:

- AttachementExportMigrationContext
- AttachementImportMigrationContext
- LinkMigrationContext
- NodeStructuresMigrationContext
- TestConfigurationsMigrationContext
- TestPlansAndSuitsMigrationContext
- TestVeriablesMigrationContext
- TestRunsMigrationContext
- WorkItemMigrationContext
- ImportProfilePictureContext
- WorkItemDelete

### Processors (Beta)

- CreateTeamFolders
- ExportProfilePictureFromADContext
- ExportTeamList
- FixGitCommitLinks

## Contributing

If you wish to contribute then feel free to fork this repository and submit a pull request. If you would like to join the team please contact.

This project is primarily managed and maintained on Visual Studio Team Services and code checked into MASTER is automatically synched between VSTS and GitHub. There is no hidden published code, but not all branches are published.

If you want to sync your GitHub repository the check out [Open-source with VSTS or TFS and Github for better DevOps
](https://nkdagility.com/open-source-vsts-tfs-github-better-devops/).

## Terms

naked Agility Limited creates and maintains the "Visual Studio Team Services Bulk Data Editor Engine" project under its [terms of business](https://nkdagility.com/company/consulting-terms-of-business/) and allows full access to the source code for customers and the general public. 


