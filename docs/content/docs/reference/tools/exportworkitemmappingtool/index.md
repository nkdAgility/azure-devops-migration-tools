---
title: Export Work Item Mapping Tool
description: Writes a JSON file that maps each source work item ID to the ID of the target work item it was migrated to, so you can trace, audit and post-process a migration.
dataFile: reference.tools.exportworkitemmappingtool.yaml
schemaFile: schema.tools.exportworkitemmappingtool.json
slug: export-work-item-mapping-tool
aliases:
  - /docs/Reference/Tools/ExportWorkItemMappingTool
  - /Reference/Tools/ExportWorkItemMappingTool
date: 2026-11-16T12:00:00Z
---

## Overview

The Export Work Item Mapping Tool records which target work item each source work item was migrated to, and writes those pairs to a JSON file at the end of the run. Use the file to trace a work item across the migration, to check that everything arrived, or as input to your own scripts.

{{< class-description >}}

### How It Works

1. **Recording:** as `TfsWorkItemMigrationProcessor` finishes each work item that has a target work item, it records the pair of IDs. This includes work items that already existed in the target from an earlier run.
2. **Saving:** when the processor finishes, including when it stops after a failure, the mappings are written to `TargetFile`. The mappings are written to `<TargetFile>.tmp` first and then copied over `TargetFile`, so a failure while writing the mappings leaves `TargetFile` as it was. The final copy is not atomic: a failure during that copy can still leave `TargetFile` incomplete.
3. **Merging:** with `PreserveExisting` set to `true` (the default), the mappings already in `TargetFile` are loaded and kept. If a source ID is mapped to a different target ID in this run, the new target ID wins and a warning is logged.

If the same source ID is recorded twice in one run with different target IDs, the first mapping is kept and an error is logged.

The tool only writes the file. The migration does not read it back to find work items on a re-run: that uses the reflected work item ID field (see [ReflectedWorkItemId]({{< ref "docs/setup/reflected-workitem-id" >}})).

### Use Cases

- **Traceability:** look up where a source work item went.
- **Auditing:** compare the number of mappings with the number of source work items you expected to migrate.
- **Post-processing:** feed the map into scripts that update references outside Azure DevOps, such as links in wikis or external systems.

## Configuration Structure

The tool is off unless you enable it. When `Enabled` is `true`, `TargetFile` must be set, or the configuration fails validation with `'TargetFile' is not set, so work item mappings cannot be saved.`

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

Add the tool to `CommonTools` in your configuration:

```json
{
  "MigrationTools": {
    "CommonTools": {
      "ExportWorkItemMappingTool": {
        "Enabled": true,
        "TargetFile": "c:\\temp\\WorkItemMappings.json",
        "PreserveExisting": true
      }
    }
  }
}
```

### Output

The file is a JSON object. Each key is a source work item ID and each value is the target work item ID, both as strings:

```json
{
  "101": "5001",
  "102": "5002",
  "107": "5003"
}
```

## Common Scenarios

- **Migrating in several runs:** keep `PreserveExisting` set to `true` and point every run at the same `TargetFile`, so the file holds every mapping from every run.
- **Starting a fresh map:** set `PreserveExisting` to `false`, and the file holds only the work items processed in this run.

## Good Practices

- Use a separate `TargetFile` for each source and target project pair.
- Keep the file with your migration logs; it is the quickest way to answer "where did work item 123 go?".

## Troubleshooting

- **No file is written:** check that `Enabled` is `true` and that the folder in `TargetFile` exists and can be written to. Failures are logged as `Failed to save work item mappings to '<file>'.`
- **A mapping is missing:** only work items that `TfsWorkItemMigrationProcessor` processed and that have a target work item are recorded. Work items filtered out before processing, for example by `FilterWorkItemsThatAlreadyExistInTarget`, are not recorded in that run; keep `PreserveExisting` on to keep them from earlier runs.

## Schema

{{< class-schema >}}
