---
title: Tfs Test Plans And Suites Migration Processor
description: Rebuilds Suites and plans for Test Cases migrated using the WorkItemMigration
dataFile: reference.processors.tfstestplansandsuitesmigrationprocessor.yaml
slug: tfs-test-plans-and-suites-migration-processor
aliases:
  - /docs/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor
  - /Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2613
---

{{< class-description >}}

## Options

{{< class-options >}}

## Samples

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Metadata

{{< class-metadata >}}

## Troubleshooting

### Test Cases missing from rebuilt suites

The processor adds a Test Case to a suite only if it can find the migrated Test Case in the target by its reflected work item id. If it cannot, it logs an error naming the Test Case and suite, carries on with the rest of the plans, and the run exits with code 1. Migrate the Test Cases with `TfsWorkItemMigrationProcessor` first, check that their reflected work item id field is populated (see [Migrating Plans and Suites](/docs/how-to/migrating-plans-and-suites/)), then run this processor again.

## Schema

{{< class-schema >}}
