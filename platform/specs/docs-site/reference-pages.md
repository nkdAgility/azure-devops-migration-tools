# Reference pages

The reference pages under `docs/content/docs/reference/` describe each processor and tool. Their option tables and
samples come from `docs/data/` and `docs/static/schema/`, which `GenerateDocs.ps1` writes from the option classes; the
prose is written by hand and changes with the behaviour it describes.

## Re-runs and Git links

- `reference/processors/tfs-workitem-migration-processor/index.md` describes re-running against migrated work items:
  what a repair save does, that every catch-up save sets the state-stamped fields to the source values, and which
  stamped values a re-run corrects and which fail the item (#3326).
- `reference/tools/tfsgitrepositorytool/index.md` describes links left pointing at the source when the target lacks
  the commit or branch, and that a re-run counts such a link as present until the commit or branch is in the target
  (#3326).

The behaviour behind these pages is in `platform/specs/tfs-object-model/rerun.md`.
