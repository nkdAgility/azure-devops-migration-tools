---
title: TestConfigurationsMigrationContext
layout: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TestConfigurationsMigrationConfig",
  "Enabled": false
}
```