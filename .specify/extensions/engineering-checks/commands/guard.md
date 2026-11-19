---
description: "Blocks pushes to main and hand edits to run state"
scripts:
  ps: scripts/guard.ps1
---

# Guard

Runs before every tool use. It reads the tool call on stdin and exits with 2 to block it.