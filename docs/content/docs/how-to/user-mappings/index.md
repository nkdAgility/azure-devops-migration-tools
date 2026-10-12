---
title: 'How-To: User Mappings'
description: |
  This guide explains how to map users between source and target systems in Azure DevOps migrations, ensuring integrity across different systems.
short_title: User Mappings
discussionId: 2266
date: 2025-06-24T12:07:31Z

---
The `TfsUserMappingTool` maps users in identity fields (such as Assigned To) from the source to the target. Users whose display name is the same in source and target need no mapping. Running the `TfsExportUsersForMappingProcessor` writes a starter mapping file of the users that differ.

By default the file maps source display names to target display names:

```json
{
  "Martin Hinshelwood nkdAgility.com": "Hinshelwood, Martin",
  "Rollup Bot": "Service Account 4",
  "John Smith <john.smith@old.com>": "John Smith <john.smith@new.com>",
  "John Smith <jsmith@old.com>": "John Smith <jsmith@new.com>"
}
```

Where several users share a display name, the export writes the entry as `Display Name <upn>`. Azure DevOps returns identity values in that form when a display name is not unique, and accepts it when saving, so each entry identifies one user. You can write entries in this form by hand as well.

When `UseIdentityMapDataFormat` is enabled, the export instead writes a JSON array of `Source` and `Target` identities (display name, account name, email and SID), and source values are matched on either the display name or the account name.

## How it works

1. Run `TfsExportUsersForMappingProcessor`. It loads the users from source and target, pairs them, and writes the users whose names differ to `UserMappingFile`. Review and edit the file.
2. Run `TfsWorkItemMigrationProcessor`. It warns about source users that do not exist in the target and, for each identity field listed in `IdentityFieldsToCheck`, replaces the source value with the mapped target value.

Users are paired by display name. With `MatchUsersByEmail` enabled they are paired by email address first, then by display name.

## Email domain or tenant changes

When users move from `first.last@old.com` to `first.last@new.com`, exact email matching finds nothing. With `MatchUsersByEmail` enabled:

- `EmailDomainMappings` rewrites source domains to target domains, for example `{ "old.com": "new.com" }`.
- `MatchUsersByEmailLocalPart` matches on the part before `@` when the exact and domain-mapped checks find no unique match.

A match is used only when it identifies a single target user. Otherwise the display name is used, or the user is left for you to map in the file.

## Notes

- Applies to all identity fields specified in `IdentityFieldsToCheck`.
- On `TfsExportUsersForMappingProcessor` you can set `OnlyListUsersInWorkItems` to export only the users found in the work items selected by `WIQLQuery`. This is better if you have many users.
- Both processors use the `TfsUserMappingTool` settings in `CommonTools`. See the [TfsUserMappingTool reference]({{< ref "docs/reference/tools/tfsusermappingtool" >}}) for all options.

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
        "MatchUsersByEmail": true,
        "EmailDomainMappings": {
          "old.com": "new.com"
        },
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
