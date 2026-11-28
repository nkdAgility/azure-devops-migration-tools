# Review log

`WorkItemReviewLog` (`src/MigrationTools/Services/WorkItemReviewLog.cs`) writes `work-items-to-review.jsonl` in the run's log folder, one row per problem someone must look at after the migration, and one follow-up file per concern. Each category is a constant on `WorkItemReviewCategory`; the end-of-run summary counts rows per category.

## Categories added by later work

- `reference-left-on-source`: a Test Case's step or parameter reference names a Shared Steps or Shared Parameter that has no match in the target, so the field still holds the source id. It is also written to `follow-up-links.jsonl`. The check is marked as run (`MarkChecked`) each time a Test Case's references are remapped, so a re-run that finds every reference resolves the earlier row. (#3256)
