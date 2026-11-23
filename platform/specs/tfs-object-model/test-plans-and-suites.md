# Test Plans and Suites migration

`TfsTestPlansAndSuitesMigrationProcessor` (`src/MigrationTools.Clients.TfsObjectModel/Processors/`) rebuilds Test Plans
and their suites in the target with the TFS Object Model. It does not migrate Test Case work items: they must already
be in the target, found through the ReflectedWorkItemId.

## Plans

- A source plan is matched to a target plan by name. More than one target plan with the same name stops the run.
- No target plan: one is created, with the source plan's field maps, ReflectedWorkItemId and AssignedTo applied to the
  plan and its root suite, and the source root suite's default configurations.
- A target plan exists: it is reused as it is. The source root suite's default configurations are applied to its root
  suite again.

## Test Cases in a suite

For the root suite and for every static suite under it, on a new plan and on a re-run into an existing one:

- each source Test Case already in the target suite is left as it is, so a re-run adds no duplicates;
- each source Test Case missing from the target suite is added;
- a source Test Case not yet migrated is not added: an error names it and the suite, and the run exits with code 1.
  Migrate it, then run the processor again.

`OnlyElementsWithTag` applies before this: a suite without the tag has no Test Cases added, and a Test Case without the
tag is skipped.

## Suites

A suite under the root suite is matched by title under its parent. A missing static, dynamic or requirement-based suite
is created; an existing one is reused and its default configurations are applied again. Only suites reachable from a
plan's root suite are rebuilt.
