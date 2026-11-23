---
purpose: Remove-E2EObjects.ps1 deletes only the fields that belong to the prefix it is given, refuses a prefix it cannot map, and can list what it would delete without deleting it.
paths:
  - tools/e2e/Remove-E2EObjects.ps1
  - tools/e2e/E2EProcess.psm1
  - tools/e2e/E2EAzDo.psm1
  - tools/e2e/reports/2026-10-regression.md
  - .github/copilot-instructions.md
  - .github/workflows/*.yml
  - tools/e2e/*.Tests.ps1
  - tools/e2e/scenarios/*/seed.ps1
  - tools/e2e/scenarios/*/assert.ps1
  - tools/e2e/scenarios/*/*.json
  - tools/e2e/README.md
---

# 3278 — Remove-E2EObjects.ps1 deletes other scenarios' Custom.amte2e* fields

## Scope

In: the prefix-to-field-filter mapping in `tools/e2e/Remove-E2EObjects.ps1`, refusing an unmappable prefix,
`-WhatIf` (via `SupportsShouldProcess`), tests for the mapping, and (if Martin chooses option B below) bringing
seeders' field names into one convention tied to the scenario prefix.

Out: project and process deletion logic (already scoped by `-Prefix`), the migration tool itself (`src/`),
recovering fields already deleted on the test orgs.

## Sources read

- Issue #3278.
- `tools/e2e/Remove-E2EObjects.ps1` (whole file).
- `tools/e2e/E2EProcess.psm1`: `New-E2EField` (l.44-50, field reference is `Custom.<name>`, names must start
  `amte2e`), `Remove-E2EField` (l.54, refuses anything not `Custom.amte2e*`), `Get-E2EFields` (l.58).
- `tools/e2e/E2EAzDo.psm1:11` (harness prefix `amte2e-`).
- Field names used by scenarios: `scenarios/procdef-visibility/assert.ps1` (`Custom.amte2e3242Hidden|Secret|Shown`),
  `scenarios/picklists/assert.ps1` (`Custom.amte2eAmt3241*`), `scenarios/fields/seed.ps1` and `config*.json`
  (`Custom.amte2eIssueOnly`, `Custom.amte2eLegacy`, no issue number).
- `tools/e2e/reports/2026-10-regression.md` (documents the all-fields default as intended for a full clean).
- CLAUDE.md, `.github/copilot-instructions.md`, `.github/instructions/*.instructions.md`.
- `platform/specs/systems.md`: `tools/e2e/` is the **e2e-harness** system. There is no
  `platform/specs/e2e-harness/` spec yet, so the scripts and their comments are the only source of truth.
- `platform/specs/rules.md`: its commenting rules apply to `src/**` only, so not to this item.
  `platform/specs/architecture.md`: a copy of the copilot instructions, with no rules for `tools/`.
- `.specify/memory/constitution.md`: still the unfilled template, so it has no principles to check against.

## Reproduction

Not run against the test orgs (it would delete shared fields). The mapping at
`tools/e2e/Remove-E2EObjects.ps1:15` was evaluated locally with the same expression:

| `-Prefix`       | field filter          | effect                                    |
|-----------------|-----------------------|-------------------------------------------|
| `amte2e-`       | `Custom.amte2e`       | all e2e fields (intended full clean)      |
| `amt3272-`      | `Custom.amte2e3272`   | that issue's fields only                  |
| `amte2e-3272-`  | `Custom.amte2e`       | **all e2e fields** — the bug              |
| `amte2e-abc-`   | `Custom.amte2e`       | **all e2e fields**                        |
| `x`             | `Custom.amte2e`       | **all e2e fields**, no projects/processes |

The regex only recognises `amt<digits>-`; every other value silently falls back to the full filter. The
comment at l.9 documents `amt<issue>-`, but the harness and scenarios use `amte2e-…` names, so the fallback is
the path people actually hit.

## Cause

1. The fallback in the `else` branch of l.15 turns "can't interpret this prefix" into "match everything".
2. There is no single field-naming convention linking a scenario prefix to its fields: some scenarios embed the
   issue number (`amte2e3242…`), some embed `Amt<issue>` (`amte2eAmt3241…`), some embed nothing
   (`amte2eIssueOnly`). So no mapping from a project prefix can be complete today.
3. No dry-run, so the scope can't be checked before deleting.

## Proposed fix

1. Extract the mapping into a pure function (e.g. `Get-E2EFieldFilter -Prefix`) in `E2EProcess.psm1` so it is
   testable without the API.
   - `amte2e-` (exactly) → `Custom.amte2e` (full clean, unchanged).
   - `amt<n>-` and `amte2e-<n>-` → `Custom.amte2e<n>` (and, if option A, also `Custom.amte2eAmt<n>`).
   - Anything else → throw: "Cannot map prefix '<p>' to a field filter; fields were not touched." The script
     stops before deleting anything (projects included), so a bad prefix never half-runs.
2. Add `[CmdletBinding(SupportsShouldProcess)]`; wrap every delete (project, recycled project, process, field) in
   `$PSCmdlet.ShouldProcess(...)`, so `-WhatIf` lists the full plan and deletes nothing.
3. Print the resolved field filter at the start of every run.
4. Pester tests (`tools/e2e/Remove-E2EObjects.Tests.ps1`, PowerShell 7): mapping table above, refusal of
   unmappable prefixes, and `-WhatIf` with `Invoke-E2ERest`/`Remove-E2E*` mocked asserting no delete is called
   and the scoped fields are listed while others (e.g. `Custom.amte2e3242Hidden` for prefix `amte2e-3272-`) are not.
5. Update the header comment/README to the supported prefix forms.

## Outcome checks

- `-Prefix amte2e-3272- -WhatIf` (mocked) lists only `Custom.amte2e3272*` fields and calls no delete.
- `-Prefix amte2e-abc-` and `-Prefix x` exit non-zero with the refusal message and touch nothing.
- `-Prefix amte2e-` still targets all `Custom.amte2e*` (full clean behaviour kept).
- Pester tests pass locally and in CI.
- Live proof, safe: run `-WhatIf` only against the test orgs with a scratch prefix on `main` (shows all fields)
  and on the change (shows only scratch fields); then a real run on a scratch prefix whose fields were created
  for the test, and check other scenarios' fields are still present.

## Rubber duck

Walking the proposed fix step by step turned up these points:

1. **The issue's premise is partly wrong.** The issue says the harness only accepts `amte2e-` names, but
   `E2EAzDo.psm1:32-35` (`Assert-E2EName`) also accepts `amt<issue>-`, and l.32 documents it as the convention
   for workers ("so they never collide with amte2e-* or each other"). So `amt<n>-` is the intended form, and
   `amte2e-3272-` is a scenario-style name used for an issue's scratch work. The fix must accept both forms; the
   real defect is still the fall back to everything.
2. **Over-matching between issue numbers.** `StartsWith("Custom.amte2e3272")` also matches
   `Custom.amte2e32720…` (issue 32720) and `Custom.amte2e327…` matches 3272. Field reference names can't contain
   `-`, so there is no natural separator. The filter needs one: a convention such as `amte2e<n>x…` / `amte2e<n>_…`,
   or a regex `^Custom\.amte2e<n>(?!\d)`. The regex works with today's names (`amte2e3242Hidden`).
3. **What actually got hurt.** `Remove-E2EField` fails for fields still attached to a process, so the bug deletes
   fields that are *unused at that moment*: those a parallel run has created but not attached yet, and the shared
   fields that seeders expect to find (`amte2eIssueOnly`, `amte2eLegacy`). Those are exactly the objects a
   parallel run depends on, so the impact in the issue is real.
4. **`-WhatIf` has to cover more than the deletes.** The process retry loop (l.33-36) sleeps 15 s × 4 per process,
   and the "remaining" report (l.41) would list everything as remaining. Under `-WhatIf`, skip the retries and
   report "would delete" instead of "remaining".
5. **Fail before touching anything.** The prefix must be validated before the first project deletion, not just
   before fields. Otherwise an unmappable prefix still removes projects and processes and then stops halfway.
6. **Changing the default is a behaviour change.** `tools/e2e/reports/2026-10-regression.md:108` documents a
   bare run as a full clean. Requiring `-All` (decision 2) breaks that line and anyone's habit, so the docs must
   change in the same PR.

## Architecture check

Checked against `.github/copilot-instructions.md`, `.github/instructions/*.instructions.md` and the existing
`tools/e2e` modules. `platform/specs/architecture.md` is a copy of the copilot instructions and has no rules for
`tools/`.

1. **Where the mapping lives.** The naming rules already sit in `E2EAzDo.psm1` (`$script:Prefix`,
   `Assert-E2EName`). The new `Get-E2EFieldFilter` belongs there too, next to them, not in `E2EProcess.psm1`. One
   place then defines which names an issue owns, and `Assert-E2EName` and the field filter can't drift apart.
   `paths` updated to include `E2EAzDo.psm1`.
2. **No PowerShell test convention exists.** The repo has no `*.Tests.ps1` and no workflow runs Pester. Adding
   `tools/e2e/*.Tests.ps1` sets a new convention, and the tests protect nothing unless CI runs them. See decision 4.
3. **PowerShell version.** The copilot instructions say the harness supports Windows PowerShell 5.1 or 7+. The
   fix should use syntax that works in both (no ternary or `??`), even though this work runs under 7.
4. **Docs.** `.github/copilot-instructions.md` documents `Invoke-MigrationE2E.ps1` but not
   `Remove-E2EObjects.ps1`. The fix should add its prefix forms, `-WhatIf` and the full-clean switch there and in
   the regression-report command line. This is a tools change, so `GenerateDocs.ps1` has nothing to regenerate.
5. **Safety in depth.** The guard in `Remove-E2EField` (`Custom.amte2e*` only) stays as it is: it limits the
   damage to e2e fields, while the new filter scopes deletion to one issue.

None of the points above breaks a written architecture rule. The repo's rules cover only `src/**`, and the
constitution is unfilled. They are design points, and the proposed fix below takes them in.

Open findings: 0

Proposed fix, amended by both checks:

- `Get-E2EFieldFilter` in `E2EAzDo.psm1`, matching with a regex that has a digit boundary.
- Accept both `amt<n>-` and `amte2e-<n>-`.
- Check the prefix before any deletion.
- `-WhatIf` also covers the retries and the final report.

## Decisions for Martin

All answered; see Questions Q-1 to Q-4.

## Questions

- Q-1 Field naming: map the current names (A) or rename seeders' fields to `amte2e<n>…` (B)? Answer: B, rename all fields. (for Martin) answered
- Q-2 Should a full clean need an explicit `-All` switch? Answer: yes, require -All. (for Martin) answered
- Q-3 Should an unmappable prefix refuse the whole run (proposed), or skip only the fields? Answer: skip fields only. (for Martin) answered
- Q-4 Pester tests with a CI step, local-only tests, or `-WhatIf` proof only? Answer: Pester tests plus a CI step. (for Martin) answered

## Spec

Updated with Martin's answers to Q-1 to Q-4.

- R9 Pester tests `tools/e2e/*.Tests.ps1` cover the mapping, the refusal and `-WhatIf` with mocked API calls; a CI step in `.github/workflows/` runs them on PRs that change `tools/e2e/` (Q-4).

- R1 `-Prefix <p>` deletes only what the prefix owns. Fields match `^Custom\.amte2e<n>(?!\d)`, where `<n>` comes from `amt<n>-` or `amte2e-<n>-`. Every scenario's fields are renamed to `amte2e<n>…` (`seed.ps1`, `assert.ps1`, scenario `*.json`), so each issue's cleanup is complete (Q-1 B).
- R2 A full clean needs `-All`. A bare run with no `-Prefix` and no `-All` stops with an error (Q-2).
- R3 A prefix that can't be mapped, including an empty one, still deletes the matching projects and processes but no fields, warns "Cannot map prefix '<p>' to a field filter; no fields were deleted.", and exits non-zero (Q-3).
- R3a An empty prefix, or one that would match every e2e project (for example `amte2e`), is refused before any delete. Otherwise skipping only the fields would still let it remove every project and process without `-All`.
- R1a Scenarios with no issue of their own (`fields`: `amte2eIssueOnly`, `amte2eLegacy`) are renamed to the number of the issue that created the scenario, found from git history.
- R4 `-WhatIf` lists every project, recycled project, process and field the run would delete, deletes nothing, skips the retry waits, and reports "would delete".
- R5 Every run prints its resolved field filter first.
- R6 The mapping is one function in `E2EAzDo.psm1`, next to `Assert-E2EName`.
- R7 Runs on Windows PowerShell 5.1 and PowerShell 7.
- R8 `.github/copilot-instructions.md` and `tools/e2e/reports/2026-10-regression.md:108` document the prefix forms, `-WhatIf` and the full-clean switch.

## Personas

- **Issue worker** cleans up only their own `amt<n>-` / `amte2e-<n>-` objects, and needs parallel runs left untouched.
- **Harness maintainer** needs the full clean after a regression pass, with a dry-run first.
- **Parallel scenario run** (indirect) needs its fields to survive anyone else's cleanup.

## Red team

- Short numbers: the digit boundary stops `amte2e3` matching `amte2e32…`, but `amte2e3Hidden` from another naming scheme would still match. Q-1 option B removes this risk.
- Regex injection: build the pattern from the parsed digits only, never from raw input.
- Fields attached to a process can't be deleted, so they are reported as leftovers, never retried forever.

## MCP story

Not applicable: a local maintenance script with no MCP surface.

## API story

Not applicable to the product API. The script uses the same Azure DevOps REST endpoints as today and adds none.

## Refactor assessment

Small and required: move the field-prefix logic from the script into a function in `E2EAzDo.psm1` (R6), so the naming rules live in one place and can be tested without the API. The deletion loops stay as they are, apart from the `ShouldProcess` wrappers.


## Spec

Working spec: `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/assessment.md` (the bug assessment Martin approves).


## Reproduction

The script was not run against the test orgs, because doing so would delete the shared fields. The expression at
`Remove-E2EObjects.ps1:15` was evaluated locally:

1. `amte2e-3272-` → `Custom.amte2e` (every e2e field).
2. `amte2e-abc-` and `x` → `Custom.amte2e` (every e2e field).
3. `amt3272-` → `Custom.amte2e3272` (scoped correctly).


## Cause

The prefix-to-field mapping treats "unrecognised prefix" as "match all", and there is no single field-naming
convention tied to an issue number. Confidence: high.


## Proposed fix

**Preferred**: as agreed with Martin (Q-1 to Q-4 in `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/item.md`):

- Add `Get-E2EFieldFilter` in `E2EAzDo.psm1`, using a digit-boundary regex over `amt<n>-` and `amte2e-<n>-`.
- Rename every scenario's fields to `amte2e<n>…`.
- Make a full clean need `-All`.
- An unmappable prefix skips the fields and exits non-zero. An empty or too-broad prefix is refused outright.
- Add `-WhatIf` through `SupportsShouldProcess`.
- Add Pester tests and a CI step that runs them.

**Requirements** (agreed with Martin):

Updated with Martin's answers to Q-1 to Q-4.

- R9 Pester tests `tools/e2e/*.Tests.ps1` cover the mapping, the refusal and `-WhatIf` with mocked API calls; a CI step in `.github/workflows/` runs them on PRs that change `tools/e2e/` (Q-4).

- R1 `-Prefix <p>` deletes only what the prefix owns. Fields match `^Custom\.amte2e<n>(?!\d)`, where `<n>` comes from `amt<n>-` or `amte2e-<n>-`. Every scenario's fields are renamed to `amte2e<n>…` (`seed.ps1`, `assert.ps1`, scenario `*.json`), so each issue's cleanup is complete (Q-1 B).
- R2 A full clean needs `-All`. A bare run with no `-Prefix` and no `-All` stops with an error (Q-2).
- R3 A prefix that can't be mapped, including an empty one, still deletes the matching projects and processes but no fields, warns "Cannot map prefix '<p>' to a field filter; no fields were deleted.", and exits non-zero (Q-3).
- R3a An empty prefix, or one that would match every e2e project (for example `amte2e`), is refused before any delete. Otherwise skipping only the fields would still let it remove every project and process without `-All`.
- R1a Scenarios with no issue of their own (`fields`: `amte2eIssueOnly`, `amte2eLegacy`) are renamed to the number of the issue that created the scenario, found from git history.
- R4 `-WhatIf` lists every project, recycled project, process and field the run would delete, deletes nothing, skips the retry waits, and reports "would delete".
- R5 Every run prints its resolved field filter first.
- R6 The mapping is one function in `E2EAzDo.psm1`, next to `Assert-E2EName`.
- R7 Runs on Windows PowerShell 5.1 and PowerShell 7.
- R8 `.github/copilot-instructions.md` and `tools/e2e/reports/2026-10-regression.md:108` document the prefix forms, `-WhatIf` and the full-clean switch.

**Files likely to change**: `tools/e2e/Remove-E2EObjects.ps1`, `tools/e2e/E2EAzDo.psm1`, scenario `seed.ps1` /
`assert.ps1` / `*.json`, `tools/e2e/Remove-E2EObjects.Tests.ps1`, a workflow in `.github/workflows/`,
`.github/copilot-instructions.md`, `tools/e2e/reports/2026-10-regression.md`.

**Tests to add or update**:

- The mapping table above.
- The digit boundary (3272 does not match 32720).
- An unmappable prefix deletes no fields.
- Empty and `amte2e` prefixes are refused.
- `-WhatIf` calls no delete, with mocked REST calls.


## Spec

Working spec: `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3278-remove-e2eobjects-ps1-deletes-other-scen/assessment.md sha256 e501b300da886bac472202bd3fb8e06d92187f75e0ab0ce1b1843348195aa1ef

## Reproduction

The script was not run against the test orgs, because doing so would delete the shared fields. The expression at
`Remove-E2EObjects.ps1:15` was evaluated locally:

1. `amte2e-3272-` → `Custom.amte2e` (every e2e field).
2. `amte2e-abc-` and `x` → `Custom.amte2e` (every e2e field).
3. `amt3272-` → `Custom.amte2e3272` (scoped correctly).


## Cause

The prefix-to-field mapping treats "unrecognised prefix" as "match all", and there is no single field-naming
convention tied to an issue number. Confidence: high.


## Proposed fix

**Preferred**: as agreed with Martin (Q-1 to Q-4 in `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/item.md`):

- Add `Get-E2EFieldFilter` in `E2EAzDo.psm1`, using a digit-boundary regex over `amt<n>-` and `amte2e-<n>-`.
- Rename every scenario's fields to `amte2e<n>…`.
- Make a full clean need `-All`.
- An unmappable prefix skips the fields and exits non-zero. An empty or too-broad prefix is refused outright.
- Add `-WhatIf` through `SupportsShouldProcess`.
- Add Pester tests and a CI step that runs them.

**Requirements** (agreed with Martin):

Updated with Martin's answers to Q-1 to Q-4.

- R9 Pester tests `tools/e2e/*.Tests.ps1` cover the mapping, the refusal and `-WhatIf` with mocked API calls; a CI step in `.github/workflows/` runs them on PRs that change `tools/e2e/` (Q-4).

- R1 `-Prefix <p>` deletes only what the prefix owns. Fields match `^Custom\.amte2e<n>(?!\d)`, where `<n>` comes from `amt<n>-` or `amte2e-<n>-`. Every scenario's fields are renamed to `amte2e<n>…` (`seed.ps1`, `assert.ps1`, scenario `*.json`), so each issue's cleanup is complete (Q-1 B).
- R2 A full clean needs `-All`. A bare run with no `-Prefix` and no `-All` stops with an error (Q-2).
- R3 A prefix that can't be mapped, including an empty one, still deletes the matching projects and processes but no fields, warns "Cannot map prefix '<p>' to a field filter; no fields were deleted.", and exits non-zero (Q-3).
- R3a An empty prefix, or one that would match every e2e project (for example `amte2e`), is refused before any delete. Otherwise skipping only the fields would still let it remove every project and process without `-All`.
- R1a Scenarios with no issue of their own (`fields`: `amte2eIssueOnly`, `amte2eLegacy`) are renamed to the number of the issue that created the scenario, found from git history.
- R4 `-WhatIf` lists every project, recycled project, process and field the run would delete, deletes nothing, skips the retry waits, and reports "would delete".
- R5 Every run prints its resolved field filter first.
- R6 The mapping is one function in `E2EAzDo.psm1`, next to `Assert-E2EName`.
- R7 Runs on Windows PowerShell 5.1 and PowerShell 7.
- R8 `.github/copilot-instructions.md` and `tools/e2e/reports/2026-10-regression.md:108` document the prefix forms, `-WhatIf` and the full-clean switch.

**Files likely to change**: `tools/e2e/Remove-E2EObjects.ps1`, `tools/e2e/E2EAzDo.psm1`, scenario `seed.ps1` /
`assert.ps1` / `*.json`, `tools/e2e/Remove-E2EObjects.Tests.ps1`, a workflow in `.github/workflows/`,
`.github/copilot-instructions.md`, `tools/e2e/reports/2026-10-regression.md`.

**Tests to add or update**:

- The mapping table above.
- The digit boundary (3272 does not match 32720).
- An unmappable prefix deletes no fields.
- Empty and `amte2e` prefixes are refused.
- `-WhatIf` calls no delete, with mocked REST calls.


## Spec

Working spec: `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3278-remove-e2eobjects-ps1-deletes-other-scen/assessment.md sha256 e7692bbfc8d55c32730ed0ceef68f6a1b2771238e09484b17cc7213f28fb300a

## Reproduction

The script was not run against the test orgs, because doing so would delete the shared fields. The expression at
`Remove-E2EObjects.ps1:15` was evaluated locally:

1. `amte2e-3272-` → `Custom.amte2e` (every e2e field).
2. `amte2e-abc-` and `x` → `Custom.amte2e` (every e2e field).
3. `amt3272-` → `Custom.amte2e3272` (scoped correctly).


## Cause

The prefix-to-field mapping treats "unrecognised prefix" as "match all", and there is no single field-naming
convention tied to an issue number. Confidence: high.


## Proposed fix

**Preferred**: as agreed with Martin (Q-1 to Q-4 in `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/item.md`):

- Add `Get-E2EFieldFilter` in `E2EAzDo.psm1`, using a digit-boundary regex over `amt<n>-` and `amte2e-<n>-`.
- Rename every scenario's fields to `amte2e<n>…`.
- Make a full clean need `-All`.
- An unmappable prefix skips the fields and exits non-zero. An empty or too-broad prefix is refused outright.
- Add `-WhatIf` through `SupportsShouldProcess`.
- Add Pester tests and a CI step that runs them.

**Requirements** (agreed with Martin):

- R1 `-Prefix <p>` deletes only what the prefix owns. Fields match `^Custom\.amte2e<n>(?!\d)`, where `<n>` comes from `amt<n>-` or `amte2e-<n>-`. Every scenario's fields are renamed to `amte2e<n>…` (`seed.ps1`, `assert.ps1`, scenario `*.json`), so each issue's cleanup is complete (Q-1 B).
- R1a Scenarios with no issue of their own (`fields`: `amte2eIssueOnly`, `amte2eLegacy`) are renamed to the number of the issue that created the scenario, found from git history.
- R2 A full clean needs `-All`. A bare run with no `-Prefix` and no `-All` stops with an error (Q-2).
- R3 A prefix that can't be mapped (other than those R3a refuses) still deletes the matching projects and processes but no fields, warns "Cannot map prefix '<p>' to a field filter; no fields were deleted.", and exits non-zero (Q-3).
- R3a An empty prefix, or one that would match every e2e project (for example `amte2e`), is refused before any delete. Otherwise skipping only the fields would still let it remove every project and process without `-All`.
- R4 `-WhatIf` lists every project, recycled project, process and field the run would delete, deletes nothing, skips the retry waits, and reports "would delete".
- R5 Every run prints its resolved field filter first.
- R6 The mapping is one function in `E2EAzDo.psm1`, next to `Assert-E2EName`.
- R7 Runs on Windows PowerShell 5.1 and PowerShell 7.
- R8 `.github/copilot-instructions.md` and `tools/e2e/reports/2026-10-regression.md:108` document the prefix forms, `-WhatIf` and the full-clean switch.
- R9 Pester tests `tools/e2e/*.Tests.ps1` cover the mapping, the refusal and `-WhatIf` with mocked API calls; a CI step in `.github/workflows/` runs them on PRs that change `tools/e2e/` (Q-4).

**Files likely to change**: `tools/e2e/Remove-E2EObjects.ps1`, `tools/e2e/E2EAzDo.psm1`, scenario `seed.ps1` /
`assert.ps1` / `*.json`, `tools/e2e/Remove-E2EObjects.Tests.ps1`, a workflow in `.github/workflows/`,
`.github/copilot-instructions.md`, `tools/e2e/reports/2026-10-regression.md`.

**Tests to add or update**:

- The mapping table above.
- The digit boundary (3272 does not match 32720).
- An unmappable prefix deletes no fields.
- Empty and `amte2e` prefixes are refused.
- `-WhatIf` calls no delete, with mocked REST calls.


Spec agreed: Cleanup only removes the issue's own objects, full clean needs -All
Spec version: specs/3278-remove-e2eobjects-ps1-deletes-other-scen/assessment.md sha256 e7692bbfc8d55c32730ed0ceef68f6a1b2771238e09484b17cc7213f28fb300a

## Docs updated

- e2e-harness: `platform/specs/e2e-harness/cleanup.md`: Who owns which objects

Also changed: `platform/specs/architecture.md` (a Remove-E2EObjects.ps1 entry next to Invoke-MigrationE2E.ps1) and the cleanup command in `tools/e2e/reports/2026-10-regression.md` (now `-All`).

## Output done evidence

- Pester, the change: `Invoke-Pester ./tools/e2e` gave 19 passed, 0 failed.
- Pester, the same tests against main's `Remove-E2EObjects.ps1` and `E2EAzDo.psm1`: 1 passed, 18 failed, including "deletes only the issue's own fields". The bug shows on main.
- Windows PowerShell 5.1: both files parse with no errors, and `Get-E2EFieldFilter -Prefix amte2e-3272-` returns `^Custom\.amte2e3272(?!\d)` (R7).
- Live, nkdagility-adomt-source, own scratch field `Custom.amte2e3278Probe`:
  - 12 e2e fields before the probe was created.
  - `-Prefix amte2e-3278- -WhatIf` listed only that field ("would delete field …/Custom.amte2e3278Probe") and deleted nothing. Exit 0.
  - `-Prefix amte2e-3278-` printed "deleted field …/Custom.amte2e3278Probe" and "remaining: none". Exit 0. Afterwards there were 12 e2e fields again: every other scenario's field was untouched.
  - `-Prefix amte2e-fields- -WhatIf` printed "field filter: none" and listed only that prefix's projects. Its exit 1 is covered by the unit test; the live call was cut short by output filtering.
  - `-Prefix amte2e-` was refused with "it matches every e2e object". A bare run was refused with "…or -All for every e2e object". Nothing was deleted.
- main was not run live: without `-WhatIf` it would delete the shared fields.

Confidence: 90%

Reasons:
- The bug is reproduced on main: the new tests fail there (18 of 19) and pass on the change (19 of 19).
- A live scoped run on the test org deleted only this issue's scratch field and left all 12 other e2e fields in place.
- The refusal paths were run live and deleted nothing; -WhatIf deleted nothing.
- What keeps it from higher: -All was not run live (it would wipe the shared fields), the target org was not exercised, and the new CI workflow has not yet run on GitHub.
