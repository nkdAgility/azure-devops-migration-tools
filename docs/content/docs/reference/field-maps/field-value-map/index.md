---
title: Field Value Map
description: |
  The Field Value Map transforms field values based on a lookup table, allowing specific source values to be translated to different target values. This is essential for mapping between different value systems, process templates, and organizational standards.
dataFile: reference.fieldmaps.fieldvaluemap.yaml
slug: field-value-map
aliases:
  - /docs/Reference/FieldMaps/FieldValueMap
  - /Reference/FieldMaps/FieldValueMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldValueMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldValueMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2785
---

## Overview

The Field Value Map is a powerful field mapping tool that performs value translation using a configurable lookup table. It enables you to map specific source field values to corresponding target values, making it essential for migrating between systems with different value standards, process templates, or organizational conventions.

This field map is particularly valuable when the same concept is represented by different values in source and target systems, such as different state names, priority levels, or category classifications.

{{< class-description >}}

## How It Works

The Field Value Map follows a structured lookup process:

1. **Source Value Extraction**: Retrieves the value from the specified source field
2. **Null Value Handling**: Checks for null values and applies null-specific mapping if configured
3. **Lookup Table Search**: Searches the value mapping dictionary for an exact match
4. **Value Translation**: Applies the mapped target value if a match is found
5. **Default Value Fallback**: Uses the default value if no mapping match is found
6. **Type Conversion**: Converts the final value to the target field's data type
7. **Target Assignment**: Sets the target field with the converted value

## Use Cases

This field map is essential for:

- **State Mapping**: Translating workflow states between different process templates
- **Priority Standardization**: Converting between different priority value systems
- **Category Translation**: Mapping categories, types, or classifications
- **User/Team Mapping**: Converting user names or team assignments
- **Severity Mapping**: Translating severity levels between systems
- **Process Template Migration**: Adapting values when changing process templates
- **Localization**: Converting values between different language versions
- **Legacy Value Translation**: Updating deprecated values to current standards

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### State Mapping Between Process Templates

```json
{
  "FieldMapType": "FieldValueMap",
  "ApplyTo": ["User Story"],
  "sourceField": "System.State",
  "targetField": "System.State",
  "valueMapping": {
    "New": "To Do",
    "Active": "Doing", 
    "Resolved": "Done",
    "Closed": "Done"
  },
  "defaultValue": "To Do"
}
```

#### Priority Level Translation

```json
{
  "FieldMapType": "FieldValueMap",
  "ApplyTo": ["Bug", "Task"],
  "sourceField": "Microsoft.VSTS.Common.Priority",
  "targetField": "Microsoft.VSTS.Common.Priority", 
  "valueMapping": {
    "Critical": "1",
    "High": "2",
    "Medium": "3",
    "Low": "4"
  },
  "defaultValue": "3"
}
```

#### Category Standardization

```json
{
  "FieldMapType": "FieldValueMap",
  "ApplyTo": ["Bug"],
  "sourceField": "Custom.LegacyCategory",
  "targetField": "Custom.Category",
  "valueMapping": {
    "UI Bug": "User Interface",
    "Backend Issue": "Service Layer", 
    "DB Problem": "Database",
    "Security": "Security & Compliance"
  },
  "defaultValue": "General"
}
```

#### Handling Null Values

```json
{
  "FieldMapType": "FieldValueMap",
  "ApplyTo": ["*"],
  "sourceField": "System.AssignedTo",
  "targetField": "System.AssignedTo",
  "valueMapping": {
    "null": "Unassigned",
    "john.doe@oldcompany.com": "john.doe@newcompany.com",
    "jane.smith@oldcompany.com": "jane.smith@newcompany.com"
  },
  "defaultValue": "Unassigned"
}
```

#### Work Item Type Mapping

```json
{
  "FieldMapType": "FieldValueMap",
  "ApplyTo": ["*"],
  "sourceField": "System.WorkItemType",
  "targetField": "Custom.OriginalType",
  "valueMapping": {
    "User Story": "Story",
    "Product Backlog Item": "Story",
    "Issue": "Bug"
  },
  "defaultValue": "Unknown"
}
```

## Advanced Features

### Null Value Handling

The Field Value Map provides sophisticated null value handling:

- **Explicit Null Mapping**: Use `"null"` as a key in valueMapping to handle null source values
- **Null Detection**: Automatically detects both null values and fields with null content
- **Default Fallback**: Applies default value when null mapping is not specified

### Type Conversion

The field map automatically handles type conversions:

- **String to Numeric**: Converts mapped string values to numeric types for numeric fields
- **String to DateTime**: Parses date strings for date fields
- **Boolean Conversion**: Handles true/false mappings for boolean fields
- **Type Safety**: Uses `Convert.ChangeType()` for safe type conversion

### Case Sensitivity

Value mappings are case-sensitive by default:

- Ensure exact case matching between source values and mapping keys
- Consider creating mappings for different case variations if needed
- Use consistent casing conventions across your mappings

## Best Practices

### Comprehensive Mapping

- Map all possible source values to avoid relying solely on default values
- Include common variations and edge cases in your mappings
- Test with actual data to identify unmapped values

### Default Value Strategy

- Always provide meaningful default values
- Choose defaults that won't cause validation errors in the target system
- Consider using neutral or safe values as defaults

### Value Validation

- Verify that target values are valid for the target field
- Check allowed values lists for choice fields
- Ensure mapped values comply with target system validation rules

### Performance Considerations

- Field Value Maps are efficient for reasonable mapping table sizes
- Large mapping tables (hundreds of entries) may impact performance
- Consider alternative approaches for extremely large value sets

### Maintenance

- Document the rationale behind value mappings
- Keep mapping tables updated as systems evolve
- Regular review of unmapped values in migration logs

## Common Patterns

### State Workflow Migration

When migrating between different workflow configurations:

```json
[
  {
    "FieldMapType": "FieldValueMap",
    "ApplyTo": ["Bug"],
    "sourceField": "System.State",
    "targetField": "System.State",
    "valueMapping": {
      "New": "Proposed",
      "Active": "Active",
      "Resolved": "Resolved",
      "Closed": "Closed"
    },
    "defaultValue": "Proposed"
  },
  {
    "FieldMapType": "FieldValueMap", 
    "ApplyTo": ["Task"],
    "sourceField": "System.State",
    "targetField": "System.State",
    "valueMapping": {
      "New": "To Do",
      "Active": "In Progress",
      "Resolved": "Done",
      "Closed": "Done"
    },
    "defaultValue": "To Do"
  }
]
```

### User Assignment Migration

Mapping user accounts between organizations:

```json
{
  "FieldMapType": "FieldValueMap",
  "ApplyTo": ["*"],
  "sourceField": "System.AssignedTo",
  "targetField": "System.AssignedTo",
  "valueMapping": {
    "olduser1@company.com": "newuser1@company.com",
    "olduser2@company.com": "newuser2@company.com", 
    "contractor@external.com": "internal.contact@company.com"
  },
  "defaultValue": ""
}
```

## Error Handling

The field map includes robust error handling:

### Value Lookup

- Missing mapping entries fall back to default value
- Logs debug information for successful mappings
- No errors thrown for unmapped values

### Type Conversion

- Uses .NET's `Convert.ChangeType()` for safe conversion
- Handles conversion failures gracefully
- Maintains data integrity during type transformation

### Field Validation

- Target system validation rules still apply after mapping
- Invalid mapped values may cause save errors
- Consider target field constraints when designing mappings

## Schema

{{< class-schema >}}
