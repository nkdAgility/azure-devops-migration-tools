# Docs conversion plan: azure-devops-migration-tools

Moves this repo's source-of-truth docs into the docs layout (`docs/specs/`). Each move is a `git mv`, so history follows the file. Nothing is edited in the moves; edits come afterwards in their own commits.

## Systems

- core: src/MigrationTools/
- tfs-object-model: src/MigrationTools.Clients.TfsObjectModel/
- rest-client: src/MigrationTools.Clients.AzureDevops.Rest/
- file-system-client: src/MigrationTools.Clients.FileSystem/
- host: src/MigrationTools.Host/ src/MigrationTools.ConsoleFull/
- docs-generator: src/MigrationTools.ConsoleDataGenerator/ GenerateDocs.ps1 buildDocs.ps1
- telemetry: src/MigrationTools.Telemetery/
- packaging: src/MigrationTools.Chocolatey/ src/MigrationTools.WinGet/ src/MigrationTools.Extension/ build/
- e2e-harness: tools/e2e/
- docs-site: docs/content/ docs/layouts/ docs/assets/
- config-wizard: docs/_site-docs/

## Moves

git mv .github/copilot-instructions.md docs/specs/architecture.md
git mv .github/instructions/src.instructions.md docs/specs/rules.md
git mv .github/instructions/docs.content.docs.single.instructions.md docs/specs/docs-site/rules.md
git mv .github/instructions/docs.content.docs.list.instructions.md docs/specs/docs-site/list-pages.md
git mv docs/copilot-instructions.md docs/specs/docs-site/architecture.md
git mv docs/_site-docs/CONFIG_WIZARD_README.md docs/specs/config-wizard/README.md

## After the moves

Each in its own commit, after the moves:

1. Split `docs/specs/architecture.md`. The executable assemblies, the TFS Object Model vs REST notes and MigrationToolHost go to each system's `architecture.md`. "Validating Changes End to End" goes to `e2e-harness/rules.md`, and "Documentation Synchronization" goes to `docs-generator/rules.md`.
2. Merge `docs-site/list-pages.md` into `docs-site/rules.md`.
3. Remove the `applyTo` front matter from the moved instruction files.
4. Put back small entry files that point to `docs/specs/`:
   - `.github/copilot-instructions.md`, which Copilot loads from that path;
   - one `.github/instructions/*.instructions.md` for each `applyTo` scope, each pointing to the moved rules.
5. Repoint `AGENTS.md`, `CLAUDE.md` and `docs/specs/docs-site/architecture.md` at the new paths, so that no link points to a moved file.
6. Delete the empty `.vscode/copilot-instructions.md`.
7. Reduce `config-wizard/README.md` to what is true today. The wizard is off in every environment (#3261).

Changes to rule wording (the files in steps 1–5 that are rules) need Martin's approval of the exact wording first.

## New

- `docs/specs/lexicon.md`: the terms used across the systems (processor, tool, endpoint, enricher, field map, ReflectedWorkItemId…).
- `docs/specs/seams.md`: the seams between the systems (Object Model vs REST clients, host and processors, the docs generator and the docs site).
- `docs/specs/{system}/README.md` for each system above, saying what it is and does today.
- `docs/adr/`: started from the architecture decisions recorded in `docs/notes/martin-notes.md`.

## Stays where it is

`README.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md` and `change-log.md` are for people. `docs/notes/martin-notes.md` stays as the notes file. `platform/development/definition-of-workflow.md` is marked as replaced at carry-over.
