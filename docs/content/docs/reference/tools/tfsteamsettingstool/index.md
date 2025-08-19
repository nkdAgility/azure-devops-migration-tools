---
title: TFS Team Settings Tool
description: Configures team settings including backlog navigation levels, iteration paths, and area paths during migration to ensure proper team configuration in the target system.
dataFile: reference.tools.tfsteamsettingstool.yaml
schemaFile: schema.tools.tfsteamsettingstool.json
slug: tfs-team-settings-tool
aliases:
  - /docs/Reference/Tools/TfsTeamSettingsTool
  - /Reference/Tools/TfsTeamSettingsTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsTeamSettingsTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsTeamSettingsTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2805
---

## Overview

The TFS Team Settings Tool configures team-specific settings during migration, including backlog navigation levels, iteration paths, and area paths. This tool ensures that teams maintain their organizational structure and agile process configuration when migrating to a new Azure DevOps environment.

The tool is crucial for maintaining team productivity by preserving their configured work item hierarchies, sprint planning capabilities, and organizational boundaries.

### How It Works

The TFS Team Settings Tool operates during team configuration migration:

1. **Team Discovery**: Identifies all teams in the source project
2. **Settings Analysis**: Analyzes current team settings including backlogs and paths
3. **Configuration Migration**: Migrates team-specific configurations to target system
4. **Path Mapping**: Maps area paths and iteration paths to target structure
5. **Validation**: Verifies team settings are correctly applied in target system

The tool integrates with project structure migration to ensure teams can operate effectively in the migrated environment.

### Use Cases

Common scenarios where the TFS Team Settings Tool is essential:

- **Team Organization**: Preserving team structure and responsibilities
- **Agile Process Continuity**: Maintaining sprint and backlog configurations
- **Area Path Management**: Ensuring teams have correct area path assignments
- **Iteration Planning**: Preserving iteration path configurations for sprint planning
- **Hierarchical Work Items**: Maintaining proper backlog level configurations

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Team Settings Tool provides configuration options for team migration:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": true,
    "UpdateTeamSettings": true,
    "PrefixProjectToNodes": false,
    "Teams": [
      "Team A",
      "Team B"
    ]
  }
}
```

### Complex Examples

#### Full Team Settings Migration

Complete migration of all team settings:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": true,
    "UpdateTeamSettings": true,
    "PrefixProjectToNodes": true,
    "Teams": ["Development Team", "QA Team", "DevOps Team"]
  }
}
```

#### Selective Team Migration

Migrate only specific teams:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": true,
    "UpdateTeamSettings": false,
    "PrefixProjectToNodes": false,
    "Teams": ["Core Development Team"]
  }
}
```

## Common Scenarios

### Complete Team Migration

Migrate all team settings with full configuration:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": true,
    "UpdateTeamSettings": true,
    "PrefixProjectToNodes": true
  }
}
```

### Path Prefix Management

Control whether project names are prefixed to node paths:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": true,
    "UpdateTeamSettings": true,
    "PrefixProjectToNodes": false
  }
}
```

### Team-Specific Migration

Migrate settings for specific teams only:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": true,
    "UpdateTeamSettings": true,
    "Teams": [
      "Scrum Team Alpha",
      "Scrum Team Beta",
      "Platform Team"
    ]
  }
}
```

### Settings Update Only

Update existing team settings without full migration:

```json
{
  "TfsTeamSettingsTool": {
    "Enabled": true,
    "MigrateTeamSettings": false,
    "UpdateTeamSettings": true,
    "Teams": ["Development Team"]
  }
}
```

## Good Practices

### Team Configuration

- **Document Structure**: Document current team organization before migration
- **Validate Paths**: Ensure area and iteration paths exist in target before migration
- **Team Alignment**: Coordinate with team leads about configuration changes
- **Testing**: Test team settings migration with a small subset first

### Path Management

- **Consistent Naming**: Use consistent naming conventions for area and iteration paths
- **Hierarchy Planning**: Plan the team hierarchy structure for the target system
- **Permission Alignment**: Ensure team permissions align with area path assignments
- **Sprint Configuration**: Verify iteration paths support team sprint planning needs

### Migration Strategy

- **Incremental Approach**: Migrate teams incrementally to identify issues early
- **Validation Process**: Establish validation process for team settings
- **Rollback Plan**: Have rollback procedures for team configuration issues
- **Communication**: Communicate changes to affected teams

## Troubleshooting

### Common Issues

**Team Settings Not Applied:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that `MigrateTeamSettings` is set to true
- Ensure target project exists and is accessible
- Verify team names in configuration match source teams

**Path Configuration Problems:**

- Verify area paths exist in target project structure
- Check iteration path configuration in target system
- Ensure `PrefixProjectToNodes` setting matches target structure
- Validate path permissions for teams

**Selective Team Issues:**

- Check team names in `Teams` array match exactly
- Verify teams exist in source system
- Ensure team names don't contain special characters
- Check for case sensitivity in team names

**Update Problems:**

- Verify `UpdateTeamSettings` is configured correctly
- Check permissions for updating team settings
- Ensure target teams exist before updating settings
- Validate team configuration dependencies

### Performance Considerations

- **Large Team Counts**: Consider batch processing for organizations with many teams
- **Complex Hierarchies**: Monitor performance with complex area/iteration structures
- **Permission Dependencies**: Account for permission inheritance in path structures
- **Validation Time**: Allow extra time for team settings validation

## Schema

{{< class-schema >}}
