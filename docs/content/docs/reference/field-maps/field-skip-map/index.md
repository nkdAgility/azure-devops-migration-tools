---
title: Field Skip Map
description: |
  The Field Skip Map ensures that specific target fields remain unchanged during migration by restoring their original values. This is useful for preserving existing data in target fields that should not be modified during the migration process.
dataFile: reference.fieldmaps.fieldskipmap.yaml
slug: field-skip-map
aliases:
  - /docs/Reference/FieldMaps/FieldSkipMap
  - /Reference/FieldMaps/FieldSkipMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldSkipMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldSkipMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2789
---

## Overview

The Field Skip Map is a specialized field map that prevents changes to specific target fields during migration by restoring their original values. This field map is essential when you need to preserve existing data in certain fields that should remain untouched during the migration process.

Unlike other field maps that transform or copy data, the Field Skip Map actively protects target field values from being modified, making it useful for maintaining data integrity in specific scenarios.

{{< class-description >}}

## How It Works

The Field Skip Map follows a validation and restoration process:

1. **Field Existence Check**: Verifies that the target field exists on the work item
2. **Validation Checks**: Ensures the field can be safely processed (not restricted, editable, and not required)
3. **Original Value Restoration**: Sets the field value back to its original value, effectively undoing any changes
4. **Logging**: Records the skip operation for tracking and debugging

## Use Cases

This field map is commonly used for:

- **Preserving System Fields**: Keeping system-generated values that shouldn't be overwritten
- **Maintaining Audit Data**: Protecting audit trails and historical information
- **Selective Migration**: Excluding specific fields from migration while processing others
- **Data Protection**: Ensuring critical business fields remain unchanged
- **Incremental Updates**: Preventing overwrites of fields that have been manually updated
- **Custom Business Logic**: Preserving fields that follow organization-specific rules

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Skip System Created Date

```json
{
  "FieldMapType": "FieldSkipMap",
  "ApplyTo": ["*"],
  "targetField": "System.CreatedDate"
}
```

#### Preserve Custom Business Field

```json
{
  "FieldMapType": "FieldSkipMap", 
  "ApplyTo": ["Bug", "Task"],
  "targetField": "Custom.BusinessCriticality"
}
```

#### Skip Modified By Field

```json
{
  "FieldMapType": "FieldSkipMap",
  "ApplyTo": ["*"],
  "targetField": "System.ChangedBy"
}
```

#### Protect Priority Override

```json
{
  "FieldMapType": "FieldSkipMap",
  "ApplyTo": ["User Story"],
  "targetField": "Custom.PriorityOverride"
}
```

## Field Validation

The Field Skip Map includes comprehensive validation to ensure safe operation:

### Allowed Values Check
- For fields with restricted value lists, verifies the original value is still valid
- Prevents restoration of values that are no longer allowed
- Logs validation failures for debugging

### Editability Verification
- Ensures the target field is editable before attempting to restore values
- Skips read-only or system-controlled fields
- Prevents errors during field assignment

### Required Field Protection
- Avoids processing required fields that cannot be safely modified
- Prevents potential validation errors during work item save
- Maintains data integrity for mandatory fields

## When to Use Field Skip Map

### Recommended Scenarios
- **System Fields**: Fields like Created Date, Created By that should preserve original values
- **Audit Fields**: Fields that track important business or compliance information
- **Manual Overrides**: Fields that users have manually updated and shouldn't be overwritten
- **Calculated Fields**: Fields that are computed and shouldn't be directly modified

### Alternative Approaches
- **Exclude from Migration**: Simply don't include the field in any field maps
- **Conditional Logic**: Use other field maps with conditions to selectively skip
- **Post-Migration Updates**: Handle field preservation through separate processes

## Best Practices

### Field Selection
- Use specific work item types in `ApplyTo` to target exactly which items need protection
- Avoid using `["*"]` unless the field truly needs protection across all work item types
- Consider the impact on migration completeness when skipping fields

### Migration Strategy
- Document which fields are being skipped and why
- Communicate with stakeholders about preserved vs. migrated data
- Test skip behavior with sample work items before full migration

### Validation
- Verify that skipped fields maintain appropriate values
- Check that business processes still function with preserved data
- Monitor for any unintended side effects of field preservation

## Error Handling

### Field Validation Failures
- Missing target fields are silently skipped with debug logging
- Invalid allowed values prevent restoration and log warnings
- Non-editable fields are bypassed without errors

### Processing Continuation
- Field validation failures don't stop the migration process
- Other field maps continue to execute normally
- Debug logs provide visibility into skip operations

## Common Patterns

### System Field Protection

```json
[
  {
    "FieldMapType": "FieldSkipMap",
    "ApplyTo": ["*"],
    "targetField": "System.CreatedDate"
  },
  {
    "FieldMapType": "FieldSkipMap", 
    "ApplyTo": ["*"],
    "targetField": "System.CreatedBy"
  }
]
```

### Business Logic Preservation

```json
[
  {
    "FieldMapType": "FieldSkipMap",
    "ApplyTo": ["Bug"],
    "targetField": "Custom.EscalationFlag"
  },
  {
    "FieldMapType": "FieldSkipMap",
    "ApplyTo": ["Task"],
    "targetField": "Custom.ApprovalStatus"
  }
]
```

## Schema

{{< class-schema >}}
