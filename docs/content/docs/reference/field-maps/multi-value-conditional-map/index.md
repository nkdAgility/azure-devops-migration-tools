---
title: Multi Value Conditional Map
description: |
  The Multi Value Conditional Map applies conditional logic to map field values based on multiple criteria, enabling complex value transformations that depend on combinations of field values and conditions.
dataFile: reference.fieldmaps.multivalueconditionalmap.yaml
slug: multi-value-conditional-map
aliases:
  - /docs/Reference/FieldMaps/MultiValueConditionalMap
  - /Reference/FieldMaps/MultiValueConditionalMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/MultiValueConditionalMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/MultiValueConditionalMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2784
---

## Overview

The Multi Value Conditional Map provides advanced conditional mapping capabilities that allow you to transform field values based on complex criteria involving multiple source fields. This field map enables sophisticated business logic during migration, where the target value depends on combinations of source field values and conditional expressions.

This field map is essential when simple one-to-one mappings are insufficient and you need to implement business rules that consider multiple data points to determine the appropriate target value.

{{< class-description >}}

## How It Works

The Multi Value Conditional Map evaluates conditions and applies transformations:

1. **Condition Evaluation**: Examines multiple source fields against defined conditions
2. **Logic Processing**: Applies conditional logic (AND, OR, NOT operations)
3. **Value Determination**: Selects the appropriate target value based on condition results
4. **Target Assignment**: Sets the target field with the determined value
5. **Default Handling**: Falls back to default value if no conditions match

## Use Cases

This field map is ideal for:

- **Business Rule Implementation**: Applying complex organizational business logic during migration
- **Multi-Field Dependencies**: Creating mappings that depend on combinations of field values
- **Priority Calculations**: Determining priority based on multiple factors
- **Status Derivation**: Computing work item status from multiple source conditions
- **Category Assignment**: Assigning categories based on complex criteria
- **Risk Assessment**: Calculating risk levels from multiple input factors
- **Compliance Mapping**: Ensuring data meets regulatory requirements based on multiple conditions

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Priority Based on Severity and Impact

```json
{
  "FieldMapType": "MultiValueConditionalMap",
  "ApplyTo": ["Bug"],
  "conditions": [
    {
      "expression": "Severity = 'Critical' AND Impact = 'High'",
      "value": "1"
    },
    {
      "expression": "Severity = 'High' OR Impact = 'High'", 
      "value": "2"
    },
    {
      "expression": "Severity = 'Medium'",
      "value": "3"
    }
  ],
  "targetField": "Microsoft.VSTS.Common.Priority",
  "defaultValue": "4"
}
```

#### Category Assignment

```json
{
  "FieldMapType": "MultiValueConditionalMap",
  "ApplyTo": ["Task"],
  "conditions": [
    {
      "expression": "AreaPath CONTAINS 'Frontend' AND TaskType = 'Development'",
      "value": "UI-Development"
    },
    {
      "expression": "AreaPath CONTAINS 'Backend' AND TaskType = 'Development'",
      "value": "API-Development" 
    },
    {
      "expression": "TaskType = 'Testing'",
      "value": "Quality-Assurance"
    }
  ],
  "targetField": "Custom.WorkCategory",
  "defaultValue": "General"
}
```

## Condition Syntax

The field map supports various conditional operators and expressions:

### Comparison Operators
- `=` or `==` - Equality
- `!=` or `<>` - Inequality  
- `>` - Greater than
- `<` - Less than
- `>=` - Greater than or equal
- `<=` - Less than or equal

### Logical Operators
- `AND` - Logical AND
- `OR` - Logical OR
- `NOT` - Logical NOT

### String Operations
- `CONTAINS` - Check if string contains substring
- `STARTSWITH` - Check if string starts with value
- `ENDSWITH` - Check if string ends with value

### Value Types
- String values in single quotes: `'Active'`
- Numeric values: `1`, `2.5`
- Boolean values: `true`, `false`
- Field references: `FieldName`

## Advanced Scenarios

### Complex Business Logic

```json
{
  "FieldMapType": "MultiValueConditionalMap",
  "ApplyTo": ["User Story"],
  "conditions": [
    {
      "expression": "StoryPoints > 8 AND BusinessValue = 'High' AND Risk = 'Low'",
      "value": "Epic-Candidate"
    },
    {
      "expression": "StoryPoints <= 3 AND Effort = 'Small'",
      "value": "Quick-Win"
    },
    {
      "expression": "BusinessValue = 'High' AND (Risk = 'Medium' OR Risk = 'High')",
      "value": "High-Risk-High-Value"
    }
  ],
  "targetField": "Custom.WorkClassification", 
  "defaultValue": "Standard"
}
```

### Multi-Criteria Status Mapping

```json
{
  "FieldMapType": "MultiValueConditionalMap",
  "ApplyTo": ["*"],
  "conditions": [
    {
      "expression": "State = 'Closed' AND Reason = 'Completed'",
      "value": "Done"
    },
    {
      "expression": "State = 'Active' AND AssignedTo != ''",
      "value": "In Progress"
    },
    {
      "expression": "State = 'New' OR AssignedTo = ''",
      "value": "To Do"
    }
  ],
  "targetField": "Custom.WorkflowStatus",
  "defaultValue": "Unknown"
}
```

## Performance Considerations

### Condition Complexity
- More complex conditions require more processing time
- Consider the number of fields involved in conditions
- Optimize condition order for common cases first

### Evaluation Order
- Conditions are evaluated in the order specified
- Place most likely conditions first for better performance
- Use specific conditions before general ones

### Field Access
- Each field reference requires data access
- Minimize the number of fields referenced
- Cache field values when possible

## Best Practices

### Condition Design
- Use clear, readable condition expressions
- Test conditions with sample data before migration
- Document business logic behind each condition

### Value Mapping
- Ensure target values are valid for the target field
- Use meaningful names for mapped values
- Test with various input combinations

### Error Handling
- Always provide appropriate default values
- Handle null or missing field values gracefully
- Log condition evaluation results for debugging

## Troubleshooting

### Common Issues
- **Field Not Found**: Ensure all referenced fields exist
- **Invalid Expressions**: Verify condition syntax
- **Type Mismatches**: Check data types in comparisons
- **Performance**: Optimize complex condition logic

### Debugging Tips
- Enable detailed logging to see condition evaluation
- Test with small data sets first
- Validate condition logic with sample scenarios

## Integration Patterns

### With Other Field Maps
- Use after Field Value Maps to work with transformed values
- Combine with Field To Field Maps for comprehensive mapping
- Coordinate with Field Clear Maps for cleanup operations

### Sequential Processing
- Multiple conditional maps can be chained together
- Each map can refine values set by previous maps
- Order matters for dependent mappings

## Schema

{{< class-schema >}}
