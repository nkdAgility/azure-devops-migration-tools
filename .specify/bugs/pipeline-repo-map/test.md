# Bug Verification: Pipeline migration keeps the source repository id when the definition's repository has no properties

- **Slug**: pipeline-repo-map
- **Tested**: 2026-11-23
- **Assessment**: ./assessment.md
- **Fix**: ./fix.md
- **Result**: verified

## Summary

The reproduction from the assessment no longer fails. The definition without repository `properties` is created and points at the target repository. A definition whose repository is missing gets the one clear error, and the existing pipeline scenarios behave as on `main`.

## Checks Performed

| Check | Command / Action | Result | Notes |
|-------|------------------|--------|-------|
| Reproduction (post-fix) | `tools/e2e/scenarios/pipelines-3312/assert.ps1 -Case mapped` | pass | exit 0. Target `amte2e-3312-yaml` repository id `3cc16230-…` = `amte2e-3312-app`, and the URL is the target's |
| Missing repository | `assert.ps1 -Case missing` | pass | tool exit 1, one clear error, no TF401019, no stack trace, `amte2e-3312-yaml` created, `amte2e-3312-missing` absent |
| New / updated tests | `dotnet test src/MigrationTools.Clients.AzureDevops.Rest.Tests --filter "TestCategory=L0"` | pass | 82 passed, 0 failed |
| Regression (existing pipeline scenarios) | `assert.ps1 -Case existing` | pass | novariablegroups exits 1 with only the variable-group skip; config exits 0, and the classic build points at the target repository |
| Docs | `GenerateDocs.ps1` | pass | no changes |
| Full solution build and tests | `done-checks.ps1` | not-run | runs at the Output Done gate |

## Output Excerpts

```
[ERR] BuildDefinition "amte2e-3312-missing" cannot be migrated because its repository "amte2e-3312-gone" does not exist in the target project "amte2e-3312-tgt". Migrate the repository first, or add a RepositoryNameMaps entry that maps it to an existing target repository.
Passed!  - Failed: 0, Passed: 82, Skipped: 0, Total: 82
```

## Residual Risks

- Non-`TfsGit` repositories (GitHub, Bitbucket, TFVC) were not exercised end to end; they are covered only by the L0 test that leaves them unchanged.

## Recommendation

Proceed to the Output Done gate. Verified end to end on the test orgs.
