---
applyTo: "docs/content/docs/how-to/migrating-plans-and-suites/index.md"
---

# Migrating Plans and Suites page

What the how-to page at `docs/content/docs/how-to/migrating-plans-and-suites/index.md` must say, so it matches
`platform/specs/tfs-object-model/test-plans-and-suites.md`.

- The three steps, in order: work items with Test Cases, Shared Steps and Shared Parameters in one run; Test Variables
  and Test Configurations; then rebuilding Test Plans and Suites.
- Re-running: running the Plans and Suites step again into an existing target plan adds the Test Cases missing from its
  suites, the root suite included, and leaves those already there. The page must not describe the root suite as an
  exception.
- A Test Case not yet migrated is left out, an error names it, and the run exits with code 1; the page tells the reader
  to migrate it and run the step again.
- `OnlyElementsWithTag`: the page lists, level by level, what the tag filter skips, and says that a suite's own Test
  Cases, the root suite's included, are added only when that suite has the tag.
