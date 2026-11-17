---
title: Tag Mapping Map
description: |
  The Tag Mapping Map renames tags on the target work item from a list of source and target pairs, and keeps every other tag unchanged.
dataFile: reference.fieldmaps.tagmappingmap.yaml
slug: tag-mapping-map
date: 2026-11-17T12:00:00Z
---

## Overview

The Tag Mapping Map renames tags on the way to the target: `Bug-Bash` can become `BugBash`, and every `v1.x` tag can become `Legacy`.

{{< class-description >}}

## How It Works

For each work item revision it applies to, after the tags have been copied:

1. The target work item's tags are split on `;`. Spaces at the ends of each tag are trimmed, and empty or duplicate tags are removed.
2. Each tag is checked against `mappings` in order. The first entry whose `source` matches renames it to `target`.
3. A tag that matches no entry is kept unchanged.
4. If a rename gives a tag that is already there, the duplicate is removed.

A `source` matches a tag in one of two ways:

- **Exact name:** `Bug-Bash` matches the tag `Bug-Bash` only. Matching is case-sensitive, so it does not match `bug-bash`.
- **Pattern:** a `source` starting with `regex:` is a .NET regular expression. The part of the tag it matches is replaced with `target`, so `target` can use groups such as `$1`. Anchor the pattern with `^` and `$` to replace the whole tag.

An invalid pattern, an empty `source` or an empty `target` stops the migration at start-up with a message naming the entry. To remove tags, use the [Tag Filter Map]({{< ref "docs/reference/field-maps/tag-filter-map" >}}).

In configuration, set `FieldMapType` to `TagMappingMap`.

## Use Cases

- **Tidying names:** merge spellings such as `Bug-Bash` and `Bugbash` into one tag.
- **Restructuring:** turn `Sprint 12` into `S12`, or every `v1.x` release tag into `Legacy`.

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Rename tags

```json
{
  "FieldMapType": "TagMappingMap",
  "ApplyTo": ["*"],
  "mappings": [
    { "source": "Bug-Bash", "target": "BugBash" },
    { "source": "Bugbash", "target": "BugBash" }
  ]
}
```

#### Rename with a pattern

```json
{
  "FieldMapType": "TagMappingMap",
  "ApplyTo": ["*"],
  "mappings": [
    { "source": "regex:^Sprint (\\d+)$", "target": "S$1" },
    { "source": "regex:^v1\\..*$", "target": "Legacy" }
  ]
}
```

## Good Practices

- Run the Tag Mapping Map **before** the [Tag Filter Map]({{< ref "docs/reference/field-maps/tag-filter-map" >}}), so the approved list only needs the new names. Maps with `ApplyTo` `["*"]` run before type-specific maps whatever their configuration order, so give both maps the same `ApplyTo`; within one scope they run in configuration order.
- A `regex:` rename that would give an empty tag (for example `$1` when the group is empty) keeps the original tag.
- Put specific entries before general patterns: the first entry that matches wins.

## Troubleshooting

- **A tag is not renamed:** check the spelling and case of `source`, and that no earlier entry matches the tag first. Renames are logged at Debug level as `TagMappingMap: renamed tag`.

## Schema

{{< class-schema >}}
