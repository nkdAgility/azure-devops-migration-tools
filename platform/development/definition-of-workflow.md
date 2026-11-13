# Definition of Workflow

This is how work is done on the Azure DevOps Migration Tools. The machine-readable part sits beside
it in `definition-of-workflow.json`, which the nkda-engineering-delivery plugin reads.

## Unit of value

The unit of value is a GitHub issue in `nkdAgility/azure-devops-migration-tools`: a bug a customer
reported, a problem found in a review, or a change Martin asks for. Where no issue exists, one is
created before work starts. An issue is started when Martin says it is to be worked on, and finished
when the pull request that fixes it is merged and the issue is closed.

## States

**Reported.** The issue is open and carries the `triage` label. The manager reads the code behind
it and tells Martin whether it can be solved here, why, and what it would take. Work moves on when
Martin says it is to be worked on.

**Fixing.** The `triage` label is removed and a single status comment says Martin is working on it.
One worker chat owns the issue, in its own worktree, branch and pull request. It reproduces the
problem, finds the cause, makes the fix and tests it: unit tests, and end-to-end runs on the test
organisations against `main` and the branch, covering failure paths as well as the happy path, in
its own scratch projects. The worker replaces the status comment's current status, through the stage-gate tool, as findings and the fix progress. Work moves
on when the pull request is open, says which issue it fixes, and reports against the Definition of
Done: up to date with `main`, builds, tests pass, the end-to-end migration completes on the test
orgs, meets the architectural norms and the original ask, the docs are updated with
`GenerateDocs.ps1`, anything created only for the work is cleaned up, and a confidence figure is
given.

**In review.** The pull request is bound to its worker chat, which answers CI failures and review
comments until it merges. Auto-merge is on only when confidence is 90% or higher. The status comment
says what the pull request is waiting on. Work moves on when the pull request merges.

**Closed.** The pull request is merged and the issue is closed by it.

## Who does what

Martin decides what is worked on, approves changes to this definition and to the rules, and gives
the say-so to push. The manager session orchestrates: it reads the issues, recommends what to take,
starts one worker chat per issue and keeps the repository tidy; it does not do an issue's work. Each
worker chat does the work of one issue and nothing else.

## Work in progress

Work in progress is the issues in fixing and in review. The limit is in the JSON file.

## What is taken next

Martin chooses. The manager recommends the order: a regression that stops migrations comes first.

## Service level expectation

Not yet set.

## Changing this definition

The manager or CONTROL session proposes a change to Martin, who approves its exact wording. A change
to the plugin, or to how this repository uses it, is first worked through with every active
CONTROL session. An approved change applies from each issue's next move.
