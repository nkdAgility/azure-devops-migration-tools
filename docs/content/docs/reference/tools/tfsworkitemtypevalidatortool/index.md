---
title: TFS Work Item Type Validator Tool
description: Validates work item types and fields between source and target systems before migration begins, ensuring compatibility and preventing migration failures due to missing or incompatible work item configurations.
dataFile: reference.tools.tfsworkitemtypevalidatortool.yaml
schemaFile: schema.tools.tfsworkitemtypevalidatortool.json
slug: work-item-type-validator-tool
aliases:
- /TfsWorkItemTypeValidatorTool
- /docs/Reference/Tools/WorkItemTypeValidatorTool
- /Reference/Tools/WorkItemTypeValidatorTool
- /learn/azure-devops-migration-tools/Reference/Tools/WorkItemTypeValidatorTool
- /learn/azure-devops-migration-tools/Reference/Tools/WorkItemTypeMappingTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2903

---
{{< class-description >}}

## Options

{{< class-options >}}

## How it Works

TfsWorkItemTypeValidatorTool validates work item types and fields between the source and target systems before migration starts. Its purpose is to catch missing types or fields early and stop the migration if the configuration is invalid. Validation runs at the very beginning of migration in the `TfsWorkItemMigrationProcessor`, so issues are detected quickly.

* **Enabled by default**: The tool runs even without configuration. It can be disabled explicitly, in which case a warning will be logged.
* **Validation respects mappings**: Work item type mappings (`IWorkItemTypeMappingTool`) are applied before validation.
* **Validation checks**:
  * Existence of work item types in the target system after applying type mappings
  * Existence of the reflected work item ID field in target work item types
  * Existence of source fields in the corresponding target work item type
  * Field type compatibility between source and target fields
  * Allowed values compatibility for fields with restricted values
  * Special handling for identity fields (validation skipped due to typically different allowed values)
* **Failure behaviour**: If validation fails, the migration stops before any work item is migrated and the tool exits with code 1.
* **Scope control**:
  * `IncludeWorkItemTypes` or `ExcludeWorkItemTypes` (not both) choose which source work item types are validated. If neither is set, all types are validated. `ExcludeDefaultWorkItemTypes` (on by default) also excludes Code Review Request/Response, Feedback Request/Response, Shared Parameter and Shared Steps; set it to `false` when you use `IncludeWorkItemTypes`.
  * `ExcludeSourceFields` lists source fields, per source type or for all types with `*`, whose issues are still logged but as information, and do not fail validation. Use it for differences you have resolved or accept.
* **Field maps are honoured**: a source field read by a `FieldToFieldMap` (mode `SourceToTarget`) or `FieldToFieldMultiMap` in `FieldMappingTool` is validated against the mapped target field, and `FieldValueMap` mappings are taken into account when allowed values differ. Maps are matched by the **source** work item type in their `ApplyTo`, as they are during migration (see [Upgrading: ApplyTo names the source type]({{< ref "docs/reference/tools/fieldmappingtool" >}}#upgrading-applyto-names-the-source-type)).
* **Advanced features**:
  * Provides suggestions for similar work item type names when exact matches are not found
  * Detailed logging of field information including types and allowed values for troubleshooting
  * Optional scan of revision history for fields that only exist there (see [History-only fields](#history-only-fields))

## Samples

### Sample

{{< class-sample sample="sample" >}}

This sample configuration demonstrates:

* **Enabled**: Set to `true` to run the validation tool
* **ExcludeDefaultWorkItemTypes**: Skips the default excluded types (code review, feedback, shared steps and parameters)
* **ExcludeSourceFields**: Issues with `Microsoft.VSTS.Common.AcceptanceCriteria` and `Microsoft.VSTS.Scheduling.Effort` on Bug are logged as information and do not fail validation
* **ScanRevisionHistoryForFields**: Scans moved or retyped revisions for history-only fields before migration; with `StopIfHistoryOnlyFieldsFound` set to `false`, findings are warnings and the migration continues

### Advanced Configuration Example

Field mappings for validation are configured once, in `FieldMappingTool`, and the validator uses the same maps the migration applies:

```json
{
  "MigrationTools": {
    "Version": "16.0",
    "CommonTools": {
      "WorkItemTypeMappingTool": {
        "Enabled": true,
        "Mappings": {
          "User Story": "Product Backlog Item"
        }
      },
      "FieldMappingTool": {
        "Enabled": true,
        "FieldMaps": [
          {
            "FieldMapType": "FieldToFieldMap",
            "ApplyTo": [ "User Story" ],
            "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
            "targetField": "Custom.Priority"
          },
          {
            "FieldMapType": "FieldToFieldMultiMap",
            "ApplyTo": [ "*" ],
            "SourceToTargetMappings": {
              "Custom.Field2": "Custom.NewField2"
            }
          }
        ]
      },
      "TfsWorkItemTypeValidatorTool": {
        "Enabled": true,
        "ExcludeSourceFields": {
          "*": [ "Custom.Field1" ]
        },
        "ScanRevisionHistoryForFields": true,
        "StopIfHistoryOnlyFieldsFound": true
      }
    }
  }
}
```

This configuration:

* Maps User Story to Product Backlog Item, and validates User Story fields against Product Backlog Item
* Validates `BacklogPriority` against `Custom.Priority` for User Story; `ApplyTo` names the source type, User Story, not the target type
* Validates `Custom.Field2` against `Custom.NewField2` for all work item types
* Logs issues with `Custom.Field1` as information instead of failing validation
* Stops the migration if any revision holds a value for a field that exists only in history

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Troubleshooting

### Common Validation Issues

**Missing Work Item Types:**
If you see warnings like "Work item type 'Risk' does not exist in target system", you have several options:

1. Create the missing work item type in the target system
2. Configure a work item type mapping using `IWorkItemTypeMappingTool`
3. Remove the work item type from the source or exclude it from migration

**Missing Fields:**
When fields are missing in the target, you can:

1. Add the field to the target work item type
2. Map the source field to an existing target field with a `FieldToFieldMap` (mode `SourceToTarget`) or `FieldToFieldMultiMap` in `FieldMappingTool`, with the **source** type in `ApplyTo`
3. List the field in `ExcludeSourceFields` to accept the difference; it is then logged as information

**Field maps that list only the target type in `ApplyTo`:**
Since [#3151](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3151) the validator selects field maps by the source work item type, as the migration always has. Configurations that put only the target type in `ApplyTo`, a workaround for the earlier validator, are now reported as missing fields. Those maps were not applied during migration either. Change `ApplyTo` to the source type; see [Upgrading: ApplyTo names the source type]({{< ref "docs/reference/tools/fieldmappingtool" >}}#upgrading-applyto-names-the-source-type).

**Field Type Mismatches:**
If source and target fields have different types (e.g., Boolean vs String), consider:

1. Updating the target field type to match the source
2. Using `FieldValueMap` in `FieldMappingTool` to convert values during migration
3. Listing the field in `ExcludeSourceFields` if you have proper value mapping configured

**Different Allowed Values:**
When fields have different allowed values, you can:

1. Update the target field's allowed values to include all source values
2. Configure `FieldValueMap` to translate values during migration; the validator treats mapped values as valid
3. List the field in `ExcludeSourceFields` if the remaining differences are acceptable

### Configuration Tips

* Use the `*` key in `ExcludeSourceFields` for fields that apply to all work item types
* Configure field mappings once, in `FieldMappingTool`; the validator reads the same maps the migration applies
* Use `ExcludeSourceFields` to reduce noise in validation logs when you've already resolved field differences
* Identity fields are automatically skipped during validation due to their typically different allowed values

### Validation Failure Handling

When validation fails, the migration stops before any work item is migrated and the tool exits with code 1. Review the detailed logs to:

1. Identify specific missing types, fields, or mismatches
2. Configure appropriate mappings or exclusions
3. Re-run the migration after making configuration changes

The tool provides comprehensive logging including field types, allowed values, and suggested mappings to help troubleshoot validation issues.

### History-only fields

Type validation compares only the current work item type definitions. When an item was moved from another project or process, or its type was changed, its older revisions can hold values for fields that the current type does not have. Those values are dropped during migration, and field maps cannot read them, because maps validate against the current source type.

Set `ScanRevisionHistoryForFields` to `true` to check the items that `TfsWorkItemMigrationProcessor` will migrate (after `FilterWorkItemsThatAlreadyExistInTarget`) before migration starts. The scan checks each revision that was saved under a different team project or work item type than the item has now, against the target type that revision will be replayed to. Other revisions only carry fields of the current type, which type validation already covers. Each checked revision is loaded with one server request, so the cost grows with the number of moved or retyped revisions; the log reports how many were loaded and how long the scan took. It is off by default. Each history-only field is reported once per source and target type pair, with revision and work item counts and sample IDs.

By default the findings are warnings and the migration continues. Set `StopIfHistoryOnlyFieldsFound` to `true` to stop instead. To keep the data, add the field to the target work item type. To accept the loss, list the field in `ExcludeSourceFields`; excluded fields are still logged, as information.

```json
"TfsWorkItemTypeValidatorTool": {
  "Enabled": true,
  "ScanRevisionHistoryForFields": true,
  "StopIfHistoryOnlyFieldsFound": true
}
```

When the scan is off, the same fields are still reported during the run and in the summary at the end of it.

{{< class-metadata >}}

## Schema

{{< class-schema >}}


## Log output example

This output is from an earlier version, before `SourceFieldMappings` and `FixedTargetFields` were replaced by field maps in `FieldMappingTool` and `ExcludeSourceFields`. The messages that mention them are worded differently now.

```text
[10:54:09 INF] [ValidateWorkItemTypes]
[10:54:09 INF] ------------------------------------
[10:54:09 INF] Starting Pre-Validation: Validating work item types and fields with `WorkItemTypeValidatorTool`.
[10:54:09 INF] Refer to https://devopsmigration.io/TfsWorkItemTypeValidatorTool/ for configuration.
[10:54:09 INF] ------------------------------------
[10:54:09 INF] Validating work item types.
[10:54:09 INF] Source work item types are: Bug, Task, Quality of Service Requirement, User Story, Risk, Test Plan, Test Suite, Code Review Request, Code Review Response, Epic, Feature, Feedback Request, Feedback Response, Impediment, Product Backlog Item, Shared Parameter, Shared Steps, Test Case.
[10:54:09 INF] Target work item types are: Bug, Code Review Request, Code Review Response, Epic, Feature, Feedback Request, Feedback Response, Shared Steps, Task, Test Case, Test Plan, Test Suite, User Story, Issue, Shared Parameter.
[10:54:09 INF] Validating work item type 'Feature'
[10:54:09 WRN]   'Feature' does not contain reflected work item ID field 'Custom.TfsWorkItemId'.
[10:54:09 WRN]   Missing field 'Microsoft.VSTS.Common.AcceptanceCriteria' in 'Feature'.
[10:54:09 INF]     Source field reference name: Microsoft.VSTS.Common.AcceptanceCriteria
[10:54:09 INF]     Source field name: Microsoft.VSTS.Common.AcceptanceCriteria
[10:54:09 INF]     Field type: Html
[10:54:09 INF]     Allowed values: 
[10:54:09 INF]     Allowed values type: String
[10:54:09 WRN]   Missing field 'Microsoft.VSTS.Common.BacklogPriority' in 'Feature'.
[10:54:09 INF]     Source field reference name: Microsoft.VSTS.Common.BacklogPriority
[10:54:09 INF]     Source field name: Microsoft.VSTS.Common.BacklogPriority
[10:54:09 INF]     Field type: Double
[10:54:09 INF]     Allowed values: 
[10:54:09 INF]     Allowed values type: Double
[10:54:09 INF] Validating work item type 'Risk'
[10:54:09 WRN] Work item type 'Risk' does not exist in target system.
[10:54:09 INF]  Suggested mapping: 'Risk' – 'Issue'
[10:54:09 INF] Skipping validation of work item type 'Test Plan' because it is not included in validation list.
[10:54:09 INF] Skipping validation of work item type 'Test Suite' because it is not included in validation list.
[10:54:09 INF] Validating work item type 'Product Backlog Item'
[10:54:09 INF]   This work item type is mapped to 'User Story' in target.
[10:54:09 WRN]   'User Story' does not contain reflected work item ID field 'Custom.TfsWorkItemId'.
[10:54:09 WRN]   Missing field 'Microsoft.VSTS.Common.Novika' in 'User Story'.
[10:54:09 INF]     Source field reference name: Microsoft.VSTS.Common.Novika
[10:54:09 INF]     Source field name: Microsoft.VSTS.Common.Novika
[10:54:09 INF]     Field type: Boolean
[10:54:09 INF]     Allowed values: 
[10:54:09 INF]     Allowed values type: Boolean
[10:54:09 INF]   Source field 'Microsoft.VSTS.Common.Prirucka' is mapped as empty string, so it is not validated in target.
[10:54:09 WRN]   Missing field 'Microsoft.VSTS.Common.StavHelpu' in 'User Story'.
[10:54:09 INF]     Source field reference name: Microsoft.VSTS.Common.StavHelpu
[10:54:09 INF]     Source field name: Microsoft.VSTS.Common.StavHelpu
[10:54:09 INF]     Field type: String
[10:54:09 INF]     Allowed values: '0-Nezačaté', '1-Rozrobené', '2-Dokončené'
[10:54:09 INF]     Allowed values type: String
[10:54:09 WRN]   Source field 'Microsoft.VSTS.Common.Help' and target field 'Microsoft.VSTS.Common.Help' have different types: source = 'Boolean', target = 'String'.
[10:54:09 WRN]   Source field 'Microsoft.VSTS.Common.BusinessValue' and target field 'Microsoft.VSTS.Common.BusinessValue' have different allowed values.
[10:54:09 INF]     Source allowed values: '1', '2', '3', '4', '5'
[10:54:09 INF]     Target allowed values: '1', '2', '3'
[10:54:09 INF]   Target field 'Custom.Prirucka' in 'User Story' is considered valid, because it is listed in 'FixedTargetFields'.
[10:54:09 ERR] Some work item types or their fields are not present in the target system (see previous logs). Either add these fields into target work items, or map source fields to other target fields in options (SourceFieldMappings).
[10:54:09 INF] If you have some field mappings defined for validation, do not forget also to configure proper field mapping in FieldMappingTool so data will preserved during migration.
[10:54:09 INF] If you have different allowed values in some field, either update target field to match allowed values from source, or configure FieldValueMap in FieldMappingTool.
[10:54:09 INF] ------------------------------------
[10:54:09 INF] [/ValidateWorkItemTypes]
```

