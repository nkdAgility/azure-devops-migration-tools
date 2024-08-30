---
title: ReflectedWorkItemId
layout: page
pageType: index
---

The Azure DevOps migrations Tools has no internal state, and uses a field on the work item to track the migration of work items. This field is always referd to in the docs as `ReflectedWorkItemId` and is used to track the work item in the target. It enables the ability to resume migrations as well as to be able to scope the work items based on a query and have multiple runs overlap.

## How to add the ReflectedWorkItemId

To add the `ReflectedWorkItemId` to your target project you can use the follow the [Add a custom field to a work item type (Inheritance process)](https://learn.microsoft.com/en-us/azure/devops/organizations/settings/work/add-custom-field?view=azure-devops) documentation from Microsoft. If you are on the older XML process you can follow the [Add a custom field to a work item type (On-premises XML process)](https://learn.microsoft.com/en-us/azure/devops/organizations/settings/work/import-process/customize-process?view=azure-devopss) documentation.

Note: We can [help you get off those horible legacy XML Process](https://nkdagility.com/capabilities/azure-devops-migration-services/).

## How to use the ReflectedWorkItemId

In your configuration file under `MigrationTools:Endpoints` there willbe both a `Source` and a `Target` endpoint. On the `Target` endpoint there should be a property called `ReflectedWorkItemID*` (depending on the specific endpoint implmnetation) that will have a property value like `Custom.ReflectedWorkItemId`. This is the field that the tool will use to track the work items in the target.

```json
{
  "Endpoints": {
	"Source": {
	  "Collection": "https://dev.azure.com/nkdagility-preview/",
	  "Project": "MigrationTest",
	  "ReflectedWorkItemIDFieldName": "Custom.ReflectedWorkItemId",
	  "AllowCrossProjectLinking": false,
	  "AuthenticationMode": "Prompt",
	  "PersonalAccessToken": ""
	},
	"Target": {
	  "Collection": "https://dev.azure.com/nkdagility-preview/",
	  "Project": "MigrationTest",
	  "ReflectedWorkItemIDFieldName": "Custom.ReflectedWorkItemId",
	  "AllowCrossProjectLinking": false,
	  "AuthenticationMode": "Prompt",
	  "PersonalAccessToken": ""
	}
  }
}
```

When you create the field you will be able to see the`RefName` (diferent from the display name) in the field settings. This is the value that you will use in the configuration file. It will always have at least one `.` in the name. On the inherited processes it will be `Custom.ReflectedWorkItemId` (unless you created your process and added the field many moons ago, inwhich case it will be `processName.ReflectedWorkItemId`). On the XML process it will be whatever you want to call it But I recommned something like `TfsMigrationTool.ReflectedWorkItemId` or just `ReflectedWorkItemId`.

## Notes for Work Items you cant Customise!

If you need to migratae work items that you cant customsie, then you will need to use one of the built in fields and I recommned `Microsoft.VSTS.Build.IntegrationBuild`. This field is only used by builds, and is realitively safe to use. This is primerally of concern for [How-to: Migrating Plans and Suits](_howto/migrating-plans-and-suits.md).
