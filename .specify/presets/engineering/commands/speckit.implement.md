---
description: "Stock implement, plus the Definition of Done"
strategy: wrap
---

{CORE_TEMPLATE}

## Definition of Done

You are not done until the done checks pass: a fresh full build, the full test suite, and the repository's real-system proof (`.specify/memory/done-checks.json`). Update the docs the change affects.

Then run `/speckit.common.completion-report`. Never report a check as passed that you did not run.