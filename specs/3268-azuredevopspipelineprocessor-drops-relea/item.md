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
