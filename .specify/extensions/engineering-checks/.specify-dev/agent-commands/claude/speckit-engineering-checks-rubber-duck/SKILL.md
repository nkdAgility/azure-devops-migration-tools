---
name: speckit-engineering-checks-rubber-duck
description: 'Spec bar for a Product Backlog Item: Rubber duck'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/rubber-duck.md
---

# Rubber duck

For the item in `.specify/items/current.json` and its current spec and plan:

Explain the spec and plan step by step to someone who knows nothing about it. Every place the explanation stalls, or needs a fact the spec does not give, is a finding.

$ARGUMENTS

Write it to the item's record (`specs/<number>-<name>/item.md`) as a section headed `## Rubber duck`. If something cannot be settled, write it under `## Questions` as `- Q-n <question> (for <who>) open`, and leave the section saying `open`.