# Import profile picture

This processor can import profile pictures. This processor must run after `VstsSyncMigrator.Engine.Configuration.Processing.ExportProfilePictureFromADConfig`.

| Parameter name | Type    | Description                              | Default Value                            |
|----------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`      | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`   | string  | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.ImportProfilePictureConfig |
