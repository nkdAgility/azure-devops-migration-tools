---
title: "How-To: User Mappings"
layout: page
toc: true
discussionId: 
---

There was a request to have the ability to map users to try and maintain integrity across different systems. We added a TfsUserMappingEnricher` that allows you to map users from Source to Target... this is not free and takes some work. Runnin the `ExportUsersForMappingConfig` to get the list of users will produce:

```
[
  {
    "Source": {
      "FriendlyName": "Martin Hinshelwood nkdAgility.com",
      "AccountName": "martin@nkdagility.com"
    },
    "target": {
      "FriendlyName": "Hinshelwood, Martin",
      "AccountName": "martin@othercompany.com"
    }
  },
  {
    "Source": {
      "FriendlyName": "Rollup Bot",
      "AccountName": "Bot@nkdagility.com"
    },
    "target": {
      "FriendlyName": "Service Account 4",
      "AccountName": "randoaccount@somecompany.com"
    }
  },
  {
    "Source": {
      "FriendlyName": "Another non mapped Account",
      "AccountName": "not-mapped@nkdagility.com"
    },
    "target": null
  }
]
```

##How it works

1. Run `ExportUsersForMappingConfig` which will export all of the Users in Soruce Mapped or not to target.
2. Run `WorkItemMigrationConfig` which will run a validator by detail to warn you of missing users. If it finds a mapping it will convert the field... 

##Notes
- Applies to all identity fields specified in the list
- It really sucks that we have to match on Display name! Email is included for internal matching
- On 'ExportUsersForMappingConfig` you can set `OnlyListUsersInWorkItems` to filter the mapping based on the scope of the query. This is greater if you have many users.
- Both use the `TfsUserMappingEnricherOptions` setting in `CommonEnrichersConfig` to know what to do.

```
{
  "ChangeSetMappingFile": null,
  "Source": {
    "$type": "TfsTeamProjectConfig",
    "Collection": "https://dev.azure.com/nkdagility/",
    "Project": "AzureDevOps-Tools",
    "ReflectedWorkItemIdField": "nkdScrum.ReflectedWorkItemId",
    "AllowCrossProjectLinking": false,
    "AuthenticationMode": "Prompt",
    "PersonalAccessToken": "",
    "PersonalAccessTokenVariableName": "",
    "LanguageMaps": {
      "AreaPath": "Area",
      "IterationPath": "Iteration"
    }
  },
  "Target": {
    "$type": "TfsTeamProjectConfig",
    "Collection": "https://dev.azure.com/nkdagility-preview/",
    "Project": "migrationTest5",
    "ReflectedWorkItemIdField": "nkdScrum.ReflectedWorkItemId",
    "AllowCrossProjectLinking": false,
    "AuthenticationMode": "Prompt",
    "PersonalAccessToken": "",
    "PersonalAccessTokenVariableName": "",
    "LanguageMaps": {
      "AreaPath": "Area",
      "IterationPath": "Iteration"
    }
  },
  "FieldMaps": [],
  "GitRepoMapping": null,
  "LogLevel": "Debug",
  "CommonEnrichersConfig": [
    {
      "$type": "TfsUserMappingEnricherOptions",
      "Enabled": true,
      "UserMappingFile": "C:\\temp\\userExport.json",
      "IdentityFieldsToCheck": [
        "System.AssignedTo",
        "System.ChangedBy",
        "System.CreatedBy",
        "Microsoft.VSTS.Common.ActivatedBy",
        "Microsoft.VSTS.Common.ResolvedBy",
        "Microsoft.VSTS.Common.ClosedBy"
      ]
    }
  ],
  "Processors": [
    {
      "$type": "ExportUsersForMappingConfig",
      "Enabled": true,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "OnlyListUsersInWorkItems": true
    }
  ],
  "Version": "15.0"
}
```
