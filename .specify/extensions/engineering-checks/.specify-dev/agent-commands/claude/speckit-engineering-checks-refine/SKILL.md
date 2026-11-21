---
name: speckit-engineering-checks-refine
description: 'Refining: gather what is needed to understand the item before any spec is written'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/refine.md
---

# Refine

For the item in `.specify/items/current.json`:

1. Read the source-of-truth docs for the area it touches (the spec root, `specRoot` in `.specify/memory/workflow.json`, default `docs/specs/`, by the repo's list of systems and their paths).
2. Work out the paths it will touch and what is out of scope.
3. For a Bug, reproduce it on `main` and record the exact steps and the result.
4. Rubber duck it: explain the item plainly, start to end — what it changes, for whom, and what it touches — and write down every gap that explaining it shows.
5. Check the approach and the paths it touches against each of the repo's architecture rules (its constitution and `platform/specs/` rules). One finding per rule it breaks.

$ARGUMENTS

A small item does every step the same, just smaller: each section is as long as the item needs, and a one-line change can have a one-line section.

Write these to the item's record (`specs/<number>-<name>/item.md`). It opens with front matter: the Purpose (the worker outcome: the one outcome this worker exists to deliver, in one or two sentences) and the `paths` (globs) the item may edit. Martin agrees both at Ready; the item is not Ready without them. The guard asks Martin before any edit outside the paths, and before any build, test or end-to-end run when the paths include no product code.

```
---
purpose: <the outcome this worker delivers>
paths:
  - <glob, for example src/MigrationTools/**>
---

## Sources read
- <doc path>: <what it says that matters here>

## Scope
- In: <paths and behaviour>
- Out: <what is not part of this item>

## Reproduction        (Bugs only)
1. <step> → <result on main>

## Rubber duck
<the item explained plainly, and the gaps it showed>

## Architecture check
- <rule>: <what in the approach breaks it, and what would settle it>
Open findings: N
```

Anything you cannot settle goes under `## Questions` as `- Q-n <question> (for <who>) open`. Do not write a spec or change any code.