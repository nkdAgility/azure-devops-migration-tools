---
title: "Validating your Azure DevOps Migration Tools migration"
description: This tutorial covers the steps to validate and verify your migration using the Azure DevOps Migration Tools.
short_title: Get Validated
weight: 10
aliases:
  - /validation/
date: 2025-09-01T09:00:00Z

---

The Azure DevOps Migration Tools use a two-step validation process to ensure a smooth and successful migration. This process helps catch configuration or data issues early, saving time and preventing unnecessary data loading when migration cannot proceed.

## 1. Pre-Data-Load Validation

Before any data is loaded, the tool runs the [`TfsWorkItemTypeValidatorTool`]({{< ref "/docs/reference/tools/tfsworkitemtypevalidatortool/" >}}). This validator checks that all required work item types exist and are correctly mapped between the source and target systems. If any issues are found at this stage, the migration process stops immediately, preventing wasted effort on loading data that cannot be migrated.

**Key Points:**
- Ensures work item types are valid and mapped before loading data
- Prevents unnecessary data processing if migration cannot continue
- Fast feedback for configuration issues


## 2. Post-Data-Load Validation (Before Migration)

Before any data is actually migrated, the [`TfsValidateRequiredFieldTool`]({{< ref "/docs/reference/tools/tfsvalidaterequiredfieldtool/" >}}) runs. This validator checks that all required fields are present and populated in the data to be migrated. Running this validation step before migration helps minimize errors and ensures that the migration process will not fail due to missing or incomplete required fields.

**Key Points:**
- Validates required fields before migration begins
- Ensures all work items to be migrated meet requirements
- Minimizes migration errors by catching issues early

## Why This Matters

By running validation both before and after data loading, the migration process minimizes wasted effort and maximizes the chances of a successful migration. Early detection of issues allows for quick resolution, while post-load validation ensures data quality and compliance.

For more details on each validator, see their dedicated documentation pages:
- [`TfsWorkItemTypeValidatorTool`]({{< ref "/docs/reference/tools/tfsworkitemtypevalidatortool/" >}})
- [`TfsValidateRequiredFieldTool`]({{< ref "/docs/reference/tools/tfsvalidaterequiredfieldtool/" >}})