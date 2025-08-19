---
title: Field To Field Multi Map
description: |
  The Field To Field Multi Map enables efficient batch mapping of multiple field pairs in a single configuration, allowing you to map several source fields to their corresponding target fields simultaneously.
dataFile: reference.fieldmaps.fieldtofieldmultimap.yaml
slug: field-to-field-multi-map
aliases:
  - /docs/Reference/FieldMaps/FieldToFieldMultiMap
  - /Reference/FieldMaps/FieldToFieldMultiMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldToFieldMultiMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldToFieldMultiMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2787
---

## Overview

The Field To Field Multi Map provides an efficient way to configure multiple field-to-field mappings in a single field map definition. Instead of creating separate field maps for each field pair, this map allows you to specify multiple source-to-target field mappings in one configuration block.

This field map is particularly useful when you have many fields that need direct one-to-one mapping and want to simplify your configuration structure.

{{< class-description >}}

## How It Works

The Field To Field Multi Map processes multiple field mappings:

1. **Configuration Processing**: Reads the source-to-target field mapping dictionary
2. **Batch Execution**: For each work item, processes all field pairs in the mapping
3. **Individual Field Mapping**: Applies standard field-to-field logic for each pair
4. **Value Copying**: Copies values from each source field to its corresponding target field

## Use Cases

This field map is ideal for:

- **Bulk Field Migration**: Moving many related fields between systems
- **Process Template Changes**: Adapting multiple fields when changing process templates
- **Organization Migration**: Mapping standard fields across different Azure DevOps organizations
- **Schema Standardization**: Aligning field names across different projects
- **Configuration Simplification**: Reducing the number of field map configurations needed

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Example

```json
{
  "FieldMapType": "FieldToFieldMultiMap",
  "ApplyTo": ["Bug", "Task"],
  "SourceToTargetMappings": {
    "Microsoft.VSTS.Common.Priority": "Microsoft.VSTS.Common.Priority",
    "Microsoft.VSTS.Common.Severity": "Microsoft.VSTS.Common.Severity",
    "System.AssignedTo": "System.AssignedTo",
    "System.AreaPath": "System.AreaPath",
    "System.IterationPath": "System.IterationPath"
  }
}
```

### Cross-Organization Migration

```json
{
  "FieldMapType": "FieldToFieldMultiMap",
  "ApplyTo": ["User Story"],
  "SourceToTargetMappings": {
    "Microsoft.VSTS.Scheduling.StoryPoints": "Microsoft.VSTS.Scheduling.Effort",
    "Microsoft.VSTS.Common.BusinessValue": "Custom.BusinessValue",
    "Microsoft.VSTS.Common.AcceptanceCriteria": "System.Description",
    "Custom.LegacyField1": "Custom.NewField1",
    "Custom.LegacyField2": "Custom.NewField2"
  }
}
```

### Custom Field Batch Migration

```json
{
  "FieldMapType": "FieldToFieldMultiMap",
  "ApplyTo": ["*"],
  "SourceToTargetMappings": {
    "Custom.OldCategory": "Custom.NewCategory",
    "Custom.OldOwner": "Custom.NewOwner", 
    "Custom.OldStatus": "Custom.NewStatus",
    "Custom.OldPriority": "Custom.NewPriority"
  }
}
```

## Benefits Over Individual Field Maps

### Configuration Efficiency
- Single field map handles multiple field pairs
- Reduced configuration file size
- Easier maintenance and updates

### Performance Considerations
- Optimized execution for batch operations
- Single field map instance processes multiple fields
- Reduced overhead compared to many individual maps

### Management Simplification
- Related field mappings grouped together
- Easier to understand migration scope
- Simplified testing and validation

## Field Processing Rules

### Source Field Handling
- Each source field is processed independently
- Missing source fields are skipped without errors
- Null or empty values are copied as-is

### Target Field Assignment
- Target fields are set with source field values
- Existing target values are overwritten
- String conversion is applied automatically

### Error Handling
- Individual field mapping failures don't affect other pairs
- Non-existent fields are logged and skipped
- Processing continues for valid field pairs

## Best Practices

### Field Grouping
- Group related fields in single multi-map configurations
- Use separate multi-maps for different functional areas
- Consider work item type specificity for field availability

### Configuration Organization
- Document the purpose of each field mapping group
- Use meaningful naming conventions for clarity
- Test field mappings with sample data

### Performance Optimization
- Use multi-maps for batches of simple field-to-field mappings
- Avoid mixing simple and complex mapping logic
- Consider target system field validation requirements

## Common Patterns

### Standard System Fields

```json
{
  "FieldMapType": "FieldToFieldMultiMap",
  "ApplyTo": ["*"],
  "SourceToTargetMappings": {
    "System.Title": "System.Title",
    "System.Description": "System.Description",
    "System.State": "System.State",
    "System.Reason": "System.Reason",
    "System.AssignedTo": "System.AssignedTo"
  }
}
```

### Process Template Migration

```json
{
  "FieldMapType": "FieldToFieldMultiMap", 
  "ApplyTo": ["Bug"],
  "SourceToTargetMappings": {
    "Microsoft.VSTS.TCM.ReproSteps": "Microsoft.VSTS.TCM.ReproSteps",
    "Microsoft.VSTS.Common.Priority": "Microsoft.VSTS.Common.Priority",
    "Microsoft.VSTS.Common.Severity": "Microsoft.VSTS.Common.Severity",
    "Microsoft.VSTS.TCM.SystemInfo": "Microsoft.VSTS.TCM.SystemInfo"
  }
}
```

## Schema

{{< class-schema >}}
