---
purpose: Pipeline migration maps a build definition's Azure Repos Git repository to the target repository whether or not the definition's repository has properties, and fails that one definition with a clear message when the target repository does not exist.
paths:
  - src/MigrationTools.Clients.AzureDevops.Rest/Processors/**
  - src/MigrationTools.Clients.AzureDevops.Rest.Tests/Processors/**
  - tools/e2e/scenarios/pipelines-3312/**
  - tools/e2e/scenarios/pipelines/seed.ps1
  - docs/**
---
# WKR-3312

Run: 02501ab7

## Sources read
- Issue #3312: definitions whose `repository` has no `properties` are posted with the source repository id and rejected with TF401019. It expects mapping by name (honouring `RepositoryNameMaps`) regardless of `properties`, one clear message per unmapped definition, and the others to carry on.
- `AGENTS.md`, `CLAUDE.md`: e2e validation on the test orgs is required for migration behaviour. Use your own scratch projects, and run on `main` and on the change.
- `.github/copilot-instructions.md`, `.github/instructions/src.instructions.md`: point to `platform/specs/`.
- `platform/specs/rules.md`: comments only for what code cannot say.
- `platform/specs/architecture.md`: REST processors, "Validating Changes End to End", and "Documentation Synchronization" (regenerate only when option classes change).
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs`: the mapping sits inside the `Properties != null` check at lines 249–253, and `MapRepositoriesInBuidDefinition` is at lines 329–344.
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/DefinitionMigrationOutcome.cs`: a definition filtered out before it is sent counts as skipped, and that fails the run.
- `src/MigrationTools/DataContracts/Pipelines/BuildDefinitions.cs`: `Repository` has `Id`, `Type`, `Name`, `Url` and `Properties` (where `ConnectedServiceId` lives).

## Scope
- In: build definition repository mapping in `AzureDevOpsPipelineProcessor`, a clear per-definition failure when the target repository is missing, L0 tests, and the e2e scenario `pipelines-3312`.
- Out: release definitions, service connection mapping beyond `ConnectedServiceId`, remapping the repository URL, and repositories that aren't `TfsGit` (left unchanged).

## Reproduction
1. Seed nkdagility-adomt-source/amte2e-3312-src with a YAML build definition `amte2e-3312-yaml` whose repository `amte2e-3312-app` has no `properties` (the stored definition reports `repository properties: False`). Create the same repository in nkdagility-adomt-target/amte2e-3312-tgt.
2. On `main` (56bab21f), run `tools/e2e/Invoke-MigrationE2E.ps1` with the processor limited to that definition. Result: `TF401019: The Git repository with name or identifier f6ad1e84-ca6a-4c38-ba8d-c943943278be does not exist ...`, then `0 of 1 source BuildDefinition(s) were skipped and 1 were rejected by the target`, exit code 1.

## Rubber duck
A migrator moves YAML build definitions between projects. When the source definition's repository object has no `properties` (common for YAML definitions created through the REST API), the processor skips repository mapping entirely. It sends the source repository id, and the target rejects the definition. The fix maps the repository by name for every Azure Repos Git definition and keeps only `ConnectedServiceId` behind the `properties` check.

Gaps the explanation showed:
- There is no defined behaviour when the target repository is missing. Today the id is set to null when `properties` is present, or the source id is kept when it isn't. The issue settles this: fail that definition with one clear message and carry on.
- Non-Git repositories (GitHub, Bitbucket, TFVC) aren't in the source repository list, so mapping them by name would null their id. Leave them unchanged.
- `Repository` can be null; the mapping must guard against it.

## Architecture check
- `platform/specs/rules.md` (comments): the approach needs no restating comments. Met.
- `platform/specs/architecture.md` (REST processors, no TFS object model in REST code): the change stays in the REST processor and data contracts. Met.
- `platform/specs/architecture.md` (Documentation Synchronization): no option class changes, so no regenerated docs are expected. `GenerateDocs.ps1` must still be run and show no diff. Met.
- `.specify/memory/constitution.md`: an unfilled template, so it sets no rules.
Open findings: 0

## Spec

Working spec: `.specify/bugs/pipeline-repo-map/assessment.md` (the bug assessment Martin approves).

Spec presented: .specify/bugs/pipeline-repo-map/assessment.md sha256 7b83b3a0da20cfa5eb575072401ce0e9b3f0aef88aaf696e905f7ff2037fa173

## Reproduction

1. In nkdagility-adomt-source, project `amte2e-3312-src`, create repository `amte2e-3312-app` with `/azure-pipelines.yml`. Create the same repository in nkdagility-adomt-target, project `amte2e-3312-tgt`.
2. Create a YAML build definition `amte2e-3312-yaml` (`process.type = 2`) whose `repository` has `id`, `name`, `type = TfsGit`, `url` and `defaultBranch`, but no `properties`. The stored definition reports no `properties`.
3. Run `tools/e2e/Invoke-MigrationE2E.ps1` on `main` (56bab21f) with `AzureDevOpsPipelineProcessor`, `MigrateBuildPipelines: true` and `BuildPipelines: ["amte2e-3312-yaml"]`.
4. Result: `TF401019: The Git repository with name or identifier f6ad1e84-ca6a-4c38-ba8d-c943943278be does not exist ...`, then `0 of 1 source BuildDefinition(s) were skipped and 1 were rejected by the target`, exit code 1.


## Cause

The repository mapping was placed inside the `Properties` null check that only `ConnectedServiceId` needs. Definitions without `properties` (typical of YAML definitions created through the REST API) keep the source repository id, and the target rejects it. Confidence: high (reproduced on `main`).


## Proposed fix

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


## Outcome checks

- Repository mapping unit tests, local; pass = exit 0 and BuildDefinitionRepositoryMapTests all pass: `dotnet test src/MigrationTools.Clients.AzureDevops.Rest.Tests --filter "TestCategory=L0"`
- Definition without repository properties migrates to the target repository, on nkdagility-adomt-source to nkdagility-adomt-target (amte2e-3312-*); pass = exit 0 and the target amte2e-3312-yaml repository id is the target amte2e-3312-app id: `pwsh -NoProfile -File tools/e2e/scenarios/pipelines-3312/assert.ps1 -Case mapped`
- Definition whose repository is missing in the target fails clearly and the others carry on, on the same test orgs; pass = exit 0 from the assert (tool exit 1, one "cannot be migrated because its repository" error, no TF401019, amte2e-3312-yaml created, amte2e-3312-missing absent): `pwsh -NoProfile -File tools/e2e/scenarios/pipelines-3312/assert.ps1 -Case missing`
- Existing pipeline scenarios still behave as on main, in amte2e-3312-pipe-*; pass = exit 0 from the assert (config.json exits 0 with the classic build on the target repo, config-novariablegroups.json exits 1 with only the variable-group skip): `pwsh -NoProfile -File tools/e2e/scenarios/pipelines-3312/assert.ps1 -Case existing`

