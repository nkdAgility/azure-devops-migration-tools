---
title: "Tutorial: Get started with the Azure DevOps Migration Tools"
description: Learn how to get started with the Azure DevOps Migration Tools with step-by-step instructions and configuration guidance.
short_title: Get Started
weight: 1
aliases:
  - /getting-started/
date: 2025-06-24T12:07:31Z
discussionId: 2828
---

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get started in 30 minutes. This tool is complicated and it's not always easy to discover what you need to do.

## Prerequisites

1. [Install]({{< ref "docs/setup/installation" >}}) the tools using your preferred method.
2. Check that you have the required [Permissions]({{< ref "docs/setup/permissions" >}}) to run the tools.
3. Get to grips with the [Configuration]({{< ref "docs/reference" >}}) to understand how to configure the tool. (you can skip this for now and come back to it later)


## Validation Process

> **Before you begin migration, review the [Validation Process documentation]({{< ref "docs/get-started/validation" >}}) to understand how the tools automatically check your configuration and data for common issues. This can save you significant time and help avoid common migration errors.**

## Getting Started

This is going to be a crash course and I really recommend watching [What can go wrong and what can go right with a migration via Azure DevOps](https://youtu.be/3jYFD-6_kZk?si=xxvBoljBWjGAOVuv) and then [Basic Work Item Migration with the Azure DevOps Migration Tools](https://youtu.be/Qt1Ywu_KLrc?si=uEXjLS2pwe244ugV) before you get started! This will prep you for the journey ahead.

### 1. Create a default configuration file

1. Open your [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/) in your chosen working folder
2. Run `devopsmigration init --template Basic` to create a default configuration
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
3. Set the `AuthenticationMode` (`Prompt`, `AccessToken` or `EntraId`) for Source and Target

   If you set Authentication mode to `AccessToken`, enter a valid PAT as value
   for the `PersonalAccessToken` attribute, or set the
   `PersonalAccessTokenVariableName` to the name of an environment variable containing your PAT.

   If you set Authentication mode to `EntraId`, no token goes in the configuration. The tool signs in as
   the identity you are already signed in with: `az login`, Visual Studio, a managed identity, or
   workload identity federation in CI. That identity must be a member of the organisation, and for the
   TFS Object Model endpoints a Project Collection Administrator.

   At startup the log shows which credential supplied the token (for example `AzureCliCredential`) and
   the identity, tenant and validity window it is for; the token itself is never logged. If the first
   connection is rejected, the tool retries it twice with a new token before stopping. If it still
   fails, check that the logged identity is the account you expect: an earlier source in the chain,
   such as Visual Studio, may be signed in as a different account.

   To use only one sign-in source, set the `AZURE_TOKEN_CREDENTIALS` environment variable to its
   credential name before running the tool, for example `AzureCliCredential` to use only your `az login`,
   or `ManagedIdentityCredential` on an Azure VM. `dev` and `prod` limit the chain to developer tools or
   deployed-service credentials. This is recommended for unattended runs. The log shows the value in use.

4. Adjust the value of the `ReflectedWorkItemIdField` attribute (field name of the migration tracking field) for Source and Target

   For example: `TfsMigrationTool.ReflectedWorkItemId` for TFS, `ReflectedWorkItemId` for VSTS, or `Custom.ReflectedWorkItemId` for Azure DevOps

5. Enable the `TfsWorkItemMigrationProcessor` processor by setting `Enabled` to `true`, and keep its `SourceName` and `TargetName` set to the names of your endpoints (`Source` and `Target`). Every processor requires both, and the tool will not start without them
6. [OPTIONAL] Modify the `WIQLQueryBit` to migrate only the work items you want. The default WIQL will migrate all open work items and revisions excluding test suites and plans
7. Adjust the [`NodeBasePaths`]({{< ref "docs/reference/processors/workitem-tracking-processor" >}}) or leave empty to migrate all nodes
8. From your working folder run `devopsmigration execute --config .\configuration.json`

There is no dry-run or `-validate` option: `execute` always migrates. The work item type validation described below runs at the start of `TfsWorkItemMigrationProcessor` and stops that processor before it writes anything if it fails. It does not protect processors listed before it, which run first and can write to the target (for example `TfsTestVariablesMigrationProcessor` creates test variables). Not every check in `TfsWorkItemMigrationProcessor` runs that early, either: the area and iteration tool (`TfsNodeStructureTool`, which by default replicates every existing node) and the team settings tool write to the target before the query runs, and the node and user checks run after the query. To try a configuration safely, point the target at a scratch project. A narrow `WIQLQuery` (for example `[System.Id] IN (101, 102)`) limits which work items are migrated, but not those area, iteration and team changes. `TfsWorkItemMigrationProcessor` does not read `WorkItemIDs`, so that option does not limit a run.

### Where the logs are written

Each run writes `migration*.log` and `migration-errors*.log` to `logs\<timestamp>` under the folder you run the tool from, not the folder the tool is installed in. The full path is printed at the start of the run as `Logpath:` and again at the end as `Check the logs for errors:`. To write them somewhere else, pass `--logPath <folder>`; the `logs\<timestamp>` folder is created inside it. If the folder cannot be written to, the logs go to `%LOCALAPPDATA%\MigrationTools\logs\<timestamp>` instead. The work item migration also writes `work-items-to-review.jsonl` there as it goes, listing every work item that needs a person to look at it; see [Work items that need review](../../reference/processors/tfs-workitem-migration-processor/#work-items-that-need-review).

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

- **Solution:** Add the custom field to every work item type you can customise in your target project (see [ReflectedWorkItemId setup]({{< ref "docs/setup/reflected-workitem-id" >}})). Shared Steps, Shared Parameter, Code Review Request/Response and Feedback Request/Response can't be customised on an inherited process; for them the migration uses `Microsoft.VSTS.Build.IntegrationBuild` automatically.

**Missing Custom Fields:**

```text
Missing field 'Microsoft.VSTS.Common.CustomField' in 'User Story'.
```

- **Solution 1:** Add the missing field to the target work item type
- **Solution 2:** Configure field mapping to map to an existing field
- **Solution 3:** Exclude the field from validation if it's not needed


### Common Issue: "Unable to load Common Structure"

```text
Unable to load Common Structure for Target. This is usually due to TFS having a different installed language version than was expected.
```

The tool looks for the root area and iteration nodes by the names in the endpoint's `LanguageMaps` (`AreaPath` and `IterationPath`, `Area` and `Iteration` by default). If your server uses another language, set them to the names shown at the top of the area and iteration trees, for example `Fläche` or `Aire`. Don't put a specific area or iteration path there. See the [TFS Team Project Endpoint]({{< ref "docs/reference/endpoints/tfs-team-project-endpoint" >}}). The same message for the Source with "more than one node path matches" means more than one root node starts with the project name and `AreaPath`.

For a complete overview of the validation process—including both pre-migration and pre-data-load validation steps—see the [Validation Process documentation]({{< ref "docs/get-started/validation" >}}).

For detailed troubleshooting and configuration options, see the [TfsWorkItemTypeValidatorTool documentation]({{< ref "docs/reference/tools/tfsworkitemtypevalidatortool" >}}).

**Remember:** If you want a processor to run, its `Enabled` attribute must be set to `true`.

Refer to the [Reference Guide]({{< ref "docs/reference" >}}) for more details.

## Other Configuration Options

When running `devopsmigration init` you can pass `--template` (or its short form `-t`) with one of the following. `--options` and `--outputMode` still work as aliases. Add `--overwrite` to replace an existing `configuration.json`.

- _WorkItemTracking_ - the default. A `TfsWorkItemMigrationProcessor`, the `FieldMappingTool` with a `FieldLiteralMap`, and `Source` and `Target` `TfsTeamProjectEndpoint` endpoints.
- _Basic_ - currently produces the same configuration as _WorkItemTracking_.
- _PipelineProcessor_ - an `AzureDevOpsPipelineProcessor` with `PipelineSource` and `PipelineTarget` `AzureDevOpsEndpoint` endpoints.
- _Reference_ - every processor, tool, field map and endpoint, filled with sample values. The values are examples, not the defaults the tool uses at run time. Use it as a reference, not as a configuration to run.

Every template fills its options from the samples, so check each value rather than assuming it is the default.
