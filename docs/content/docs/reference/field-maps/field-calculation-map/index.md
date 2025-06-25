---
title: Field Calculation Map
description: |
  The Field Calculation Map is a specialised field map used in the Azure DevOps Migration Tools. It allows you to compute values dynamically during migration by applying mathematical or logical operations on numeric fields. Calculations are defined using NCalc expressions and can reference one or more source fields using named parameters.

  This is especially useful for scenarios where derived values need to be migrated, such as calculating cost based on effort and rate, or generating summary fields that are not explicitly stored in the source system.
dataFile: data/reference.fieldmaps.fieldcalculationmap.yaml
slug: field-calculation-map
aliases:
  - /docs/Reference/FieldMaps/FieldCalculationMap
  - /Reference/FieldMaps/FieldCalculationMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldCalculationMap
  - /learn/azure-devops-migration-tools/Reference/FieldMaps/FieldCalculationMap/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2756
---

## Purpose and Use Cases

This map is typically used when:

- You want to calculate **Custom.EstimatedCost** as `EstimatedHours × HourlyRate`
- You need to **convert days to hours** or apply rounding
- You wish to **assign weights** to different numeric fields and store a computed score
- Your source project uses **different naming or units**, and you want to normalise during migration

---

## Options

{{< class-options >}}

## How NCalc Expressions Work

The [NCalc](https://github.com/ncalc/ncalc) engine supports:

- Standard arithmetic: `+`, `-`, `*`, `/`
- Logical operations: `&&`, `||`, `==`, `!=`
- Functions: `Pow(a, b)`, `Min(a, b)`, `Max(a, b)`, `Abs(x)`, etc.
- Conditional evaluation using `if(condition, trueValue, falseValue)`

**Example Expression:**

```ncalc
[effort] * [rate]
```

This multiplies the values of the two fields `Custom.EstimatedHours` and `Custom.HourlyRate`.

---

## Behaviour and Execution

- The field map executes during **work item migration** and **before saving** to the target.
- It applies to the **latest revision** only unless otherwise configured.
- The calculated value **overwrites** any existing value in `targetField`.

---

## Field Resolution Rules

- Each variable in the expression (e.g. `[effort]`) must be defined in `parameters`.
- Values are pulled from the **source work item** using the mapped reference names.
- If a referenced field is missing or null, the expression evaluation may fail.
- Non-numeric fields will cause an evaluation error.

---

## Practices

- Use consistent **naming conventions** for parameters to avoid confusion.
- Log intermediate values when troubleshooting expression evaluation.
- Always **test with a sample dataset** using `run-local` before production use.
- Use `ApplyTo` to limit the map to only relevant work item types.

---

## Limitations

- Only works with **numeric** source fields.
- Errors during evaluation are **not recoverable** mid-run unless caught at validation.
- Does **not support cross-work-item calculations**.

---
