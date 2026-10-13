---
title: Tfs Shared Query Processor
description: The TfsSharedQueryProcessor migrates shared queries and their folders from one project to another, keeping the team context of @CurrentIteration and @TeamAreas.
dataFile: reference.processors.tfssharedqueryprocessor.yaml
slug: tfs-shared-query-processor
aliases:
  - /docs/Reference/Processors/TfsSharedQueryProcessor
  - /Reference/Processors/TfsSharedQueryProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/TfsSharedQueryProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/TfsSharedQueryProcessor/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2663
---

{{< class-description >}}

## Overview

### How It Works

The processor walks the source project's shared query folders, creates any missing folders in the target, and creates each query that does not already exist there. Personal queries ("My Queries") are not migrated, and queries that already exist in the target folder are skipped, so the processor can be run again safely.

Each query's WIQL is read from the source and rewritten for the target:

- Area and iteration paths that start with the source project name are changed to the target project name. With `PrefixProjectToNodes`, the source project name is also added as the first node under the target project.
- Team-scoped macros such as `@CurrentIteration('[Source]\Team')`, `@CurrentIteration('[Source]\Team') + 1` and `@TeamAreas('[Source]\Team')` are changed to point at the same team in the target project, `[Target]\Team`.
- References to the source default team (`<Source> Team`) are changed to the target default team (`<Target> Team`) when the target has one.
- Field reference names are replaced as configured in `SourceToTargetFieldMappings`.

The WIQL is read and written through the REST Queries API, because the TFS Object Model drops the team from `@CurrentIteration` and rejects `@CurrentIteration + n`. If either server does not support the REST Queries API (TFS 2013), the processor logs a warning and migrates queries with the Object Model instead. In that case, queries that use team-scoped macros use the default team in the target.

### Use Cases

- Moving a project's shared queries to a new project or organisation, together with work items.
- Consolidating several projects into one, using `PrefixProjectToNodes` to keep each source project's area and iteration paths apart.

## Options

{{< class-options >}}

## Samples

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Good Practices

- Migrate teams before queries, for example with the TfsTeamSettingsProcessor, so that queries using `@CurrentIteration` or `@TeamAreas` find their team in the target.
- Migrate area and iteration paths before queries, so that the rewritten paths exist in the target.

## Troubleshooting

- **"Query '...' refers to team '...', which does not exist in target project"**: the query is migrated, but it returns no results until a team with that name exists in the target. Create the team, or migrate teams first.
- **"Error saving query '...', probably due to invalid area or iteration paths"**: the target rejected the rewritten WIQL. Run with `Debug` logging to see the source and target WIQL, then check that the area and iteration paths and any mapped fields exist in the target.
- **"The REST Queries API is not available"**: one of the servers is older than TFS 2015. Queries are migrated with the TFS Object Model, which cannot keep the team context of `@CurrentIteration` or `@TeamAreas`, so check those queries after the migration.

## Metadata

{{< class-metadata >}}

## Schema

{{< class-schema >}}
