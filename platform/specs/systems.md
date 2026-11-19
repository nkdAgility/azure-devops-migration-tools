# Systems

Each system and the source paths that belong to it. The docs check uses this to find which `platform/specs/{system}/` files a change must update.

- core: src/MigrationTools/ src/MigrationTools.Shadows/
- tfs-object-model: src/MigrationTools.Clients.TfsObjectModel/
- rest-client: src/MigrationTools.Clients.AzureDevops.Rest/
- file-system-client: src/MigrationTools.Clients.FileSystem/
- host: src/MigrationTools.Host/ src/MigrationTools.ConsoleFull/
- docs-generator: src/MigrationTools.ConsoleDataGenerator/ GenerateDocs.ps1 buildDocs.ps1
- telemetry: src/MigrationTools.Telemetery/
- packaging: src/MigrationTools.Chocolatey/ src/MigrationTools.WinGet/ src/MigrationTools.Extension/ build/ .github/workflows/
- e2e-harness: tools/e2e/
- docs-site: docs/content/ docs/layouts/ docs/data/ docs/static/ docs/hugo.yaml docs/hugo.canary.yaml docs/hugo.local.yaml docs/hugo.preview.yaml docs/hugo.production.yaml
- config-wizard: docs/_site-docs/

