---
description: "Refining: gather what is needed to understand the item before any spec is written"
---

# Refine

For the item in `.specify/items/current.json`:

1. Read the source-of-truth docs for the area it touches (the spec root, `specRoot` in `.specify/memory/workflow.json`, default `docs/specs/`, by the repo's list of systems and their paths).
2. Work out the paths it will touch and what is out of scope.
3. For a Bug, reproduce it on `main` and record the exact steps and the result.

$ARGUMENTS

Write these to the item's WKR file (`.specify/wkr/<number>.md`):

```
## Sources read
- <doc path>: <what it says that matters here>

## Scope
- In: <paths and behaviour>
- Out: <what is not part of this item>

## Reproduction        (Bugs only)
1. <step> → <result on main>
```

Anything you cannot settle goes under `## Questions` as `- Q-n <question> (for <who>) open`. Do not write a spec or change any code.