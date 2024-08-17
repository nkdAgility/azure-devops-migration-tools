---
title: Getting Started
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
---

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get started in 30 minutes. This tool is complicated and it's not always easy to discover what you need to do.

## Install

In order to run the migration you will need to install the tools first.

1. Install [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/)
1. Run `winget install nkdAgility.AzureDevOpsMigrationTools` from the [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/) (Not eleveated) to install on Windows 10 and Windows 11. For other operating systems you can download the [latest release](https://github.com/nkdAgility/azure-devops-migration-tools/releases/latest) and unzip it to a folder of your choice.

Note: The tools will be installed to `%Localappdata%\Microsoft\WinGet\Packages\nkdAgility.AzureDevOpsMigrationTools_Microsoft.Winget.Source_XXXXXXXXXX` and a symbolic link to `devopsmigration.exe` that lets you run it from anywhere using `devopsmigration init`.

**Note: There is a known issue with the winget package that it does not add the tools to the PATH if you use an elevated Terminal. You can add the tools to the PATH manually by adding `%Localappdata%\Microsoft\WinGet\Packages\nkdAgility.AzureDevOpsMigrationTools_Microsoft.Winget.Source_XXXXXXXXXX\tools` to the PATH.**

## Upgrade

1. Run `winget upgrade  nkdAgility.AzureDevOpsMigrationTools` to upgrade the tools.

## Server configuration and setup

Follow the [setup instructions](server-configuration.md) to make sure that you can run the tool against your environments and more importantly add the required custom field 'ReflectedWorkItemId'

## Create a default configuration file

1. Open your [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/) in your chosen working folder
2. Run `devopsmigration init` to create a default configuration
3. Open `configuration.json` from the current directory

You can now customize the configuration depending on what you need to do. However, a basic config that you can use to migrate from one team project to another with the same process template is:

```JSON
{% include sampleConfig/configuration.json %}
```

The default [WorkItemMigrationConfig](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) processor will perform the following operations:

* Migrate iterations and sprints
* Attachments
* Links including source code. Optionally clone the repositories before starting the migration to have links maintained on the initial pass.

## How to execute configuration.json with minimal adjustments

> Remember to add custom field ['ReflectedWorkItemId'](/docs/server-configuration.md) to only the target team project before starting migration!

> [NOTE!]
> In older versions of the tool we updated the Source work items with a link back to the Target to make migration easier. This has been removed and replaced with the `FilterWorkItemsThatAlreadyExistInTarget` option instead. **You do not need to add the reflected work item ID custom field to the Source environment.**

1. Adjust the value of the `Collection` attribute for Source and Target
2. Adjust the value of the `Project` attribute for Source and Target
3. Set the `AuthenticationMode` (`Prompt` or `AccessToken`) for Source and Target

    If you set Authentication mode to `AccessToken`, enter a valid PAT as value
    for the `PersonalAccessToken` attribute, or set the
    `PersonalAccessTokenVariableName` to the name of an environment variable containing your PAT.

4. Adjust the value of the `ReflectedWorkItemIDFieldName` attribute (field name of the migration tracking field) for Source and Target

   For example: `TfsMigrationTool.ReflectedWorkItemId` for TFS, `ReflectedWorkItemId` for VSTS, or `Custom.ReflectedWorkItemId` for Azure DevOps

5. Enable the `WorkItemMigrationConfig` processor by setting `Enabled` to `true`
6. [OPTIONAL] Modify the `WIQLQueryBit` to migrate only the work items you want. The default WIQL will migrate all open work items and revisions excluding test suites and plans
7. Adjust the [`NodeBasePaths`](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) or leave empty to migrate all nodes
8. From your working folder run `devopsmigration execute --config .\configuration.json`

**Remember:** If you want a processor to run, its `Enabled` attribute must be set to `true`. 

Refer to the [Reference Guide](/docs/Reference/index.md) for more details.

## Other Configuration Options
When running `devopsmigration init` you can also pass `--options` with one of the following:

- *Full* - 
- *WorkItemTracking* - 
- *Fullv2* - 
- *WorkItemTrackingv2* - 
