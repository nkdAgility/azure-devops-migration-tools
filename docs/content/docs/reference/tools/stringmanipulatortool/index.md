---
title: String Manipulator Tool
description: Processes and cleans up string fields in work items by applying regex patterns, length limitations, and text transformations. Essential for data cleanup and standardization during migration.
dataFile: reference.tools.stringmanipulatortool.yaml
schemaFile: schema.tools.stringmanipulatortool.json
slug: string-manipulator-tool
aliases:
  - /docs/Reference/Tools/StringManipulatorTool
  - /Reference/Tools/StringManipulatorTool
  - /learn/azure-devops-migration-tools/Reference/Tools/StringManipulatorTool
  - /learn/azure-devops-migration-tools/Reference/Tools/StringManipulatorTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2643
---

## Overview

The String Manipulator Tool provides powerful text processing capabilities for work item migration. It applies configurable string manipulations to all text fields in work items, enabling data cleanup, standardization, and format corrections during the migration process.

The tool processes string fields through a series of regex-based manipulators that can remove invalid characters, standardize formats, replace text patterns, and enforce field length limits. Each manipulation is applied in sequence and can be individually enabled or disabled.

### How It Works

The String Manipulator Tool operates on all string fields within work items during migration:

1. **Field Processing**: The tool identifies all string-type fields in each work item
2. **Sequential Application**: Each configured manipulator is applied in the order defined in the configuration
3. **Regex Transformations**: Pattern-based replacements using regular expressions
4. **Length Enforcement**: Truncates fields that exceed the maximum allowed length
5. **Conditional Execution**: Each manipulator can be individually enabled or disabled

The tool is automatically invoked by migration processors and applies transformations before work items are saved to the target system.

### Use Cases

Common scenarios where the String Manipulator Tool is essential:

- **Data Cleanup**: Removing invalid Unicode characters, control characters, or formatting artifacts
- **Format Standardization**: Converting text patterns to consistent formats
- **Length Compliance**: Ensuring field values don't exceed target system limits
- **Character Encoding**: Fixing encoding issues from legacy systems
- **Pattern Replacement**: Updating URLs, paths, or references to match target environment

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The String Manipulator Tool is configured with an array of manipulators, each defining a specific text transformation:

```json
{
  "StringManipulatorTool": {
    "Enabled": true,
    "MaxStringLength": 1000000,
    "Manipulators": [
      {
        "$type": "RegexStringManipulator",
        "Enabled": true,
        "Description": "Remove invalid characters",
        "Pattern": "[^\\x20-\\x7E\\r\\n\\t]",
        "Replacement": ""
      }
    ]
  }
}
```

### Complex Examples

#### Manipulator Types

Currently, the tool supports the following manipulator types:

- **RegexStringManipulator**: Applies regular expression pattern matching and replacement

#### Manipulator Properties

Each manipulator supports these properties:

- **$type**: Specifies the manipulator type (e.g., "RegexStringManipulator")
- **Enabled**: Boolean flag to enable/disable this specific manipulator
- **Description**: Human-readable description of what the manipulator does
- **Pattern**: Regular expression pattern to match text
- **Replacement**: Text to replace matched patterns (can be empty string for removal)

## Common Scenarios

### Removing Invalid Characters

Remove non-printable characters that may cause issues in the target system:

```json
{
  "$type": "RegexStringManipulator",
  "Description": "Remove invalid characters from the end of the string",
  "Enabled": true,
  "Pattern": "[^( -~)\n\r\t]+",
  "Replacement": ""
}
```

### Standardizing Line Endings

Convert all line endings to a consistent format:

```json
{
  "$type": "RegexStringManipulator",
  "Description": "Standardize line endings to CRLF",
  "Enabled": true,
  "Pattern": "\r\n|\n|\r",
  "Replacement": "\r\n"
}
```

### Cleaning HTML Content

Remove or clean HTML tags from text fields:

```json
{
  "$type": "RegexStringManipulator",
  "Description": "Remove HTML tags",
  "Enabled": true,
  "Pattern": "<[^>]*>",
  "Replacement": ""
}
```

### Fixing Encoding Issues

Replace common encoding artifacts:

```json
{
  "$type": "RegexStringManipulator",
  "Description": "Fix common encoding issues",
  "Enabled": true,
  "Pattern": "â€™|â€œ|â€\u009d",
  "Replacement": "'"
}
```

## Good Practices

### Pattern Testing

- **Test regex patterns** thoroughly before applying to production data
- **Use regex testing tools** to validate patterns against sample data
- **Consider edge cases** and unintended matches in your patterns

### Performance Considerations

- **Order manipulators efficiently**: Place simpler patterns before complex ones
- **Use specific patterns**: Avoid overly broad regex that may match unintended content
- **Consider field length**: Set appropriate `MaxStringLength` to prevent excessive processing

### Data Safety

- **Backup source data**: Always maintain backups before applying string manipulations
- **Test with sample data**: Validate manipulations on a subset before full migration
- **Review results**: Check processed fields to ensure transformations are correct

### Configuration Management

- **Document patterns**: Include clear descriptions for each manipulator
- **Version control**: Maintain configuration files in version control
- **Incremental changes**: Test one manipulator at a time when developing complex transformations

## Troubleshooting

### Common Issues

**Manipulations Not Applied:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that individual manipulators are enabled
- Review regex patterns for syntax errors
- Ensure the tool is configured in the processor's tool list

**Unexpected Results:**

- Test regex patterns in isolation with sample data
- Check the order of manipulators (they execute sequentially)
- Verify escape sequences in JSON configuration
- Review field content before and after processing

**Performance Issues:**

- Consider reducing `MaxStringLength` if processing very large fields
- Optimize regex patterns to avoid catastrophic backtracking
- Disable unnecessary manipulators
- Process smaller batches of work items

**Regex Pattern Errors:**

- Validate regex syntax using online tools or testing utilities
- Escape special characters properly in JSON configuration
- Consider case sensitivity requirements
- Test patterns against various input scenarios

## Schema

{{< class-schema >}}
