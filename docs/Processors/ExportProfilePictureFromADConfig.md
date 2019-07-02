# Export profile picture from AD

This processor can extract the profile pictures of the user from the AD. The user for the export must have `todo: find out what rights are required to do this` rights. 

> Note: Make sure you have enough disk space.


| Parameter name       | Type    | Description                              | Default Value                            |
|----------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`            | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`         | string  | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.ExportProfilePictureFromADConfig |
| `Domain`             | string  | The source domain where the pictures should be exported. |                                          |
| `Username`           | string  | The user name of the user that is used to export the pictures. |                                          |
| `Password`           | string  | The password of the user that is used to export the pictures. |                                          |
| `PictureEmpIDFormat` | string  | TODO: I currently don't know it          |                                          |

