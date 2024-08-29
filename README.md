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

The Azure DevOps Migration Tools allow you to bulk edit and migrate data between Team Projects on both Microsoft Team Foundation Server (TFS) and Azure DevOps Services. Take a look at the  [documentation](https://nkdagility.com/docs/azure-devops-migration-tools/) to find out how. This project is published as [code on GitHub](https://github.com/nkdAgility/azure-devops-migration-tools/) as well as a Winget package a `nkdAgility.AzureDevOpsMigrationTools`.

**Ask Questions on Github: https://github.com/nkdAgility/azure-devops-migration-tools/discussions**

## Some Data from the last 30 days (as of 05/03/2024)

| Category  | Metric | Notes |
| ------------- | ------------- | ------------- |
| Work Items | **1m** | A single Work Item may have many revisions that we need to migrate |
| Work Item Revisions | **23m** | A single Work Item may have many revisions that we need to migrate |
| RelatedLinkCount | **11m** | Each work item may have many links or none. |
| Git Commit Links  | **1.3m** |  |
| Attachments | **1.2m**  | Total number of attachments migrated |
| Test Suits | 52k | total suits migrated | 
| Test Cases Mapped | **1.4m** | Total test cases mapped into Suits |
| Migration Run Ave  | **14 minutes** | Includes dry-runs as well.  |
| Migration Run Total   |  **19bn Seconds** | Thats **316m hours** or **13m days** of run time in the last 30 days. |
| Average Work item Migration Time  | **22s** | Work Item (includes all revisions, links, and attachments for the work item) |

Exceptions shipped to Application Insights and [Elmah.io](https://elmah.io) for analysis and improvement.

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

- [Documenation](https://nkdagility.com/docs/azure-devops-migration-tools/)] 
- [Installation](https://nkdagility.com/learn/azure-devops-migration-tools/installation/)
- [Getting Started](https://nkdagility.com/learn/azure-devops-migration-tools/getting-started/)
- [Reference](https://nkdagility.com/learn/azure-devops-migration-tools/Reference/)
- [Community Support](https://github.com/nkdAgility/azure-devops-migration-tools/discussions)
- [Commercial Support](https://nkdagility.com/capabilities/azure-devops-migration-services/)

The documentation for the preview is on [Preview](https://nkdagility.com/docs/azure-devops-migration-tools/preview/)]

## Minimum Permission Requirements

At this time the documented minimum required permissions for running the tools are:

- Account in both the source and target projects with "Project Collection Administrator" rights
- PAT with "full access" for both the Source and the Target

Note: I have been informed by the Azure DevOps product team information that ObjectModel API only works with full scoped PATs, so it won't work with any PAT that has specific scopes. 

### Advanced Unsupported Permission Options

We have seen that the tools may work with less permissions however the following has not been full tested and is not currently supported:

- Project and Team (Read, write, & manage)
- Work Items (Read, Write & Manage)
- Identity (Read & Manage)
- Security (Manage)

If you do try this out then please let us know how you get on!

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

## Change Log

- [v16.0.0](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v16.0.0) - Enhanced configuration with support for command line, and environmental variable overrides.
- [v15.1.7](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v15.1.7) - The latest release brings a host of enhancements and fixes designed to improve user experience and configuration options. Noteworthy features include a new GitHub Actions workflow for automatic updates to pull request titles, enhanced management of area and iteration paths using regex mapping, and a more streamlined query format for migration configurations. Users can now enjoy greater flexibility in configuring attachment processing, including options for export paths and size limits. Additionally, updates to authentication methods and improved logging for user retrieval processes have been implemented. The release also addresses various bugs and makes adjustments to enhance overall functionality.
- [v14.4.7](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v14.4.7) - The latest major release brings a host of user-focused enhancements and improvements. Key changes include the adoption of Winget as the primary installation method, making it easier for users to get started. The main executable has been renamed to `devopsmigration.exe`, and new configuration options enhance customization capabilities, including parallel builds and test case timeouts. The command for initializing configurations has been updated for greater flexibility, and logging improvements provide better insights during migration operations. Subsequent updates have refined version detection, improved command line arguments, and introduced new configuration files to prevent blank issues. Enhanced logging and error handling further improve user experience, while package upgrades and better handling of specific fields streamline migration processes. Overall, these updates aim to enhance functionality, usability, and reliability for users.
- [v13.2.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v13.2.1) - The latest updates bring a range of enhancements and new features aimed at improving user experience and functionality. A key addition is the `WorkItemMigrationContext` processor, which facilitates the migration of work items, including their history and attachments, between Azure DevOps instances. Users will find clearer documentation and a new configuration file to simplify work item type and field mappings. The introduction of the `ExportUsersForMapping` feature allows for easy JSON file exports for field mapping, while security is bolstered with an updated authentication mode. Users can now disable telemetry collection during migration, and various improvements have been made to migration behavior and configuration settings, enhancing the overall robustness and integrity of the migration tools.
- [v12.8.10](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v12.8.10) - The latest major release brings a host of enhancements designed to improve user experience and streamline migration processes. New configuration options for migration processors offer greater flexibility, allowing users to define custom remapping rules for area and iteration paths. Significant improvements in field mapping configurations and enhanced documentation provide clearer guidance for users. The introduction of features like case-insensitive matching for regular expressions and new parameters for work item migration enhances functionality. Additionally, updates to logging, error handling, and overall documentation structure ensure a more robust and user-friendly experience. Various bug fixes further contribute to the reliability and clarity of the migration tools, making the overall process smoother for users.
- [v11.9.55](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v11.9.55) - The latest major release introduces a variety of impactful changes designed to enhance user experience and streamline migration processes. Key features include a rebranding of the project to "MigrationTools," improved configuration options, and enhanced error handling for migration operations. Users can now limit revisions during work item migrations, customize field retrieval, and benefit from new logging capabilities for better traceability. The introduction of new interfaces and methods, along with refined documentation, supports improved work item management and configuration flexibility. Overall, these updates aim to provide a more efficient, user-friendly migration experience while addressing previous bugs and enhancing system performance.
- [v10.2.13](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v10.2.13) - The latest updates to the Migration Tools suite introduce a range of impactful enhancements for users. New projects, such as "MigrationTools.Sinks.AzureDevOps," have been added, along with a revamped console UI for improved Azure DevOps integration. Configuration management has been enhanced, allowing for easier JSON file loading and new telemetry settings. The migration engine has been optimized for better work item handling, and logging has been clarified. Users will need to update their configuration files due to a namespace change and new parameters for work item migration. Subsequent updates further simplify the configuration process, improve field mapping options, and enhance documentation for migrating test artifacts. Overall, these changes provide users with greater flexibility, control, and usability in their migration tasks.
- [v9.3.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v9.3.1) - The latest major release brings a host of user-focused enhancements and improvements. Key features include multi-language support for Azure DevOps migrations, allowing for greater flexibility in handling different language versions. Users will benefit from improved configuration documentation, which now includes new fields for language mapping of Area and Iteration paths. Subsequent updates have introduced customizable field mappings, conditional logic for excluding specific work item types, and enhanced error handling for better troubleshooting. Additionally, logging capabilities have been significantly upgraded, providing more structured output and insights into application performance. Overall, these changes aim to streamline the migration process and improve user experience.
- [v8.9.10](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/v8.9.10) - The latest major release introduces a range of impactful enhancements and features designed to improve user experience and streamline migration processes. Users can now take advantage of enhanced configuration options, including custom paths for configuration files and new modes for the `init` command. The migration process has been significantly refined with improved error handling, better logging, and new parameters for managing attachments and links. Notable features include the ability to sync changes post-migration, retry failed work item saves, and customize attachment handling. Additionally, the rebranding of the tool ensures users have access to accurate documentation and resources. Overall, these updates focus on providing greater control, efficiency, and clarity throughout the migration experience.
- [7.5.74](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/7.5.74) - The latest major release brings a host of user-focused enhancements and features designed to improve performance and usability. Key updates include a framework upgrade that boosts application performance and compatibility, alongside dependency updates for improved functionality and security. New configuration options allow for greater flexibility during data migration, including filtering elements by tags and replaying work item revisions. Enhancements to error handling and logging, as well as improvements in attachment management, contribute to a more reliable user experience. Additionally, the introduction of online status checks for version updates ensures users stay informed about the latest changes while connected to the internet.
- [6.3.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/6.3.1) - The latest major release brings a host of impactful changes for users. A key highlight is the rebranding of the command-line tool to `vstssyncmigrator`, accompanied by updated documentation to assist with the new command structure. Enhancements to attachment export and import migration contexts improve ID formatting, while the restructuring of project organization may necessitate updates to project references. Users will also benefit from improved global configuration documentation, with various processors now enabled by default for immediate functionality. New features include the `WorkItemQueryMigrationContext`, allowing for selective migration of work item queries, and the option to prefix project names in folder paths for better organization. Enhanced logging and an updated FAQ section further support users in managing their migration processes effectively.
- [5.3.2](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/5.3.2) - The latest major release brings a host of impactful changes designed to enhance user experience and functionality. Key updates include a rebranding to "VSTS Sync Migration Tools" and a simplified command name for installation and uninstallation. Users can now benefit from the new `MultiValueConditionalMapConfig` class, which allows for more complex field mapping configurations. Version 5.1 introduces customizable user preferences and improved command-line functionality, while 5.3 enhances the migration process for test plans with a new method for handling test cases, updates to installation scripts for better package verification, and optimizations in field merging. Comprehensive new documentation supports these changes, ensuring users can easily adapt to the updated features and configurations.
- [4.4.0](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/4.4.0) - The latest major release introduces a range of impactful enhancements aimed at improving user experience and flexibility during work item migration. A key feature is the new configuration option, `PrefixProjectToNodes`, which allows users to customize the prefixing of project names to area and iteration paths, as well as nodes, enhancing project structure management. The migration logic has been updated to support these options, streamlining the migration process. Additionally, users will benefit from improved documentation, including clearer installation instructions and new JSON configuration files. The release also includes an uninstall script for easier tool management and enhancements to caching and field exclusion during migrations, further refining the overall functionality.
- [3.6.0.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/3.6.0.1) - The latest updates bring a host of enhancements designed to improve user experience and functionality. Key features include new configuration files for the VSTS Data Bulk Editor, allowing for tailored migration processes, and the introduction of classes for managing team settings. Users can now specify additional query criteria for attachment exports and customize employee picture URL formats. The migration process has been streamlined with improved telemetry tracking and error handling, while new documentation provides valuable context and guidance. Significant improvements to the TfsWitMigrator tool enhance work item tagging flexibility, and updates to the migration context for test plans and variables offer greater control during migrations. Overall, these changes aim to make data migration more efficient and user-friendly.
- [0.5.1](https://github.com/nkdAgility/azure-devops-migration-tools/releases/tag/0.5.1) - The latest update brings a range of enhancements designed to improve user experience. Users will benefit from increased performance and stability, alongside new features that simplify interactions. Additionally, numerous bugs identified in earlier releases have been resolved, contributing to a more seamless and dependable operation. This update focuses on creating a more efficient and user-friendly environment for all.

