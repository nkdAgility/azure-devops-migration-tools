---
title: TFS Work Item Link Tool
description: Migrates work item links and relationships between work items, preserving hierarchical structures, dependencies, and cross-references in the target system.
dataFile: reference.tools.tfsworkitemlinktool.yaml
schemaFile: schema.tools.tfsworkitemlinktool.json
slug: tfs-work-item-link-tool
aliases:
  - /docs/Reference/Tools/TfsWorkItemLinkTool
  - /Reference/Tools/TfsWorkItemLinkTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsWorkItemLinkTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsWorkItemLinkTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2801
---

## Overview

The TFS Work Item Link Tool migrates work item links and relationships between work items. This tool preserves hierarchical structures, dependencies, cross-references, and other relationship types when migrating work items to the target system.

The tool is essential for maintaining work item relationships that define project structure, dependencies, and traceability in the migrated environment.

### How It Works

The TFS Work Item Link Tool operates during work item relationship migration:

1. **Link Discovery**: Identifies all work item links and relationships in source system
2. **Link Type Mapping**: Maps link types from source to target system equivalents
3. **Relationship Migration**: Migrates links while preserving relationship integrity
4. **Cross-Project Links**: Handles links that span multiple projects or collections
5. **Link Validation**: Validates that migrated links maintain proper relationships

The tool integrates with work item migration processors to ensure relationship data is preserved during migration.

### Use Cases

Common scenarios where the TFS Work Item Link Tool is essential:

- **Hierarchical Structures**: Preserving parent-child relationships between work items
- **Dependency Management**: Maintaining predecessor-successor and dependency links
- **Traceability Links**: Preserving test-requirement and other traceability relationships
- **Cross-Project References**: Handling links between work items in different projects
- **Custom Link Types**: Migrating organization-specific work item relationship types

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Work Item Link Tool provides configuration options for link migration:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": false,
    "FilterWorkItemTypes": [],
    "LinkMappings": {
      "System.LinkTypes.Hierarchy": "System.LinkTypes.Hierarchy",
      "System.LinkTypes.Dependency": "System.LinkTypes.Dependency"
    }
  }
}
```

### Complex Examples

#### Complete Link Migration

Migrate all link types with comprehensive mapping:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": true,
    "FilterWorkItemTypes": ["Epic", "Feature", "User Story", "Bug", "Task"],
    "LinkMappings": {
      "System.LinkTypes.Hierarchy": "System.LinkTypes.Hierarchy",
      "System.LinkTypes.Dependency": "System.LinkTypes.Dependency",
      "System.LinkTypes.Related": "System.LinkTypes.Related",
      "Microsoft.VSTS.TestCase.SharedSteps": "Microsoft.VSTS.TestCase.SharedSteps",
      "Microsoft.VSTS.Common.TestedBy": "Microsoft.VSTS.Common.TestedBy"
    },
    "ExcludeLinkTypes": [],
    "ProcessCrossProjectLinks": true
  }
}
```

#### Selective Link Migration

Migrate only specific link types:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": false,
    "FilterWorkItemTypes": ["Epic", "Feature", "User Story"],
    "LinkMappings": {
      "System.LinkTypes.Hierarchy": "System.LinkTypes.Hierarchy"
    },
    "ExcludeLinkTypes": ["System.LinkTypes.Related"],
    "ProcessCrossProjectLinks": false
  }
}
```

## Common Scenarios

### Hierarchical Structure Migration

Preserve parent-child relationships in work item hierarchies:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": false,
    "LinkMappings": {
      "System.LinkTypes.Hierarchy": "System.LinkTypes.Hierarchy"
    },
    "FilterWorkItemTypes": ["Epic", "Feature", "User Story", "Task"]
  }
}
```

### Dependency Link Migration

Focus on dependency and successor relationships:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": true,
    "LinkMappings": {
      "System.LinkTypes.Dependency": "System.LinkTypes.Dependency",
      "System.LinkTypes.Successor": "System.LinkTypes.Successor"
    }
  }
}
```

### Test Case Link Migration

Preserve test case relationships and coverage:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": true,
    "FilterWorkItemTypes": ["Test Case", "User Story", "Bug"],
    "LinkMappings": {
      "Microsoft.VSTS.Common.TestedBy": "Microsoft.VSTS.Common.TestedBy",
      "Microsoft.VSTS.TestCase.SharedSteps": "Microsoft.VSTS.TestCase.SharedSteps",
      "System.LinkTypes.Related": "System.LinkTypes.Related"
    }
  }
}
```

### Cross-Project Link Handling

Handle links that span multiple projects:

```json
{
  "TfsWorkItemLinkTool": {
    "Enabled": true,
    "MigrateLinks": true,
    "SaveAfterEachLinkIsAdded": true,
    "ProcessCrossProjectLinks": true,
    "CrossProjectLinkStrategy": "CreateReference",
    "LinkMappings": {
      "System.LinkTypes.Hierarchy": "System.LinkTypes.Hierarchy",
      "System.LinkTypes.Related": "System.LinkTypes.Related"
    }
  }
}
```

## Good Practices

### Link Migration Strategy

- **Analyze Link Types**: Understand all link types used in your source system
- **Map Link Types**: Create comprehensive mappings between source and target link types
- **Hierarchy Preservation**: Ensure parent-child relationships are maintained
- **Testing Approach**: Test link migration with small batches first

### Performance Optimization

- **Batch Processing**: Consider batch size for link processing performance
- **Save Strategy**: Use `SaveAfterEachLinkIsAdded` judiciously for performance
- **Filter Work Items**: Use work item type filters to reduce processing scope
- **Cross-Project Impact**: Consider performance impact of cross-project link processing

### Data Integrity

- **Link Validation**: Validate that all critical links are migrated
- **Relationship Testing**: Test work item relationships in target system
- **Circular Reference Detection**: Watch for circular references in hierarchies
- **Missing Link Reporting**: Report and address missing or failed link migrations

## Troubleshooting

### Common Issues

**Links Not Migrated:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that `MigrateLinks` is set to true
- Ensure link type mappings are correctly configured
- Verify source work items have links to migrate

**Link Type Mapping Problems:**

- Check that target system supports the mapped link types
- Verify link type reference names are correct
- Ensure custom link types exist in target system
- Test link type mappings with sample work items

**Performance Issues:**

- Consider disabling `SaveAfterEachLinkIsAdded` for better performance
- Use work item type filters to reduce processing scope
- Monitor memory usage during link processing
- Break large link migrations into smaller batches

**Cross-Project Link Problems:**

- Verify `ProcessCrossProjectLinks` setting matches requirements
- Check permissions for cross-project link creation
- Ensure referenced projects exist in target system
- Validate cross-project work item ID mappings

### Link-Specific Issues

**Hierarchy Problems:**

- Verify parent-child relationships are correctly mapped
- Check for circular references in work item hierarchies
- Ensure hierarchy link types are supported in target
- Test hierarchy navigation after migration

**Dependency Issues:**

- Validate dependency link directions are preserved
- Check that predecessor-successor relationships are maintained
- Ensure dependency cycles are handled appropriately
- Test dependency chain integrity after migration

**Test Case Links:**

- Verify test case coverage links are preserved
- Check shared steps relationships are maintained
- Ensure test-requirement traceability is preserved
- Validate test plan structure after migration

### Data Validation

- **Link Count Verification**: Compare link counts between source and target
- **Relationship Testing**: Test work item navigation and relationships
- **Link Type Coverage**: Ensure all link types are accounted for
- **Cross-Reference Validation**: Verify cross-project links work correctly

## Schema

{{< class-schema >}}
