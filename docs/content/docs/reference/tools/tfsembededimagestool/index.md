---
title: TFS Embedded Images Tool
description: Processes and migrates embedded images within work item descriptions, comments, and HTML content, ensuring images are properly transferred and referenced in the target system.
dataFile: reference.tools.tfsembededimagestool.yaml
schemaFile: schema.tools.tfsembededimagestool.json
slug: tfs-embeded-images-tool
aliases:
  - /docs/Reference/Tools/TfsEmbededImagesTool
  - /Reference/Tools/TfsEmbededImagesTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsEmbededImagesTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsEmbededImagesTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2808
---

## Overview

The TFS Embedded Images Tool handles the migration of embedded images within work item content during the migration process. This tool processes HTML content in work item descriptions, comments, and other rich text fields to identify, extract, and migrate embedded images to the target system.

The tool ensures that images embedded in work item content are properly preserved and accessible in the target environment, maintaining the visual context and documentation integrity of work items.

### How It Works

The TFS Embedded Images Tool operates during work item migration to handle embedded images:

1. **Content Scanning**: Analyzes HTML content in work item fields for embedded image references
2. **Image Extraction**: Downloads embedded images from the source system
3. **Image Processing**: Processes images and prepares them for target system upload
4. **Reference Updates**: Updates HTML content to reference images in the target system
5. **Upload Management**: Uploads images to the target system and maintains proper linking

The tool integrates seamlessly with work item migration processors to ensure embedded images are handled transparently.

### Use Cases

Common scenarios where the TFS Embedded Images Tool is essential:

- **Rich Content Migration**: Preserving visual documentation in work item descriptions
- **Knowledge Preservation**: Maintaining screenshots, diagrams, and visual references
- **Cross-System Migration**: Moving embedded images between different TFS/Azure DevOps instances
- **Content Integrity**: Ensuring complete work item content migration including visual elements
- **Documentation Migration**: Preserving visual documentation and reference materials

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Embedded Images Tool requires minimal configuration to enable image processing:

```json
{
  "TfsEmbededImagesTool": {
    "Enabled": true
  }
}
```

### Complex Examples

The tool operates automatically when enabled, processing all embedded images found in work item content during migration.

## Common Scenarios

### Standard Image Migration

Enable embedded image processing for work item migration:

```json
{
  "TfsEmbededImagesTool": {
    "Enabled": true
  }
}
```

### Complete Content Migration

Use with other content migration tools for comprehensive work item migration:

```json
{
  "CommonTools": {
    "TfsEmbededImagesTool": {
      "Enabled": true
    },
    "TfsAttachmentTool": {
      "Enabled": true,
      "ExportBasePath": "C:\\temp\\Attachments"
    },
    "TfsWorkItemEmbededLinkTool": {
      "Enabled": true
    }
  }
}
```

## Good Practices

### Content Management

- **Enable for Rich Content**: Always enable when work items contain embedded images
- **Storage Planning**: Ensure adequate storage for image processing and migration
- **Network Considerations**: Plan for image download and upload bandwidth requirements
- **Content Validation**: Verify image migration with sample work items

### Performance Optimization

- **Batch Processing**: Process images efficiently during work item migration
- **Storage Management**: Use appropriate temporary storage for image processing
- **Network Efficiency**: Optimize image transfer based on network capabilities
- **Resource Monitoring**: Monitor system resources during image-heavy migrations

### Quality Assurance

- **Image Integrity**: Verify images are properly migrated and accessible
- **Reference Validation**: Ensure HTML references are correctly updated
- **Visual Testing**: Test migrated work items to confirm image display
- **Content Review**: Review migrated content for any missing or broken images

## Troubleshooting

### Common Issues

**Images Not Migrating:**

- Verify the tool is enabled (`"Enabled": true`)
- Check source system permissions for image access
- Ensure target system allows image uploads
- Review migration logs for image-related errors

**Broken Image References:**

- Verify HTML content is properly processed
- Check that images were successfully uploaded to target
- Ensure image references are correctly updated
- Review target system image storage configuration

**Performance Issues:**

- Monitor storage space during image processing
- Consider network bandwidth for image transfers
- Review image sizes and migration batch sizes
- Optimize temporary storage location for performance

**Permission Errors:**

- Verify permissions to access source images
- Check target system permissions for image uploads
- Ensure proper authentication for image operations
- Review storage location permissions for temporary files

## Schema

{{< class-schema >}}
