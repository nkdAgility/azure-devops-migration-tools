---
title: Field Calculation Map
description: |
  The Field Calculation Map performs mathematical calculations on numeric fields using NCalc expressions during work item migration. It allows you to compute values dynamically by applying mathematical or logical operations on source fields and storing the result in a target field.
dataFile: reference.fieldmaps.fieldcalculationmap.yaml
slug: field-calculation-map
aliases:
  - /docs/Reference/FieldMaps/FieldCalculationMap
  - /Reference/FieldMaps/FieldCalculationMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldCalculationMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldCalculationMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2756
---

{{< class-description >}}

The Field Calculation Map is a specialised field map used in the Azure DevOps Migration Tools. It allows you to compute values dynamically during migration by applying mathematical or logical operations on numeric fields. Calculations are defined using NCalc expressions and can reference one or more source fields using named parameters.

This is especially useful for scenarios where derived values need to be migrated, such as calculating cost based on effort and rate, or generating summary fields that are not explicitly stored in the source system.

## How It Works

During work item processing, the Field Calculation Map:

1. **Validates Configuration**: Ensures the expression, target field, and parameter mappings are properly configured
2. **Collects Source Values**: Retrieves values from all source fields specified in the parameters dictionary
3. **Validates Field Values**: Confirms that all source fields exist and contain numeric values
4. **Evaluates Expression**: Executes the NCalc expression using the collected field values as parameters
5. **Converts Result**: Automatically converts the calculation result to match the target field's data type
6. **Sets Target Field**: Assigns the calculated value to the specified target field

## Use Cases

This field map is commonly used for:

- **Cost Calculations**: Computing `Custom.EstimatedCost` as `EstimatedHours × HourlyRate`
- **Unit Conversions**: Converting days to hours, points to hours, or other unit transformations
- **Weighted Scoring**: Applying weights to different numeric fields and storing computed scores
- **Mathematical Transformations**: Applying formulas, rounding, or mathematical functions to field values
- **Derived Metrics**: Creating calculated fields that combine multiple source values

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Example

```json
{
  "FieldMapType": "FieldCalculationMap",
  "ApplyTo": ["Bug", "Task"],
  "expression": "[effort] * [rate]",
  "parameters": {
    "effort": "Custom.EstimatedHours",
    "rate": "Custom.HourlyRate"
  },
  "targetField": "Custom.EstimatedCost"
}
```

### Advanced Example

```json
{
  "FieldMapType": "FieldCalculationMap",
  "ApplyTo": ["User Story"],
  "expression": "if([priority] <= 2, [points] * 1.5, [points])",
  "parameters": {
    "priority": "Microsoft.VSTS.Common.Priority",
    "points": "Microsoft.VSTS.Scheduling.StoryPoints"
  },
  "targetField": "Custom.WeightedPoints"
}
```

## Supported Expression Features

The [NCalc engine](https://github.com/ncalc/ncalc) supports a wide range of mathematical operations:

### Arithmetic Operations

- Basic operations: `+`, `-`, `*`, `/`, `%`
- Parentheses for order of operations: `([a] + [b]) * [c]`

### Logical Operations

- Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`
- Boolean logic: `&&`, `||`, `!`

### Mathematical Functions

- `Abs(x)` - Absolute value
- `Round(x, decimals)` - Round to specified decimal places
- `Pow(x, y)` - Power function
- `Min(a, b)` - Minimum value
- `Max(a, b)` - Maximum value
- `Sqrt(x)` - Square root

### Conditional Logic

- `if(condition, trueValue, falseValue)` - Conditional evaluation

## Data Type Handling

The Field Calculation Map automatically handles data type conversions:

### Source Field Types

- Accepts any numeric field types (integer, double, decimal)
- Automatically converts string representations of numbers
- Validates that all source fields contain numeric values

### Target Field Types

- **Integer fields**: Results are rounded to nearest integer
- **Double fields**: Results maintain decimal precision
- **String fields**: Results are converted to string representation
- **Other types**: Direct assignment attempted

## Error Handling and Validation

The field map includes comprehensive error handling:

### Pre-Execution Validation

- Verifies that the target field exists on the work item
- Ensures all source fields specified in parameters exist
- Validates that source field values are numeric

### Expression Evaluation

- Catches and logs NCalc expression errors
- Reports detailed error messages for debugging
- Gracefully skips calculation if validation fails

### Type Conversion

- Handles automatic type conversion between numeric types
- Logs warnings for conversion failures
- Preserves data integrity during type conversions

## Best Practices

### Expression Design

- Use descriptive parameter names that match your field purpose
- Test expressions with sample data before migration
- Consider edge cases like null or zero values

### Performance Considerations

- Complex expressions may impact migration performance
- Use simple expressions when possible
- Consider the frequency of execution for your work item types

### Field Validation

- Ensure source fields contain numeric data
- Validate target field types support calculated results
- Test with various data scenarios

## How NCalc Expressions Work

The [NCalc](https://github.com/ncalc/ncalc) engine supports:

- Standard arithmetic: `+`, `-`, `*`, `/`
- Logical operations: `&&`, `||`, `==`, `!=`
- Functions: `Pow(a, b)`, `Min(a, b)`, `Max(a, b)`, `Abs(x)`, etc.
- Conditional evaluation using `if(condition, trueValue, falseValue)`

**Example Expression:**

```ncalc
[effort] * [rate]
```

This multiplies the values of the two fields `Custom.EstimatedHours` and `Custom.HourlyRate`.

## Behaviour and Execution

- The field map executes during **work item migration** and **before saving** to the target.
- It applies to the **latest revision** only unless otherwise configured.
- The calculated value **overwrites** any existing value in `targetField`.

## Field Resolution Rules

- Each variable in the expression (e.g. `[effort]`) must be defined in `parameters`.
- Values are pulled from the **source work item** using the mapped reference names.
- If a referenced field is missing or null, the expression evaluation may fail.
- Non-numeric fields will cause an evaluation error.

## Practices

- Use consistent **naming conventions** for parameters to avoid confusion.
- Log intermediate values when troubleshooting expression evaluation.
- Always **test with a sample dataset** using `run-local` before production use.
- Use `ApplyTo` to limit the map to only relevant work item types.

## Limitations

- Only works with **numeric** source fields.
- Errors during evaluation are **not recoverable** mid-run unless caught at validation.
- Does **not support cross-work-item calculations**.

## Schema

{{< class-schema >}}
