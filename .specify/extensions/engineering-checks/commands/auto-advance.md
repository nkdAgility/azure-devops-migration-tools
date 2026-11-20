---
description: "Moves an item on as soon as its evidence meets the exit policy for its state"
scripts:
  ps: scripts/auto-advance.ps1
---

# Auto-advance

Runs after every tool use. When the item's evidence (`.specify/wkr/` or `.specify/reviews/`) has just changed and the item is in a working state, it checks the exit policy for that state and moves the item on if the policy passes. A move that needs Martin, a wait state, or a policy with a slow check (build, tests, outcome checks) is never moved here. It says nothing unless the item moved.
