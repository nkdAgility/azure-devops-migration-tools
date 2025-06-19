# Azure DevOps Migration Tools [![GitHub release](https://img.shields.io/github/v/release/nkdAgility/azure-devops-migration-tools)](https://github.com/nkdAgility/azure-devops-migration-tools/releases) ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/94/badge) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](https://nkdagility.com/docs/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a Winget package a `nkdAgility.AzureDevOpsMigrationTools`.

**Ask Questions on Github: https://github.com/nkdAgility/azure-devops-migration-tools/discussions**

## Some Data from the last 30 days (as of 01/04/2022)

| Catagory  | Metric | Notes |
| ------------- | ------------- | ------------- |
| Work Item Revisions | **14m** | A single Work Item may have many revisions that we need to migrate |
| Average Work item Migration Time  | **35s** | Work Item (inlcudes all revisions, links, and attachements for the work item) |
| Git Commit Links  | **480k** |  |
| Attachments | **252.37k**  | Total number of attachments migrated |
| Migration Run Ave  | **14 minutes** | Includes dryruns as well.  |
| Migration Run Total   |  **19bn Seconds** |  |


## What can you do with this tool?

- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, `Pipelines`, & `Processes` from one `Team Project` to another
- Migrate `Work Items`, `TestPlans & Suits`, `Teams`, `Shared Queries`, `Pipelines`, & `Processes` from one `Organisation` to another
- Bulk edit of `Work Items` accross an entire `Project`.

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
- Migrate from one Language version of TFS / Azure Devops to another (*new v9.0*)
- _new_  Migration of Processes

**NOTE: If you are able to migrate your entire Collection to Azure DevOps Services you should use [Azure DevOps Migration Service](https://azure.microsoft.com/services/devops/migrate/) from Microsoft. If you have a requirement to change Process Template then you will need to do that before you move to Azure DevOps Services.**

## Quick Links

 - [Video Overview](https://www.youtube.com/watch?v=RCJsST0xBCE)
 - [Getting Started](https://devopsmigration.io/getting-started.html)
 - [Documentation](https://nkdagility.com/docs/azure-devops-migration-tools/)
 - [Questions on Usage](https://stackoverflow.com/questions/tagged/azure-devops-migration-tools)
 - [Bugs and New Features](https://github.com/nkdAgility/azure-devops-migration-tools)

 ## Installing an running the tools

We use [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/) to host the tools, and you can use the command `winget install nkdAgility.AzureDevOpsMigrationTools` to install them on Windows 10 and Windows 11. For other operating systems you can download the [latest release](https://github.com/nkdAgility/azure-devops-migration-tools/releases/latest) and unzip it to a folder of your choice.

The tools will be installed to `%Localappdata%\Microsoft\WinGet\Packages\nkdAgility.AzureDevOpsMigrationTools_Microsoft.Winget.Source_XXXXXXXXXX` and a symbolic link to `devopsmigration.exe` that lets you run it from anywhere using `devopsmigration init`.

For a more detailed getting started guide please see the [documentation](https://nkdagility.com/docs/azure-devops-migration-tools/getting-started.html).

## Support

1. [Question & Discussion](https://github.com/nkdAgility/azure-devops-migration-tools/discussions) - The first place to look for unsage, configuration, and general help. 
1. [Issues on Gitbub](https://github.com/nkdAgility/azure-devops-migration-tools/issues) - If you have identified a bug and have logs then please raise an issue.

### Professional Support

You can get free support from the community above and on social media on a best effort basis if folks are available. If you are *looking for paid support* [naked Agility with Martin Hinshelwood & Co](https://nkdagility.com) has a number of experts, many of whom contribute to this project, that can help. Find out how [we can help you](https://nkdagility.com/technical-consulting-and-coaching/azure-devops-migration-tools-consulting/) with your migration and [book a free consultation](https://nkdagility.com/technical-consulting-and-coaching/azure-devops-migration-tools-consulting/) to discuss how we can make things easyer.

We use these tools with our customers, and for fun, to do real world migrations on a daily basis and we can:

 - Consult with your internal folks who need help and guidance in runnign the tooling.
 - Make changes to the tool to support your needs; all additions are commited to the main repo.
 - Run the migration for you:- you would need to pay for the hours that we would spend baby-sitting the running migrations