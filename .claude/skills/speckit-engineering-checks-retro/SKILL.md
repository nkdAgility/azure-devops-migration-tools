---
name: speckit-engineering-checks-retro
description: 'The retrospective on the process: a blind analysis of the worker''s turns against the Definition of Workflow, written as feedback for the process agent'
compatibility: Requires spec-kit project structure with .specify/ directory
metadata:
  author: nkdAgility
  source: engineering-checks:commands/retro.md
---

# Retrospective

Run this once the item's pull request has merged, before the item can be Closed. It checks how the process worked for this item, not how the code turned out.

$ARGUMENTS

1. Write your transcript: `pwsh -NoProfile -File .specify/extensions/engineering-checks/scripts/retro-transcript.ps1`. It writes `.specify/reviews/retro-transcript.md`.
2. Start a blind reviewer: a subagent that sees only the evidence, never your account of how it went. Give it exactly these files and nothing else:
   - `.specify/reviews/retro-transcript.md`;
   - the Definition of Workflow: `spec/definition-of-workflow-engineering.md` in the shared source (see step 3);
   - the repo's rules: `AGENTS.md` and `CLAUDE.md`;
   - the worker rules: `.specify/extensions/engineering-checks/commands/wkr.md`;
   - the repo's workflow settings: `.specify/memory/workflow.json`.
   Ask it, in these words: "Read the transcript turn by turn against the rules. Report, with the exact turn quoted for each: (1) deviations: where the work did not follow the Definition of Workflow, the worker rules or the repo's rules, including stages skipped, states moved without their steps, gates answered without Martin, scope left, and anything written that the rules forbid; (2) constraints: where the process itself got in the way, refused something correct, gave a misleading message, or could not be followed; (3) corrections: every time Martin had to correct, stop or redirect the work, and what the process should have done instead; (4) changes needed: each change to the rules or the workflow that would have prevented a deviation, removed a constraint or made a correction unnecessary, with the reason. Report only what the transcript shows. Do not judge the code."
3. Save the reviewer's report, unedited, as the retrospective note: `<nkda-speckit>/feedback/<yyyy-mm-dd>-<repo>-<item>.md`, where `<nkda-speckit>` is the shared source (`sharedSource` in `.specify/memory/workflow.json`, relative to the repo's main checkout, or `NKDA_SPECKIT_SOURCE`). Start it with one line: `Retrospective: <repo> #<item> <title>`.
4. Tell the manager in one line that the retrospective is written. The process agent reads the notes and brings Martin any change to the rules, worded exactly, for his approval.

The item cannot be Closed until the note exists.
