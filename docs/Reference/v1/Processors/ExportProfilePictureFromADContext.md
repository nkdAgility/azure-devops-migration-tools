---
title: ExportProfilePictureFromADContext
layout: default
template: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Downloads corporate images and updates TFS/Azure DevOps profiles

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Domain | String | The source domain where the pictures should be exported. | String.Empty |
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| Password | String | The password of the user that is used to export the pictures. | String.Empty |
| PictureEmpIDFormat | String | TODO: You wpuld need to customise this for your system. Clone repo and run in Debug | String.Empty |
| Username | String | The user name of the user that is used to export the pictures. | String.Empty |
{: .table .table-striped .table-bordered .d-none .d-md-block}


### Example JSON

```JSON
{
  "$type": "ExportProfilePictureFromADConfig",
  "Enabled": false,
  "Domain": null,
  "Username": null,
  "Password": null,
  "PictureEmpIDFormat": null
}
```