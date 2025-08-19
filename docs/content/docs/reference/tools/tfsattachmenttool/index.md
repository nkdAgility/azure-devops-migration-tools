---
title: TFS Attachment Tool
description: Manages work item attachment migration by downloading attachments from source systems, processing metadata, and uploading to target systems. Essential for preserving file attachments during migration.
dataFile: reference.tools.tfsattachmenttool.yaml
schemaFile: schema.tools.tfsattachmenttool.json
slug: tfs-attachment-tool
aliases:
  - /docs/Reference/Tools/TfsAttachmentTool
  - /Reference/Tools/TfsAttachmentTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsAttachmentTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsAttachmentTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2810
---

## Overview

The TFS Attachment Tool handles the migration of work item attachments between source and target systems. It manages the complete attachment lifecycle including downloading files from the source, processing metadata, handling file storage, and uploading attachments to the target system.

The tool ensures that all file attachments associated with work items are properly preserved during migration, maintaining file integrity, metadata, and accessibility in the target environment.

### How It Works

The TFS Attachment Tool operates during work item migration to handle attachments:

1. **Attachment Discovery**: Identifies all attachments associated with work items being migrated
2. **Download Process**: Downloads attachment files from the source system to local storage
3. **Metadata Processing**: Preserves attachment metadata including filenames, upload dates, and user information
4. **Size Validation**: Checks attachment sizes against configured limits
5. **Upload Process**: Uploads attachments to the target system and updates work item references
6. **Cleanup**: Manages temporary storage and cleanup of downloaded files

The tool integrates seamlessly with migration processors to ensure attachments are handled transparently during work item processing.

### Use Cases

Common scenarios where the TFS Attachment Tool is essential:

- **Complete Work Item Migration**: Ensuring all attachments are preserved when migrating work items
- **File Size Management**: Controlling attachment migration based on file size limits
- **Storage Optimization**: Managing local storage during the migration process
- **Incremental Migration**: Handling attachments in batched or incremental migration scenarios
- **Cross-Platform Migration**: Moving attachments between different TFS/Azure DevOps instances

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Attachment Tool supports configuration for file size limits and storage management:

```json
{
  "TfsAttachmentTool": {
    "Enabled": true,
    "ExportBasePath": "C:\\temp\\WorkItemAttachmentExport",
    "MaxAttachmentSize": 480000000
  }
}
```

### Complex Examples

#### Configuration Options

- **Enabled**: Controls whether the attachment tool is active during migration
- **ExportBasePath**: Local directory path for temporary attachment storage during migration
- **MaxAttachmentSize**: Maximum file size (in bytes) for attachments to be migrated

## Common Scenarios

### Standard Attachment Migration

Enable attachment migration with default settings:

```json
{
  "TfsAttachmentTool": {
    "Enabled": true,
    "ExportBasePath": "C:\\temp\\Attachments",
    "MaxAttachmentSize": 480000000
  }
}
```

### Large File Handling

Configure for environments with larger attachment limits:

```json
{
  "TfsAttachmentTool": {
    "Enabled": true,
    "ExportBasePath": "D:\\MigrationStorage\\Attachments",
    "MaxAttachmentSize": 1073741824
  }
}
```

### Network Storage Configuration

Use network storage for attachment processing:

```json
{
  "TfsAttachmentTool": {
    "Enabled": true,
    "ExportBasePath": "\\\\shared-storage\\migration\\attachments",
    "MaxAttachmentSize": 480000000
  }
}
```

### Size-Limited Migration

Migrate only smaller attachments to manage bandwidth or storage:

```json
{
  "TfsAttachmentTool": {
    "Enabled": true,
    "ExportBasePath": "C:\\temp\\Attachments",
    "MaxAttachmentSize": 10485760
  }
}
```

## Good Practices

### Storage Management

- **Adequate Disk Space**: Ensure sufficient local storage for temporary attachment files
- **Fast Storage**: Use SSDs or fast storage for the export base path to improve performance
- **Network Considerations**: When using network storage, ensure reliable connectivity and adequate bandwidth

### File Size Planning

- **Target System Limits**: Configure MaxAttachmentSize based on target system capabilities
- **Network Bandwidth**: Consider network speed when setting size limits for remote migrations
- **Storage Costs**: Balance attachment completeness with storage and transfer costs

### Performance Optimization

- **Batch Processing**: Process attachments in manageable batches to optimize memory usage
- **Parallel Processing**: Leverage multiple threads for attachment download/upload operations
- **Cleanup Processes**: Regularly clean up temporary files to free storage space

### Security Considerations

- **File Permissions**: Ensure appropriate permissions on export base path directories
- **Temporary Storage**: Secure temporary storage location to protect sensitive attachments
- **Cleanup Procedures**: Implement proper cleanup to remove temporary files after migration

## Troubleshooting

### Common Issues

**Attachments Not Migrating:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that the ExportBasePath directory exists and is writable
- Ensure sufficient disk space in the export location
- Review attachment sizes against the MaxAttachmentSize limit

**Permission Errors:**

- Verify write permissions on the ExportBasePath directory
- Check that the migration process has access to source attachments
- Ensure target system permissions allow attachment uploads
- Validate network path accessibility for shared storage

**File Size Issues:**

- Check individual attachment sizes against MaxAttachmentSize configuration
- Review target system attachment size limits
- Consider adjusting MaxAttachmentSize for specific migration requirements
- Monitor available storage space during migration

**Performance Problems:**

- Optimize ExportBasePath storage (use local SSDs when possible)
- Adjust batch sizes for attachment processing
- Check network bandwidth for remote storage scenarios
- Monitor system resources during attachment processing

**Storage Space Problems:**

- Monitor disk space in ExportBasePath during migration
- Implement cleanup procedures for processed attachments
- Consider using larger storage volumes for extensive migrations
- Use network storage for distributed migration scenarios

## Schema

{{< class-schema >}}
