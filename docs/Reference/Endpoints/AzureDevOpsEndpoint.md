## Endpoints: AzureDevOpsEndpoint

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](./index.md) > **AzureDevOpsEndpoint**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Organisation | String | missng XML code comments | missng XML code comments |
| Project | String | missng XML code comments | missng XML code comments |
| AuthenticationMode | AuthenticationMode | missng XML code comments | missng XML code comments |
| AccessToken | String | missng XML code comments | missng XML code comments |
| ReflectedWorkItemIdField | String | missng XML code comments | missng XML code comments |
| EndpointEnrichers | List | missng XML code comments | missng XML code comments |
| RefName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "AzureDevOpsEndpointOptions",
  "Organisation": "https://dev.azure.com/nkdagility-preview/",
  "Project": "NeedToSetThis",
  "AuthenticationMode": "AccessToken",
  "AccessToken": "iksmyfwmracmyqb22p2rlytagg6mpzxu7ntowjvpihvk4fwcjzcq",
  "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
  "EndpointEnrichers": null
}
```