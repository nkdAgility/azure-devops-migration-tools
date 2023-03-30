---
title: TestVariablesMigrationContext
layout: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TestVariablesMigrationConfig",
  "Enabled": false
}
```