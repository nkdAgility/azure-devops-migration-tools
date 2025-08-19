---
title: Regex Field Map
description: |
  The Regex Field Map applies regular expression transformations to map values from a source field to a target field using pattern matching and replacement. This enables sophisticated text transformations, data extraction, and format standardization during migration.
dataFile: reference.fieldmaps.regexfieldmap.yaml
slug: regex-field-map
aliases:
  - /docs/Reference/FieldMaps/RegexFieldMap
  - /Reference/FieldMaps/RegexFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/RegexFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/RegexFieldMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2783
---

## Overview

The Regex Field Map provides powerful text transformation capabilities using regular expressions to modify field values during migration. It enables pattern-based matching and replacement operations, making it ideal for data cleanup, format standardization, and content extraction scenarios.

This field map is particularly valuable when you need to transform text data that follows specific patterns, extract portions of field content, or standardize formats across migrated work items.

{{< class-description >}}

## How It Works

The Regex Field Map performs pattern-based text transformation:

1. **Source Value Extraction**: Retrieves the value from the specified source field
2. **Pattern Matching**: Applies the configured regular expression pattern to check for matches
3. **Conditional Processing**: Only proceeds if the pattern matches the source value
4. **Text Replacement**: Uses the replacement pattern to transform matched content
5. **Target Assignment**: Sets the transformed value to the target field
6. **Dual Update**: Updates both WorkItemData and WorkItem instances for consistency

## Use Cases

This field map is essential for:

- **Data Cleanup**: Removing unwanted characters, formatting, or prefixes/suffixes
- **Format Standardization**: Converting between different data formats or conventions
- **Content Extraction**: Pulling specific information from larger text fields
- **URL Transformation**: Modifying links, references, or paths during migration
- **ID Format Changes**: Converting between different identifier formats
- **Text Normalization**: Standardizing text case, spacing, or punctuation
- **Legacy Data Migration**: Adapting old data formats to new system requirements

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Remove Prefixes

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["Bug"],
  "sourceField": "System.Title",
  "targetField": "System.Title",
  "pattern": "^(BUG|ISSUE):\\s*",
  "replacement": ""
}
```

#### Extract ID Numbers

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Custom.LegacyID",
  "targetField": "Custom.ExtractedID",
  "pattern": "ID-([0-9]+)",
  "replacement": "$1"
}
```

#### Standardize Formatting

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["User Story"],
  "sourceField": "System.Description",
  "targetField": "System.Description",
  "pattern": "\\b(TODO|todo|Todo)\\b",
  "replacement": "TO-DO"
}
```

#### Transform URLs

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Custom.DocumentLink",
  "targetField": "Custom.DocumentLink",
  "pattern": "https://oldserver\\.com/(.*)",
  "replacement": "https://newserver.com/$1"
}
```

#### Clean Phone Numbers

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Custom.ContactPhone",
  "targetField": "Custom.ContactPhone",
  "pattern": "[^0-9]",
  "replacement": ""
}
```

## Regular Expression Features

The Regex Field Map supports full .NET regular expression functionality:

### Pattern Matching

- **Literal Characters**: Match exact text
- **Character Classes**: `[abc]`, `[0-9]`, `\d`, `\w`, `\s`
- **Quantifiers**: `*`, `+`, `?`, `{n}`, `{n,m}`
- **Anchors**: `^` (start), `$` (end), `\b` (word boundary)
- **Groups**: `()` for capturing and non-capturing groups

### Replacement Patterns

- **Captured Groups**: `$1`, `$2`, etc. for referencing captured groups
- **Entire Match**: `$&` for the entire matched text
- **Before/After**: `$\`` (before match), `$'` (after match)
- **Literal Dollar**: `$$` for literal dollar sign

### Advanced Features

- **Case-Insensitive Matching**: Use `(?i)` at pattern start
- **Multiline Mode**: Use `(?m)` for line-by-line matching
- **Single Line Mode**: Use `(?s)` to make `.` match newlines
- **Non-Greedy Matching**: Use `*?`, `+?` for minimal matching

## Pattern Examples

### Data Extraction

Extract version numbers:
```regex
Pattern: "Version\\s+(\\d+\\.\\d+)"
Replacement: "$1"
```

Extract email domains:
```regex
Pattern: "@([\\w.-]+)"
Replacement: "$1"
```

### Format Standardization

Standardize date formats:
```regex
Pattern: "(\\d{1,2})/(\\d{1,2})/(\\d{4})"
Replacement: "$3-$1-$2"
```

Convert case:
```regex
Pattern: "\\b\\w"
Replacement: "${ToUpper($&)}" // Note: Use with caution
```

### Content Cleanup

Remove HTML tags:
```regex
Pattern: "<[^>]+>"
Replacement: ""
```

Normalize whitespace:
```regex
Pattern: "\\s+"
Replacement: " "
```

## Best Practices

### Pattern Design

- Test patterns with sample data before migration
- Use specific patterns rather than overly broad ones
- Consider edge cases and special characters
- Escape special regex characters in literal matches

### Performance Considerations

- Complex regex patterns can impact migration performance
- Avoid catastrophic backtracking with nested quantifiers
- Consider simpler alternatives for basic transformations
- Test performance with representative data volumes

### Safety and Validation

- Always validate transformed results
- Consider what happens when patterns don't match
- Ensure replacement patterns produce valid target field values
- Use capturing groups carefully to avoid unintended replacements

### Testing Strategy

- Test with a variety of input data
- Verify that non-matching values are handled correctly
- Check for potential data loss during transformation
- Validate that target field constraints are met

## Error Handling

The field map includes conditional processing:

### Pattern Matching

- Only processes values that match the regex pattern
- Non-matching values are left unchanged
- No errors thrown for non-matching patterns

### Field Validation

- Checks for source field existence and non-null values
- Verifies target field exists before assignment
- Graceful handling of missing or invalid fields

### Transformation Safety

- Maintains original data integrity when patterns don't match
- Updates both data representations consistently
- Logs transformation results for debugging

## Common Patterns

### URL Migration

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "System.Description", 
  "targetField": "System.Description",
  "pattern": "https://oldwiki\\.company\\.com/([^\\s]+)",
  "replacement": "https://newwiki.company.com/$1"
}
```

### Identifier Transformation

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "Custom.TicketReference",
  "targetField": "Custom.TicketReference", 
  "pattern": "TICK-(\\d+)",
  "replacement": "REF-$1"
}
```

### Text Cleanup

```json
{
  "FieldMapType": "RegexFieldMap",
  "ApplyTo": ["*"],
  "sourceField": "System.Title",
  "targetField": "System.Title",
  "pattern": "\\[RESOLVED\\]\\s*",
  "replacement": ""
}
```
## Schema

{{< class-schema >}}
