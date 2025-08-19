---
title: TFS Work Item Embedded Link Tool
description: Processes and migrates embedded links within work item fields, ensuring that internal references and hyperlinks are correctly updated for the target system.
dataFile: reference.tools.tfsworkitemembededlinktool.yaml
schemaFile: schema.tools.tfsworkitemembededlinktool.json
slug: tfs-work-item-embeded-link-tool
aliases:
  - /docs/Reference/Tools/TfsWorkItemEmbededLinkTool
  - /Reference/Tools/TfsWorkItemEmbededLinkTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsWorkItemEmbededLinkTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsWorkItemEmbededLinkTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2802
---

## Overview

The TFS Work Item Embedded Link Tool processes and migrates embedded links within work item fields. This tool ensures that internal references, hyperlinks, and embedded content within work item descriptions and other HTML fields are correctly updated to point to the target system.

The tool is crucial for maintaining the integrity of work item content that contains links to other work items, documents, or system resources during migration.

### How It Works

The TFS Work Item Embedded Link Tool operates during work item field processing:

1. **Link Detection**: Scans work item fields for embedded links and references
2. **Link Analysis**: Analyzes link types and target destinations
3. **URL Transformation**: Updates links to point to target system locations
4. **Content Processing**: Processes HTML content to update embedded references
5. **Link Validation**: Validates that updated links are correctly formatted

The tool integrates with work item migration processors to ensure content integrity during migration.

### Use Cases

Common scenarios where the TFS Work Item Embedded Link Tool is essential:

- **Internal Link Migration**: Updating links between work items during migration
- **Documentation References**: Maintaining links to documentation and resources
- **Cross-Project Links**: Handling links that span multiple projects
- **URL Format Updates**: Converting link formats between different system versions
- **Content Integrity**: Preserving rich content with embedded links and references

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Work Item Embedded Link Tool provides configuration options for link processing:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": true,
    "FilterWorkItemTypes": [],
    "SourceUrlPattern": "https://source-tfs:8080/tfs/",
    "TargetUrlPattern": "https://target-devops.visualstudio.com/"
  }
}
```

### Complex Examples

#### Complete Link Processing

Process all embedded links with URL pattern replacement:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": true,
    "FilterWorkItemTypes": ["Bug", "User Story", "Task"],
    "SourceUrlPattern": "https://old-tfs.company.com:8080/tfs/DefaultCollection/",
    "TargetUrlPattern": "https://dev.azure.com/company/",
    "LinkUpdatePatterns": {
      "workitem": "/_workitems/edit/{id}",
      "build": "/_build/results?buildId={id}",
      "changeset": "/_versionControl/changeset/{id}"
    }
  }
}
```

#### Selective Processing

Process embedded links for specific work item types only:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": true,
    "FilterWorkItemTypes": ["Epic", "Feature"],
    "SourceUrlPattern": "https://tfs2018.internal/",
    "TargetUrlPattern": "https://dev.azure.com/organization/"
  }
}
```

## Common Scenarios

### Complete URL Migration

Update all embedded links to target system URLs:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": true,
    "SourceUrlPattern": "https://source-server/tfs/",
    "TargetUrlPattern": "https://dev.azure.com/organization/"
  }
}
```

### Work Item Link Updates

Focus on work item internal links:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": true,
    "FilterWorkItemTypes": ["Bug", "User Story", "Task", "Epic"],
    "SourceUrlPattern": "https://old-tfs:8080/",
    "TargetUrlPattern": "https://new-devops.visualstudio.com/"
  }
}
```

### Cross-Project Link Handling

Handle links that span multiple projects:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": true,
    "ProjectMapping": {
      "OldProject1": "NewProject1",
      "OldProject2": "NewProject2"
    },
    "SourceUrlPattern": "https://tfs.company.com/",
    "TargetUrlPattern": "https://dev.azure.com/company/"
  }
}
```

### Link Validation Mode

Process links with validation without updates:

```json
{
  "TfsWorkItemEmbeddedLinkTool": {
    "Enabled": true,
    "ProcessEmbeddedLinks": true,
    "UpdateUrls": false,
    "ValidateOnly": true,
    "ReportBrokenLinks": true
  }
}
```

## Good Practices

### Link Processing Strategy

- **Analyze Link Types**: Understand the types of embedded links in your content
- **URL Pattern Planning**: Plan URL transformation patterns carefully
- **Testing Approach**: Test link processing with sample work items first
- **Backup Content**: Ensure original content is backed up before processing

### URL Transformation

- **Pattern Accuracy**: Ensure source and target URL patterns are accurate
- **Protocol Consistency**: Maintain consistent protocols (HTTP/HTTPS) in transformations
- **Project Mapping**: Map project names correctly between source and target
- **Validation Testing**: Validate transformed links work in target system

### Configuration Management

- **Environment Specific**: Use environment-specific URL patterns
- **Regular Expression Testing**: Test URL pattern matching with sample data
- **Link Type Coverage**: Ensure all link types are covered in transformation rules
- **Performance Consideration**: Consider performance impact of extensive link processing

## Troubleshooting

### Common Issues

**Links Not Updated:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that `ProcessEmbeddedLinks` is set to true
- Ensure `UpdateUrls` is enabled for URL transformation
- Verify source URL pattern matches actual links in content

**Incorrect URL Transformations:**

- Check source and target URL patterns for accuracy
- Verify project name mappings are correct
- Ensure URL patterns include all necessary path components
- Test patterns with sample URLs before full migration

**Performance Problems:**

- Consider filtering work item types to reduce processing load
- Monitor memory usage during link processing
- Break large migrations into smaller batches
- Review content complexity and link density

**Broken Links After Migration:**

- Validate target URLs are accessible and correct
- Check project and collection names in target system
- Verify work item IDs are correctly mapped
- Test transformed links manually in target system

### Content Processing Issues

- **HTML Format Problems**: Ensure HTML content is properly formatted
- **Encoding Issues**: Check for character encoding problems in links
- **Special Characters**: Handle special characters in URLs correctly
- **Relative vs Absolute**: Ensure proper handling of relative and absolute URLs

### Validation and Testing

- **Link Validation**: Test transformed links in target system
- **Content Integrity**: Verify content remains intact after processing
- **Cross-Reference Testing**: Test links between migrated work items
- **Documentation Links**: Verify external documentation links still work

## Schema

{{< class-schema >}}
