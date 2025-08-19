---
title: Field To Tag Field Map
description: |
  The Field To Tag Field Map converts field values into work item tags, enabling you to transform structured field data into a more flexible tagging system during migration.
dataFile: reference.fieldmaps.fieldtotagfieldmap.yaml
slug: field-to-tag-field-map
aliases:
  - /docs/Reference/FieldMaps/FieldToTagFieldMap
  - /Reference/FieldMaps/FieldToTagFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldToTagFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldToTagFieldMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2786
---

## Overview

The Field To Tag Field Map transforms structured field values into work item tags, providing a way to preserve important categorical information as tags when migrating between systems with different field schemas. This field map is particularly useful when moving from systems with numerous custom fields to more streamlined configurations.

Tags provide a flexible way to maintain searchable, filterable metadata without requiring specific field definitions in the target system.

{{< class-description >}}

## How It Works

The Field To Tag Field Map converts field data to tags:

1. **Source Field Reading**: Extracts the value from the specified source field
2. **Value Processing**: Processes the field value based on configuration (formatting, prefixing)
3. **Tag Creation**: Converts the processed value into a work item tag
4. **Tag Application**: Adds the tag to the work item's tag collection
5. **Duplicate Handling**: Ensures tags are not duplicated if they already exist

## Use Cases

This field map is commonly used for:

- **Custom Field Preservation**: Converting custom fields to tags when target system doesn't support them
- **Category Tagging**: Transforming category fields into searchable tags
- **Metadata Migration**: Preserving important metadata as tags for future reference
- **System Simplification**: Reducing field complexity while maintaining data visibility
- **Search Enhancement**: Making field data searchable through tag-based queries
- **Process Template Migration**: Adapting to target systems with different field schemas
- **Legacy Data Preservation**: Maintaining historical field data in tag format

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Simple Field to Tag

```json
{
  "FieldMapType": "FieldToTagFieldMap",
  "ApplyTo": ["Bug", "Task"],
  "sourceField": "Custom.Category"
}
```

#### Prefixed Tag Creation

```json
{
  "FieldMapType": "FieldToTagFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Microsoft.VSTS.Common.Priority",
  "tagPrefix": "Priority-"
}
```

#### Formatted Tag with Label

```json
{
  "FieldMapType": "FieldToTagFieldMap",
  "ApplyTo": ["User Story"],
  "sourceField": "Microsoft.VSTS.Scheduling.StoryPoints",
  "formatExpression": "Points: {0}"
}
```

#### Business Value Tagging

```json
{
  "FieldMapType": "FieldToTagFieldMap",
  "ApplyTo": ["Feature"],
  "sourceField": "Custom.BusinessValue",
  "tagPrefix": "BizValue-"
}
```

## Tag Formatting Options

### Simple Value Tags
- Direct conversion of field value to tag
- No formatting applied, uses raw field value
- Suitable for category-type fields

### Prefixed Tags
- Adds consistent prefix to all tags created by this map
- Helps organize and identify tags from specific sources
- Useful for grouping related tags

### Formatted Tags  
- Uses format expressions to create structured tag text
- Supports labels and structured information
- Enables more descriptive tag content

## Data Handling

### Field Value Processing
- Null or empty field values are skipped (no tag created)
- String values are used directly for tag creation
- Non-string values are converted to string representation

### Tag Validation
- Tags are validated against Azure DevOps tag requirements
- Invalid characters may be removed or replaced
- Long tag values may be truncated

### Duplicate Prevention
- Existing tags with the same value are not duplicated
- Tag comparison is case-sensitive
- Multiple field maps can contribute to the same work item's tags

## Best Practices

### Tag Naming Conventions
- Use consistent prefixes to group related tags
- Choose meaningful, searchable tag names
- Consider tag length limitations in Azure DevOps

### Field Selection
- Focus on fields that provide valuable categorical information
- Avoid converting frequently changing or numeric fields to tags
- Consider the impact on tag proliferation

### Migration Strategy
- Document which fields are being converted to tags
- Communicate tagging strategy to end users
- Plan for tag-based queries and reporting

## Common Scenarios

### Legacy Custom Field Migration

```json
[
  {
    "FieldMapType": "FieldToTagFieldMap",
    "ApplyTo": ["*"],
    "sourceField": "Custom.Department",
    "tagPrefix": "Dept-"
  },
  {
    "FieldMapType": "FieldToTagFieldMap",
    "ApplyTo": ["*"],
    "sourceField": "Custom.Component",
    "tagPrefix": "Component-"
  }
]
```

### Process Template Adaptation

```json
[
  {
    "FieldMapType": "FieldToTagFieldMap",
    "ApplyTo": ["Bug"],
    "sourceField": "Custom.FoundInEnvironment",
    "formatExpression": "Environment: {0}"
  },
  {
    "FieldMapType": "FieldToTagFieldMap",
    "ApplyTo": ["Bug"],
    "sourceField": "Custom.BugType",
    "tagPrefix": "Type-"
  }
]
```

### Metadata Preservation

```json
{
  "FieldMapType": "FieldToTagFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Custom.LegacyID",
  "formatExpression": "Legacy-{0}"
}
```

## Considerations

### Tag Management
- Tags are organization-wide in Azure DevOps
- Consider tag naming collisions across projects
- Plan for tag cleanup and maintenance

### Query Impact
- Tagged work items become searchable by tag values
- Consider performance impact of large tag sets
- Design tag structure for efficient querying

### User Training
- Educate users on new tagging conventions
- Provide guidance on tag-based searching
- Document tag meanings and usage

## Integration with Other Field Maps

The Field To Tag Field Map works well with other field maps:

- **Field Clear Map**: Clear original field after converting to tag
- **Field Value Map**: Transform values before converting to tags
- **Field Skip Map**: Preserve original field while adding tags

## Schema

{{< class-schema >}}
