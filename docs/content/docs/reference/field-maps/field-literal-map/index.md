---
title: Field Literal Map
description: |
  The Field Literal Map sets a static, constant value to a target field on all migrated work items. This is useful for applying consistent values across work items during migration, such as setting default states, assigning standard tags, or populating fields with organizational defaults.
dataFile: reference.fieldmaps.fieldliteralmap.yaml
slug: field-literal-map
aliases:
  - /docs/Reference/FieldMaps/FieldLiteralMap
  - /Reference/FieldMaps/FieldLiteralMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldLiteralMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldLiteralMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2790
---

## Overview

The Field Literal Map is one of the simplest and most straightforward field maps available in the Azure DevOps Migration Tools. It assigns a predefined static value to a target field on all work items that match the specified criteria, regardless of the source field values.

This field map is particularly useful when you need to standardize certain field values across all migrated work items or when the target system requires specific values that don't exist in the source system.

{{< class-description >}}

## How It Works

The Field Literal Map operates with simple logic:

1. **Configuration**: Defines a static value and target field during setup
2. **Execution**: For each matching work item, sets the target field to the specified literal value
3. **Overwrite**: Replaces any existing value in the target field with the configured literal value

## Use Cases

This field map is commonly used for:

- **Default State Assignment**: Setting all migrated work items to a specific state (e.g., "New" or "Imported")
- **Organizational Standards**: Applying company-specific field values across all work items
- **Process Compliance**: Ensuring required fields have valid default values in the target system
- **Batch Tagging**: Adding consistent tags or labels to identify migrated work items
- **Field Initialization**: Setting default values for custom fields that don't exist in the source
- **Area Path Defaults**: Assigning work items to a specific area when source areas don't map directly

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Setting a Default State

```json
{
  "FieldMapType": "FieldLiteralMap",
  "ApplyTo": ["Bug", "Task", "User Story"],
  "value": "New",
  "targetField": "System.State"
}
```

#### Adding Migration Tags

```json
{
  "FieldMapType": "FieldLiteralMap",
  "ApplyTo": ["*"],
  "value": "Migrated-2024",
  "targetField": "System.Tags"
}
```

#### Setting Default Area Path

```json
{
  "FieldMapType": "FieldLiteralMap",
  "ApplyTo": ["*"],
  "value": "MyProject\\Imported Items",
  "targetField": "System.AreaPath"
}
```

#### Assigning Default Priority

```json
{
  "FieldMapType": "FieldLiteralMap",
  "ApplyTo": ["Bug"],
  "value": "2",
  "targetField": "Microsoft.VSTS.Common.Priority"
}
```

## Data Type Support

The Field Literal Map can work with various data types:

### String Values

- Text fields, states, area paths, iteration paths
- Tags (note: tags are typically additive, so consider existing values)

### Numeric Values

- Priority, severity, effort, and other numeric fields
- Values should be provided as strings in the configuration

### Boolean Values

- True/false fields should be specified as "True" or "False" strings

### Date Values

- ISO 8601 format recommended for date fields
- Example: "2024-01-15T00:00:00Z"

## Best Practices

### Value Validation

- Ensure the literal value is valid for the target field type
- Verify that state values exist in the target work item type definition
- Check that area paths and iteration paths exist in the target project

### Strategic Application

- Use `ApplyTo` to target specific work item types when appropriate
- Consider the impact of overwriting existing field values
- Test with a small subset before applying to all work items

### Field Compatibility

- Verify target fields exist in the destination work item types
- Ensure field values comply with any validation rules in the target system
- Consider field length limitations for string values

## Considerations and Limitations

### Value Overwriting

The Field Literal Map always overwrites the target field value, regardless of existing content. If you need to preserve existing values, consider using conditional field maps instead.

### Field Validation

The target system's field validation rules still apply. Invalid values will cause migration errors.

### Performance

Field Literal Maps are highly performant since they don't require complex logic or data transformation.

## Error Handling

The field map includes basic validation:

- **Configuration Validation**: Ensures the target field is specified
- **Runtime Execution**: Direct field assignment with minimal error handling
- **Field Existence**: Target field must exist on the work item type

## Schema

{{< class-schema >}}
