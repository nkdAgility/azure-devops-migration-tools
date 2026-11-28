# AzureDevOpsPipelineProcessor

Migrates service connections, variable groups, task groups, build definitions and classic release definitions
between Azure DevOps projects through the REST API, in that order. Each step passes its source-to-target mappings to
the later steps.

## Release artifacts

A release definition keeps its artifacts, and each one is pointed at the target before the definition is created:

- **Build**, from the migrated project: the artifact points at the target project and at the target build
  definition. The build definition comes from the build step's mappings, or from a build definition with the same
  name already in the target project.
- **Git** (Azure Repos), from the migrated project: the artifact points at the target project and at the target
  repository with the same name, or the name given in `RepositoryNameMaps`.
- **Types that come through a service connection** (GitHub, external builds, Jenkins and similar): the artifact
  points at the target service connection from the service-connection mappings. The rest of its reference is outside
  the project and is kept as is.

Fields of the artifact that the tool does not model are kept, and the link back to the source build is removed.

A release definition is not created when one of its artifacts cannot be mapped: an Azure Artifacts feed (feeds are not
migrated), a Build or Git artifact from another project, or an artifact whose build definition, repository or service
connection is not in the target. The tool logs an error naming the release definition, the artifact's alias, its
type and the reason; the definition counts as skipped and the run fails.

Release triggers are not migrated.
