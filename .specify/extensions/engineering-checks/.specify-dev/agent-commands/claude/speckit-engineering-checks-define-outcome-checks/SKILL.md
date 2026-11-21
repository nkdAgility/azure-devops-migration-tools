---
name: speckit-engineering-checks-define-outcome-checks
description: Define the item's outcome checks so a machine can run them against the real system
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/define-outcome-checks.md
---

# Define outcome checks

From the item (`.specify/items/current.json`), its spec, and the repository's own proof commands (`.specify/memory/done-checks.json`), write the checks that prove the outcome on the real system. Each check states what is checked, against which system, the command that proves it, and what a pass looks like.

$ARGUMENTS

Write them to the item's record (`specs/<number>-<name>/item.md`) as a section, one line per check, in exactly this form:

```
## Outcome checks

- <what is checked, on which system; pass = exit 0 and …>: `<command>`
```

Every acceptance check, and every rider's or child's check, has at least one line. A check that cannot be run by a command is written under `## Questions` as an open question for Martin instead.