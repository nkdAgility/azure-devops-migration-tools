---
title: Permissions
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
redirect_from:
  - /permissions/
---

The permissions are courently governed by the needs of the [TFS Client Object Model](https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops) from Microsoft. Microsoft [Announced the deprecation of the WIT and Test Client OM](https://devblogs.microsoft.com/devops/announcing-the-deprecation-of-the-wit-and-test-client-om-at-jan-1-2020-2/) in 2020, however this is still the only way to interact with versions of TFS from 2010 to 2018 with any consistantcy. We are working on moving our tools to the REST API, but this is a large task and will take some time.

## Minimum Permission Requirements

At this time the documented minimum required permissions for running the tools are:

- Account in both the source and target projects with "Project Collection Administrator" rights
- PAT with "full access" for both the Source and the Target

Note: I have been informed by the Azure DevOps product team information that ObjectModel API only works with full scoped PATs, so it won't work with any PAT that has specific scopes. 

### However! Unsupported Permission Options

We have seen that the tools may work with less permissions however the following has not been full tested and is not currently supported:

- Project and Team (Read, write, & manage)
- Work Items (Read, Write & Manage)
- Identity (Read & Manage)
- Security (Manage)

If you do try this out then please let us know how you get on!

## TFS, VSTS, & Azure DevOps Server Configuration (On-Premises Only)

There are some additional requirements that you will need to meet in order to use the tool against your TFS or VSTS server. The Azure DevOps Migration Tools use a flag to Bypass the Work Item rules engine and write data into TFS\VSTS that may not comply with the rules. For example you can write data directly into the `Closed` state without starting at `New`. This is very useful for migrations but requires some pre-requisites.

## Bypass Rules

For on-premises instances you need to be part of the `Project Collection Service Accounts` group. You can do this by calling the following command:

`tfssecurity /g+ "Project Collection Service Accounts" n:domainusername ALLOW /server:http://myserver:8080/tfs`

This is not required for Azure DevOps Service targets, but you do need to be a Collection Admin on the Target.

## Migration State

In order to store the state for the migration you need to use a custom field, the `ReflectedWorkItemId` field which needs added to the Target only. The way you add this depends on the platform you are using.

### Azure DevOps

1. Create a custom field called `ReflectedWorkItemId` of type `Text (single line)` by following this [guide](https://docs.microsoft.com/en-us/azure/devops/organizations/settings/work/add-custom-field?view=azure-devops)
1.  Add this custom field to all work item types to be migrated (e.g. Bug, User Story, Task, Test Case, etc)

### Team Foundation Server (TFS)

Use the `witadmin exportwitd` command to export each work item and add:

```xml
<FIELD name="ReflectedWorkItemId" refname="TfsMigrationTool.ReflectedWorkItemId" type="String" />
```

to the Fields section.

Then use `witadmin importwitd` to re-import the customized work item type template. 

See [MSDN for more details](https://msdn.microsoft.com/en-us/library/dd236914.aspx)

### Visual Studio Team Services (VSTS)

With the advent of the [data migration tool for Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/migrate/migration-overview) there are potentially two ways you need to consider when customising a VSTS process template. It all depends how the Team Project was created.

#### Inherited Templates - the future ####

If you created the Team Project via the web based VSTS UI. You need to [add a custom field through the VSTS UI](https://blogs.msdn.microsoft.com/visualstudioalm/2015/12/10/adding-a-custom-field-to-a-work-item/) to be able to use the tool.

The name you should use for the custom field on a VSTS instance is not `TfsMigrationTool.ReflectedWorkItemId` as .(period) are not valid characters for field name. Instead just use `ReflectedWorkItemId` but note that in the `configuration.json` file the name you need to enter is `NameOfYourCustomisedTemplate.ReflectedWorkItemId`. Where`NameOfYourCustomisedTemplate` is the name of your customised template, any spaces in the name will be replaced by _ (underscore). 

#### Custom Templates ####

If you migrated a TPC to VSTS using the [data migration tool for Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/migrate/migration-overview) that already had customisations then it will not allow creating an inherited process. You have to exit you customisation in a different manner:

- Export the current process (this is done from the same Account > Settings page where you do any process customisation in VSTS); this downloads the current customised process as a .ZIP file. 
- Once you have the .ZIP file, unpack it and edit the work item type definitions as detailed above for an on-premises installation.
- Once the fields have been added then zip up the folder structure again and re-import it into your VSTS instance, where it will be applied to the Team Project it was exported from. 

### TIP: Checking the actual 'ReflectedWorkItemIDFieldName' ###

If you are in any doubt over the full name in use for the `ReflectedWorkItemIDFieldName` on either the source or target systems, it can vary based on the type of customisation, use the following process to confirm it

- Open Visual Studio, connect to the customised Team Project
- Via Team Explorer > Work, create a new work item query. Make sure the `ReflectedWorkItemId` is one of the output columns
- Save the work item query as a file on your local PC
- Open the resulting file in a text editor and check the actual name, it should be in the form `NameOfYourCustomisedTemplate.ReflectedWorkItemId` or `TfsMigrationTool.ReflectedWorkItemId`

### Editing the configuration ###

Once you have created the `ReflectedWorkItemId` field and confirmed you have the correct full name for both, the Source and the Target, you will need to edit `configuration.json` to match your values:


```JSON
{
	"Source": {
		...
		"ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",
		...
	},
	"Target": {
		...
		"ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",
		...
	},

    	...... other stuff

	],
	
	...... other stuff
    
}
```
