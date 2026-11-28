# Work item fields

`TfsWorkItemMigrationProcessor` (`src/MigrationTools.Clients.TfsObjectModel/Processors/`) copies each source revision's
fields onto the target work item before it is saved.

- `System.Title` is passed through `StringManipulatorTool` like every other String field before it is set on the target.
- Every other String field that differs between source and target is passed through `StringManipulatorTool`; fields of
  other types are copied as they are.
