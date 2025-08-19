---
title: Field To Field Map
description: |
  The Field To Field Map performs direct field-to-field copying from source to target work items, with optional default value substitution for empty or null values. This is the most commonly used field map for straightforward field mappings.
dataFile: reference.fieldmaps.fieldtofieldmap.yaml
slug: field-to-field-map
aliases:
  - /docs/Reference/FieldMaps/FieldToFieldMap
  - /Reference/FieldMaps/FieldToFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldToFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldToFieldMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2788
---

## Overview

The Field To Field Map is the most fundamental and widely used field map in the Azure DevOps Migration Tools. It provides direct, one-to-one mapping between source and target fields, with the ability to specify default values when source fields are empty or null.

This field map forms the backbone of most migration configurations, handling the majority of straightforward field copying scenarios while providing flexibility for handling missing or empty data.

{{< class-description >}}

## How It Works

The Field To Field Map operates with simple but effective logic:

1. **Field Validation**: Checks that both source and target fields exist on their respective work items
2. **Value Extraction**: Retrieves the value from the source field and converts it to a string
3. **Default Value Handling**: If the source value is null or empty, applies the configured default value (if specified)
4. **Target Assignment**: Sets the target field to the processed value
5. **Logging**: Records the field mapping operation for debugging and tracking

## Use Cases

This field map is essential for:

- **Direct Field Mapping**: Copying values between fields with the same purpose but different names
- **Cross-System Migration**: Mapping fields between different Azure DevOps organizations with varying field schemas
- **Process Template Changes**: Adapting to different process templates with renamed fields
- **Default Value Application**: Ensuring fields have valid values even when source data is missing
- **Simple Data Transformation**: Converting field values to strings for target compatibility
- **Legacy Field Migration**: Moving data from deprecated fields to new standard fields

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Simple Field Mapping

```json
{
  "FieldMapType": "FieldToFieldMap",
  "ApplyTo": ["Bug", "Task"],
  "sourceField": "System.Title",
  "targetField": "System.Title"
}
```

#### Cross-Organization Field Mapping

```json
{
  "FieldMapType": "FieldToFieldMap",
  "ApplyTo": ["User Story"],
  "sourceField": "Microsoft.VSTS.Scheduling.StoryPoints",
  "targetField": "Microsoft.VSTS.Scheduling.Effort"
}
```

#### Mapping with Default Value

```json
{
  "FieldMapType": "FieldToFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Microsoft.VSTS.Common.Priority",
  "targetField": "Microsoft.VSTS.Common.Priority",
  "defaultValue": "2"
}
```

#### Custom Field Migration

```json
{
  "FieldMapType": "FieldToFieldMap",
  "ApplyTo": ["Bug"],
  "sourceField": "Custom.LegacyCategory",
  "targetField": "Custom.NewCategory",
  "defaultValue": "Unassigned"
}
```

#### Area Path Mapping

```json
{
  "FieldMapType": "FieldToFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "System.AreaPath",
  "targetField": "System.AreaPath",
  "defaultValue": "MyProject\\Default Area"
}
```

## Field Type Compatibility

The Field To Field Map handles various field type combinations:

### String to String

- Direct copying of text values
- Most common and straightforward mapping scenario
- Supports all text-based fields

### Numeric to String

- Automatic conversion from numbers to text representation
- Useful when target field type differs from source
- Preserves numeric values as readable text

### Date to String

- Converts date values to string representation
- Format depends on system locale and field configuration
- Consider using specific date formatting if needed

### Choice to Choice

- Maps between fields with predefined value lists
- Ensure target field accepts the source values
- Consider using FieldValueMap for value transformation

### Mixed Type Handling

- All source values are converted to strings before assignment
- Target field validation still applies
- Complex types may need specialized field maps

## Default Value Behavior

The default value feature provides flexible handling of missing data:

### When Default Values Apply

- Source field value is null
- Source field value is an empty string
- Source field contains only whitespace (treated as empty)

### When Default Values Don't Apply

- Source field has any non-empty value
- Source field contains zero (for numeric fields)
- Source field contains false (for boolean fields)

### Default Value Types

- **Strings**: Most common, used for text fields
- **Numbers**: Specified as strings but converted appropriately
- **Dates**: Should be in recognizable date format
- **Empty String**: Use `""` to explicitly set empty values

## Error Handling and Validation

The field map includes basic validation and error handling:

### Field Existence

- Silently skips mapping if source field doesn't exist
- Silently skips mapping if target field doesn't exist
- No error thrown, processing continues with other field maps

### Value Conversion

- All source values converted to strings using `Convert.ToString()`
- Handles null values gracefully
- Preserves original data representation where possible

### Target Field Validation

- Target system validation rules still apply
- Invalid values may cause migration errors during save
- Consider target field constraints when mapping

## Best Practices

### Field Reference Names

- Always use field reference names (e.g., `System.Title`, not `Title`)
- Verify field names exist in both source and target systems
- Use Azure DevOps REST API or process template to confirm field names

### Default Value Strategy

- Provide default values for fields that can't be empty in target system
- Use meaningful defaults that won't confuse users
- Consider using organization-specific default conventions

### Work Item Type Targeting

- Use `ApplyTo` to target specific work item types
- Different work item types may have different field availability
- Avoid mapping fields that don't exist on certain work item types

### Testing and Validation

- Test field mappings with sample work items first
- Verify that target fields accept mapped values
- Check for data truncation in fields with length limits

### Performance Considerations

- Field To Field Maps are highly performant
- Large numbers of field maps may impact migration speed
- Consider grouping related mappings for better organization

## Common Patterns

### Process Template Migration

When migrating between different process templates:

```json
[
  {
    "FieldMapType": "FieldToFieldMap",
    "ApplyTo": ["User Story"],
    "sourceField": "Microsoft.VSTS.Scheduling.StoryPoints",
    "targetField": "Microsoft.VSTS.Scheduling.Effort",
    "defaultValue": "0"
  },
  {
    "FieldMapType": "FieldToFieldMap", 
    "ApplyTo": ["User Story"],
    "sourceField": "Microsoft.VSTS.Common.AcceptanceCriteria",
    "targetField": "System.Description"
  }
]
```

### Custom Field Preservation

Maintaining custom field data across organizations:

```json
{
  "FieldMapType": "FieldToFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Custom.BusinessValue",
  "targetField": "Custom.BusinessValue",
  "defaultValue": "Medium"
}
```

## Schema

{{< class-schema >}}
