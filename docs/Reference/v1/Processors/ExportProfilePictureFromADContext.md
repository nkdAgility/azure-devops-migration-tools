## Processors: ExportProfilePictureFromADContext

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v1](../index.md) > [Processors](index.md)> **ExportProfilePictureFromADContext**

Downloads corporate images and updates TFS/Azure DevOps profiles

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Domain | String | The source domain where the pictures should be exported. | String.Empty |
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| Password | String | The password of the user that is used to export the pictures. | String.Empty |
| PictureEmpIDFormat | String | TODO: You wpuld need to customise this for your system. Clone repo and run in Debug | String.Empty |
| Username | String | The user name of the user that is used to export the pictures. | String.Empty |


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