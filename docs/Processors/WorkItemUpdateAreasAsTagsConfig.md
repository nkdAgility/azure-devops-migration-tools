# Work item update areas as tags

This processor split the area path by `\` and paste the peaces to the tags.


| Parameter name      | Type    | Description                                                                                            | Default Value                   |
| ------------------- | ------- | ------------------------------------------------------------------------------------------------------ | ------------------------------- |
| `Enabled`           | Boolean | Active the processor if it true.                                                                       | false                           |
| `$type`             | string  | The name of the processor                                                                              | WorkItemUpdateAreasAsTagsConfig |
| `AreaIterationPath` | string  | This is a required parameter. That define the root path of the iteration. To get the full path use `\` |                                 |
