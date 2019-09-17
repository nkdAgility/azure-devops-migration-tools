# Using the Command Line

Download from [executable](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and extract. Use `migration.exe init` to create a reference `configuration.json` configuration file, Or you can [install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

Note that:

- The created reference `configuration.json` shows all the various options available, so is probably more complex than the final edited version you will use.
- All the `Processors` have their `Enabled` property set to `false`.
 
 **This means they are not run. So the default behavior of the generate template is to do nothing. You need to enable the processors you require.**.


```json
{
	"TelemetryEnableTrace": true,
	"Source": {
		"Collection": "https://dev.azure.com/sdd2016",
		"Name": "DemoProjs"
	},
	"Target": {
		"Collection": "https://dev.azure.com/sdd2016",
		"Name": "DemoProjt"
	},
	"ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",
	"FieldMaps": [{
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.MultiValueConditionalMapConfig",
			"WorkItemTypeName": "*",
			"sourceFieldsAndValues": {
				"Field1": "Value1",
				"Field2": "Value2"
			},
			"targetFieldsAndValues": {
				"Field1": "Value1",
				"Field2": "Value2"
			}
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldBlankMapConfig",
			"WorkItemTypeName": "*",
			"targetField": "TfsMigrationTool.ReflectedWorkItemId"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldValueMapConfig",
			"WorkItemTypeName": "*",
			"sourceField": "System.State",
			"targetField": "System.State",
			"valueMapping": {
				"Approved": "New",
				"New": "New",
				"Committed": "Active",
				"In Progress": "Active",
				"To Do": "New",
				"Done": "Closed"
			}
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoFieldMapConfig",
			"WorkItemTypeName": "*",
			"sourceField": "Microsoft.VSTS.Common.BacklogPriority",
			"targetField": "Microsoft.VSTS.Common.StackRank"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoFieldMultiMapConfig",
			"WorkItemTypeName": "*",
            "SourceToTargetMappings" = {
                "SourceField1": "TargetField1",
                "SourceField2": "TargetField2"
            }
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoTagMapConfig",
			"WorkItemTypeName": "*",
			"sourceField": "System.State",
			"formatExpression": "ScrumState:{0}"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldMergeMapConfig",
			"WorkItemTypeName": "*",
			"sourceField1": "System.Description",
			"sourceField2": "Microsoft.VSTS.Common.AcceptanceCriteria",
			"targetField": "System.Description",
			"formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
			"doneMatch": "##DONE##"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.RegexFieldMapConfig",
			"WorkItemTypeName": "*",
			"sourceField": "COMPANY.PRODUCT.Release",
			"targetField": "COMPANY.DEVISION.MinorReleaseVersion",
			"pattern": "PRODUCT \\d{4}.(\\d{1})",
			"replacement": "$1"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldValuetoTagMapConfig",
			"WorkItemTypeName": "*",
			"sourceField": "Microsoft.VSTS.CMMI.Blocked",
			"pattern": "Yes",
			"formatExpression": "{0}"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.TreeToTagMapConfig",
			"WorkItemTypeName": "*",
			"toSkip": 3,
			"timeTravel": 1
		}
	],
	"WorkItemTypeDefinition": {
		"Bug": "Bug",
		"Product Backlog Item": "Product Backlog Item"
	},
	"Processors": [{
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig",
			"Enabled": false,
			"PrefixProjectToNodes": true,
			"UpdateCreatedDate": true,
			"UpdateCreatedBy": true,
			"UpdateSourceReflectedId": true,
			"QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
		},{		
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemRevisionReplayMigrationConfig",
			"Enabled": false,
			"PrefixProjectToNodes": false,
			"UpdateCreatedDate": true,
			"UpdateCreatedBy": true,
			"UpdateSourceReflectedId": false,
			"QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND [System.Tags] Contains 'Xyz'"
	    },{
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemUpdateConfig",
			"WhatIf": false,
			"Enabled": false,
			"QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig",
			"Enabled": false,
			"PrefixProjectToNodes": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.LinkMigrationConfig",
			"Enabled": false,
			"QueryBit": "AND ([System.ExternalLinkCount] > 0 OR [System.RelatedLinkCount] > 0)"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemPostProcessingConfig",
			"Enabled": false,
			"QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ",
			"WorkItemIDs": [1, 2, 3]
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemDeleteConfig",
			"Enabled": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.AttachementExportMigrationConfig",
			"Enabled": false,
			"QueryBit": "AND [System.AttachedFileCount] > 0"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.AttachementImportMigrationConfig",
			"Enabled": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestVeriablesMigrationConfig",
			"Enabled": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestConfigurationsMigrationConfig",
			"Enabled": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestPlansAndSuitsMigrationConfig",
			"Enabled": false,
			"PrefixProjectToNodes": true,
			"OnlyElementsWithTag" : "tag"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestRunsMigrationConfig",
			"Enabled": false,
			"Status": "Experimental"
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.ImportProfilePictureConfig",
			"Enabled": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.ExportProfilePictureFromADConfig",
			"Enabled": false,
			"Domain": null,
			"Username": null,
			"Password": null,
			"PictureEmpIDFormat": null
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.FixGitCommitLinksConfig",
			"TargetRepository" : "TargetRepositoryNameIfNotTheSameAsOnSource",
			"Enabled": false
		}, {
			"ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.HtmlFieldEmbeddedImageMigrationConfig",
			"Enabled": false,
			"QueryBit": " AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')",
			"FromAnyCollection": false,
			"AlternateCredentialsUsername": null,
			"AlternateCredentialsPassword": null,
			"UseDefaultCredentials": false,
			"Ignore404Errors": true,
			"DeleteTemporaryImageFiles": true
		}
	]
}

```
