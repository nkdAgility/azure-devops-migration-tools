---
title: Field Clear Map
description: |
  The Field Clear Map removes data from a specified field by setting its value to null during work item migration. It includes intelligent validation to ensure the field can be safely cleared without causing migration errors.
dataFile: reference.fieldmaps.fieldclearmap.yaml
slug: field-clear-map
aliases:
  - /docs/Reference/FieldMaps/FieldClearMap
  - /Reference/FieldMaps/FieldClearMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldClearMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldClearMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2791
---

## Overview

The Field Clear Map is designed to safely remove data from specific fields during work item migration by setting the target field value to null. This field map includes comprehensive validation logic to prevent errors when clearing fields that have restrictions or requirements.

{{< class-description >}}

## How It Works

The Field Clear Map follows a careful validation process before clearing any field:

1. **Field Existence Check**: Verifies that the target field exists on the work item
2. **Allowed Values Validation**: For fields with restricted values, ensures clearing is permitted
3. **Editability Check**: Confirms the field is editable in the current context
4. **Required Field Check**: Prevents clearing fields that are required
5. **Safe Clearing**: Sets the field value to null only if all validations pass

## Use Cases

This field map is commonly used for:

- **Data Sanitization**: Removing confidential or sensitive information during migration
- **Legacy Field Cleanup**: Clearing obsolete fields that are no longer needed
- **Process Compliance**: Removing values that don't comply with target system validation rules
- **Field Reset**: Starting with clean slate for fields that will be populated differently
- **Privacy Requirements**: Removing personally identifiable information (PII)
- **Validation Error Prevention**: Clearing fields that cause validation errors in target system

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Clearing Custom Fields

```json
{
  "FieldMapType": "FieldClearMap",
  "ApplyTo": ["Bug", "Task"],
  "targetField": "Custom.LegacyField"
}
```

#### Removing Confidential Information

```json
{
  "FieldMapType": "FieldClearMap",
  "ApplyTo": ["*"],
  "targetField": "Custom.InternalNotes"
}
```

#### Clearing Calculated Fields

```json
{
  "FieldMapType": "FieldClearMap",
  "ApplyTo": ["User Story"],
  "targetField": "Microsoft.VSTS.Scheduling.Effort"
}
```

## Validation Logic

The Field Clear Map includes robust validation to prevent migration errors:

### Field Existence Validation

- Checks if the target field exists on the work item type
- Logs debug message and skips if field doesn't exist
- Prevents runtime errors during migration

### Allowed Values Validation

- For fields with restricted value lists, verifies that null/empty is allowed
- Checks the `AllowedValues` collection of the field
- Ensures clearing won't violate field constraints

### Editability Validation

- Confirms the field is editable in the current work item state
- Some fields become read-only in certain states or conditions
- Prevents attempts to modify non-editable fields

### Required Field Protection

- Identifies required fields and prevents clearing them
- Required fields must have values, so clearing would cause validation errors
- Maintains data integrity during migration

## Field Types and Considerations

### String Fields

- Most string fields can be safely cleared
- Consider impact on reports and queries that depend on field values

### Numeric Fields

- Clearing numeric fields sets them to null (not zero)
- Some calculations may be affected by null values

### Date Fields

- Clearing date fields removes the date entirely
- Consider impact on workflows that depend on date values

### Choice Fields

- Fields with allowed values may not permit null values
- Validation logic handles these cases automatically

### System Fields

- Be cautious when clearing system fields
- Some system fields are required or calculated automatically

## Best Practices

### Strategic Field Selection

- Only clear fields that truly need to be removed
- Consider the impact on historical data and reporting
- Test with a subset of work items first

### Work Item Type Targeting

- Use `ApplyTo` to target specific work item types
- Different work item types may have different field requirements
- Avoid blanket clearing across all work item types

### Validation Awareness

- Understand which fields are required in your target process
- Check for custom validation rules that might be affected
- Review field dependencies and calculated field impacts

### Documentation

- Document which fields are being cleared and why
- Maintain records for audit and compliance purposes
- Consider creating a rollback plan if needed

## Error Prevention

The field map includes several safety mechanisms:

### Graceful Handling

- Logs informational messages when fields cannot be cleared
- Continues processing other field maps without failing
- Provides clear debugging information

### Validation Messages

- Field doesn't exist: Logged as debug message, processing continues
- Field not editable: Logged as debug message, processing continues  
- Field is required: Logged as debug message, processing continues
- Allowed values restriction: Logged as debug message, processing continues

### Safe Execution

- Never throws exceptions that would halt migration
- Always validates before attempting to clear
- Maintains data integrity throughout the process

## Performance Considerations

- Field Clear Maps are highly performant
- Validation checks are lightweight
- No complex data processing required
- Minimal impact on migration speed

## Schema

{{< class-schema >}}
