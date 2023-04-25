---
title: Getting Started
layout: default
pageType: index
template: <template>
toc: true
pageStatus: production
discussionId: 
redirect_from: /getting-started.html
---

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get you started in 30 minutes. This tool is complicated and it's not always easy to discover what you need to do.

## Install

In order to run the migration you will need to install the tools first.

1. Install Chocolatey from [https://chocolatey.org/install](https://chocolatey.org/install)
1. Run `choco install vsts-sync-migrator` to install the tools [source](https://chocolatey.org/packages/vsts-sync-migrator)

The tools are now installed. To run them you will need to switch to `c:\tools\MigrationTools\` and run `migration.exe`.

## Upgrade

1. Run `choco upgrade  vsts-sync-migrator` to upgrade the tools [source](https://chocolatey.org/packages/vsts-sync-migrator)

## Server configuration and setup

Follow the [setup instructions](/docs/server-configuration.md) to make sure that you can run the tool against your environments and importantly add the required custom field 'ReflectedWorkItemId'

## Create a default configuration file

1. Open a command prompt or PowerShell window at `C:\tools\MigrationTools\`
2. Run `./migration.exe init` to create a default configuration
3. Open `configuration.json` from the current directory

You can now customise the configuration depending on what you need to do. However a basic config that you can use to migrate from one team project to another with the same process template is:

```JSON
<Import:Reference/Generated/configuration.config>
```

The default [WorkItemMigrationConfig](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) processor will perform the following operations:

* Migrate interations and sprints
* Attachments
* Links including for source code. Optionally clone the repositories before starting the migration to have links maintained on the initial pass.

## How to execute configuration.json with minimal adjustments

> Remember to add custom field ['ReflectedWorkItemId'](/docs/server-configuration.md) to both, the source and the target team project before starting migration!

1. Adjust the value of the `Collection` attribute for Source and Target
1. Adjust the value of the `Project` attribute for Source and Target
1. Set the `AuthenticationMode` (`Prompt` or `AccessToken`) for Source and Target

    If you set Authentication mode to `AccessToken`, enter a valid PAT as value
    for the `PersonalAccessToken` attribute, or set the
    `PersonalAccessTokenVariableName` to the name of an environment variable containing your PAT.

1. Adjust the value of the `ReflectedWorkItemIDFieldName` attribute (field name of the migration tracking field) for Source and Target

   For example: `TfsMigrationTool.ReflectedWorkItemId` for TFS, `ReflectedWorkItemId` for VSTS or `Custom.ReflectedWorkItemId` for Azure DevOps

1. Enable the `WorkItemMigrationConfig` processor by setting `Enabled` to `true`
1. [OPTIONAL] Modify the `WIQLQueryBit` to migrate only the work items you want. The default WIQL will migrate all open work items and revisions excluding test suites and plans
1. Adjust the [`NodeBasePaths`](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) or leave empty to migrate all nodes
1. From the `C:\tools\MigrationTools\` path run `.\migration.exe execute --config .\configuration.json`

**Remember:** if you want a processor to run, it's `Enabled` attribute must be set to `true`. 

Refer to the [Reference Guide](/docs/reference/index.md) for more details.

