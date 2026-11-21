---
name: speckit-engineering-checks-refactor-assessment
description: 'Spec bar for a Product Backlog Item: Refactor assessment'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/refactor-assessment.md
---

# Refactor assessment

For the item in `.specify/items/current.json` and its current spec and plan:

What existing code this change touches that should be refactored first or alongside, what each refactor buys, and what it costs. Say plainly if none is worth doing.

$ARGUMENTS

Write it to the item's record (`specs/<number>-<name>/item.md`) as a section headed `## Refactor assessment`. If something cannot be settled, write it under `## Questions` as `- Q-n <question> (for <who>) open`, and leave the section saying `open`.