---
title: TFS Revision Manager Tool
description: Manages work item revision history during migration, allowing control over the number of revisions migrated and enabling revision replay for complete historical preservation.
dataFile: reference.tools.tfsrevisionmanagertool.yaml
schemaFile: schema.tools.tfsrevisionmanagertool.json
slug: tfs-revision-manager-tool
aliases:
  - /docs/Reference/Tools/TfsRevisionManagerTool
  - /Reference/Tools/TfsRevisionManagerTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsRevisionManagerTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsRevisionManagerTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2806
---

## Overview

The TFS Revision Manager Tool provides control over work item revision history during migration. This tool allows you to manage the number of revisions that are migrated and configure whether to replay the complete revision history or migrate only the final state of work items.

The tool is essential for balancing migration performance with historical data preservation, allowing you to optimize migration speed while maintaining the necessary audit trail and change history.

### How It Works

The TFS Revision Manager Tool operates during work item migration to control revision processing:

1. **Revision Analysis**: Analyzes the revision history of source work items
2. **Revision Limiting**: Applies maximum revision limits when configured
3. **Replay Control**: Determines whether to replay all revisions or migrate final state only
4. **Performance Optimization**: Reduces migration overhead by controlling revision volume
5. **History Preservation**: Maintains essential change history based on configuration

The tool integrates with work item migration processors to optimize the migration process based on your requirements.

### Use Cases

Common scenarios where the TFS Revision Manager Tool is essential:

- **Performance Optimization**: Reducing migration time by limiting revision history
- **Large Work Item Migration**: Managing memory and performance for work items with extensive history
- **Selective History**: Preserving only recent or important revisions
- **Audit Compliance**: Maintaining complete revision history when required
- **Migration Efficiency**: Balancing historical data with migration speed

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Revision Manager Tool provides configuration options for revision control:

```json
{
  "TfsRevisionManagerTool": {
    "Enabled": true,
    "ReplayRevisions": true,
    "MaxRevisions": 100
  }
}
```

### Complex Examples

#### Configuration Options

- **Enabled**: Controls whether revision management is active during migration
- **ReplayRevisions**: Whether to replay all revisions or migrate only final state
- **MaxRevisions**: Maximum number of revisions to migrate per work item

## Common Scenarios

### Complete History Migration

Migrate all revisions while maintaining performance limits:

```json
{
  "TfsRevisionManagerTool": {
    "Enabled": true,
    "ReplayRevisions": true,
    "MaxRevisions": 500
  }
}
```

### Limited History Migration

Migrate only recent revisions to optimize performance:

```json
{
  "TfsRevisionManagerTool": {
    "Enabled": true,
    "ReplayRevisions": true,
    "MaxRevisions": 50
  }
}
```

### Final State Only

Migrate only the final state of work items for maximum performance:

```json
{
  "TfsRevisionManagerTool": {
    "Enabled": true,
    "ReplayRevisions": false,
    "MaxRevisions": 1
  }
}
```

### Balanced Approach

Moderate revision limit for balanced performance and history:

```json
{
  "TfsRevisionManagerTool": {
    "Enabled": true,
    "ReplayRevisions": true,
    "MaxRevisions": 100
  }
}
```

## Good Practices

### Revision Strategy

- **Assess Requirements**: Determine how much revision history is actually needed
- **Performance Testing**: Test migration performance with different revision limits
- **Stakeholder Alignment**: Align revision strategy with business requirements
- **Audit Compliance**: Consider compliance requirements for change history

### Performance Optimization

- **Start Conservative**: Begin with lower revision limits and increase if needed
- **Monitor Resources**: Watch memory and performance during migration
- **Batch Testing**: Test revision settings with small batches first
- **Target System Limits**: Consider target system performance with large revision counts

### Configuration Management

- **Environment Specific**: Use different revision limits for different environments
- **Documentation**: Document revision management decisions and rationale
- **Testing Strategy**: Test different revision scenarios before full migration
- **Backup Strategy**: Ensure source data is backed up before applying revision limits

## Troubleshooting

### Common Issues

**Performance Problems:**

- Reduce MaxRevisions to improve migration speed
- Consider setting ReplayRevisions to false for performance-critical migrations
- Monitor memory usage during migration
- Review overall migration batch sizes

**Incomplete History:**

- Increase MaxRevisions if more history is needed
- Ensure ReplayRevisions is set to true for complete history
- Verify revision limits meet business requirements
- Check source work item revision counts

**Configuration Errors:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that MaxRevisions is set to a reasonable number
- Ensure ReplayRevisions setting matches requirements
- Validate configuration syntax

**Memory Issues:**

- Reduce MaxRevisions for work items with extensive history
- Consider migrating work items in smaller batches
- Monitor system memory during migration
- Review target system memory requirements

## Schema

{{< class-schema >}}
