---
name: speckit-engineering-checks-guard
description: Blocks pushes to main and hand edits to run state
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/guard.md
---

# Guard

Runs before every tool use. It reads the tool call on stdin and exits with 2 to block it.