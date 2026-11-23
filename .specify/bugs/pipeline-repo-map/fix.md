# Bug Fix: Pipeline migration keeps the source repository id when the definition's repository has no properties

- **Slug**: pipeline-repo-map
- **Fixed**: 2026-11-23
- **Assessment**: ./assessment.md
- **Status**: applied

## Summary

Build definitions that use an Azure Repos Git repository now have the repository mapped by name, honouring `RepositoryNameMaps`, whether or not `properties` is present. A definition whose target repository is missing is not sent. It gets one clear error, and the others carry on.

## Changes

| File | Change | Notes |
|------|--------|-------|
| `src/MigrationTools.Clients.AzureDevops.Rest/Processors/BuildDefinitionRepositoryMap.cs` | added | `UsesAzureReposGit` and `Apply` (source name by id or definition name, name map, target id and name) |
| `src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs` | modified | `MapRepositories` runs after the map filters for every definition. Only `ConnectedServiceId` stays behind the `Properties` check. Replaces `MapRepositoriesInBuidDefinition` |
| `src/MigrationTools.Clients.AzureDevops.Rest.Tests/Processors/BuildDefinitionRepositoryMapTests.cs` | added test | 6 L0 tests |
| `tools/e2e/scenarios/pipelines-3312/seed.ps1`, `config.json`, `config-mapped.json`, `assert.ps1` | added | e2e scenario and outcome checks |
| `tools/e2e/scenarios/pipelines/seed.ps1` | modified | comment no longer says mapping needs `properties` |

## Diff Highlights

```csharp
definitionsToBeMigrated = MapRepositories(definitionsToBeMigrated, sourceRepositories, targetRepositories).ToList();
...
if (definitionToBeMigrated.Repository?.Properties != null)
{
    definitionToBeMigrated.Repository.Properties.ConnectedServiceId = targetConnectedServiceId;
}
```

## Tests Added or Updated

- `BuildDefinitionRepositoryMapTests.Apply_RepositoryWithProperties_MapsToTargetRepository`: a repository with `properties` maps to the target id.
- `Apply_RepositoryWithoutProperties_MapsToTargetRepository`: the #3312 case.
- `Apply_RepositoryNameMap_MapsToRenamedTargetRepository`: `RepositoryNameMaps` is honoured.
- `Apply_TargetRepositoryMissing_ReportsNotFoundAndLeavesIdUnchanged` and `Apply_TargetRepositoryMissingWithProperties_ReportsNotFound`: not found is reported, and nothing is changed.
- `UsesAzureReposGit_GitHubRepository_IsFalse`: non-Git repositories are left alone.

## Local Verification

- `dotnet test src/MigrationTools.Clients.AzureDevops.Rest.Tests --filter "TestCategory=L0"`: 82 passed, 0 failed.
- `tools/e2e/scenarios/pipelines-3312/assert.ps1 -Case mapped`: PASSED (exit 0; the target repository id is `3cc16230-…`, which is `amte2e-3312-app`).
- `assert.ps1 -Case missing`: PASSED (tool exit 1, one clear error, no TF401019, no stack trace, the other definition created).
- `assert.ps1 -Case existing`: PASSED (novariablegroups exits 1 with only the variable-group skip; config exits 0, and the classic build points at the target repository).
- `GenerateDocs.ps1`: no changes.

## Deviations from Assessment

The `Repository` null guard noted at refinement is handled by `UsesAzureReposGit`, which returns false for a null repository.

## Follow-ups

None.
