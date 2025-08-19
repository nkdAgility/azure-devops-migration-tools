---
title: Work Item Type Mapping Tool
description: Maps work item types between source and target systems, enabling migration between different process templates or when work item type names differ between environments.
dataFile: reference.tools.workitemtypemappingtool.yaml
schemaFile: schema.tools.workitemtypemappingtool.json
slug: work-item-type-mapping-tool
aliases:
  - /docs/Reference/Tools/WorkItemTypeMappingTool
  - /Reference/Tools/WorkItemTypeMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/WorkItemTypeMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/WorkItemTypeMappingTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2776
---

## Overview

The Work Item Type Mapping Tool provides essential functionality for mapping work item types between source and target systems during migration. This tool is crucial when migrating between different process templates, Azure DevOps organizations with different configurations, or when standardizing work item types across systems.

The tool allows you to specify which source work item types should be migrated as different work item types in the target system, ensuring compatibility and maintaining proper work item classification during migration.

### How It Works

The Work Item Type Mapping Tool operates during work item migration to transform work item types:

1. **Type Discovery**: Identifies work item types in the source system
2. **Mapping Application**: Applies configured mappings to transform source types to target types
3. **Validation Integration**: Works with validation tools to ensure target types exist
4. **Field Compatibility**: Ensures field mappings are compatible with mapped work item types
5. **Reference Updates**: Updates work item type references throughout the migration process

The tool integrates with other migration components to ensure consistent work item type handling across all migration activities.

### Use Cases

Common scenarios where the Work Item Type Mapping Tool is essential:

- **Process Template Migration**: Moving from one process template to another (e.g., Scrum to Agile)
- **Work Item Type Standardization**: Consolidating similar work item types into standard types
- **Custom Type Migration**: Mapping custom work item types to standard types in the target
- **Cross-Organization Migration**: Handling different work item type names between organizations
- **Legacy System Migration**: Mapping legacy work item types to modern equivalents

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The Work Item Type Mapping Tool is configured with a mappings dictionary that specifies source-to-target work item type relationships:

```json
{
  "WorkItemTypeMappingTool": {
    "Enabled": true,
    "Mappings": {
      "Product Backlog Item": "User Story",
      "Issue": "Bug",
      "Code Review Request": "Task"
    }
  }
}
```

### Complex Examples

#### Mapping Syntax

Each mapping entry follows the pattern:

```json
"SourceWorkItemType": "TargetWorkItemType"
```

- **SourceWorkItemType**: The exact name of the work item type in the source system
- **TargetWorkItemType**: The exact name of the work item type in the target system

## Common Scenarios

### Scrum to Agile Process Migration

Map Scrum process work item types to Agile process equivalents:

```json
{
  "WorkItemTypeMappingTool": {
    "Enabled": true,
    "Mappings": {
      "Product Backlog Item": "User Story",
      "Bug": "Bug",
      "Task": "Task",
      "Impediment": "Issue",
      "Epic": "Epic",
      "Feature": "Feature"
    }
  }
}
```

### Custom Type Standardization

Map custom work item types to standard types:

```json
{
  "WorkItemTypeMappingTool": {
    "Enabled": true,
    "Mappings": {
      "Enhancement Request": "User Story",
      "Defect": "Bug",
      "Investigation": "Task",
      "Support Request": "Issue"
    }
  }
}
```

### Legacy System Migration

Map legacy work item types to modern equivalents:

```json
{
  "WorkItemTypeMappingTool": {
    "Enabled": true,
    "Mappings": {
      "Change Request": "Feature",
      "Problem": "Bug",
      "Work Order": "Task",
      "Question": "Issue"
    }
  }
}
```

### Consolidation Mapping

Consolidate multiple source types into fewer target types:

```json
{
  "WorkItemTypeMappingTool": {
    "Enabled": true,
    "Mappings": {
      "User Story": "User Story",
      "Requirement": "User Story",
      "Specification": "User Story",
      "Bug": "Bug",
      "Defect": "Bug",
      "Issue": "Bug"
    }
  }
}
```

## Good Practices

### Mapping Strategy

- **Target System Compatibility**: Ensure all target work item types exist in the target system
- **Field Compatibility**: Verify that mapped types have compatible field structures
- **Process Alignment**: Align mappings with target system process templates and workflows
- **Documentation**: Document the rationale behind each mapping decision

### Validation and Testing

- **Use Validation Tools**: Always run TfsWorkItemTypeValidatorTool before migration
- **Test Mappings**: Validate mappings with sample work items before full migration
- **Field Mapping Coordination**: Ensure field mappings are compatible with work item type mappings
- **Incremental Testing**: Test mappings with small batches before full migration

### Configuration Management

- **Version Control**: Store mapping configurations in version control
- **Environment-Specific**: Maintain separate mappings for different target environments
- **Change Tracking**: Document changes to mappings and reasons for updates
- **Review Process**: Implement review processes for mapping changes

### Performance Considerations

- **Minimal Mappings**: Only map work item types that actually need transformation
- **Efficient Lookups**: Tool performs efficient lookups, so mapping complexity doesn't impact performance significantly
- **Batch Processing**: Mappings are applied efficiently during batch work item processing

## Troubleshooting

### Common Issues

**Work Item Type Not Found:**

- Verify that target work item types exist in the target system
- Check spelling and case sensitivity of work item type names
- Ensure target process template includes the required work item types
- Review target system configuration and available work item types

**Mapping Not Applied:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that source work item type names match exactly (case sensitive)
- Ensure mappings configuration syntax is correct
- Review migration logs for mapping-related messages

**Field Compatibility Issues:**

- Use TfsWorkItemTypeValidatorTool to identify field mismatches
- Configure field mappings for incompatible fields between mapped types
- Review required fields in target work item types
- Consider using FieldMappingTool for field-level transformations

**Process Template Conflicts:**

- Verify that target work item types are valid for the target project's process template
- Check work item type definitions and allowed values
- Review state transitions and workflow compatibility
- Consider process template updates in the target system

**Performance Issues:**

- Mappings themselves don't typically cause performance issues
- Consider the overall work item processing batch size
- Review system resources during migration
- Monitor target system performance during migration

## Schema

{{< class-schema >}}
