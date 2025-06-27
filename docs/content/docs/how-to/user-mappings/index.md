---
title: 'How-To: User Mappings'
description: |
  This guide explains how to map users between source and target systems in Azure DevOps migrations, ensuring integrity across different systems.
short_title: User Mappings
discussionId: 2266
date: 2025-06-24T12:07:31Z

---
There was a request to have the ability to map users to try and maintain integrity across different systems. We added a `TfsUserMappingTool` that allows you to map users from Source to Target... this is not free and takes some work. Running the `TfsExportUsersForMappingProcessor` to get the list of users will produce:

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

1. Run `TfsExportUsersForMappingProcessor` which will export all of the Users in Source mapped or not to target.
2. Run `TfsWorkItemMigrationProcessor` which will run a validator by detail to warn you of missing users. If it finds a mapping it will convert the field...

##Notes

- Applies to all identity fields specified in the list
- It really sucks that we have to match on Display name! Email is included for internal matching
- On `TfsExportUsersForMappingProcessor` you can set `OnlyListUsersInWorkItems` to filter the mapping based on the scope of the query. This is greater if you have many users.
- Both use the `TfsUserMappingTool` setting in `CommonTools` to know what to do.

```json
{
  "$schema": "https://devopsmigration.io/schema/configuration.schema.json",
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility/",
        "Project": "AzureDevOps-Tools",
        "AllowCrossProjectLinking": false,
        "ReflectedWorkItemIdField": "nkdScrum.ReflectedWorkItemId",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "",
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
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "AllowCrossProjectLinking": false,
        "ReflectedWorkItemIdField": "nkdScrum.ReflectedWorkItemId",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "",
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
    },
    "CommonTools": {
      "TfsUserMappingTool": {
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
    },
    "Processors": [
      {
        "ProcessorType": "TfsExportUsersForMappingProcessor",
        "Enabled": true,
        "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
        "OnlyListUsersInWorkItems": true,
        "SourceName": "Source",
        "TargetName": "Target"
      }
    ]
  }
}
```
