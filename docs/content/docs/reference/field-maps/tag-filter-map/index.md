---
title: Tag Filter Map
description: |
  The Tag Filter Map keeps only the approved tags on the target work item and drops the blocked ones, logging every tag it drops.
dataFile: reference.fieldmaps.tagfiltermap.yaml
slug: tag-filter-map
date: 2026-11-17T12:00:00Z
---

## Overview

The Tag Filter Map controls which tags arrive on the target work items. Give it an approved list and only those tags get through; give it a blocked list and those tags never get through.

{{< class-description >}}

## How It Works

For each work item revision it applies to, after the tags have been copied:

1. The target work item's tags are split on `;`. Spaces at the ends of each tag are trimmed, and empty or duplicate tags are removed.
2. If `includeTags` has entries, a tag is kept only when it matches one of them. If `includeTags` is empty, every tag is kept at this step.
3. A tag that matches any entry in `excludeTags` is dropped, even when it also matches `includeTags`.
4. The tags that are dropped are removed from the target work item, and each one is logged at Information level.

An entry matches a tag in one of two ways:

- **Exact name:** `Approved` matches the tag `Approved` only. Matching is case-sensitive, so it does not match `approved`.
- **Pattern:** an entry starting with `regex:` is a .NET regular expression. `regex:^Team-` matches every tag that starts with `Team-`. The pattern matches anywhere in the tag unless you anchor it with `^` and `$`.

An invalid pattern or an empty entry stops the migration at start-up with a message naming the entry.

In configuration, set `FieldMapType` to `TagFilterMap`.

## Use Cases

- **Approved tags only:** a migration where only a governed list of tags may reach the target.
- **Removing noise:** drop personal or temporary tags such as `todo` or `regex:^tmp-` while keeping the rest.

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Only approved tags

```json
{
  "FieldMapType": "TagFilterMap",
  "ApplyTo": ["*"],
  "includeTags": ["Approved", "Release", "Customer"]
}
```

#### A family of tags, except one

```json
{
  "FieldMapType": "TagFilterMap",
  "ApplyTo": ["*"],
  "includeTags": ["regex:^Team-"],
  "excludeTags": ["Team-Legacy"]
}
```

#### Drop some tags, keep everything else

```json
{
  "FieldMapType": "TagFilterMap",
  "ApplyTo": ["*"],
  "excludeTags": ["todo", "regex:^tmp-"]
}
```

## Good Practices

- Run the Tag Filter Map **after** every field map that adds tags, such as the [Field Value To Tag Map]({{< ref "docs/reference/field-maps/field-value-to-tag-map" >}}). The filter only sees the tags present when it runs, so a tag added later is neither kept by `includeTags` nor removed by `excludeTags`.
- Field maps with `ApplyTo` `["*"]` run before the maps for a specific work item type, whatever their order in the configuration. Within one `ApplyTo` scope, maps run in configuration order. Give the Tag Filter Map the same `ApplyTo` as the maps it must follow, or a narrower one.
- To rename tags as well, list the [Tag Mapping Map]({{< ref "docs/reference/field-maps/tag-mapping-map" >}}) before this one, so `includeTags` only needs the new names.
- Because matching is case-sensitive, list each spelling you want to keep, or use a pattern such as `regex:(?i)^approved$`.

## Troubleshooting

- **A tag you expected is missing:** search the log for `TagFilterMap: dropped tag`. Check its spelling and case against `includeTags`, and that it does not match `excludeTags`.

## Schema

{{< class-schema >}}
