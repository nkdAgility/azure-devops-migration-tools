---
name: speckit-engineering-checks-api-story
description: 'Spec bar for a Product Backlog Item: API story'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/api-story.md
---

# API story

For the item in `.specify/items/current.json` and its current spec and plan:

How a client will use this capability through the API: which endpoints change or are added, their requests, responses and errors, and one worked example. If the change has no API surface, say why.

$ARGUMENTS

Write it to the item's record (`specs/<number>-<name>/item.md`) as a section headed `## API story`. If something cannot be settled, write it under `## Questions` as `- Q-n <question> (for <who>) open`, and leave the section saying `open`.