---
title: "How-to: Migrating Plans and Suits"
layout: page
toc: true
discussionId: 
---

Migrating Plans and Suits is quite convoluted since Shared Steps, which we need to map, can't have a custom field.

1. Migrate Basic Work Items
2. Migrate `Test Cases` with their `Shared Steps` and `Shared Parameter`
3. Migrate `Test Variables` & `Test Configurations`
4. Rebuild `Test Plans` & `Test Suits`

_WARNING: The configs below are for illustration and were correct as of the version number in the `Version` field._

## 1. Migrate Basic Work Items

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
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        },
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsWorkItemMigrationProcessor",
        "Enabled": false,
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

## 2. Migrate `Test Cases` with their `Shared Steps` and `Shared Parameter`

This will migrate Test Cases while fixing the links to the Shared bits that can't be customised. This will use the `Microsoft.VSTS.Build.IntegrationBuild` to wire everything up as needed. It will also copy the `Microsoft.VSTS.Build.IntegrationBuild` to  `ReflectedWorkItemId` for Test Cases so that we can also use them with work items.

The important bits:

- Target ReflectedWorkItemIdField is a common field that is available on the non-customisable work items
- Field Map copies `Microsoft.VSTS.Build.IntegrationBuild` to `ReflectedWorkItemId` for Test Cases only
- The query includes only the Test items that we can migrate as work items (No Suits or Plans)

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
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        },
        "ReflectedWorkItemIdField": "Microsoft.VSTS.Build.IntegrationBuild"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsWorkItemMigrationProcessor",
        "Enabled": true,
        "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] IN ('Test Case','Shared Steps','Shared Parameter') ORDER BY [System.ChangedDate] desc"
      }
    ]
  }
}
```

## 3. Migrate `Test Variables` & `Test Configurations`

These are pre-requisites for rebuilding the Plans and Suits.

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
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        },
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsTestVariablesMigrationProcessor",
        "Enabled": true,
        "Processor": "TestVariablesMigrationContext"
      },
      {
        "ProcessorType": "TfsTestConfigurationsMigrationProcessor",
        "Enabled": true
      }
    ]
  }
}
```

## 4. Rebuild `Test Plans` & `Test Suits`

This will rebuild the Plans and Suits and wire up all of the Test Cases. 

_note: Runs are not migrated._

The important bits:

- Processors for `TestPlansAndSuitesMigrationConfig`

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
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        }
      },
      "Target": {
        "EndpointType": "TfsTeamProjectEndpoint",
        "Collection": "https://dev.azure.com/nkdagility-preview/",
        "Project": "migrationTest5",
        "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
        "Authentication": {
          "AuthenticationMode": "AccessToken",
          "AccessToken": "ouiasdhusahfkahfksdhksdhvksbdlsdvnlsdvsld"
        }
      }
    },
    "Processors": [
      {
        "ProcessorType": "TfsTestPlansAndSuitesMigrationProcessor",
        "Enabled": true,
        "OnlyElementsWithTag": "",
        "TestPlanQuery": null,
        "RemoveAllLinks": false,
        "MigrationDelay": 0,
        "RemoveInvalidTestSuiteLinks": false,
        "FilterCompleted": false
      }
    ]
  }
}
```