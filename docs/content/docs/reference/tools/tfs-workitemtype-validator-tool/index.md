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

TfsWorkItemTypeValidatorTool validates work item types and fields between the source and target systems before migration starts. Its purpose is to catch missing types or fields early and stop the migration if the configuration is invalid. Validation now runs at the very beginning of migration, so issues are detected quickly.

* **Enabled by default**: The tool runs even without configuration. It can be disabled explicitly, in which case a warning will be logged.
* **Validation respects mappings**: Work item type mappings (`IWorkItemTypeMappingTool`) are applied before validation.
* **Validation checks**:
  * Existence of work item types in the target system.
  * Existence of the reflected work item ID field.
  * Existence of source fields in the corresponding target work item type.
* **Failure behaviour**: If validation fails, migration is stopped immediately.
* **Scope control**:
  * Users can configure which work item types to validate. If none are specified, all are validated.
  * Users can configure field mappings per work item type to:

    * Map a source field to a different target field.
    * Exclude a field from validation (by mapping it to an empty string).

## Samples

### Sample

{{< class-sample sample="sample" >}}


* **Enabled**: Controls whether the tool runs (default is `true`).
* **IncludeWorkItemtypes**: Limits validation to specific work item types.
* **FieldMappings**: Defines per-type field mappings or exclusions.

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Metadata

{{< class-metadata >}}

## Schema

{{< class-schema >}}


## Log output example

```
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Validating work item types.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Source work item types are: Bug, Task, Quality of Service Requirement, User Story, Risk, Test Plan, Test Suite, Code Review Request, Code Review Response, Epic, Feature, Feedback Request, Feedback Response, Impediment, Product Backlog Item, Shared Parameter, Shared Steps, Test Case.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Target work item types are: Bug, Code Review Request, Code Review Response, Epic, Feature, Feedback Request, Feedback Response, Shared Steps, Task, Test Case, Test Plan, Test Suite, User Story, Issue, Shared Parameter.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Validating work item type 'Feature'
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   'Feature' does not contain reflected work item ID field 'Custom.TfsWorkItemId'.
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.AcceptanceCriteria' in 'Feature'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.AcceptanceCriteria
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Html
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.BacklogPriority' in 'Feature'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.BacklogPriority
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Double
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Validating work item type 'Risk'
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55] Work item type 'Risk' does not exist in target system.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Skipping validation of work item type 'Test Plan' because it is not included in validation list.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Skipping validation of work item type 'Test Suite' because it is not included in validation list.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55] Validating work item type 'Product Backlog Item'
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]   This work item type is mapped to 'User Story' in target.
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   'User Story' does not contain reflected work item ID field 'Custom.TfsWorkItemId'.
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.Novika' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.Novika
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Boolean
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]   Source field 'Microsoft.VSTS.Common.Prirucka' is mapped as empty string, so it is not validated in target.
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.StavHelpu' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.StavHelpu
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: String
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values: '0-Nezačaté', '1-Rozrobené', '2-Dokončené'
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.Help' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.Help
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Boolean
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.SuperUserStory' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.SuperUserStory
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: String
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.Planovac' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.Planovac
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: String
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.BusinessValue' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.BusinessValue
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Integer
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Scheduling.Effort' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Scheduling.Effort
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Double
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 WRN] [16.2.10-Local.8-1-gfcb86e55]   Missing field 'Microsoft.VSTS.Common.BacklogPriority' in 'User Story'.
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Source field name: Microsoft.VSTS.Common.BacklogPriority
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Field type: Double
[10:54:09 INF] [16.2.10-Local.8-1-gfcb86e55]     Allowed values:
[10:54:09 ERR] [16.2.10-Local.8-1-gfcb86e55] Some work item types or their fields are not present in the target system (see previous logs). Either add these fields into target work items, or map source fields to other target fields in options (FieldMappings).
```

