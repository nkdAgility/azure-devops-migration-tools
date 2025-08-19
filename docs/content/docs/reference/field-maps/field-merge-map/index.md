---
title: Field Merge Map
description: |
  The Field Merge Map merges values from multiple source fields into a single target field using a configurable format template. This enables combining related information from separate fields into consolidated data.
dataFile: reference.fieldmaps.fieldmergemap.yaml
slug: field-merge-map
aliases:
  - /docs/Reference/FieldMaps/FieldMergeMap
  - /Reference/FieldMaps/FieldMergeMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldMergeMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldMergeMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2682
---

## Overview

The Field Merge Map combines values from multiple source fields into a single target field using a configurable format template. This powerful field map enables you to consolidate related information, combine separate data elements, or create formatted composite fields during migration.

This is particularly useful when migrating from systems where related information is stored in separate fields but needs to be combined in the target system, or when creating summary fields that incorporate multiple data points.

{{< class-description >}}

## How It Works

The Field Merge Map follows a straightforward merging process:

1. **Source Field Collection**: Retrieves values from all specified source fields
2. **Null Value Handling**: Replaces null or missing values with empty strings
3. **Value Array Creation**: Builds an array of string values in the order of source fields
4. **Format Application**: Applies the format expression using standard string formatting
5. **Target Assignment**: Sets the target field with the formatted result

## Use Cases

This field map is commonly used for:

- **Contact Information**: Combining separate name fields into full names
- **Address Consolidation**: Merging address components into complete addresses
- **Description Enhancement**: Combining multiple descriptive fields into comprehensive descriptions
- **Summary Fields**: Creating summary information from multiple data points
- **Legacy Data Migration**: Consolidating fields that were separate in legacy systems
- **Reporting Fields**: Creating formatted fields for reporting purposes
- **User Display Names**: Combining user information into display-friendly formats

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Simple Name Combination

```json
{
  "FieldMapType": "FieldMergeMap",
  "ApplyTo": ["*"],
  "sourceFields": [
    "Custom.FirstName", 
    "Custom.LastName"
  ],
  "targetField": "Custom.FullName",
  "formatExpression": "{0} {1}"
}
```

#### Detailed Description Merge

```json
{
  "FieldMapType": "FieldMergeMap",
  "ApplyTo": ["Bug"],
  "sourceFields": [
    "System.Description",
    "Custom.StepsToReproduce", 
    "Custom.ExpectedResult"
  ],
  "targetField": "System.Description",
  "formatExpression": "{0}\n\nSteps to Reproduce:\n{1}\n\nExpected Result:\n{2}"
}
```

#### Address Consolidation

```json
{
  "FieldMapType": "FieldMergeMap",
  "ApplyTo": ["*"],
  "sourceFields": [
    "Custom.Street",
    "Custom.City",
    "Custom.State",
    "Custom.ZipCode"
  ],
  "targetField": "Custom.FullAddress",
  "formatExpression": "{0}, {1}, {2} {3}"
}
```

#### Contact Information

```json
{
  "FieldMapType": "FieldMergeMap",
  "ApplyTo": ["*"],
  "sourceFields": [
    "Custom.Email",
    "Custom.Phone"
  ],
  "targetField": "Custom.ContactInfo",
  "formatExpression": "Email: {0} | Phone: {1}"
}
```

## Format Expression Syntax

The format expression uses standard .NET string formatting:

### Basic Placeholders

- `{0}` - First source field value
- `{1}` - Second source field value  
- `{n}` - nth source field value (zero-indexed)

### Common Patterns

#### Simple Concatenation

```text
formatExpression: "{0} {1}"
```

#### Labeled Format

```text
formatExpression: "Name: {0}, Department: {1}"
```

#### Multi-line Format

```text
formatExpression: "{0}\n\nAdditional Info:\n{1}"
```

#### Conditional-like Format

```text
formatExpression: "{0} (Priority: {1}, Severity: {2})"
```

## Data Handling

### Null and Empty Values

- Null source field values are converted to empty strings
- Empty strings are preserved in the formatting
- Missing fields are treated as empty strings

### Field Order

- Source fields are processed in the order specified in the `sourceFields` array
- Format placeholders correspond to array positions (0-indexed)
- Ensure format expression matches the number of source fields

### Target Field Considerations

- Target field must be able to accept string values
- Consider target field length limitations
- Large merged content may be truncated

## Best Practices

### Format Design
- Use clear, readable format expressions
- Include appropriate separators and labels
- Consider how the merged content will be displayed

### Field Selection
- Choose source fields that contain related information
- Ensure source fields exist on the specified work item types
- Consider field data types and content

### Content Length
- Be aware of target field length restrictions
- Test with realistic data to ensure content fits
- Consider truncation strategies for long content

### Performance Considerations
- Field merging is performed for each work item
- Complex format expressions may impact performance
- Consider the number of fields being merged

## Common Scenarios

### Legacy System Migration

When migrating from systems with separate contact fields:

```json
{
  "FieldMapType": "FieldMergeMap",
  "ApplyTo": ["*"],
  "sourceFields": [
    "Custom.ContactFirstName",
    "Custom.ContactLastName",
    "Custom.ContactEmail",
    "Custom.ContactPhone"
  ],
  "targetField": "Custom.ContactInformation",
  "formatExpression": "{0} {1} - {2} | {3}"
}
```

### Requirements Documentation

Combining multiple requirement fields:

```json
{
  "FieldMapType": "FieldMergeMap",
  "ApplyTo": ["User Story"],
  "sourceFields": [
    "Custom.UserStory",
    "Custom.AcceptanceCriteria",
    "Custom.Notes"
  ],
  "targetField": "System.Description",
  "formatExpression": "User Story:\n{0}\n\nAcceptance Criteria:\n{1}\n\nNotes:\n{2}"
}
```

## Error Handling

### Field Validation
- Missing source fields are treated as empty strings
- No error is thrown for non-existent fields
- Target field must exist or assignment will fail

### Format Validation
- Invalid format expressions may cause runtime errors
- Ensure placeholder count matches source field count
- Test format expressions with sample data


## Schema

{{< class-schema >}}
