---
title: ReflectedWorkItemId 
layout: page
pageType: index
pageStatus: published
redirect_from:
  - /Reference/ReflectedWorkItemId/
  - /server-configuration/
---

The Azure DevOps migrations Tools has no internal state, and uses a field on the work item to track the migration of work items. This field is always referd to in the docs as `ReflectedWorkItemId` and is used to track the work item in the target. It enables the ability to resume migrations as well as to be able to scope the work items based on a query and have multiple runs overlap.

Se below how to add the `ReflectedWorkItemId` to your target project as its diferent for Azure DevOps, TFS, and if you imported your Collection from TFS to Azure DevOps.

## How to use the ReflectedWorkItemId

In your configuration file under `MigrationTools:Endpoints` there willbe both a `Source` and a `Target` endpoint. On the `Target` endpoint there should be a property called `ReflectedWorkItemID*` (depending on the specific endpoint implmnetation) that will have a property value like `Custom.ReflectedWorkItemId`. This is the field that the tool will use to track the work items in the target.

```json
{
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
        "AllowCrossProjectLinking": false,
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "alakjhsaggdsad67869asdjksafksldjhgsjkdghsdkfhskdf",
          "NetworkCredentials": {
            "UserName": "",
            "Password": "",
            "Domain": ""
          }
        },
        "LanguageMaps": {
          "AreaPath": "Area",
          "IterationPath": "Iteration"
        }
      }
    }
  }
}
```

When you create the field you will be able to see the`RefName` (diferent from the display name) in the field settings. This is the value that you will use in the configuration file. It will always have at least one `.` in the name. On the inherited processes it will be `Custom.ReflectedWorkItemId` (unless you created your process and added the field many moons ago, inwhich case it will be `processName.ReflectedWorkItemId`). On the XML process it will be whatever you want to call it But I recommned something like `TfsMigrationTool.ReflectedWorkItemId` or just `ReflectedWorkItemId`.

## Work Items you cant Customise!

If you need to migratae work items that you cant customsie, then you will need to use one of the built in fields and I recommned `Microsoft.VSTS.Build.IntegrationBuild`. This field is only used by builds, and is realitively safe to use.

This is primerally of concern for [How-to: Migrating Plans and Suits](_howto/migrating-plans-and-suits.md).

## How to add the ReflectedWorkItemId

To add the `ReflectedWorkItemId` to your target project you can use the follow the [Add a custom field to a work item type (Inheritance process)](https://learn.microsoft.com/en-us/azure/devops/organizations/settings/work/add-custom-field?view=azure-devops) documentation from Microsoft. If you are on the older XML process you can follow the [Add a custom field to a work item type (On-premises XML process)](https://learn.microsoft.com/en-us/azure/devops/organizations/settings/work/import-process/customize-process?view=azure-devopss) documentation.

**Note: We can [help you get off those horible legacy XML Process](https://nkdagility.com/capabilities/azure-devops-migration-services/).**

### Azure DevOps

With the advent of the [data migration tool for Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/migrate/migration-overview) there are potentially two ways you need to consider when customising an Azure DevOps process. It all depends how the Team Project was created.

#### Inherited Process ####

If you created the Team Project via the web based Azure DevOps UI. You need to [add a custom field through the VSTS UI](https://blogs.msdn.microsoft.com/visualstudioalm/2015/12/10/adding-a-custom-field-to-a-work-item/) to be able to use the tool.

The name you should use for the custom field on a VSTS instance is not `TfsMigrationTool.ReflectedWorkItemId` as .(period) are not valid characters for field name. Instead just use `ReflectedWorkItemId` but note that in the `configuration.json` file the name you need to enter is `NameOfYourCustomisedTemplate.ReflectedWorkItemId`. Where`NameOfYourCustomisedTemplate` is the name of your customised template, any spaces in the name will be replaced by _ (underscore). 

#### XML Process ####

If you migrated from on-premises to the cloud using the [data migration tool for Azure DevOps](https://learn.microsoft.com/en-us/azure/devops/migrate/migration-overview) that already had customisations then it will not allow creating an inherited process. Editing the XML Process is much like the on-premises process, but you dont have access to `witadmin` so you will need to do the following:

- Export the current process (this is done from the same Account > Settings page where you do any process customisation in VSTS); this downloads the current customised process as a .ZIP file. 
- Once you have the .ZIP file, unpack it and edit the work item type definitions as detailed above for an on-premises installation.
- Once the fields have been added then zip up the folder structure again and re-import it into your VSTS instance, where it will be applied to the Team Project it was exported from. 

### Team Foundation Server (TFS) Customisation

Use the `witadmin exportwitd` command to export each work item and add:

```xml
<FIELD name="ReflectedWorkItemId" refname="TfsMigrationTool.ReflectedWorkItemId" type="String" />
```

to the Fields section.

Then use `witadmin importwitd` to re-import the customized work item type template. 

See [MSDN for more details](https://msdn.microsoft.com/en-us/library/dd236914.aspx)

