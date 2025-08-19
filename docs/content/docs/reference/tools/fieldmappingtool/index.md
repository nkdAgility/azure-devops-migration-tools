---
title: Field Mapping Tool
description: The Field Mapping Tool applies field transformation strategies during work item migration, enabling sophisticated field mappings, value transformations, and data manipulations between source and target systems.
dataFile: reference.tools.fieldmappingtool.yaml
schemaFile: schema.tools.fieldmappingtool.json
slug: field-mapping-tool
aliases:
  - /docs/Reference/Tools/FieldMappingTool
  - /Reference/Tools/FieldMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/FieldMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/FieldMappingTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2811
---

## Overview

The Field Mapping Tool is a core component of the Azure DevOps Migration Tools that enables sophisticated field transformations during work item migration. It applies configured field mappings to work items as they are processed, allowing you to transform data, map fields between different schemas, and manipulate values to match your target system requirements.

The tool supports a wide variety of field mapping strategies through its extensible field map system, from simple field-to-field mappings to complex calculations and regex transformations.

## How It Works

The Field Mapping Tool operates during work item processing in migration processors. When a work item is being migrated:

1. **Initialization**: The tool loads all configured field maps during startup, organizing them by work item type
2. **Application**: During migration, the tool applies field maps in two phases:
   - **Universal mappings**: Maps configured with `ApplyTo: ["*"]` are applied to all work items
   - **Type-specific mappings**: Maps configured for the specific work item type are applied

3. **Processing**: Each field map executes its transformation logic on the source and target work item data

The tool is automatically invoked by migration processors including:

- **TfsWorkItemMigrationProcessor**: Applies field mappings during standard work item migration
- **TfsWorkItemOverwriteProcessor**: Applies field mappings when overwriting existing work items

## Field Map Types

The Field Mapping Tool supports numerous field map types, each designed for specific transformation scenarios. All available field maps are documented in the [Field Maps section]({{< ref "docs/reference/field-maps" >}}).

Key categories include:

- **Direct Mapping**: Simple field-to-field copying and transformations
- **Value Mapping**: Converting values using lookup tables and conditional logic  
- **Calculations**: Mathematical operations and computed field values
- **Text Processing**: Regex transformations, merging, and text manipulations
- **Metadata Handling**: Working with tags, areas, iterations, and other metadata
- **Conditional Logic**: Applying mappings based on specific conditions

## Configuration Structure

Field mappings are configured in the `FieldMaps` array within the `FieldMappingTool` section. Each field map includes:

- **FieldMapType**: Specifies which field map implementation to use
- **ApplyTo**: Array of work item types the mapping applies to (use `["*"]` for all types)
- **Additional Properties**: Configuration specific to each field map type

### Common Configuration Pattern

```json
{
  "FieldMappingTool": {
    "Enabled": true,
    "FieldMaps": [
      {
        "FieldMapType": "FieldToFieldMap",
        "ApplyTo": ["Bug", "Task"],
        "sourceField": "Source.Field",
        "targetField": "Target.Field"
      }
    ]
  }
}
```

### Field Map Defaults

The `FieldMapDefaults` section allows you to set default `ApplyTo` values that will be inherited by field maps that don't specify their own `ApplyTo` configuration:

```json
{
  "FieldMappingTool": {
    "Enabled": true,
    "FieldMapDefaults": {
      "ApplyTo": ["Bug", "Task", "User Story"]
    },
    "FieldMaps": [
      // Field maps without ApplyTo will inherit the defaults above
    ]
  }
}
```

## Best Practices

### Order Matters

Field maps are processed in the order they appear in the configuration. Consider dependencies between mappings when ordering them.

### Work Item Type Targeting

Use specific work item types in `ApplyTo` rather than `["*"]` when possible to avoid unintended side effects.

### Testing Field Maps

Test field mappings with a small subset of work items before running full migrations to ensure they produce expected results.

### Performance Considerations

- Complex regex patterns and calculations can impact migration performance
- Consider the frequency of execution when designing field mappings
- Use specific work item type targeting to reduce unnecessary processing

{{< class-description >}}

## Options

{{< class-options >}}

## Samples

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Metadata

{{< class-metadata >}}

## Schema

{{< class-schema >}}
