# Server Configuration and Requirements

There are some requirements that you will need to meet in order to use the tool against your TFS or VSTS server. The VSTS Bulk Data Editor uses a flag to Bypass the Work Item rules engine and write data into TFS\VSTS that may not comply with the rules. For example you can write data directly into the `Closed` state without starting at `New`. This is very useful for migrations but requires some pre-requisits.

## Bypass Rules

For on-premises TFS instances you need to be part of the `Project Collection Service Accounts` group. You can do this by calling the following command:

`tfssecurity /g+ "Project Collection Service Accounts" n:domainusername ALLOW /server:http://myserver:8080/tfs`

This is not required for VSTS targets.

## Migration State

In order to store the state for the migration you need to use a custom field, the `reflectedWorkItemId` field. The way you add this depends on the platform you are using.

### Team Foundation Server (TFS)

Use the `witadmin exportwitd` command to export each work item and add:

```xml
<FIELD name="ReflectedWorkItemId" refname="TfsMigrationTool.ReflectedWorkItemId" type="String" />
```

to the Fields section.

Then use `witadmin importwitd` to re-import the customized work item type template. 

See [MSDN for more details](https://msdn.microsoft.com/en-us/library/dd236914.aspx)

### Visual Studio Team Services (VSTS)

With the advent of the [VSTS Migration Tool](https://blogs.msdn.microsoft.com/visualstudioalm/2016/11/16/import-your-tfs-database-into-visual-studio-team-services/) there are potentially two ways you need to consider when customising a VSTS process template. It all depends how the Team Project was created.

#### Inherited Templates - the future ####

If you created the Team Project via the web based VSTS UI. You need to [add a custom field through the VSTS UI](https://blogs.msdn.microsoft.com/visualstudioalm/2015/12/10/adding-a-custom-field-to-a-work-item/) to be able to use the tool.

The name you should use for the custom field on a VSTS instance is not `TfsMigrationTool.ReflectedWorkItemId` as .(period) are not valid characters for field name. Instead just use `ReflectedWorkItemId` but note that in the `VstsBulkEditor.json` file the name you need to enter is `NameOfYourCustomisedTemplate.ReflectedWorkItemId`. Where`NameOfYourCustomisedTemplate` is the name of your customised template, any spaces in the name will be replaced by _ (underscore). 

#### Custom Templates ####

If you migrated a TPC to VSTS using the [VSTS Migration Tool](https://blogs.msdn.microsoft.com/visualstudioalm/2016/11/16/import-your-tfs-database-into-visual-studio-team-services/) that already had customisations then it will not allow creating an inherited process. You have to exit you customisation in a different manner:

- Export the current process (this is done from the same Account > Settings page where you do any process customisation in VSTS); this downloads the current customised process as a .ZIP file. 
- Once you have the .ZIP file, unpack it and edit the work item type definitions as detailed above for an on-premises installation.
- Once the fields have been added then zip up the folder structure again and re-import it into your VSTS instance, where it will be applied to the Team Project it was exported from. 

### TIP: Checking the actual name of the 'ReflectWorkItemID' field ###

If you are in any doubt over the full name in use for the `ReflectWorkItemID` field on either the source or target systems, it can vary based on the type of customisation, use the following process to confirm it

- Open Visual Studio, connect to the customised Team Project
- Via Team Explorer > Work, create a new work item query. Make sure the `ReflectedWorkItemId` is one of the output columns
- Save the work item query as a file on your local PC
- Open the resultant file in a text editor and check the actual name, it should in the form `NameOfYourCustomisedTemplate.ReflectedWorkItemId` or `TfsMigrationTool.ReflectedWorkItemId`

### Editing the configuration ###

Once you have created the `ReflectedWorkItemId` field and confirmed you have the correct full name, you will need to edit `VstsBulkEditor.json` to match your values, you need to set the 


```json
{
    ...... other stuff
    
	"ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",

    ...... other stuff

 	"Processors": [{
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig",
			"Enabled": false,
			"PrefixProjectToNodes": true,
			"UpdateCreatedDate": true,
			"UpdateCreatedBy": true,
			"UpdateSoureReflectedId": true,
			"QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
      },
      
      ...... other stuff

    
}
```