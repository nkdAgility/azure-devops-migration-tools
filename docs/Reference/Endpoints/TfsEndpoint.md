## Endpoints: TfsEndpoint

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](./index.md) > **TfsEndpoint**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| AuthenticationMode | AuthenticationMode | missng XML code comments | missng XML code comments |
| AccessToken | String | missng XML code comments | missng XML code comments |
| Organisation | String | missng XML code comments | missng XML code comments |
| Project | String | missng XML code comments | missng XML code comments |
| ReflectedWorkItemIdField | String | missng XML code comments | missng XML code comments |
| Direction | EndpointDirection | missng XML code comments | missng XML code comments |
| EndpointEnrichers | List | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "MigrationTools.Endpoints.TfsEndpointOptions, MigrationTools.Clients.AzureDevops.ObjectModel",
  "AuthenticationMode": "AccessToken",
  "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
  "Organisation": "https://dev.azure.com/nkdagility-preview/",
  "Project": "NeedToSetThis",
  "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
  "Direction": "Source",
  "EndpointEnrichers": null
}
```