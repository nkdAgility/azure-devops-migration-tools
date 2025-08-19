---
title: Tree To Tag Field Map
description: |
  The Tree To Tag Field Map converts hierarchical area path or iteration path structures into flat tags, enabling tree-based organizational information to be preserved as searchable tags during migration.
dataFile: reference.fieldmaps.treetotagfieldmap.yaml
slug: tree-to-tag-field-map
aliases:
  - /docs/Reference/FieldMaps/TreeToTagFieldMap
  - /Reference/FieldMaps/TreeToTagFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/TreeToTagFieldMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/TreeToTagFieldMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2782
---

## Overview

The Tree To Tag Field Map transforms hierarchical path structures (Area Paths and Iteration Paths) into work item tags, providing a way to preserve organizational hierarchy information in a flat, searchable format. This field map is particularly valuable when migrating between systems with different organizational structures or when you need to maintain historical path information as metadata.

This field map enables teams to preserve the hierarchical context of their work items while gaining the flexibility and searchability that tags provide.

{{< class-description >}}

## How It Works

The Tree To Tag Field Map processes hierarchical paths and creates tags:

1. **Path Analysis**: Examines the area path or iteration path structure
2. **Hierarchy Extraction**: Extracts individual levels from the path hierarchy
3. **Tag Generation**: Creates tags for each level or the complete path based on configuration
4. **Tag Formatting**: Applies prefixes, suffixes, or formatting to the generated tags
5. **Tag Application**: Adds the generated tags to the work item's tag collection

## Use Cases

This field map is essential for:

- **Organizational Restructuring**: Preserving old area/iteration structures when reorganizing
- **Cross-Project Migration**: Maintaining team structure information across projects
- **Historical Data Preservation**: Keeping track of original organizational contexts
- **Enhanced Searchability**: Making hierarchical information searchable through tags
- **Reporting and Analytics**: Enabling tag-based reporting on organizational structures
- **Team Tracking**: Maintaining visibility into team assignments and sprint affiliations
- **Process Changes**: Adapting to new process templates while preserving historical context

## Configuration Structure

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Skip Root Levels and Apply Time Travel

```json
{
  "FieldMapType": "TreeToTagFieldMap",
  "ApplyTo": ["*"],
  "toSkip": 2,
  "timeTravel": 0
}
```

#### Skip First Level Only

```json
{
  "FieldMapType": "TreeToTagFieldMap",
  "ApplyTo": ["User Story", "Bug"],
  "toSkip": 1,
  "timeTravel": 0
}
```

#### Use Historical Area Path Values

```json
{
  "FieldMapType": "TreeToTagFieldMap",
  "ApplyTo": ["*"],
  "toSkip": 0,
  "timeTravel": 6
}
```

## Tag Generation Options

### Complete Path Tags
- Creates a single tag representing the entire hierarchy
- Useful for preserving exact organizational context
- Example: `Team-ProjectName\Development\Frontend\Components`

### Individual Level Tags  
- Creates separate tags for each level in the hierarchy
- Enables filtering and searching by specific organizational levels
- Example: `Team-ProjectName`, `Team-Development`, `Team-Frontend`, `Team-Components`

### Combined Approach
- Creates both complete path and individual level tags
- Provides maximum flexibility for searching and reporting
- May result in more tags per work item

## Path Processing Rules

### Path Parsing
- Handles standard Azure DevOps path formats
- Supports both Area Path and Iteration Path structures
- Processes escaped characters and special path elements

### Level Extraction
- Splits paths on standard separators (backslash by default)
- Removes empty or invalid path segments
- Maintains hierarchical order in tag creation

### Tag Validation
- Ensures generated tags meet Azure DevOps requirements
- Handles special characters and length limitations
- Prevents duplicate tag creation

## Common Scenarios

### Team Reorganization

When teams are restructuring but want to maintain historical context:

```json
{
  "FieldMapType": "TreeToTagFieldMap",
  "ApplyTo": ["*"],
  "toSkip": 1,
  "timeTravel": 0
}
```

### Sprint History Preservation

Maintaining sprint information for historical analysis:

```json
{
  "FieldMapType": "TreeToTagFieldMap", 
  "ApplyTo": ["*"],
  "toSkip": 0,
  "timeTravel": 3
}
```

### Multi-Level Team Tagging

Creating tags for different organizational levels:

```json
[
  {
    "FieldMapType": "TreeToTagFieldMap",
    "ApplyTo": ["*"],
    "toSkip": 2,
    "timeTravel": 0
  },
  {
    "FieldMapType": "TreeToTagFieldMap",
    "ApplyTo": ["*"], 
    "toSkip": 0,
    "timeTravel": 6
  }
]
```

## Best Practices

### Tag Organization

- Use consistent prefixes to group related path tags
- Consider tag naming conventions that align with organizational standards
- Plan for tag management and cleanup procedures

### Performance Considerations

- Monitor tag proliferation in large organizations
- Consider the impact on query performance
- Balance between detail and manageability

### Migration Planning

- Document the mapping strategy for stakeholders
- Test with representative data before full migration
- Coordinate with teams on new tagging conventions

## Integration with Other Field Maps

### Complementary Usage

- **Field Clear Map**: Clear original path fields after tag creation
- **Field Skip Map**: Preserve original paths while adding tags
- **Field Value Map**: Transform path values before tag conversion

### Sequential Processing

- Tree To Tag maps typically run after basic field mappings
- Consider order when combining with other path-related mappings
- Ensure tag creation doesn't conflict with other field operations

## Troubleshooting

### Common Issues

- **Invalid Path Formats**: Ensure source paths follow Azure DevOps conventions
- **Tag Length Limits**: Monitor generated tag lengths
- **Duplicate Tags**: Handle cases where multiple paths generate same tags
- **Special Characters**: Test with paths containing special characters

### Validation Tips

- Test with various path structures
- Verify tag generation with sample data
- Check for tag naming conflicts
- Monitor migration logs for processing issues

## Schema

{{< class-schema >}}
