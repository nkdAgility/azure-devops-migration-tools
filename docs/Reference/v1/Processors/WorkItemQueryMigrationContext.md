---
title: WorkItemQueryMigrationContext
layout: default
template: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
redirect_to: https://nkdagility.com/learn/azure-devops-migration-tools/Reference/v1/Processors/WorkItemQueryMigrationContext.html
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |
| SharedFolderName | String | The name of the shared folder, made a parameter incase it every needs to be edited | none |
| SourceToTargetFieldMappings | Dictionary | Any field mappings | none |


### Example JSON

```JSON
{
  "$type": "WorkItemQueryMigrationConfig",
  "Enabled": false,
  "PrefixProjectToNodes": false,
  "SharedFolderName": "Shared Queries",
  "SourceToTargetFieldMappings": null
}
```