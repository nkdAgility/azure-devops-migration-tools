---
title: TFS Changeset Mapping Tool
description: Maps source control changesets between source and target systems during work item migration, ensuring proper traceability and maintaining links between work items and code changes.
dataFile: reference.tools.tfschangesetmappingtool.yaml
schemaFile: schema.tools.tfschangesetmappingtool.json
slug: tfs-change-set-mapping-tool
aliases:
  - /docs/Reference/Tools/TfsChangeSetMappingTool
  - /Reference/Tools/TfsChangeSetMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsChangeSetMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsChangeSetMappingTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2809
---

## Overview

The TFS Changeset Mapping Tool provides functionality for mapping source control changesets between source and target systems during work item migration. This tool is essential for maintaining traceability between work items and code changes when migrating between different Team Foundation Server instances or Azure DevOps organizations.

The tool ensures that work item links to source control changesets are properly maintained and updated to reference the corresponding changesets in the target system.

### How It Works

The TFS Changeset Mapping Tool operates during work item migration to handle changeset references:

1. **Changeset Discovery**: Identifies changeset links within work items being migrated
2. **Mapping Application**: Maps source changeset IDs to target changeset IDs
3. **Link Updates**: Updates work item changeset links to reference target system changesets
4. **Validation**: Ensures mapped changesets exist in the target system

The tool integrates with work item migration processors to maintain proper traceability during the migration process.

### Use Cases

Common scenarios where the TFS Changeset Mapping Tool is essential:

- **Source Control Migration**: When migrating both work items and source control repositories
- **Cross-System Migration**: Moving work items between different TFS/Azure DevOps instances
- **Repository Consolidation**: Combining multiple source repositories into a unified target
- **Traceability Maintenance**: Preserving code-to-work-item relationships during migration
- **Historical Preservation**: Maintaining audit trails and change history

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Changeset Mapping Tool requires minimal configuration to enable changeset mapping:

```json
{
  "TfsChangeSetMappingTool": {
    "Enabled": true
  }
}
```

### Complex Examples

The tool operates automatically when enabled, identifying and mapping changeset references found in work items during migration.

## Common Scenarios

### Standard Changeset Mapping

Enable changeset mapping for work item migration:

```json
{
  "TfsChangeSetMappingTool": {
    "Enabled": true
  }
}
```

### Migration with Source Control

When migrating both work items and source control, enable the tool to maintain links:

```json
{
  "CommonTools": {
    "TfsChangeSetMappingTool": {
      "Enabled": true
    },
    "TfsGitRepositoryTool": {
      "Enabled": true,
      "Mappings": {
        "SourceRepo": "TargetRepo"
      }
    }
  }
}
```

## Good Practices

### Migration Planning

- **Source Control First**: Migrate source control repositories before work items when possible
- **Changeset Mapping**: Ensure changeset mappings are available before work item migration
- **Coordination**: Coordinate with source control migration tools for complete traceability
- **Validation**: Verify changeset existence in target before migration

### Configuration Management

- **Enable When Needed**: Only enable when changeset links need to be preserved
- **Integration**: Use in conjunction with source control migration tools
- **Testing**: Validate changeset mapping with sample work items
- **Documentation**: Document changeset mapping strategy and requirements

## Troubleshooting

### Common Issues

**Changeset Links Not Updated:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that changesets exist in the target system
- Ensure proper changeset mapping is available
- Review migration logs for changeset-related messages

**Missing Changesets in Target:**

- Confirm source control migration completed successfully
- Verify changeset mapping between source and target systems
- Check permissions for accessing target changesets
- Review source control migration logs

**Performance Issues:**

- Changeset mapping typically doesn't impact performance significantly
- Consider migration batch sizes for large numbers of work items
- Monitor target system performance during migration
- Review overall migration strategy timing

## Schema

{{< class-schema >}}
