---
title: TFS Git Repository Tool
description: Maps Git repository references between source and target systems during work item migration, ensuring proper links to commits, pull requests, and repository artifacts are maintained across environments.
dataFile: reference.tools.tfsgitrepositorytool.yaml
schemaFile: schema.tools.tfsgitrepositorytool.json
slug: tfs-git-repository-tool
aliases:
  - /docs/Reference/Tools/TfsGitRepositoryTool
  - /Reference/Tools/TfsGitRepositoryTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsGitRepositoryTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsGitRepositoryTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2490
---

## Overview

The TFS Git Repository Tool provides functionality for mapping Git repository references between source and target systems during work item migration. This tool ensures that work item links to Git commits, pull requests, and other repository artifacts are properly maintained when migrating between different Team Foundation Server instances or Azure DevOps organizations.

The tool handles repository name mapping and maintains traceability between work items and Git repository content across different environments.

### How It Works

The TFS Git Repository Tool operates during work item migration to handle Git repository references:

1. **Repository Discovery**: Identifies Git repository references within work items being migrated
2. **Mapping Application**: Maps source repository names to target repository names
3. **Link Updates**: Updates work item links to reference target system repositories
4. **Changeset Handling**: Coordinates with changeset mapping for complete traceability
5. **Validation**: Ensures mapped repositories exist in the target system

The tool integrates with work item migration processors to maintain proper Git repository relationships.

### Use Cases

Common scenarios where the TFS Git Repository Tool is essential:

- **Git Repository Migration**: When migrating both work items and Git repositories
- **Cross-Organization Migration**: Moving work items between different Azure DevOps organizations
- **Repository Consolidation**: Combining multiple source repositories into unified targets
- **Repository Renaming**: Handling repository name changes between source and target
- **Multi-Repository Projects**: Managing complex projects with multiple Git repositories

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS Git Repository Tool is configured with repository mappings and changeset link options:

```json
{
  "TfsGitRepositoryTool": {
    "Enabled": true,
    "Mappings": {
      "SourceRepo1": "TargetRepo1",
      "SourceRepo2": "TargetRepo2"
    },
    "ShouldDropChangedSetLinks": false
  }
}
```

### Complex Examples

#### Configuration Options

- **Enabled**: Controls whether the Git repository tool is active during migration
- **Mappings**: Dictionary mapping source repository names to target repository names
- **ShouldDropChangedSetLinks**: Whether to remove changeset links during migration

## Common Scenarios

### Repository Name Mapping

Map repositories with different names between source and target:

```json
{
  "TfsGitRepositoryTool": {
    "Enabled": true,
    "Mappings": {
      "LegacyProjectRepo": "ModernProjectRepo",
      "TeamARepo": "ConsolidatedRepo",
      "TempRepo": "MainRepo"
    },
    "ShouldDropChangedSetLinks": false
  }
}
```

### Repository Consolidation

Consolidate multiple source repositories into a single target:

```json
{
  "TfsGitRepositoryTool": {
    "Enabled": true,
    "Mappings": {
      "FrontEndRepo": "MonoRepo",
      "BackEndRepo": "MonoRepo",
      "SharedLibRepo": "MonoRepo"
    },
    "ShouldDropChangedSetLinks": false
  }
}
```

### Cross-Organization Migration

Map repositories across different Azure DevOps organizations:

```json
{
  "TfsGitRepositoryTool": {
    "Enabled": true,
    "Mappings": {
      "ProjectAlpha": "MigratedProjectAlpha",
      "ProjectBeta": "MigratedProjectBeta"
    },
    "ShouldDropChangedSetLinks": false
  }
}
```

### Drop Changeset Links

Remove changeset links when repository migration is not needed:

```json
{
  "TfsGitRepositoryTool": {
    "Enabled": true,
    "Mappings": {},
    "ShouldDropChangedSetLinks": true
  }
}
```

## Good Practices

### Repository Mapping Strategy

- **Pre-Migration Planning**: Plan repository mappings before starting work item migration
- **Name Consistency**: Use consistent naming conventions for mapped repositories
- **Validation**: Verify target repositories exist before migration
- **Documentation**: Document repository mapping decisions and rationale

### Migration Coordination

- **Repository First**: Migrate Git repositories before work items when possible
- **Changeset Coordination**: Coordinate with TfsChangeSetMappingTool for complete traceability
- **Testing**: Validate repository mappings with sample work items
- **Incremental Approach**: Test repository mappings with small batches first

### Configuration Management

- **Mapping Files**: Maintain repository mapping configurations in version control
- **Environment Specific**: Use environment-specific mappings for different target systems
- **Change Tracking**: Document changes to repository mappings over time
- **Backup Configurations**: Maintain backups of repository mapping configurations

## Troubleshooting

### Common Issues

**Repository Links Not Updated:**

- Verify the tool is enabled (`"Enabled": true`)
- Check that repository mappings are correctly configured
- Ensure target repositories exist in the target system
- Review migration logs for repository-related messages

**Missing Target Repositories:**

- Confirm Git repositories were migrated to target system
- Verify repository names match the mapping configuration
- Check permissions for accessing target repositories
- Review target system repository configuration

**Changeset Link Issues:**

- Coordinate with TfsChangeSetMappingTool configuration
- Review ShouldDropChangedSetLinks setting
- Ensure changeset mappings are available
- Check source control migration completion

**Mapping Configuration Errors:**

- Validate JSON syntax in repository mappings
- Check for typos in source and target repository names
- Ensure mapping keys match source repository names exactly
- Review case sensitivity in repository names

**Performance Issues:**

- Repository mapping typically doesn't impact performance significantly
- Consider migration batch sizes for large numbers of work items
- Monitor target system performance during migration
- Review overall migration strategy timing

## Schema

{{< class-schema >}}
