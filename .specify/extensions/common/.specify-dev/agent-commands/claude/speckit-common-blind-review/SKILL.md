---
name: speckit-common-blind-review
description: A second reviewer checks the change, seeing only the diff, the item and the rules
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: common:commands/blind-review.md
---

# Blind review

You are reviewing a change you did not make. You see only:

- the diff against `main` (`git diff main...HEAD`);
- the item it claims to deliver (the issue, and its acceptance checks);
- the repository's own rules (constitution, agent rules).

Do not read run logs, completion reports, or any reasoning from whoever made the change.

$ARGUMENTS

## Report

Write `.specify/reviews/blind-review.md` with one finding per line, each with:

- **severity**: defect, gap (an acceptance check not met) or rule (a repository rule broken);
- **where**: file and line;
- **what**: one sentence.

End with `Open findings: N`. If N is more than 0, say so plainly: the change is not done.