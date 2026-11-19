---
name: speckit-common-confidence
description: Score the manually run validation of one gate (Output Done or Outcome Done) as a confidence figure with reasons
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: common:commands/confidence.md
---

# Confidence

You are scoring one gate: `gate=output` (Output Done) or `gate=outcome` (Outcome Done), given below.

$ARGUMENTS

Its automated checks have already run and passed; do not score them again. Score only the validation that was run and judged by hand for this gate:

- **Output Done:** the blind review, the architecture check, and whether the change meets the original ask.
- **Outcome Done:** each run on the real system that was inspected rather than scripted, its logs, and whether each outcome check is really met.

Then decide whether the figure could be raised at a reasonable resource cost (one more targeted run, one more test). If it could, do that first and say what you did.

Write `.specify/reviews/<gate>-confidence.md`:

```
Confidence: NN%
Raised by: <what you did to raise it, or "nothing worth the cost">
Reasons:
- <one line per validation: what it showed, and what doubt remains>
```

Never round up. If you cannot say why you are confident, the figure is low.