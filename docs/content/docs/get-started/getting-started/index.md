---
title: "Tutorial: Get started with the Azure DevOps Migration Tools"
description: Performs mathematical calculations on numeric fields using NCalc expressions during migration.
short_title: Get Started
weight: 1
aliases:
  - /getting-started/
date: 2025-06-24T12:07:31Z
discussionId: 2828
---

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get started in 30 minutes. This tool is complicated and it's not always easy to discover what you need to do.

## Prerequisits

1. [Install]({{< ref "docs/setup/installation" >}}) the tools using your prefered method.
2. Check that you have the required [Permissions]({{< ref "docs/setup/permissions" >}}) to run the tools.
3. Get to grips with the [Configuration]({{< ref "docs/reference" >}}) to understand how to configure the tool. (you can skip this for now and come back to it later)

## Getting Started

This is going to be a crash course and I really recommend watching [What can go wrong and what can go right with a migration via Azure DevOps](https://youtu.be/3jYFD-6_kZk?si=xxvBoljBWjGAOVuv) and then [Basic Work Item Migration with the Azure DevOps Migration Tools](https://youtu.be/Qt1Ywu_KLrc?si=uEXjLS2pwe244ugV) before you get started! This will prep you for the journey ahead.

> **New to migration tools?** Try our [Interactive Configuration Wizard]({{< ref "docs/config/config-wizard" >}}) to build your configuration step-by-step with guided explanations!

### 1. Create a default configuration file

1. Open your [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/) in your chosen working folder
2. Run `devopsmigration init --options Basic` to create a default configuration
3. Open `configuration.json` from the current directory

You can now customize the configuration depending on what you need to do. However, a basic config that you can use to migrate from one team project to another with the same process will likely look something like:

{{< include-file configuration-getstarted.json json >}}

The default [TfsWorkItemMigrationProcesor]({{< ref "docs/reference/processors/tfs-workitem-migration-processor" >}}) processor will perform the following operations:

- Migrate iterations and sprints
- Attachments
- Links including source code. Optionally clone the repositories before starting the migration to have links maintained on the initial pass.

## How to execute configuration.json with minimal adjustments

> Remember to add custom field ['ReflectedWorkItemId']({{< ref "docs/setup/reflected-workitem-id" >}}) to only the target team project before starting migration!

> [NOTE!]
> In older versions of the tool we updated the Source work items with a link back to the Target to make migration easier. This has been removed and replaced with the `FilterWorkItemsThatAlreadyExistInTarget` option instead. **You do not need to add the reflected work item ID custom field to the Source environment.**

1. Adjust the value of the `Collection` attribute for Source and Target
2. Adjust the value of the `Project` attribute for Source and Target
3. Set the `AuthenticationMode` (`Prompt` or `AccessToken`) for Source and Target

   If you set Authentication mode to `AccessToken`, enter a valid PAT as value
   for the `PersonalAccessToken` attribute, or set the
   `PersonalAccessTokenVariableName` to the name of an environment variable containing your PAT.

4. Adjust the value of the `ReflectedWorkItemIdField` attribute (field name of the migration tracking field) for Source and Target

   For example: `TfsMigrationTool.ReflectedWorkItemId` for TFS, `ReflectedWorkItemId` for VSTS, or `Custom.ReflectedWorkItemId` for Azure DevOps

5. Enable the `WorkItemMigrationConfig` processor by setting `Enabled` to `true`
6. [OPTIONAL] Modify the `WIQLQueryBit` to migrate only the work items you want. The default WIQL will migrate all open work items and revisions excluding test suites and plans
7. Adjust the [`NodeBasePaths`]({{< ref "docs/reference/processors/workitem-tracking-processor" >}}) or leave empty to migrate all nodes
8. From your working folder run `devopsmigration execute --config .\configuration.json`

### Common Issue: Work Item Type Validation Failures

> **⚠️ IMPORTANT:** The migration tools include automatic validation that runs before migration starts and can cause your migration to fail if source and target systems have different work item types or fields.

The [TfsWorkItemTypeValidatorTool]({{< ref "docs/reference/tools/tfsworkitemtypevalidatortool" >}}) automatically validates that:

- All source work item types exist in the target system (or have mappings configured)
- Required fields exist in target work item types
- Field types are compatible between source and target
- The ReflectedWorkItemId field exists in target work item types

**If validation fails**, the migration will stop immediately with detailed error messages. Here's how to resolve common validation issues:

**Missing Work Item Types:**

```text
Work item type 'Risk' does not exist in target system.
```

- **Solution 1:** Create the missing work item type in your target system
- **Solution 2:** Configure a work item type mapping to map 'Risk' to an existing type like 'Issue'
- **Solution 3:** Exclude the work item type from your migration query

**Missing ReflectedWorkItemId Field:**

```text
'User Story' does not contain reflected work item ID field 'Custom.ReflectedWorkItemId'.
```

- **Solution:** Add the custom field to ALL work item types in your target project (see [ReflectedWorkItemId setup]({{< ref "docs/setup/reflected-workitem-id" >}}))

**Missing Custom Fields:**

```text
Missing field 'Microsoft.VSTS.Common.CustomField' in 'User Story'.
```

- **Solution 1:** Add the missing field to the target work item type
- **Solution 2:** Configure field mapping to map to an existing field
- **Solution 3:** Exclude the field from validation if it's not needed

For detailed troubleshooting and configuration options, see the [TfsWorkItemTypeValidatorTool documentation]({{< ref "docs/reference/tools/tfsworkitemtypevalidatortool" >}}).

**Remember:** If you want a processor to run, its `Enabled` attribute must be set to `true`.

Refer to the [Reference Guide]({{< ref "docs/reference" >}}) for more details.

## Other Configuration Options

When running `devopsmigration init` you can also pass `--options` with one of the following:

- _Full_ -
- _WorkItemTracking_ -
- _Fullv2_ -
- _WorkItemTrackingv2_ -
