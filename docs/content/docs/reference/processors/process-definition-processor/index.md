---
title: Process Definition Processor
description: Process definition processor used to keep processes between two orgs in sync
dataFile: reference.processors.processdefinitionprocessor.yaml
slug: process-definition-processor
aliases:
  - /docs/Reference/Processors/ProcessDefinitionProcessor
  - /Reference/Processors/ProcessDefinitionProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/ProcessDefinitionProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/ProcessDefinitionProcessor/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2798
---

{{< class-description >}}

## How It Works

For each source process in `Processes`, the processor finds the target process by `ProcessMaps` (or by the same name) and then syncs behaviors and work item types into it.

- **Process:** if the target process does not exist, it is created with the name from `ProcessMaps`, or with the source name when there is no map.
- **Behaviors:** custom behaviors are created or updated. Inherited behaviors, such as *Stories* once *User Story* is customised, already exist in every derived process, so they are only updated. System behaviors are skipped.
- **Work item types:** types that are not customised in the source are skipped. If the source type is inherited and the target only has the locked system type, the processor first creates an inherited copy of it in the target.
- **Fields and rules:** only custom and inherited definitions are written. System fields and system rules come with the parent process and cannot be changed, so they are skipped.
- **Layout:** pages and custom groups are created when missing. A custom group's controls are added to it, and a control that sits in a different group in the target is moved into the right group.

## Troubleshooting

- **A step fails with "Please migrate it manually."** The processor logs the failed definition with the request and the service's response, then carries on with the rest of the process. Fix the item in the target and run the processor again; it is safe to re-run.
- **"Unable to create or find the target process".** The target process could not be created, for example because of an invalid name in `ProcessMaps`. Nothing else is synced for that process.

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

## Schema

{{< class-schema >}}
