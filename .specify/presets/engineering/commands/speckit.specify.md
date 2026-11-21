---
description: "Stock specify, plus the Definition of Ready"
strategy: wrap
---

{CORE_TEMPLATE}

## Definition of Ready

The item in `.specify/items/current.json` is the source. Write the spec into the item's own folder, `specs/<issue>-<name>/` (it already holds the item's record, `item.md`): run the feature script with `-Number <issue> -AllowExistingBranch` and the folder's name after the number as `-ShortName`, so `spec.md`, `plan.md` and `tasks.md` go there, not in a new numbered folder. The spec is ready only when it states:

1. the outcome, in one sentence a user would recognise;
2. testable acceptance checks, each one something a command or a person can confirm;
3. the size, and what is out of scope;
4. the paths and areas it will touch.

For a feature with riders, each rider gets its own section with its own checks. Do not add riders that are not on the item.