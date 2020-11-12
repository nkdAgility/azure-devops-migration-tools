## Endpoints: TfsEndpoint

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](./index.md) > **TfsEndpoint**

No description, create a template

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| AuthenticationMode | AuthenticationMode | {Description} | {Default Value} |
| AccessToken | String | {Description} | {Default Value} |
| Organisation | String | {Description} | {Default Value} |
| Project | String | {Description} | {Default Value} |
| ReflectedWorkItemIdField | String | {Description} | {Default Value} |
| ToConfigure | Type | {Description} | {Default Value} |
| Direction | EndpointDirection | {Description} | {Default Value} |
| Enrichers | List | {Description} | {Default Value} |


### Example JSON

```JSON
{
  "ObjectType": "TfsEndpointOptions",
  "AuthenticationMode": "AccessToken",
  "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
  "Organisation": "https://dev.azure.com/nkdagility-preview/",
  "Project": "NeedToSetThis",
  "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
  "Direction": "Source",
  "Enrichers": null
}
```