# Release definition contract

`ReleaseDefinition` (`src/MigrationTools/DataContracts/Pipelines/ReleaseDefinitions.cs`) is the shape the REST
client reads from a source project and posts to a target project.

## Before a release definition is created

`ResetObject()` removes what only identifies the definition in the source: its id, revision, links, url, triggers
and pipeline process. It keeps the artifacts.

## Artifacts

- An artifact keeps every field the API returns, including those the contract does not model (`AdditionalData` on
  `Artifact` and on `DefinitionReference`), so a posted artifact carries its branches, repository and other settings.
- `DefinitionReference.Connection` holds the service connection of an artifact that comes through one.
- `ResetObject()` removes `artifactSourceDefinitionUrl`, the link back to the source build.
- The ids an artifact points at (project, definition, connection and `sourceId`) are rewritten for the target by the
  pipeline processor; see `platform/specs/rest-client/pipeline-processor.md`.

## Git repositories

`GitRepository.Project` holds the project a repository belongs to; the pipeline processor reads the target
project's id and name from it.
