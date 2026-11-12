---
title: 'How-to: Migrating Plans and Suites'
description: |
  This guide explains how to migrate Test Cases, Shared Steps and Shared Parameters, and then rebuild Test Plans and Suites in Azure DevOps.
short_title: Migrating Plans and Suites
discussionId: 2049
date: 2025-06-24T12:07:31Z

---
`Shared Steps` and `Shared Parameter` can't have custom fields on an inherited process, so they can't hold your `ReflectedWorkItemId` field. Only `Test Plan`, `Test Suite` and `Test Case` can be customised (see [Inheritance process model](https://learn.microsoft.com/en-us/azure/devops/organizations/settings/work/inheritance-process-model)). The same applies to `Code Review Request`, `Code Review Response`, `Feedback Request` and `Feedback Response`.

The migration handles this for you. When one of these six types does not have your `ReflectedWorkItemIdField`, the reflected work item id is stored in the built-in `Microsoft.VSTS.Build.IntegrationBuild` field instead, and every lookup (re-runs, `FilterWorkItemsThatAlreadyExistInTarget`, Test Case step and parameter references, links, plans and suites) finds the item by that field. This applies only to those six types; any other type without your field still fails validation. At startup the work item type validator lists the types that use `Microsoft.VSTS.Build.IntegrationBuild`:

```text
'Shared Steps' cannot have reflected work item ID field 'Custom.ReflectedWorkItemId'. Its reflected work item ID is stored in 'Microsoft.VSTS.Build.IntegrationBuild'.
```

The steps are:

1. Migrate Basic Work Items
2. Migrate `Test Cases` with their `Shared Steps` and `Shared Parameter`
3. Migrate `Test Variables` & `Test Configurations`
4. Rebuild `Test Plans` & `Test Suites`

_WARNING: The configs below are for illustration and were correct as of the version number in the `Version` field._

## 1. Migrate Basic Work Items

Migrate everything except the test types, with your custom `ReflectedWorkItemIdField`. No field maps are needed.

```JSON
{
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
        "Authentication": { "AuthenticationMode": "EntraId" }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": { "AuthenticationMode": "EntraId" },
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsWorkItemMigrationProcessor",
        "SourceName": "Source",
        "TargetName": "Target",
        "Enabled": true,
        "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Case', 'Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
      }
    ]
  }
}
```

## 2. Migrate `Test Cases` with their `Shared Steps` and `Shared Parameter`

Use the same `ReflectedWorkItemIdField` as step 1. The target `Test Case` type must have that field; `Shared Steps` and `Shared Parameter` use `Microsoft.VSTS.Build.IntegrationBuild` automatically.

The important bits:

- Target `ReflectedWorkItemIdField` is your main custom field, as in step 1.
- The query includes only the test items that are migrated as work items (no Suites or Plans).
- Leave the work item type validator at its defaults. If you do validate `Shared Steps` or `Shared Parameter` (`ExcludeDefaultWorkItemTypes: false` or `IncludeWorkItemTypes`), the validator accepts `Microsoft.VSTS.Build.IntegrationBuild` for them.
- Shared Steps and Shared Parameters are migrated before the Test Cases whatever the query order, so the references in each Test Case's steps (`compref`) and parameters (`sharedParameterDataSetIds`, `dataSetId`) are pointed at the migrated copies.
- You can run this step again. Items already migrated are found by their reflected work item id, so no duplicates are created, and Test Case references are repaired if they were missed before.

```JSON
{
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
        "Authentication": { "AuthenticationMode": "EntraId" }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": { "AuthenticationMode": "EntraId" },
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsWorkItemMigrationProcessor",
        "SourceName": "Source",
        "TargetName": "Target",
        "Enabled": true,
        "FilterWorkItemsThatAlreadyExistInTarget": true,
        "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] IN ('Test Case','Shared Steps','Shared Parameter') ORDER BY [System.ChangedDate] desc"
      }
    ]
  }
}
```

### Troubleshooting

- **`'<type>' does not contain reflected work item ID field`, then the run stops:** a validated target type that is not one of the six above lacks your `ReflectedWorkItemIdField`. Add the field to that type in the target process.
- **`'Shared Steps' has neither '...' nor 'Microsoft.VSTS.Build.IntegrationBuild'`:** the target process has neither field on that type (possible on some on-premises XML processes). Add one of them to the type, or set `ReflectedWorkItemIdField` to a field it does have.
- **`Overwriting 'Microsoft.VSTS.Build.IntegrationBuild' value ...`:** the source item already had a value in that field, and it is replaced by the reflected work item id on the target.

## 3. Migrate `Test Variables` & `Test Configurations`

These are pre-requisites for rebuilding the Plans and Suites.

The important bits:

- Processors for `Test Variables` & `Test Configurations`

```JSON
{
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        },
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsTestVariablesMigrationProcessor",
        "SourceName": "Source",
        "TargetName": "Target",
        "Enabled": true
      },
      {
        "ProcessorType": "TfsTestConfigurationsMigrationProcessor",
        "SourceName": "Source",
        "TargetName": "Target",
        "Enabled": true
      }
    ]
  }
}
```

## 4. Rebuild `Test Plans` & `Test Suites`

This will rebuild the Plans and Suites and wire up all of the Test Cases.

_note: Runs are not migrated._

If a Test Case in a suite has not been migrated in step 2, it is left out of the rebuilt suite, an error names it, and the run exits with code 1. Migrate it, then run this step again.

The important bits:

- Processor `TfsTestPlansAndSuitesMigrationProcessor`

```JSON
{
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
       "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        }
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsTestPlansAndSuitesMigrationProcessor",
        "Enabled": true,
        "SourceName": "Source",
        "TargetName": "Target",
        "OnlyElementsWithTag": "",
        "RemoveAllLinks": false,
        "MigrationDelay": 0,
        "RemoveInvalidTestSuiteLinks": false,
        "FilterCompleted": false
      }
    ]
  }
}
```

## Older versions: using `IntegrationBuild` for all test items

Before the migration stored the reflected work item id for these types itself, steps 1 and 2 used `Microsoft.VSTS.Build.IntegrationBuild` as the target `ReflectedWorkItemIdField` with field maps. This still works: when `ReflectedWorkItemIdField` is `Microsoft.VSTS.Build.IntegrationBuild`, every type uses it as before.
Both field maps use `TargetToTarget`, not `SourceToTarget`. The value being copied is the reflected work item id that the migration writes to the target work item; it does not exist on the source, so it has to be read from the target. In older versions the copy was missed for newly created work items with a single revision, because the field maps ran before the reflected id was written ([#2998](https://github.com/nkdAgility/azure-devops-migration-tools/issues/2998)).

_WARNING: The configs below are for illustration and were correct as of the version number in the `Version` field._

### Older step 1: Migrate Basic Work Items

This will migrate all of the work items, while also populating `IntegrationBuild`. Ensure that you have a Field Map that will copy `ReflectedWorkItemId` to `Microsoft.VSTS.Build.IntegrationBuild`.

The important bits:

- Target ReflectedWorkItemIdField is your main Custom field.
- Field map copies the `ReflectedWorkItemId` to `Microsoft.VSTS.Build.IntegrationBuild`
- Exclude all test based work items from the query

```JSON
{
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        },
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsWorkItemMigrationProcessor",
        "SourceName": "Source",
        "TargetName": "Target",
        "Enabled": true,
        "UpdateCreatedDate": false,
        "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Case', 'Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
      }
    ],
    "CommonTools": {
      "FieldMappingTool": {
        "Enabled": true,
        "FieldMaps": [
          {
            "FieldMapType": "FieldToFieldMap",
            "fieldMapMode": "TargetToTarget",
            "sourceField": "Custom.ReflectedWorkItemId",
            "targetField": "Microsoft.VSTS.Build.IntegrationBuild",
            "defaultValue": "",
            "ApplyTo": ["*"]
          }
        ]
      }
    }
  }
}
```

### Older step 2: Migrate `Test Cases` with their `Shared Steps` and `Shared Parameter`

This will migrate Test Cases while fixing the links to the Shared bits that can't be customised. This will use the `Microsoft.VSTS.Build.IntegrationBuild` to wire everything up as needed. It will also copy the `Microsoft.VSTS.Build.IntegrationBuild` to `ReflectedWorkItemId` for Test Cases so that we can also use them with work items.

The important bits:

- Target ReflectedWorkItemIdField is a common field that is available on the non-customisable work items
- Field Map copies `Microsoft.VSTS.Build.IntegrationBuild` to `ReflectedWorkItemId` for Test Cases only. The target `Test Case` type must have your custom `ReflectedWorkItemId` field (here `Custom.ReflectedWorkItemId`); if it does not, the map logs a validation warning and copies nothing
- The query includes only the Test items that we can migrate as work items (No Suites or Plans)
- Shared Steps and Shared Parameters are migrated before the Test Cases whatever the query order, so the references in each Test Case's steps are pointed at the migrated copies
- The work item type validator checks only those types, because the other types in the target may not have `Microsoft.VSTS.Build.IntegrationBuild`

```JSON
{
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "MigrationTools": {
    "Version": "16.0",
    "Endpoints": {
      "Source": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationSource1",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "** removed as a secret ***"
        },
        "ReflectedWorkItemIdField": "Microsoft.VSTS.Build.IntegrationBuild"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsWorkItemMigrationProcessor",
        "SourceName": "Source",
        "TargetName": "Target",
        "Enabled": true,
        "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] IN ('Test Case','Shared Steps','Shared Parameter') ORDER BY [System.ChangedDate] desc"
      }
    ],
    "CommonTools": {
      "TfsWorkItemTypeValidatorTool": {
        "Enabled": true,
        "ExcludeDefaultWorkItemTypes": false,
        "IncludeWorkItemTypes": ["Test Case", "Shared Steps", "Shared Parameter"]
      },
      "FieldMappingTool": {
        "Enabled": true,
        "FieldMaps": [
          {
            "FieldMapType": "FieldToFieldMap",
            "fieldMapMode": "TargetToTarget",
            "sourceField": "Microsoft.VSTS.Build.IntegrationBuild",
            "targetField": "Custom.ReflectedWorkItemId",
            "defaultValue": "",
            "ApplyTo": ["Test Case"]
          }
        ]
      }
    }
  }
}
```
