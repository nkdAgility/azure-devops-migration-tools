---
title: Field Clear Map
description: Removes data from a specified field by setting its value to null during work item migration.
dataFile: reference.fieldmaps.fieldclearmap.yaml
slug: field-clear-map
aliases:
  - /docs/Reference/FieldMaps/FieldClearMap
  - /Reference/FieldMaps/FieldClearMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldClearMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldClearMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2791
---

{{< class-description >}}

The **FieldClearMap** is used to explicitly clear a field value on migrated work items by setting the target field to `null`. This is especially useful for sanitising or removing unwanted metadata, calculated fields, or legacy data that should not be carried into the target system.

## Purpose and Use Cases

This map is typically used when:

- You want to remove **confidential** or **obsolete** field values from the target.
- You're migrating to a system where a field is **no longer in use** or **read-only**.
- A field value causes validation errors in the target and should be reset.

## Options

{{< class-options >}}

## Behaviour

- Clears the value of the specified `targetField` by setting it to `null`.
- Executed **before saving** the work item to the target.
- Only affects the latest revision unless the processor is configured to rewrite history.
- Will silently **overwrite** any existing value in that field on the target.

## Practices

- Ensure that the `targetField` is **writable** in the target project.
- Use this map only when you **intentionally want to remove data**; it will override any mapped or default value.
- Combine with `ApplyTo` to limit field clearing to specific types where the field is present.
- Be cautious when clearing **system fields**; not all fields accept `null` values.

## Limitations

- Does not validate whether the field actually exists on the target.
- If `targetField` is a **required field** in the target process, this may cause a validation error.
- Does not support conditional clearing; the field is always cleared if this map applies.

## Samples

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Metadata

{{< class-metadata >}}

## Schema

{{< class-schema >}}
