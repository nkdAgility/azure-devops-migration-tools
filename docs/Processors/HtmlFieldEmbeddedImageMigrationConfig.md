# HTML field embedded image migration

This processor migrate all images in all text fields. This must run after `VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig`.



| Parameter name                 | Type          | Description                              | Default Value                            |
|--------------------------------|---------------|------------------------------------------|------------------------------------------|
| `Enabled`                      | Boolean       | Active the processor if it true.         | false                                    |
| `ObjectType`                   | string        | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.HtmlFieldEmbeddedImageMigrationConfig |
| `QueryBit`                     | string        | A query to select work items that can have images |                                          |
| `FromAnyCollection`            | Boolean       | Define if the images can retrieved from collection instead of the collection. This can be use full if you have moved work items. | false                                    |
| `AlternateCredentialsUsername` | string        | Username used for VSTS basic authentication using alternate credentials. Leave empty for default credentials  |                                          |
| `AlternateCredentialsPassword` | string        | Username used for VSTS basic authentication using alternate credentials. Leave empty for default credentials  |                                          |
| `Ignore404Errors`              | Boolean       | Ignore 404 errors and continue on images that don't exist anymore | false                                    |
| `DeleteTemporaryImageFiles`    | Boolean       | Delete temporary files that were downloaded | false                                    |
| `SourceServerAliases`          | Array<string> | All servers you trust so the image will be copied. The source TFS / Azure DevOps (Server) are always trusted. |                                          |
