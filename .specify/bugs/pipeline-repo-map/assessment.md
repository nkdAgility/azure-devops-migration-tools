# Bug Assessment: Pipeline migration keeps the source repository id when the definition's repository has no properties

- **Slug**: pipeline-repo-map
- **Created**: 2026-11-21
- **Source**: https://github.com/nkdAgility/azure-devops-migration-tools/issues/3312 (host: github.com, allowlisted)
- **Verdict**: valid
- **Severity**: high

## Report (summarized)

Issue #3312: pipeline migration doesn't remap the repository on a build definition whose `repository` object has no `properties`. The definition is posted with the source repository id, and the target rejects it with `TF401019`. On a live migration (16.3.4 and 16.3.5 previews), 58 of 61 YAML build definitions migrated. The 3 rejected were the only ones without `properties`, and their repositories already existed in the target under the same names.

Expected: map the repository by name, honouring `RepositoryNameMaps`, whether or not `properties` is present; only the connected service id depends on `properties`. If the target repository can't be found, fail that definition with one clear message naming the definition and the repository and saying what to do. Never a raw `TF401019`, and no stack trace. The other definitions carry on.

## Symptom

A build definition whose repository has no `properties` is sent to the target with the source repository id and rejected with `TF401019`. It should be created and point at the same-named (or mapped) repository in the target.

## Reproduction

1. In nkdagility-adomt-source, project `amte2e-3312-src`, create repository `amte2e-3312-app` with `/azure-pipelines.yml`. Create the same repository in nkdagility-adomt-target, project `amte2e-3312-tgt`.
2. Create a YAML build definition `amte2e-3312-yaml` (`process.type = 2`) whose `repository` has `id`, `name`, `type = TfsGit`, `url` and `defaultBranch`, but no `properties`. The stored definition reports no `properties`.
3. Run `tools/e2e/Invoke-MigrationE2E.ps1` on `main` (56bab21f) with `AzureDevOpsPipelineProcessor`, `MigrateBuildPipelines: true` and `BuildPipelines: ["amte2e-3312-yaml"]`.
4. Result: `TF401019: The Git repository with name or identifier f6ad1e84-ca6a-4c38-ba8d-c943943278be does not exist ...`, then `0 of 1 source BuildDefinition(s) were skipped and 1 were rejected by the target`, exit code 1.

## Suspected Code Paths

- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs:249-253`: `MapRepositoriesInBuidDefinition` is called only inside `if (definitionToBeMigrated.Repository?.Properties != null)`.
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs:329-344`: `MapRepositoriesInBuidDefinition` sets `Repository.Id` to null when no target repository matches. It also maps non-Git repositories by name, which nulls their id.
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/DefinitionMigrationOutcome.cs`: a definition filtered out before it is sent counts as skipped and fails the run.

## Root Cause Hypothesis

The repository mapping was placed inside the `Properties` null check that only `ConnectedServiceId` needs. Definitions without `properties` (typical of YAML definitions created through the REST API) keep the source repository id, and the target rejects it. Confidence: high (reproduced on `main`).

## Proposed Remediation

**Preferred**: Move the repository mapping out of the `Properties` check into a step that runs for every build definition whose repository type is `TfsGit`. Put the mapping in a small `BuildDefinitionRepositoryMap` class: find the source repository's name by id (falling back to the definition's repository name), apply `RepositoryNameMaps`, and set `Repository.Id` and `Name` from the target repository. If no target repository matches, don't send the definition. Log one error: `BuildDefinition "<name>" cannot be migrated because its repository "<repo>" does not exist in the target project "<project>". Migrate the repository first, or add a RepositoryNameMaps entry that maps it to an existing target repository.` The definition then counts as skipped, so the run still exits 1 while the others are created. Keep `ConnectedServiceId` behind the `Properties` check. Leave repositories that aren't `TfsGit` unchanged.

**Alternatives**:
- Keep sending a definition whose target repository is missing, with a null id, and let the target reject it. Rejected: it produces the raw server error the issue rules out.

**Files likely to change**:
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs`
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/BuildDefinitionRepositoryMap.cs` (new)
- `src/MigrationTools.Clients.AzureDevops.Rest.Tests/Processors/BuildDefinitionRepositoryMapTests.cs` (new)
- `tools/e2e/scenarios/pipelines-3312/` (new e2e scenario)

**Tests to add or update**:
- L0: repository with properties is mapped; without properties is mapped; `RepositoryNameMaps` rename is mapped; target missing (with and without properties) is reported not found and the id is left unchanged; non-`TfsGit` repositories are not treated as Azure Repos Git.
- E2E on the test orgs: the reproduction above migrates and points at the target repository; a definition whose repository exists only in the source fails with the clear message while the other is created; the existing `pipelines` scenarios behave as on `main`.

## Risks & Considerations

- Behaviour change: a definition whose Azure Repos Git repository is missing in the target is now skipped with an error instead of being sent with a null id. That is the intended outcome.
- Non-`TfsGit` definitions with `properties` used to have their repository id set to null. They are now left unchanged, which keeps an id the target can resolve.
- The repository `url` is not remapped; the target resolves the repository by id. The e2e check confirms the stored URL is the target's.
- No option classes change, so the generated docs should not change.

## Open Questions

None.
