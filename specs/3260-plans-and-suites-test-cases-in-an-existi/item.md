---
issue: 3260
purpose: On a re-run of TfsTestPlansAndSuitesMigrationProcessor into a target plan that already exists, add the Test Cases from the source plan's root suite that are missing in the target root suite, as is already done for child suites.
paths:
  - src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessor.cs
  - src/MigrationTools.Clients.TfsObjectModel.Tests/**
  - tools/e2e/scenarios/testplans-3260/**
  - docs/content/docs/how-to/migrating-plans-and-suites/index.md
  - docs/content/docs/reference/processors/tfs-test-plans-and-suites-migration-processor/**
---

# 3260: Test Cases in an existing plan's root suite are not added on re-run

## Scope

In: `ProcessTestPlan` must call `AddChildTestCases(sourcePlan.RootSuite, targetPlan.RootSuite, targetPlan)` when the target
plan already exists, not only when it is created. Add an e2e scenario that proves it, and remove the "current behaviour"
caveats added in #3251 from the Migrating Plans and Suites page and the processor reference.

Out: removing Test Cases from the target that are no longer in the source (neither root nor child suites do this today);
re-applying field maps, ReflectedWorkItemId or AssignedTo to an existing plan/root suite; the commented-out
`TargetPlanContansTag` skip; ordering of Test Cases within a suite.

## Sources read

- Issue #3260 (body only, no comments).
- `CLAUDE.md`, `.github/copilot-instructions.md`, `.github/instructions/*.instructions.md` (point at `platform/specs/rules.md`,
  `platform/specs/systems.md`, docs-site rules).
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessor.cs`:
  `AddChildTestCases` (128-203), `FindTestPlan` (614), `ProcessTestPlan` (852-931), `ProcessTestSuite` (933-1060).
- `docs/content/docs/how-to/migrating-plans-and-suites/index.md` (the #3251 caveats naming #3260).
- `tools/e2e/Invoke-MigrationE2E.ps1`, `tools/e2e/Remove-E2EObjects.ps1`, `tools/e2e/scenarios/testplans/*`.

## Reproduction

On `main` at `56bab21f`, built `MigrationTools.ConsoleFull` (Debug), test orgs, scratch projects `amte2e-3260-src` /
`amte2e-3260-tgt` (process `amte2e-agile`). Configs were copies of `scenarios/testplans/step2-testcases.json`,
`step3-variables-configurations.json` and `step4-plans-suites.json` with the project names changed.

1. Seed: Test Case TC1; plan "amte2e 3260 plan" with static "Suite A"; TC1 added to the root suite and to Suite A.
2. Run 1: step2 (exit 0, 1 created), step3 (exit 0), step4 (exit 0). Target: root suite 1 Test Case (TC1), Suite A 1 (TC1). Correct.
3. Seed: Test Case TC2, added to the source root suite and to Suite A.
4. Run 2: step2 (exit 0, 1 checked, 1 created), step4 (exit 0). Log: `Found Plan amte2e 3260 plan`, then for Suite A
   `Test case already in suite ... 5595` and `Adding TestCase : 5599`. Nothing at all is logged for the root suite.
5. Target after run 2:
   - root suite `amte2e 3260 plan`: **1** Test Case (TC1) — TC2 missing;
   - `Suite A`: 2 Test Cases (TC1, TC2).

The run exits 0 with no warning, so the gap is silent. Both projects were deleted afterwards; Azure DevOps Services does
not allow hard delete, so they sit in each org's recycle bin until purged. No other project was touched.

## Cause

`ProcessTestPlan` (`TfsTestPlansAndSuitesMigrationProcessor.cs:864-896`) puts `AddChildTestCases` for the root suite (line
891) inside `if (targetPlan == null)`. The `else` branch only logs "Found Plan". Child suites go through
`ProcessTestSuite`, which calls `AddChildTestCases` on both the "created" and "Suite Exists" paths (line 1045), so they
are repaired on re-run and the root is not. `AddChildTestCases` is already idempotent: it skips Test Cases already in the
target suite (lines 173-180) and logs an error and sets `CompletedWithErrors` for ones not migrated (167-171).

## Proposed fix

Move `AddChildTestCases(sourcePlan.RootSuite, targetPlan.RootSuite, targetPlan);` out of the `if` so it runs for both a
new and an existing target plan (after the `if/else`, before the child-suite loop). For parity with "Suite Exists", also
call `ApplyDefaultConfigurations(sourcePlan.RootSuite, targetPlan.RootSuite)` on the existing-plan path (see decision 2).
No REST; Object Model only, as now.

Tests: a unit/L1 test of `ProcessTestPlan`'s branching is not practical (sealed TFS OM types), so the proof is the e2e
scenario `tools/e2e/scenarios/testplans-3260/` reproducing the steps above, with an `assert.ps1`. Docs: remove the #3260
caveats from the Migrating Plans and Suites page (re-run paragraph and the `OnlyElementsWithTag` bullet), regenerate with
`GenerateDocs.ps1` if the processor reference changes.

## Rubber duck

A user migrates a Test Plan, then later migrates more Test Cases and runs the Plans and Suites step again to top up the
target. Suites under the root get the new Test Cases; the root suite does not, and the run says nothing, so the user
believes the plan is complete. The change makes the existing-plan path do for the root suite what it already does for
child suites. It touches one method in the TFS Object Model processor, one new e2e scenario and the docs caveat.

Gaps it showed:
- The gap is silent: exit 0, no warning. After the fix an unmigrated root Test Case gives an error and exit 1 on re-run,
  which is a visible change for existing users (decision 3).
- Users who followed the documented workaround (adding root Test Cases by hand) are unaffected: the existing-entry check
  skips them, so no duplicates.
- `AddChildTestCases` resets the progress counters and saves the plan; calling it on every run adds one plan save per
  re-run. Harmless, noted.
- The `OnlyElementsWithTag` rule for the root suite becomes the same on first run and re-run, which the docs must say.

## Architecture check

- `platform/specs/rules.md` (comments): the fix moves one existing call; no comments added. No finding.
- "Migration code must not depend on REST": the fix stays in the TFS Object Model; REST is used only by the e2e seed
  and assert scripts, as in the existing `testplans` scenario. No finding.
- `platform/specs/systems.md`: touches `tfs-object-model`, `e2e-harness` and `docs-site`. There is no
  `platform/specs/tfs-object-model/` spec to update; the docs-site page is updated. No finding.

Open findings: 0

## Outcome checks

1. Re-run into an existing plan after a new Test Case is added to the source root suite: the target root suite contains it
   (scenario above: root 2, Suite A 2), exit 0.
2. Re-run with nothing new: no duplicates in root or child suites; root logs "Test case already in suite" for each.
3. Failure path: a Test Case in the root suite not yet migrated → error naming it, exit 1 (same as child suites); after
   migrating it and re-running, it is added and exit is 0.
4. First run into an empty target is unchanged (root and child suites populated once).
5. `OnlyElementsWithTag` set and the root suite not tagged: root Test Cases are still not added on an existing plan
   (unchanged rule from `AddChildTestCases`).
6. Each check run on `main` (shows the gap for 1 and 3) and on the change, with output and exit codes in the PR.

## Decisions for Martin

1. Behaviour change: on re-run the root suite is now topped up. Anyone relying on hand-curated target root suites (the
   workaround the docs suggest) gets source root Test Cases added back. Accept, or put it behind an option? Proposed:
   accept, no option, matching child suites.
2. Also re-apply default configurations to an existing root suite (as "Suite Exists" does for child suites)? Proposed: yes.
3. Should step 3 of the failure path (unmigrated root Test Case) exit 1 on an existing plan? It will once the call runs;
   today it silently exits 0. Proposed: yes, consistent with child suites.

Decided: 1 accept, no option; 2 yes; 3 yes (approved as proposed).

Docs unchanged: tfs-object-model: there is no platform/specs/tfs-object-model/ spec; the behaviour is documented on the Migrating Plans and Suites page, which is updated.
Docs unchanged: e2e-harness: a new scenario only; the harness and its rules are unchanged.
Docs unchanged: docs-site: one how-to page's content changes; the docs-site rules and list pages are unchanged.

## Spec

The bug assessment: `specs/3260-plans-and-suites-test-cases-in-an-existi/assessment.md`.

Spec presented: specs/3260-plans-and-suites-test-cases-in-an-existi/assessment.md sha256 07571fbddd2e86db02c313a7bbcfab1814b84ea7ec129852e0ed711649ea40f1

Spec agreed: Approve as proposed
Spec version: specs/3260-plans-and-suites-test-cases-in-an-existi/assessment.md sha256 07571fbddd2e86db02c313a7bbcfab1814b84ea7ec129852e0ed711649ea40f1
