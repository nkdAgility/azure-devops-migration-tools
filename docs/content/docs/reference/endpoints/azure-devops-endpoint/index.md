---
title: Azure DevOps Endpoint
description: Azure DevOps REST API endpoint implementation for connecting to Azure DevOps organizations. Provides HTTP client access and pipeline-related API operations for migration scenarios.
dataFile: reference.endpoints.azuredevopsendpoint.yaml
slug: azure-devops-endpoint
aliases:
- /docs/Reference/Endpoints/AzureDevOpsEndpoint
- /Reference/Endpoints/AzureDevOpsEndpoint
- /learn/azure-devops-migration-tools/Reference/Endpoints/AzureDevOpsEndpoint
- /learn/azure-devops-migration-tools/Reference/Endpoints/AzureDevOpsEndpoint/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2702

---
The Azure DevOps Endpoint is a critical component of the Azure DevOps Migration Tools that facilitates seamless connectivity between your migration processes and Azure DevOps organizations. This endpoint leverages the Azure DevOps REST API to provide comprehensive access to projects, work items, and other Azure DevOps resources.

## Purpose and Use Cases

The Azure DevOps Endpoint serves several key purposes in migration scenarios:

- **Source and Target Connectivity**: Acts as both source and destination endpoint for migrating data between Azure DevOps organizations
- **Work Item Migration**: Enables the transfer of work items, including their history, attachments, and relationships
- **Project-Level Operations**: Provides access to project-specific configurations and settings
- **Authentication Management**: Handles secure authentication using Personal Access Tokens (PAT)
- **API Integration**: Offers a standardized interface for interacting with Azure DevOps REST APIs

## Options

{{< class-options >}}

## Behaviour

The Azure DevOps Endpoint exhibits the following key behaviors:

- **Connection Pooling**: Maintains efficient HTTP connections to minimize overhead during bulk operations
- **Rate Limiting Compliance**: Automatically handles Azure DevOps API rate limits to prevent throttling
- **Error Handling**: Provides robust error handling with retry mechanisms for transient failures
- **Reflected Work Item Tracking**: Uses custom fields to track migrated items and prevent duplicates

## Best Practices

When configuring and using the Azure DevOps Endpoint, consider these best practices:

- **Security**: Use Personal Access Tokens with minimal required permissions
- **Organization URLs**: Always include the full organization URL (e.g., `https://dev.azure.com/myorganization/`)
- **Project Scope**: Ensure the specified project exists and is accessible with the provided credentials
- **Custom Fields**: Configure the ReflectedWorkItemIdField to match your organization's custom field setup
- **Network Considerations**: Ensure firewall and proxy settings allow access to Azure DevOps services

## Limitations

Be aware of these limitations when using the Azure DevOps Endpoint:

- **API Rate Limits**: Subject to Azure DevOps API throttling policies
- **Permission Dependencies**: Requires appropriate permissions on both source and target organizations
- **Network Dependencies**: Requires stable internet connectivity for cloud-based Azure DevOps
- **Custom Field Requirements**: May require custom fields to be pre-configured in target organizations

## Samples

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Metadata

{{< class-metadata >}}
