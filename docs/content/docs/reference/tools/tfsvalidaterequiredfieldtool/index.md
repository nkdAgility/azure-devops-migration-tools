---
title: TFS Validate Required Field Tool
description: Validates that required fields are properly configured and populated during migration to prevent work item creation failures in the target system.
dataFile: reference.tools.tfsvalidaterequiredfieldtool.yaml
schemaFile: schema.tools.tfsvalidaterequiredfieldtool.json
slug: tfs-validate-required-field-tool
aliases:
  - /docs/Reference/Tools/TfsValidateRequiredFieldTool
  - /Reference/Tools/TfsValidateRequiredFieldTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsValidateRequiredFieldTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsValidateRequiredFieldTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2803
---

## Overview

The TFS Validate Required Field Tool validates that required fields are properly configured and populated during migration. This tool prevents work item creation failures by ensuring all mandatory fields have valid values before attempting to create work items in the target system.

The tool is essential for ensuring migration success by identifying and resolving field validation issues before they cause work item creation to fail.

### How It Works

The TFS Validate Required Field Tool operates during work item migration validation:

1. **Field Analysis**: Analyzes target work item types to identify required fields
2. **Value Validation**: Validates that required fields have appropriate values
3. **Missing Field Detection**: Identifies work items with missing required field values
4. **Default Value Application**: Applies default values to required fields when configured
5. **Validation Reporting**: Reports validation issues for manual resolution

The tool integrates with work item migration processors to ensure data quality before migration.

### Use Cases

Common scenarios where the TFS Validate Required Field Tool is essential:

- **Migration Quality Assurance**: Ensuring all work items will migrate successfully
- **Required Field Compliance**: Meeting target system field requirements
- **Data Completeness**: Identifying incomplete work item data before migration
- **Process Template Differences**: Handling different required fields between systems
- **Migration Failure Prevention**: Avoiding work item creation failures during migration

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Validate Required Field Tool provides configuration options for field validation:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": true,
    "RequiredFieldDefaultValues": {
      "Microsoft.VSTS.Common.Priority": "2",
      "Microsoft.VSTS.Common.Severity": "3 - Medium"
    }
  }
}
```

### Complex Examples

#### Comprehensive Field Validation

Complete validation with multiple default values:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": true,
    "RequiredFieldDefaultValues": {
      "Microsoft.VSTS.Common.Priority": "2",
      "Microsoft.VSTS.Common.Severity": "3 - Medium",
      "Microsoft.VSTS.Common.Triage": "Pending",
      "System.AreaPath": "DefaultProject\\General",
      "System.IterationPath": "DefaultProject\\Iteration 1"
    }
  }
}
```

#### Validation Only Mode

Validate without applying defaults for reporting purposes:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": false,
    "RequiredFieldDefaultValues": {}
  }
}
```

## Common Scenarios

### Complete Validation with Defaults

Validate and apply default values for missing required fields:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": true,
    "RequiredFieldDefaultValues": {
      "Microsoft.VSTS.Common.Priority": "2",
      "Microsoft.VSTS.Common.Severity": "3 - Medium"
    }
  }
}
```

### Priority and Severity Defaults

Configure defaults for common required fields:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": true,
    "RequiredFieldDefaultValues": {
      "Microsoft.VSTS.Common.Priority": "3",
      "Microsoft.VSTS.Common.Severity": "4 - Low",
      "Microsoft.VSTS.Common.BusinessValue": "0"
    }
  }
}
```

### Path Field Validation

Ensure area and iteration path requirements are met:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": true,
    "RequiredFieldDefaultValues": {
      "System.AreaPath": "MigratedProject\\General",
      "System.IterationPath": "MigratedProject\\Sprint 1"
    }
  }
}
```

### Custom Field Defaults

Handle custom required fields with appropriate defaults:

```json
{
  "TfsValidateRequiredFieldTool": {
    "Enabled": true,
    "ValidateRequiredFields": true,
    "ApplyDefaultValues": true,
    "RequiredFieldDefaultValues": {
      "Custom.Department": "IT",
      "Custom.Customer": "Internal",
      "Custom.Component": "General"
    }
  }
}
```

## Good Practices

### Field Validation Strategy

- **Analyze Target Requirements**: Understand target system required field requirements
- **Map Field Differences**: Identify differences in required fields between systems
- **Default Value Selection**: Choose appropriate default values that make business sense
- **Testing Validation**: Test validation rules with sample work items

### Default Value Management

- **Business Alignment**: Ensure default values align with business processes
- **Data Quality**: Use meaningful defaults rather than placeholder values
- **Stakeholder Input**: Get stakeholder input on appropriate default values
- **Documentation**: Document default value choices and rationale

### Configuration Management

- **Environment Specific**: Use environment-specific default values when appropriate
- **Regular Review**: Regularly review and update default values as processes evolve
- **Validation Testing**: Test validation configuration before full migration
- **Backup Strategy**: Maintain backup of validation configurations

## Troubleshooting

### Common Issues

**Validation Failures:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that `ValidateRequiredFields` is set to true
- Ensure all required fields in target system are identified
- Verify field reference names are correct

**Default Value Problems:**

- Check that `ApplyDefaultValues` is enabled
- Verify default values match target field value formats
- Ensure default values are valid for the field type
- Check for special characters or formatting requirements

**Field Reference Issues:**

- Use correct field reference names (e.g., "Microsoft.VSTS.Common.Priority")
- Verify field exists in target work item type
- Check for case sensitivity in field names
- Ensure custom fields use proper naming conventions

**Migration Failures:**

- Validate that all required fields have values after processing
- Check for fields that require specific value formats
- Verify area and iteration paths exist in target system
- Ensure user fields reference valid users in target system

### Performance Considerations

- **Large Work Item Counts**: Monitor performance with extensive work item validation
- **Complex Validation Rules**: Consider impact of complex validation logic
- **Default Value Processing**: Account for time needed to apply default values
- **Validation Reporting**: Balance validation detail with performance requirements

### Data Quality Validation

- **Field Value Formats**: Ensure default values match expected formats
- **Business Rule Compliance**: Verify defaults comply with business rules
- **Cross-Field Dependencies**: Consider dependencies between required fields
- **Process Template Alignment**: Align validation with target process template requirements

## Schema

{{< class-schema >}}
