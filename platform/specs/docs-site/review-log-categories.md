# Review log categories on the docs site

Users find the review log's categories on the TFS Work Item Migration Processor reference page, `docs/content/docs/reference/processors/tfs-workitem-migration-processor/index.md`, in the category table. That table must list every constant on `WorkItemReviewCategory` with what it means and what to do.

- `reference-left-on-source` (#3256): a Test Case's step or parameter reference still holds a Shared Steps or Shared Parameter source id because that item is not in the target. The Migrating Plans and Suites how-to (`docs/content/docs/how-to/migrating-plans-and-suites/index.md`) explains the warning and says to include those types in the query and re-run.
