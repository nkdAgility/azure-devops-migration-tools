# Bug Assessment: Remove-E2EObjects.ps1 deletes other scenarios' Custom.amte2e* fields

- **Slug**: 3278-remove-e2eobjects-ps1-deletes-other-scen
- **Created**: 2026-11-23
- **Source**: https://github.com/nkdAgility/azure-devops-migration-tools/issues/3278 (host: github.com, allowlisted; read with `gh issue view`)
- **Verdict**: valid
- **Severity**: high

## Report (verbatim or summarized)

Issue #3278 reports that `tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3272-` deleted every unused
`Custom.amte2e*` field in both test organisations, not only that prefix's fields. It asks for:

- a field filter derived from the exact prefix;
- a refusal when the prefix can't be mapped;
- `-WhatIf`;
- tests for the mapping.

## Symptom

A cleanup scoped to one issue deletes every unused e2e field on the shared test orgs, and parallel scenario runs
then fail. Expected: only that issue's fields are deleted.

## Reproduction

The script was not run against the test orgs, because doing so would delete the shared fields. The expression at
`Remove-E2EObjects.ps1:15` was evaluated locally:

1. `amte2e-3272-` → `Custom.amte2e` (every e2e field).
2. `amte2e-abc-` and `x` → `Custom.amte2e` (every e2e field).
3. `amt3272-` → `Custom.amte2e3272` (scoped correctly).

## Suspected Code Paths

- `tools/e2e/Remove-E2EObjects.ps1:15`: only `^amt(\d+)-$` is recognised, and every other prefix falls back to
  `Custom.amte2e`.
- `tools/e2e/Remove-E2EObjects.ps1:16`: `StartsWith` with no digit boundary, so `amte2e3272` also matches 32720.
- `tools/e2e/E2EAzDo.psm1:32-35`: `Assert-E2EName` accepts `amt<issue>-`, so both prefix forms are legitimate.
- Scenario field names that no issue-scoped filter can reach: `scenarios/fields/*` (`amte2eIssueOnly`,
  `amte2eLegacy`) and `scenarios/picklists/*` (`amte2eAmt3241…`).

## Root Cause Hypothesis

The prefix-to-field mapping treats "unrecognised prefix" as "match all", and there is no single field-naming
convention tied to an issue number. Confidence: high.

## Proposed Remediation

**Preferred**: as agreed with Martin (Q-1 to Q-4 in `specs/3278-remove-e2eobjects-ps1-deletes-other-scen/item.md`):

- Add `Get-E2EFieldFilter` in `E2EAzDo.psm1`, using a digit-boundary regex over `amt<n>-` and `amte2e-<n>-`.
- Rename every scenario's fields to `amte2e<n>…`.
- Make a full clean need `-All`.
- An unmappable prefix skips the fields and exits non-zero. An empty or too-broad prefix is refused outright.
- Add `-WhatIf` through `SupportsShouldProcess`.
- Add Pester tests and a CI step that runs them.

**Files likely to change**: `tools/e2e/Remove-E2EObjects.ps1`, `tools/e2e/E2EAzDo.psm1`, scenario `seed.ps1` /
`assert.ps1` / `*.json`, `tools/e2e/Remove-E2EObjects.Tests.ps1`, a workflow in `.github/workflows/`,
`.github/copilot-instructions.md`, `tools/e2e/reports/2026-10-regression.md`.

**Tests to add or update**:

- The mapping table above.
- The digit boundary (3272 does not match 32720).
- An unmappable prefix deletes no fields.
- Empty and `amte2e` prefixes are refused.
- `-WhatIf` calls no delete, with mocked REST calls.

## Risks & Considerations

- Renaming fields changes existing scenarios. Fields with the old names stay on the orgs until a full clean
  (`-All`) removes them.
- The CI step for Pester is a new convention.
- Changing the default (`-All`) breaks the documented bare full clean.

## Open Questions

- None. Q-1 to Q-4 are answered. R1a and R3a wait for Martin's spec agreement.
