# Test variables migration

This processor can migrate test variables that are defined in the test plans / suites. This must run before ` TestPlansAndSuitesMigrationConfig`

| Parameter name | Type    | Description                      | Default Value                            |
|----------------|---------|----------------------------------|------------------------------------------|
| `Enabled`      | Boolean | Active the processor if it true. | false                                    |
| `ObjectType`   | string  | The name of the processor        | TestVariablesMigrationConfig |