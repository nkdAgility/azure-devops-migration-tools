---
purpose: A classic release definition whose deploy jobs download an artifact migrates with its artifacts mapped to the target, and a release whose artifact cannot be mapped is reported clearly instead of being rejected with a 400.
paths:
  - src/MigrationTools/DataContracts/Pipelines/**
  - src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor*.cs
  - src/MigrationTools.Clients.AzureDevops.Rest.Tests/**
  - tools/e2e/scenarios/pipelines-3268/**
  - docs/data/classes/reference.processors.azuredevopspipelineprocessor.yaml
  - docs/static/schema/schema.processors.azuredevopspipelineprocessor.json
  - platform/specs/rest-client/**
  - platform/specs/core/**
  - platform/specs/e2e-harness/**
  - specs/3268-azuredevopspipelineprocessor-drops-relea/**
---

# #3268 AzureDevOpsPipelineProcessor drops release artifacts, so classic releases that download an artifact are rejected by the target

Run: 22834aa4

## Sources read
- `platform/specs/systems.md`: the change is in `core` (`src/MigrationTools/`, the data contract) and `rest-client` (`src/MigrationTools.Clients.AzureDevops.Rest/`, the processor); the scenario is in `e2e-harness`.
- `platform/specs/architecture.md`: the processor is REST-only and runs in ConsoleFull and ConsoleCore; changes are proved end to end on the test orgs; option and class changes must be reflected in the generated docs (`GenerateDocs.ps1`).
- `platform/specs/rules.md`: the comment rule for `src/**`.
- `.specify/memory/constitution.md`: still the unfilled template; no extra principles.
- `platform/specs/` has no `core/` or `rest-client/` system spec, so there is no recorded behaviour for release artifacts to reconcile.
- `src/MigrationTools/DataContracts/Pipelines/ReleaseDefinitions.cs`: `ResetObject()` sets `Artifacts = null` (since #767, the processor's first version). `Artifact`/`DefinitionReference` model only `type`, `alias`, `isPrimary`, `isRetained` and `definitionReference` with `project`, `definition`, `defaultVersion*`, `artifactSourceDefinitionUrl`.
- `src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs`: `CreateReleasePipelinesAsync` (385-418) maps queues, task groups, variable groups and service connections but not artifacts. `CreateBuildPipelinesAsync` returns build mappings (new plus already-existing by name), but `InternalExecute` (84-92) discards them, so the release step has no build mapping.

## Scope
- In: keeping `Artifacts` on release definitions and mapping them to the target in the release step. Build artifacts: `definitionReference.project` to the target project, `definitionReference.definition` to the target build definition from the build mappings (migrated in this run, or already in the target with the same name). Other artifact types are mapped where the run already holds a mapping (Git repository by name/`RepositoryNameMaps`, service-connection-backed types by the service-connection mapping); an artifact that cannot be mapped skips that release with a clear error naming the release, alias and type, and the run fails as it does today for skipped items. Unit tests and one e2e scenario (`pipelines-3268`).
- Out: build pipelines, task groups, variable groups and service connections themselves; YAML pipelines; release triggers (still dropped: a separate item if wanted); migrating feeds for Azure Artifacts; the TFS object model client.

## Reproduction
Scratch projects `amte2e-3268-src` (source org) and `amte2e-3268-tgt` (target org), seeded with a classic build `amte2e-3268-build` and a release `amte2e-3268-release` that has one Build artifact `_amte2e-3268-build` downloaded by deploy job "Agent job". `AzureDevOpsPipelineProcessor` with `MigrateBuildPipelines` and `MigrateReleasePipelines` true. ConsoleFull built from `main` (e2ddb400).
1. `Invoke-MigrationE2E.ps1 -Config <repro config>` → `POST .../release/definitions` rejected: `InvalidRequestException` "Artifact alias '_amte2e-3268-build' does not matching any linked artifact aliases for deploy job 'Agent job'."
2. → "0 of 1 source ReleaseDefinition(s) were skipped and 1 were rejected by the target; marking the run as failed." `EXIT CODE: 1`.
- The build definition was also skipped in this run because the seed's repositories are named after their projects and the config had no `RepositoryNameMaps`; the scenario for the fix adds that map so the build migrates and its mapping is available.

## Rubber duck
A user migrating classic releases gets every release whose stage downloads an artifact rejected, because the tool strips all artifacts before posting, while the deploy jobs still name the alias. The fix keeps the artifacts and rewrites the ids that point at the source: the project and the build definition for Build artifacts. To do that the release step needs the build mappings, which the build step already produces but throws away; when builds are not migrated in this run, the target build definition is found by name. Gaps it showed:
- The typed `Artifact` model can lose fields the API returns (for example `definitionReference` keys such as `repository`, `connection`, `branches`, `feed`). It must round-trip unknown keys, or the posted artifact will be incomplete. Checked in Specifying against a real response.
- A Build artifact whose build definition is in another project, or not in the target, has nothing to map to: the release is skipped with a clear error (the issue's "report clearly instead of failing with a 400").
- Artifact-based triggers are still dropped (`Triggers = null`); out of scope, noted for a follow-up.
- `artifactSourceDefinitionUrl` holds a source URL; it is cleared or rewritten for the target.

## Architecture check
- REST-only processor, no TFS object model dependency: the fix stays in the REST client and the data contract. No finding.
- Comment rule (`platform/specs/rules.md`): new code explains itself through names; no restating comments. No finding.
- Docs in step with code: no option changes are planned; `GenerateDocs.ps1` is run and the generated reference checked. No finding.
- End-to-end proof: covered by the `pipelines-3268` scenario on `main` and on the change, happy and unmappable paths. No finding.
Open findings: 0

## Proposed follow-ups
- Release triggers are dropped by `ResetObject()` (`Triggers = null`); artifact triggers could be mapped the same way.

## Spec

Working spec: `specs/3268-azuredevopspipelineprocessor-drops-relea/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3268-azuredevopspipelineprocessor-drops-relea/assessment.md sha256 8933531c4964dbb052d6bda726165d926662c1f644e65c3a58d77fefafca1bea

## Reproduction

On `main` (e2ddb400), against scratch projects `amte2e-3268-src`/`amte2e-3268-tgt`: one release with Build artifact `_amte2e-3268-build`, downloaded by deploy job "Agent job". The target rejects the POST with `InvalidRequestException` "Artifact alias '_amte2e-3268-build' does not matching any linked artifact aliases for deploy job 'Agent job'", followed by "0 of 1 source ReleaseDefinition(s) were skipped and 1 were rejected by the target", `EXIT CODE: 1`.


## Cause




## Proposed fix




## Cause

- `ReleaseDefinition.ResetObject()` (`src/MigrationTools/DataContracts/Pipelines/ReleaseDefinitions.cs:54`) sets `Artifacts = null`, while the deploy phases still name the alias in `artifactsDownloadInput`.
- `CreateReleasePipelinesAsync` (`AzureDevOpsPipelineProcessor.cs:385-418`) maps queues, task groups, variable groups and service connections but never artifacts.
- `InternalExecute` (`AzureDevOpsPipelineProcessor.cs:84-92`) discards the mappings `CreateBuildPipelinesAsync` returns, so the release step has no build mapping.
- The typed `Artifact`/`DefinitionReference` models drop any field they do not declare (for example `repository`, `branches`, `connection`), so even a kept artifact would be posted incomplete.


## Proposed fix

1. Keep `Artifacts` in `ResetObject()`. Make `Artifact` and `DefinitionReference` carry unknown JSON fields (`[JsonExtensionData]`), and clear `artifactSourceDefinitionUrl`, which holds a source URL.
2. Pass the build mappings from the build step to the release step. When builds are not migrated in this run, create the build mappings by matching source and target build definitions by name (the existing `FindExistingMappings`).
3. Map each artifact before the POST:
   - **Build**, from the source project: `definitionReference.definition` becomes the target build definition id and name; `definitionReference.project` becomes the target project id and name, taken from the target build definition's `project`.
   - **Git** (Azure Repos), from the source project: `definition` becomes the target repository (by name, through `RepositoryNameMaps`), and `project` the target project.
   - **Types backed by a service connection** (GitHub, external TFS/Azure DevOps build, Jenkins and similar): `definitionReference.connection` becomes the target service connection from the service-connection mapping the run already holds. The rest of the reference (external repository, job or definition) points outside the migrated project and is kept as is.
   - **Not mappable**: an Azure Artifacts artifact (it points at a feed, and feeds are not migrated); a Build or Git artifact from another project; any artifact with no target match, including a service connection that was not migrated. The release is not posted. It is logged as an error naming the release, the alias, the type and why, counts as skipped, and the run fails as it does today for skipped items.
4. Unit tests: Build mapped; Git mapped; service-connection type mapped; Build not in target, so skipped; cross-project Build, so skipped; Azure Artifacts, so skipped; unknown fields round-trip.
5. E2E scenario `tools/e2e/scenarios/pipelines-3268` (seed, config, assert) run on `main` and on the change: the happy path (release created, artifact pointing at the target build and project, exit 0) and an unmappable artifact (clear error, exit 1). `GenerateDocs.ps1` is run; no option changes are expected.


## Outcome checks

- rest-client, test orgs: a classic release with a Build artifact downloaded by its deploy job migrates; pass = exit 0 and the target release's artifact points at the target project and the migrated build definition: `pwsh -NoProfile -File tools/e2e/scenarios/pipelines-3268/assert.ps1 -Case mapped`
- rest-client, test orgs: a release whose artifact cannot be mapped is skipped with a clear error; pass = exit 1, the log names the release, alias and type, and no 400 from the target: `pwsh -NoProfile -File tools/e2e/scenarios/pipelines-3268/assert.ps1 -Case unmappable`
- rest-client, unit: artifact mapping and skip rules; pass = exit 0: `dotnet test src/MigrationTools.Clients.AzureDevops.Rest.Tests --filter "FullyQualifiedName~ReleaseArtifact"`

Spec agreed: Approve
Spec version: specs/3268-azuredevopspipelineprocessor-drops-relea/assessment.md sha256 8933531c4964dbb052d6bda726165d926662c1f644e65c3a58d77fefafca1bea

## Output done evidence

- Build: `dotnet build MigrationTools.sln`: 0 errors, after merging origin/main (fb9bc56e).
- Tests: `dotnet test MigrationTools.sln` with the test orgs: all pass (Rest.Tests 97, MigrationTools.Tests 305, TfsObjectModel.Tests 428, Host.Tests 12, FileSystem.Tests 6; 0 failed). The 10 new `ReleaseArtifactMapTests` cover Build, Git, renamed Git, service connection, build not in target, cross-project, unmigrated service connection, Azure Artifacts, first unmapped artifact, and unknown fields kept.
- E2E `tools/e2e/scenarios/pipelines-3268/assert.ps1`, scratch projects amte2e-3268-src/tgt:
  - main, `-Case mapped`: 7 of 7 checks fail (target rejects with "Artifact alias ... does not matching any linked artifact aliases", exit 1).
  - main, `-Case unmappable`: 3 checks fail (400 from the target, not counted as skipped).
  - change, `-Case mapped`: 7 of 7 pass, tool exit 0, the artifact points at the target project and build definition 35.
  - change, `-Case unmappable`: 5 of 5 pass, tool exit 1 with one clear error naming the release, alias and type, and nothing posted.
- Regression: the existing `pipelines` scenario in scratch projects amte2e-3268-pipe-src/tgt: variable group, task group, build and release all migrated, exit 0.
- Docs: `GenerateDocs.ps1` produced no changes (no options changed). rest-client doc written; core and e2e-harness wait on Martin (QUESTION 2).

Confidence: 90%

Reasons:
- The reported failure is reproduced on main and gone on the change, against the real target, on both the happy and the failure path.
- The mapping is tested per artifact type, and the existing pipeline scenarios still pass.
- Git and service-connection artifacts are proved by unit tests only, not end to end.


## Docs updated

- rest-client: `platform/specs/rest-client/pipeline-processor.md`: A release definition keeps its artifacts
- core: `platform/specs/core/release-definition-contract.md`: It keeps the artifacts.
- e2e-harness: `platform/specs/e2e-harness/pipelines-3268.md`: proves that classic release definitions keep their artifacts


Paths extended: Martin answered QUESTION 2 with "2:a": platform/specs/core/** and platform/specs/e2e-harness/** added.

