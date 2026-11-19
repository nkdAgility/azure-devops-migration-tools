---
name: speckit-common-completion-report
description: What was done, how it was tested, what was not done
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: common:commands/completion-report.md
---

# Completion report

Write `.specify/reviews/completion-report.md` for the current item:

1. **Done**: each acceptance check, and the evidence it passes (command run, output, file).
2. **Tested**: the tests added or changed, and the real-system run.
3. **Not done**: anything left out, skipped or not verified, and why.

$ARGUMENTS

Never write "done" for something you did not run. If a check was not run, it goes under Not done.