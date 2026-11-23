# Scenario pipelines-3268: release artifacts

`tools/e2e/scenarios/pipelines-3268/` proves that classic release definitions keep their artifacts.

- `seed.ps1` creates scratch projects `amte2e-3268-src` (source org) and `amte2e-3268-tgt` (target org), with two
  classic builds and two releases whose deploy job downloads a Build artifact: `amte2e-3268-release` uses
  `amte2e-3268-build`, and `amte2e-3268-orphan-release` uses `amte2e-3268-orphan`, which is never migrated.
- `config.json` migrates `amte2e-3268-build` and `amte2e-3268-release`; `config-unmappable.json` migrates only
  `amte2e-3268-orphan-release`, without builds.
- `assert.ps1 -Case mapped` runs `config.json` and passes when the tool exits 0 and the target release's artifact
  points at the target project and the migrated build definition.
- `assert.ps1 -Case unmappable` runs `config-unmappable.json` and passes when the tool exits 1 with one error naming
  the release, the artifact's alias and type, the release counts as skipped and nothing is posted to the target.

Each case removes the release an earlier run created in the target first, so it can be run again.
