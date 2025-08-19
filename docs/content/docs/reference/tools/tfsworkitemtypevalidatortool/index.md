---
title: Tfs WorkItemType Validator Tool
description: Validates Work Item Types against a set of rules. Does not migrate Work Items, only validates types.
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
* **Failure behaviour**: If validation fails, migration is stopped immediately with `Environment.Exit(-1)`.
* **Scope control**:
  * Users can configure which work item types to validate via `IncludeWorkItemtypes`. If none are specified, all work item types are validated.
  * Users can configure field mappings per work item type via `SourceFieldMappings` to:
    * Map a source field to a different target field
    * Exclude a field from validation (by mapping it to an empty string)
  * Users can mark fields as "fixed" via `FixedTargetFields` to reduce validation warnings to informational messages when differences are known and resolved
* **Advanced features**:
  * Supports wildcard mappings using `*` for field mappings and fixed fields that apply to all work item types
  * Provides suggestions for similar work item type names when exact matches are not found
  * Detailed logging of field information including types and allowed values for troubleshooting

## Samples

### Sample

{{< class-sample sample="sample" >}}

This sample configuration demonstrates:

* **Enabled**: Set to `true` to run the validation tool
* **IncludeWorkItemtypes**: Limits validation to specific work item types (Bug, Task, User Story, Product Backlog Item, Feature, Epic, Risk)
* **SourceFieldMappings**: Maps source field `Microsoft.VSTS.Common.Prirucka` to target field `Custom.Prirucka` for User Story work items
* **FixedTargetFields**: Marks `Custom.Prirucka` as a fixed field for User Story, meaning validation differences will be logged as information rather than warnings

### Advanced Configuration Example

For more complex scenarios, you might use wildcard mappings and multiple work item types:

```json
{
  "MigrationTools": {
    "Version": "16.0",
    "CommonTools": {
      "TfsWorkItemTypeValidatorTool": {
        "Enabled": true,
        "IncludeWorkItemtypes": [
          "Bug", "Task", "User Story", "Feature", "Epic"
        ],
        "SourceFieldMappings": {
          "*": {
            "Microsoft.VSTS.Common.CustomField1": "",
            "Microsoft.VSTS.Common.CustomField2": "Custom.NewField2"
          },
          "User Story": {
            "Microsoft.VSTS.Common.BacklogPriority": "Custom.Priority"
          }
        },
        "FixedTargetFields": {
          "*": [
            "Custom.NewField2"
          ],
          "User Story": [
            "Custom.Priority"
          ]
        }
      }
    }
  }
}
```

This configuration:

* Uses `*` wildcard to apply field mappings to all work item types
* Excludes `CustomField1` from validation by mapping to empty string
* Maps `CustomField2` to `Custom.NewField2` for all work item types
* Maps `BacklogPriority` to `Custom.Priority` specifically for User Story
* Marks mapped fields as fixed to suppress validation warnings

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
2. Map the source field to an existing target field using `SourceFieldMappings`
3. Exclude the field from validation by mapping it to an empty string
4. Mark the field as "fixed" in `FixedTargetFields` if you have a field mapping configured

**Field Type Mismatches:**
If source and target fields have different types (e.g., Boolean vs String), consider:

1. Updating the target field type to match the source
2. Using `FieldValueMap` in `FieldMappingTool` to convert values during migration
3. Marking the field as fixed if you have proper value mapping configured

**Different Allowed Values:**
When fields have different allowed values, you can:

1. Update the target field's allowed values to include all source values
2. Configure `FieldValueMap` to translate values during migration
3. Mark the field as fixed if value mapping is properly configured

### Configuration Tips

* Use the `*` wildcard in `SourceFieldMappings` and `FixedTargetFields` for mappings that apply to all work item types
* Always configure corresponding field mappings in `FieldMappingTool` when you use `SourceFieldMappings` for validation
* Use `FixedTargetFields` to reduce noise in validation logs when you've already resolved field differences
* Identity fields are automatically skipped during validation due to their typically different allowed values

### Validation Failure Handling

When validation fails, the migration process stops immediately with exit code -1. Review the detailed logs to:

1. Identify specific missing types, fields, or mismatches
2. Configure appropriate mappings or exclusions
3. Re-run the migration after making configuration changes

The tool provides comprehensive logging including field types, allowed values, and suggested mappings to help troubleshoot validation issues.

{{< class-metadata >}}

## Schema

{{< class-schema >}}


## Log output example

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

