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
- **Picklist fields:** the source picklist is read and created in the target organisation with the same values, type (String or Integer) and *suggested* setting. The field is then created linked to it, so it is never created as plain text. On a work item type the field keeps its default value; its allowed values come from the picklist. If the field already exists in the target, nothing is removed or converted:
  - **as a plain field:** it is left as it is and a warning is logged.
  - **with a picklist of the same type:** any source values it lacks are added. Its other values and its *suggested* setting are kept.
  - **with a picklist of another type:** it is left as it is and a warning is logged.
- **Layout:** pages and custom groups are created when missing. A custom group's controls are added to it, and a control that sits in a different group in the target is moved into the right group. Custom fields that the source places on an inherited group are added to the same group in the target.
- **Visibility:** controls, custom groups and system controls (such as *Reason*) that are hidden in the source are hidden in the target, and ones shown in the source are shown again. This covers inherited controls, such as *Risk* or *Remaining Work*, as well as custom fields. Only what differs is changed, so a re-run changes nothing. Azure DevOps does not store a hidden flag on an inherited group, so to hide one, hide its controls.

## Troubleshooting

- **A step fails with "Please migrate it manually."** The processor logs the failed definition with the request and the service's response, then carries on with the rest of the process. Fix the item in the target and run the processor again; it is safe to re-run.
- **"Unable to create or find the target process".** The target process could not be created, for example because of an invalid name in `ProcessMaps`. Nothing else is synced for that process.
- **"already exists on [Target] as string without a picklist".** The target field was created as plain text, for example by an earlier version of the tool. A field's type can't be changed in place. To make it a picklist, delete the field in the target (this deletes its data) and run the processor again, or leave it as text and exclude it from the work item type validation with `ExcludeSourceFields`.
- **"picklist field(s) were not created".** The source picklist could not be read, or the target picklist could not be created. The field is skipped rather than created as text, and the run is marked as failed. Fix the cause shown in the log and run the processor again.

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
