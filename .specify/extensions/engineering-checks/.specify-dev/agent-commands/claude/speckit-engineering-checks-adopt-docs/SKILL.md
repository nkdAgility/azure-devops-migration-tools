---
name: speckit-engineering-checks-adopt-docs
description: 'Adoption: plan the move of this repo''s source-of-truth docs into the repo''s spec root (specRoot in .specify/memory/workflow.json, default docs/specs/)'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/adopt-docs.md
---

# Plan the docs conversion

Read this repo's documentation and agent files. Plan how they move into the docs layout:

```
docs/            for people: guides by audience; docs/adr/ for decisions
{specRoot}/      the source of truth: lexicon.md, architecture.md, rules.md, seams.md
{specRoot}/{system}/   README.md, architecture.md, rules.md, contracts.md, behaviour.md
.agents/         agent set-up only (reading order, routing, task profiles)
```

{specRoot} is `specRoot` in `.specify/memory/workflow.json` (default `docs/specs`). Where `docs/` is a website (for example a Hugo site), nothing under it is moved or changed, and the spec root is outside it.

$ARGUMENTS

Write `.specify/adopt/docs-plan.md` with:

1. `## Systems`: one line per system, `- <system>: <source paths>`.
2. `## Moves`: one line per file, exactly `git mv <from> <to>`. Move a file whole; never plan an edit here.
3. `## After the moves`: files that must then be split, merged or edited, one line each, done in later commits so history follows the move.
4. `## New`: files to create because nothing exists yet, with what is true today.

Do not move or change anything. Martin approves this plan before it is applied.