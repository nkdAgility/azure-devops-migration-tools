---
title: TFS User Mapping Tool
description: Maps user identities between source and target systems during work item migration, ensuring proper assignment of work items, history, and user references across different environments or organizations.
dataFile: reference.tools.tfsusermappingtool.yaml
schemaFile: schema.tools.tfsusermappingtool.json
slug: tfs-user-mapping-tool
aliases:
  - /docs/Reference/Tools/TfsUserMappingTool
  - /Reference/Tools/TfsUserMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsUserMappingTool
  - /learn/azure-devops-migration-tools/Reference/Tools/TfsUserMappingTool/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2804
---

## Overview

The TFS User Mapping Tool provides essential functionality for mapping user identities between source and target systems during migration. This tool ensures that work item assignments, history records, and user references are properly maintained when migrating between different environments, organizations, or domain structures.

The tool supports both automatic user matching and manual mapping configuration, allowing for flexible user identity management during complex migration scenarios.

### How It Works

The TFS User Mapping Tool operates in two main phases during migration:

1. **User Discovery and Export**: The tool can be used with export processors to identify and export user mappings
2. **Migration-Time Mapping**: During work item migration, the tool applies user mappings to maintain proper identity references

The tool processes user identity fields in work items and applies configured mappings to ensure users are properly referenced in the target system.

### Key Features

- **Automatic Email Matching**: Attempts to match users based on email addresses
- **Manual Mapping Files**: Supports external user mapping files for complex scenarios
- **Identity Field Processing**: Handles all identity-related fields in work items
- **Validation Options**: Can validate that all users exist in the target system
- **Group Membership**: Supports mapping based on group membership validation

### Use Cases

Common scenarios where the TFS User Mapping Tool is essential:

- **Cross-Domain Migration**: Moving between different Active Directory domains
- **Organization Migration**: Migrating between Azure DevOps organizations with different user bases
- **User Account Changes**: Handling scenarios where user accounts have changed between systems
- **Contractor to Employee**: Managing identity changes when contractors become employees
- **Merge Scenarios**: Consolidating users during organizational mergers

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

The TFS User Mapping Tool supports several configuration options for flexible user mapping:

```json
{
  "TfsUserMappingTool": {
    "Enabled": true,
    "IdentityFieldsToCheck": [
      "System.AssignedTo",
      "System.CreatedBy",
      "System.ChangedBy",
      "Microsoft.VSTS.Common.ActivatedBy",
      "Microsoft.VSTS.Common.ResolvedBy",
      "Microsoft.VSTS.Common.ClosedBy"
    ],
    "UserMappingFile": "usermapping.json",
    "MatchUsersByEmail": true,
    "SkipValidateAllUsersExistOrAreMapped": false,
    "ProjectCollectionValidUsersGroupName": "Project Collection Valid Users"
  }
}
```

### Complex Examples

#### Configuration Options

- **IdentityFieldsToCheck**: Array of field reference names that contain user identities
- **UserMappingFile**: Path to JSON file containing manual user mappings
- **MatchUsersByEmail**: Attempt automatic matching based on email addresses
- **SkipValidateAllUsersExistOrAreMapped**: Skip validation that all users are properly mapped
- **ProjectCollectionValidUsersGroupName**: Group name for validating user membership

## Common Scenarios

### Automatic Email-Based Matching

Enable automatic user matching based on email addresses:

```json
{
  "TfsUserMappingTool": {
    "Enabled": true,
    "MatchUsersByEmail": true,
    "SkipValidateAllUsersExistOrAreMapped": false
  }
}
```

### Manual User Mapping File

Use a custom mapping file for complex user scenarios:

```json
{
  "TfsUserMappingTool": {
    "Enabled": true,
    "UserMappingFile": "C:\\temp\\usermapping.json",
    "MatchUsersByEmail": false,
    "SkipValidateAllUsersExistOrAreMapped": false
  }
}
```

### Custom Identity Fields

Configure specific identity fields to process:

```json
{
  "TfsUserMappingTool": {
    "Enabled": true,
    "IdentityFieldsToCheck": [
      "System.AssignedTo",
      "System.CreatedBy",
      "System.ChangedBy",
      "Custom.TeamLead",
      "Custom.ProjectManager"
    ],
    "MatchUsersByEmail": true
  }
}
```

### Validation Bypass

Skip user validation for scenarios where not all users need to exist in target:

```json
{
  "TfsUserMappingTool": {
    "Enabled": true,
    "MatchUsersByEmail": true,
    "SkipValidateAllUsersExistOrAreMapped": true
  }
}
```

## User Mapping File Format

When using a manual user mapping file, the format should be a JSON array of mapping objects:

```json
[
  {
    "Source": "domain\\sourceuser",
    "Target": "targetdomain\\targetuser"
  },
  {
    "Source": "olddomain\\john.doe",
    "Target": "newdomain\\john.doe"
  },
  {
    "Source": "contractor@external.com",
    "Target": "employee@company.com"
  }
]
```

## Good Practices

### User Mapping Strategy

- **Email Matching First**: Use automatic email matching when possible to reduce manual configuration
- **Validate Mappings**: Always validate that target users exist before migration
- **Document Changes**: Maintain clear documentation of user mapping decisions
- **Test with Samples**: Validate user mappings with sample work items before full migration

### File Management

- **Secure Storage**: Store user mapping files securely due to sensitive identity information
- **Version Control**: Maintain user mapping files in version control for reproducibility
- **Backup Mappings**: Keep backups of user mapping configurations
- **Regular Updates**: Update mappings when user accounts change

### Performance Considerations

- **Efficient Lookups**: Tool performs cached lookups for optimal performance
- **Batch Processing**: User mappings are applied efficiently during work item processing
- **Identity Field Optimization**: Only configure necessary identity fields to improve performance

### Security and Compliance

- **Data Privacy**: Handle user identity information according to privacy policies
- **Access Control**: Restrict access to user mapping files and configurations
- **Audit Trail**: Maintain audit trails of user mapping changes
- **Compliance**: Ensure user mapping practices comply with organizational policies

## Troubleshooting

### Common Issues

**Users Not Found in Target:**

- Verify that target users exist in the target system
- Check that user accounts are active in the target environment
- Ensure proper permissions for user lookup operations
- Review group membership requirements

**Email Matching Failures:**

- Verify email addresses are consistent between source and target
- Check for email format differences or case sensitivity issues
- Consider using manual mapping for problematic users
- Review email address availability in both systems

**Mapping File Issues:**

- Validate JSON syntax in user mapping files
- Check file paths and accessibility
- Ensure proper encoding for special characters in usernames
- Verify source and target user format consistency

**Performance Problems:**

- Optimize the list of identity fields to check
- Use caching mechanisms for repeated user lookups
- Consider batch processing for large user sets
- Monitor system resources during user validation

**Validation Errors:**

- Review user existence in target system
- Check group membership requirements
- Validate user permissions for migration operations
- Consider using SkipValidateAllUsersExistOrAreMapped for specific scenarios

## Schema

{{< class-schema >}}
