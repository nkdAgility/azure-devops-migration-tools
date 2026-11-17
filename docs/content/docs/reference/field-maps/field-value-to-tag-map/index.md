---
title: Field Value To Tag Map
description: |
  The Field Value To Tag Map adds a tag to the target work item when a source field's value matches a regular expression, formatted the way you choose.
dataFile: reference.fieldmaps.fieldvaluetotagmap.yaml
slug: field-value-to-tag-map
aliases:
  - /docs/Reference/FieldMaps/FieldValueToTagMap
  - /Reference/FieldMaps/FieldValueToTagMap
date: 2026-11-16T12:00:00Z
---

## Overview

The Field Value To Tag Map turns a source field's value into a tag on the target work item, but only when the value matches a pattern. Use it to keep a meaning that the target process has no field for, such as a *Blocked* flag, as a tag you can query.

{{< class-description >}}

## How It Works

For each work item it applies to:

1. If the source work item does not have `sourceField`, or its value is null, nothing happens. An empty string is not skipped.
2. If `pattern` is set, the value must match it as a regular expression. If `pattern` is empty, any non-null value matches, including an empty string: with `formatExpression` set to `Blocked`, an empty field still adds the `Blocked` tag. Use a pattern such as `.+` to skip empty values.
3. The tag is `formatExpression` with the value in place of `{0}`. If `formatExpression` is empty, the tag is the value itself.
4. The tag is added to the target work item's existing tags. Duplicate tags are removed, and the tags are only written if they changed.

In configuration, set `FieldMapType` to `FieldValueToTagMap`.

## Use Cases

- **Flags:** tag work items whose `Microsoft.VSTS.CMMI.Blocked` is `Yes`.
- **Selected states or categories:** tag only the values you care about, for example `Status: Active` and `Status: Resolved`.

## Configuration Structure

### Options

{{< class-options >}}

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Basic Examples

#### Tag blocked work items

```json
{
  "FieldMapType": "FieldValueToTagMap",
  "ApplyTo": ["*"],
  "sourceField": "Microsoft.VSTS.CMMI.Blocked",
  "pattern": "Yes",
  "formatExpression": "Blocked"
}
```

#### Tag selected values with a prefix

```json
{
  "FieldMapType": "FieldValueToTagMap",
  "ApplyTo": ["Bug"],
  "sourceField": "System.State",
  "pattern": "(Active|Resolved)",
  "formatExpression": "Status: {0}"
}
```

#### Tag work items by area path

Add one specific tag to every work item under an area. Leave `{0}` out of `formatExpression`, so the tag is fixed text rather than the area path. Backslashes in the path are escaped twice: once for JSON and once for the regular expression.

```json
{
  "FieldMapType": "FieldValueToTagMap",
  "ApplyTo": ["*"],
  "sourceField": "System.AreaPath",
  "pattern": "\\\\Team A(\\\\|$)",
  "formatExpression": "Team-A"
}
```

This tags work items in `MyProject\Team A` and everything below it, but not `MyProject\Team AB`. Use one map for each area and tag.

## Good Practices

- Anchor the pattern (`^Yes$`) when a partial match would tag too much: the pattern matches anywhere in the value, and the match is case-sensitive.
- To tag every value of a field, use the [Field To Tag Field Map]({{< ref "docs/reference/field-maps/field-to-tag-field-map" >}}) instead.

## Troubleshooting

- **No tag appears:** check that the field reference name in `sourceField` is the source's, that the work item type is in `ApplyTo`, and that the value matches `pattern`.

## Schema

{{< class-schema >}}
