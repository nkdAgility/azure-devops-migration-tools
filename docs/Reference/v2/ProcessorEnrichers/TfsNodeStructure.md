---
title: TfsNodeStructure
layout: default
template: default
pageType: reference
classType: ProcessorEnrichers
architecture: v2
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| AreaMaps | Dictionary | missng XML code comments | missng XML code comments |
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| IterationMaps | Dictionary | missng XML code comments | missng XML code comments |
| NodeBasePaths | String[] | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | missng XML code comments | missng XML code comments |
| RefName | String | missng XML code comments | missng XML code comments |
| ShouldCreateMissingRevisionPaths | Boolean | missng XML code comments | missng XML code comments |
{: .table .table-striped .table-bordered .d-none .d-md-block}


### Example JSON

```JSON
{
  "$type": "TfsNodeStructureOptions",
  "Enabled": true,
  "PrefixProjectToNodes": false,
  "NodeBasePaths": null,
  "AreaMaps": {
    "$type": "Dictionary`2"
  },
  "IterationMaps": {
    "$type": "Dictionary`2"
  },
  "ShouldCreateMissingRevisionPaths": true
}
```