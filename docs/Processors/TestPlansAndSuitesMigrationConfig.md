# Test plans and test suites migration

This processor migrate test suites and test plans. This should be run after `TestConfigurationsMigrationConfig`.

> Warning: This migration can result in data lost because unsafe links are not able to migrate! (see description for parameter `RemoveInvalidTestSuiteLinks` or [this issue](https://github.com/nkdAgility/azure-devops-migration-tools/issues/178).)

| Parameter name                | Type    | Description                                 | Default Value                            |
|-------------------------------|---------|---------------------------------------------|------------------------------------------|
| `Enabled`                     | Boolean | Active the processor if it true.            | false
| `$type`                       | string  | The name of the processor                   | TestPlansAndSuitesMigrationConfig
| `PrefixProjectToNodes`        | Boolean | Prefix the nodes with the new project name. | false
| `OnlyElementsWithTag`         | string  | The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all. | null
| `RemoveInvalidTestSuiteLinks` | Boolean | This option will skip invalid links. That is usually happened if in a test plan is a link to a tfvc changeset in the test case.<br>If that option is false you get an error if you have unsaved links like this in your test plan. If it true you only get a warning. | false
| `TestPlanQueryBit`            | string  | Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields. | null

