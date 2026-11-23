# Bug Assessment: Root-suite Test Cases not added on re-run into an existing plan

- **Slug**: root-suite-rerun
- **Created**: 2026-11-20
- **Source**: https://github.com/nkdAgility/azure-devops-migration-tools/issues/3260 (host: github.com, allowlisted; read with `gh`)
- **Verdict**: valid
- **Severity**: medium

## Report (summarized)

#3260: `ProcessTestPlan` adds the Test Cases directly in a plan's root suite only when it creates the target plan. On a
re-run into an existing plan, child suites are processed but root-suite Test Cases missing on the first run are never
added. Expected: add missing root-suite Test Cases, as for child suites.

## Symptom

After a plan is migrated, Test Cases later added to the source root suite are not added to the target root suite on
re-run; child suites are topped up. The run exits 0 with no warning.

## Reproduction

Shown on `main` (`56bab21f`) against the test orgs, projects `amte2e-3260-src`/`-tgt`:

1. Source: TC1 in the root suite and in static Suite A. Migrate Test Cases, configurations, then Plans and Suites (all exit 0). Target: root 1 (TC1), Suite A 1 (TC1).
2. Source: add TC2 to the root suite and Suite A. Re-run Test Cases and Plans and Suites (exit 0).
3. Target: root suite 1 (TC1 only); Suite A 2 (TC1, TC2). Log shows `Found Plan`, then Suite A processing; nothing for the root suite.

## Suspected Code Paths

- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessor.cs:891` — the root-suite `AddChildTestCases` call sits inside `if (targetPlan == null)`.
- Same file, `ProcessTestSuite` line 1045 — child suites call `AddChildTestCases` on both the created and "Suite Exists" paths.
- Same file, `AddChildTestCases` 128-203 — already skips Test Cases present in the target and errors for unmigrated ones.

## Root Cause Hypothesis

The root-suite call is only on the new-plan branch; the existing-plan branch only logs. Confidence: high (reproduced, and the log shows no root processing).

## Proposed Remediation

**Preferred**: move `AddChildTestCases(sourcePlan.RootSuite, targetPlan.RootSuite, targetPlan)` after the `if/else` so it
runs for new and existing plans, and call `ApplyDefaultConfigurations(sourcePlan.RootSuite, targetPlan.RootSuite)` on the
existing-plan path to match "Suite Exists". TFS Object Model only; no REST.

**Alternatives**:
- Put the top-up behind an option: protects hand-curated target root suites, but adds a setting for behaviour child suites already have.

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessor.cs`
- `tools/e2e/scenarios/testplans-3260/` (new: seed, configs, assert)
- `docs/content/docs/how-to/migrating-plans-and-suites/index.md` (remove the #3260 caveats)

**Tests to add or update**:
- e2e scenario reproducing the steps above; asserts root 2 and Suite A 2 after re-run, no duplicates on a third run.
- Failure path: an unmigrated root Test Case gives an error and exit 1 on re-run.
- A unit test of the branch is not practical (sealed TFS OM types); the e2e scenario is the proof.

## Risks & Considerations

- Behaviour change: re-runs now add source root Test Cases to target root suites, and an unmigrated root Test Case is now named in an error ("was not added to suite"). The exit code is unchanged: main already exits 1 in that case, from the configurations step.
- One extra plan save per re-run.

## Open Questions

- Decisions 1-3 in `.specify/wkr/3260.md` (accept without an option; re-apply default configurations; exit 1 on unmigrated root Test Case).

## Docs

- Updated: `docs/content/docs/how-to/migrating-plans-and-suites/index.md` loses the #3260 caveats and says a re-run adds missing Test Cases, the root suite included.

Docs unchanged: tfs-object-model: there is no platform/specs/tfs-object-model/ spec; the behaviour is documented on the Migrating Plans and Suites page.
Docs unchanged: e2e-harness: a new scenario only; the harness and its rules are unchanged.
Docs unchanged: docs-site: one how-to page's content changes; the docs-site rules and list pages under platform/specs/docs-site/ are unchanged.
