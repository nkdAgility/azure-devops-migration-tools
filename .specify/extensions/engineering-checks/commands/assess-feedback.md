---
description: "Assess a Feedback item and recommend what it becomes"
---

# Assess feedback

For the Feedback item in `.specify/items/current.json`: find whether it is real, what is behind it, and what already exists (the spec root, `specRoot` in `.specify/memory/workflow.json`, default `docs/specs/`, open items, the code).

$ARGUMENTS

Write `## Assessment` to the item's record (`specs/<number>-<name>/item.md`) with what you found, then recommend exactly one of:

- **build as Feedback**: a small change to an existing feature; name the feature;
- **group into a Package** with other items; name them;
- **a Bug** or **a Product Backlog Item**: say why it is bigger than Feedback;
- **nothing**: say why.

Martin decides. Write his decision when he gives it as `Decided: <his words>`. For "nothing", also write `Removed because: <reason>`.