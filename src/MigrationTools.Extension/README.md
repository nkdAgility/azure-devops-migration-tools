# Azure DevOps Migration Tools [![GitHub release](https://img.shields.io/github/v/release/nkdAgility/azure-devops-migration-tools)](https://github.com/nkdAgility/azure-devops-migration-tools/releases) 

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](https://devopsmigration.io/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a Winget package a `nkdAgility.AzureDevOpsMigrationTools`.

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
**Community support is available through [GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/discussions) ; Paid support is available through our [recommended consultants](https://devopsmigration.io/support) as well as our contributors and many DevOps consultants around the world.**

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
 - [Getting Started](https://devopsmigration.io/)
 - [Documentation](https://devopsmigration.io/docs)
 - [Questions on Usage](https://stackoverflow.com/questions/tagged/azure-devops-migration-tools)
 - [Bugs and New Features](https://github.com/nkdAgility/azure-devops-migration-tools)

 ## Installing an running the tools

Head over to the [Installing Azure DevOps Migration Tools](https://devopsmigration.io/docs/setup/installation/) documentation to find out how to install and run the tools. We have options for Winget, Chocolatey, and manual installation.

We have a [Tutorial: Get started with the Azure DevOps Migration Tools](https://devopsmigration.io/docs/get-started/getting-started/) for those new to the tooling and wanting to get started quickly. 

## Support

Free [community support](https://devopsmigration.io/support) is available as well as [paid support](https://devopsmigration.io/support) from our recommended consultants.

This tool was create by [Martin Hinshelwood](https://www.linkedin.com/in/martinhinshelwood/) and is maintained by [naked Agility](https://nkdagility.com) with the help of many contributors from the community. We are a small team of Azure DevOps experts that have been working with TFS and Azure DevOps for over 20 years, and we are passionate about helping you get the most out of your Azure DevOps instance.

We do also provide [comprehensive migration services](https://nkdagility.com/capabilities/azure-devops-migration-services/) for enterprises. Find out how [we can help you](https://nkdagility.com/capabilities/azure-devops-migration-services/) with your migration and [book a free consultation](https://nkdagility.com/capabilities/azure-devops-migration-services/) to discuss how we can make things easer.

We use these tools with our customers, and for fun, to do real world migrations on a daily basis and we can:

 - Consult with your internal folks who need help and guidance in runnign the tooling.
 - Make changes to the tool to support your needs; all additions are commited to the main repo.
 - Run the migration for you:- you would need to pay for the hours that we would spend baby-sitting the running migrations