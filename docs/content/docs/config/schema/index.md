---
title: "JSON Schemas"
description: "JSON Schema definitions for Azure DevOps Migration Tools configuration"
weight: 10
outputs: ["html", "schema-catalog"]
aliases:
  - /schema/
---

The Azure DevOps Migration Tools provide JSON Schema definitions for configuration validation and IDE support.

## Main Configuration Schema

- **[Configuration Schema](/schema/configuration.schema.json)** - Complete configuration schema for Azure DevOps Migration Tools

## Component Schemas

### Processors

{{< schema-list type="processors" >}}

### Tools

{{< schema-list type="tools" >}}

### Field Maps

{{< schema-list type="fieldmaps" >}}

### Endpoints

{{< schema-list type="endpoints" >}}

### Processor Enrichers

{{< schema-list type="processorenrichers" >}}

## Usage

You can reference these schemas in your JSON configuration files:

```json
{
  "$schema": "https://devopsmigration.io/schema/configuration.schema.json",
  "MigrationTools": {
    // your configuration here
  }
}
```

## Schema Validation

Most modern IDEs and editors support JSON Schema validation. Simply add the `$schema` property to your configuration files to enable:

- Auto-completion
- Validation
- Documentation on hover
- Error highlighting
