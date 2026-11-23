# Bug assessment: #3268 release artifacts dropped

Slug: 3268-release-artifacts
Verdict: valid
Severity: high. Every classic release whose deploy job downloads an artifact is rejected by the target, and the run exits 1.

## Reproduction
On `main` (e2ddb400), against scratch projects `amte2e-3268-src`/`amte2e-3268-tgt`: one release with Build artifact `_amte2e-3268-build`, downloaded by deploy job "Agent job". The target rejects the POST with `InvalidRequestException` "Artifact alias '_amte2e-3268-build' does not matching any linked artifact aliases for deploy job 'Agent job'", followed by "0 of 1 source ReleaseDefinition(s) were skipped and 1 were rejected by the target", `EXIT CODE: 1`.

## Cause
- `ReleaseDefinition.ResetObject()` (`src/MigrationTools/DataContracts/Pipelines/ReleaseDefinitions.cs:54`) sets `Artifacts = null`, while the deploy phases still name the alias in `artifactsDownloadInput`.
- `CreateReleasePipelinesAsync` (`AzureDevOpsPipelineProcessor.cs:385-418`) maps queues, task groups, variable groups and service connections but never artifacts.
- `InternalExecute` (`AzureDevOpsPipelineProcessor.cs:84-92`) discards the mappings `CreateBuildPipelinesAsync` returns, so the release step has no build mapping.
- The typed `Artifact`/`DefinitionReference` models drop any field they do not declare (for example `repository`, `branches`, `connection`), so even a kept artifact would be posted incomplete.

## Proposed fix
1. Keep `Artifacts` in `ResetObject()`. Make `Artifact` and `DefinitionReference` carry unknown JSON fields (`[JsonExtensionData]`), and clear `artifactSourceDefinitionUrl`, which holds a source URL.
2. Pass the build mappings from the build step to the release step. When builds are not migrated in this run, build them by matching source and target build definitions by name (the existing `FindExistingMappings`).
3. Map each artifact before the POST:
   - Build, from the source project: `definitionReference.definition` becomes the target build definition id and name; `definitionReference.project` becomes the target project id and name, taken from the target build definition's `project`.
   - Git (`Git`/TfsGit), from the source project: `definition` becomes the target repository (by name, through `RepositoryNameMaps`), `project` the target project.
   - Any other type, or a Build or Git artifact from another project, or one with no target match: the release is not posted. It is logged as an error naming the release, the alias, the type and why, counts as skipped, and the run fails as it does today for skipped items.
4. Unit tests: Build mapped; build not in target, so skipped; other type, so skipped; unknown fields round-trip.
5. E2E scenario `tools/e2e/scenarios/pipelines-3268` (seed, config, assert) run on `main` and on the change: the happy path (release created, artifact pointing at the target build and project, exit 0) and an unmappable artifact (clear error, exit 1). `GenerateDocs.ps1` is run; no option changes are expected.

## Decided while assessing
- The target project id comes from the target build definition's `project` reference; no new endpoint call is needed.
- Cross-project artifacts are skipped, not mapped by name: the run maps only the configured project.
- Types other than Build and Git are skipped with a clear message rather than guessed. This is the issue's "otherwise report clearly instead of failing with a 400".

## Docs
- rest-client: no system spec exists under `platform/specs/rest-client/`; the processor's generated reference is regenerated. Docs unchanged: core: data contract only, no documented behaviour.
