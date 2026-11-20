---
name: speckit-engineering-checks-arch-check
description: 'Check the plan against the repository''s architecture rules'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/arch-check.md
---

# Architecture check

Read the repository's architecture rules from its constitution. Check the current plan against each one.

$ARGUMENTS

Write `.specify/reviews/arch-check.md`: one finding per rule broken, naming the rule and the part of the plan. End with `Open findings: N`.