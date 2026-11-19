---
description: "Adoption: plan the move of this repo's source-of-truth docs into docs/specs/"
---

# Plan the docs conversion

Read this repo's documentation and agent files. Plan how they move into the docs layout:

```
docs/            for people: guides by audience; docs/adr/ for decisions
docs/specs/      the source of truth: lexicon.md, architecture.md, rules.md, seams.md
docs/specs/{system}/   README.md, architecture.md, rules.md, contracts.md, behaviour.md
.agents/         agent set-up only (reading order, routing, task profiles)
```

$ARGUMENTS

Write `.specify/adopt/docs-plan.md` with:

1. `## Systems`: one line per system, `- <system>: <source paths>`.
2. `## Moves`: one line per file, exactly `git mv <from> <to>`. Move a file whole; never plan an edit here.
3. `## After the moves`: files that must then be split, merged or edited, one line each, done in later commits so history follows the move.
4. `## New`: files to create because nothing exists yet, with what is true today.

Do not move or change anything. Martin approves this plan before it is applied.