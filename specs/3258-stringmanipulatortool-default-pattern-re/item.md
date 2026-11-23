---
purpose: Only characters the SOAP (TFS object model) API cannot save are scrubbed from migrated String fields, including System.Title, by the built-in default and by the shipped configs; everything else, including letters outside the BMP and emoji, is kept.
paths:
  - src/MigrationTools/Tools/StringManipulatorTool.cs
  - src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs
  - docs/content/docs/reference/tools/stringmanipulatortool/index.md
  - docs/data/classes/reference.tools.stringmanipulatortool.yaml
  - tools/e2e/scenarios/stringmanip-3258/**
  - src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs
  - configuration-default.json
  - configuration-classic.json
  - appsettings.json
---

# #3258 StringManipulatorTool default pattern removes supplementary-plane letters

## Scope
- In: the default manipulator added by `StringManipulatorTool.AddDefaultManipulator` (`src/MigrationTools/Tools/StringManipulatorTool.cs:85`), its description, unit tests for it, and the tool's reference page and generated class data (via `GenerateDocs.ps1`).
- Out:
  - `System.Title` never reaches the tool: `TfsWorkItemMigrationProcessor.PopulateWorkItem` copies it directly (`TfsWorkItemMigrationProcessor.cs:767`), so the field loop sees source == target and skips `ProcessString`. The issue's Title example is therefore wrong; whether Title *should* be manipulated is a separate decision (Q-2).
  - The `Enabled` default: the tool is off unless configured (`ToolOptions.Enabled` is `false`), so the issue's "runs on every single-line String field by default" holds only when the tool is enabled with an empty `Manipulators` list. `configuration-default.json` and `configuration-classic.json` enable it but supply their own ASCII-only pattern `[^( -~)\n\r\t]+`, which strips all non-ASCII text; that is a different, larger problem (Q-3).
  - The "Removing Emojis for SOAP Compatibility" sample on the reference page uses the same surrogate pattern; changing it is in scope only if Martin agrees (Q-4).
  - HTML fields and other field types (only `FieldType.String` goes through the tool).

## Sources read
- Issue #3258: pattern `[\uD800-\uDBFF][\uDC00-\uDFFF]` removes every non-BMP character; proposes explicit emoji matching plus tests for `A𐓏B` (unchanged) and an emoji (removed). Docs were corrected to describe current behaviour in #3251.
- `src/MigrationTools/Tools/StringManipulatorTool.cs`: default manipulator is added only when `Manipulators` is null/empty; full pattern `[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F]|[\uD800-\uDBFF][\uDC00-\uDFFF]|️`; description claims it "preserves Unicode letters and symbols".
- `src/MigrationTools/Tools/StringManipulatorToolOptions.cs`, `Tools/Infrastructure/ToolOptions.cs`: `Enabled` defaults to false.
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs:756-815`: Title/State/Reason set directly; only `FieldType.String` fields that differ between source and target are passed to `ProcessString`.
- `docs/content/docs/reference/tools/stringmanipulatortool/index.md`: documents current behaviour and its limits (non-BMP letters removed, `⌚` kept).
- `configuration-default.json`, `configuration-classic.json`: enable the tool with an ASCII-only pattern.
- `src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs`: existing unit tests for the tool; place for new cases.
- `platform/specs/rules.md`, `platform/specs/architecture.md`, `.github/copilot-instructions.md`, `CLAUDE.md`, `AGENTS.md`: comment rules; tools sit in `MigrationTools` core; no REST dependency (none involved here).
- `docs/notes/martin-notes.md`: #3258 was raised from the #3251 docs reconciliation.

## Reproduction
On `main` at `56bab21f`.

1. Unit level (.NET `Regex.Replace` with the exact default pattern, run from PowerShell 7):

   | Input | Output on main | Expected |
   |---|---|---|
   | `A` U+104CF `B` (Osage) | `AB` | unchanged |
   | `A` U+20000 `B` (CJK Ext B) | `AB` | unchanged |
   | `A` U+231A `B` (⌚, BMP emoji) | unchanged | `AB` |
   | `A` U+1F600 `B` (😀) | `AB` | `AB` |
   | `A` U+2764 U+FE0F `B` (❤️) | `A❤B` (selector gone, heart kept) | `AB` |

2. Test orgs, scratch projects `amte2e-3258-src` / `amte2e-3258-tgt` (process `amte2e-agile`), built `MigrationTools.ConsoleFull`, `tools/e2e/Invoke-MigrationE2E.ps1` with the wicore config pointed at the scratch projects, attachments off:
   - Run A, tool not configured (default `Enabled=false`), Task title `A𐓏B 𠀀 W⌚ E😀` → target title identical; exit 0. The tool does not run.
   - Run B, `StringManipulatorTool: { Enabled: true, Manipulators: [] }`, same Task title → target title identical; exit 0. Title bypasses the tool (`PopulateWorkItem` line 767).
   - Run C, same config, Bug with `Microsoft.VSTS.Build.FoundIn` = `A𐓏B 𠀀 W⌚ E😀` → target `FoundIn` = `AB  W⌚ E` (U+0041 U+0042 U+0020 U+0020 U+0057 U+231A U+0020 U+0045); exit 0, 1 created, 0 failed. **Bug reproduced**: both letters lost silently, BMP emoji kept, non-BMP emoji removed.
   - Run D, what SOAP accepts. Same projects, `Manipulators` = one no-op pattern `(?!)` so the default is not added, `MaxGracefulFailures` 100, one Bug per character with `FoundIn` = `X<char>Y`:
     | Character | Seeded via REST | Saved through SOAP to the target |
     |---|---|---|
     | U+0001, U+000B, U+001F, U+007F, U+0085, U+009F | 400, could not seed | not tested |
     | U+FFFE | yes | **rejected**: WebException, retry limit, item failed, run exit 1 |
     | lone U+D800 | stored by the source as U+FFFD | saved (as U+FFFD) |
     | U+FE0F | yes | saved unchanged |
     | U+104CF (Osage) | yes | saved unchanged |
     | U+1F600 (😀) | yes | saved unchanged |
     | U+231A (⌚) | yes | saved unchanged |
     Runs A and B also saved `A𐓏B 𠀀 W⌚ E😀` through SOAP as the Title.
   - Cleanup: both scratch projects deleted from both orgs after each run; local scratch configs and logs removed.

## Cause
Martin's rule (2026-11-20): the default scrubs what the SOAP API does not support, and nothing more. The default fails that rule both ways. It removes characters SOAP saves fine: every non-BMP character (letters and emoji) and U+FE0F. It also misses a character SOAP rejects: U+FFFE fails the save, and the work item is not migrated.

The issue's framing ("remove emoji, keep letters") is superseded. Originally, the default pattern treated "a surrogate pair" as "an emoji". UTF-16 surrogate pairs encode every code point above U+FFFF, which includes thousands of letters (Osage, Adlam, CJK Extensions B–H, historic scripts, mathematical alphanumerics), not only emoji. Conversely, many emoji (U+231A, U+2600–U+27BF, U+2B50…) sit inside the BMP and are not surrogates, so they are kept. The description promises the opposite of both.

## Proposed fix
The default pattern is narrowed to the characters XML 1.0 (the SOAP payload) cannot carry, which is what the save rejects:

`[\x00-\x08\x0B\x0C\x0E-\x1F]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uFFFE\uFFFF]`

- That covers C0 controls except tab, LF and CR; lone surrogates; and U+FFFE/U+FFFF. Every valid surrogate pair (letters and emoji), U+FE0F and all BMP emoji are kept.
- `\x7F-\x9F` (DEL and C1) are legal in XML 1.0. Whether SOAP rejects them can't be tested on Azure DevOps Services, because REST will not seed them (Q-6).
- Replace `Description` with "Default: removes characters the SOAP API cannot save (XML-invalid control characters, lone surrogates, U+FFFE/U+FFFF)".
- Unit tests in `StringManipulatorEnricherTests.cs`:
  - Unchanged: `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café`, tab/LF/CR.
  - Removed: U+0001, U+000B, U+001F, U+FFFE, U+FFFF, lone high surrogate, lone low surrogate → `AB`.
  - A user-supplied manipulator still replaces the default.
- Docs: rewrite the default's description and limits on the reference page. Keep the "Removing Emojis" sample, but say it is optional and goes beyond what SOAP needs. Regenerate with `GenerateDocs.ps1`.
- Release note: anyone using the built-in default now keeps emoji and non-BMP letters, and items holding U+FFFE now migrate.

## Outcome checks

- Unit, on the build: the new default keeps `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café` and tab/LF/CR, and reduces U+0001, U+000B, U+001F, U+FFFE, U+FFFF and lone surrogates to `AB`; a user manipulator replaces the default; pass = exit 0 and every StringManipulator test passes: `dotnet test src/MigrationTools.Tests --filter "FullyQualifiedName~StringManipulator"`
- Whole suite, on the build and test orgs; pass = exit 0: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- End to end, test orgs, scratch `amte2e-3258-*`, tool enabled with `Manipulators: []`, Bug `FoundIn` = `A𐓏B 𠀀 W⌚ E😀`; pass = exit 0 and target `FoundIn` unchanged (main gives `AB  W⌚ E`): `pwsh -NoProfile -File tools/e2e/scenarios/stringmanip-3258/seed.ps1; ./tools/e2e/Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/stringmanip-3258/config.json; pwsh -NoProfile -File tools/e2e/scenarios/stringmanip-3258/assert.ps1`
- End to end failure path, same scenario, Bug `FoundIn` = `X`U+FFFE`Y`; pass = exit 0, 0 failed and target `FoundIn` = `XY` (main fails the item and exits 1): same command as above, asserted by `assert.ps1`
- User manipulator still wins, same scenario with a no-op manipulator configured; pass = exit 0 and target `FoundIn` keeps U+FE0F and the emoji: `./tools/e2e/Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/stringmanip-3258/config-user.json; pwsh -NoProfile -File tools/e2e/scenarios/stringmanip-3258/assert.ps1 -User`
- Docs match the code; pass = exit 0 and no diff after regeneration: `pwsh -NoProfile -File .specify/extensions/engineering-checks/scripts/docs-check.ps1`
- Clean-up; pass = no `amte2e-3258-*` project left in either org: `pwsh -NoProfile -File tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3258-` once #3278's field-prefix fix is on main, so it touches only this item's scratch objects (Q-7)
## Decisions for Martin
- Q-1 resolved by Martin (2026-11-20): scrub what SOAP does not support. Emoji are kept.
- Q-2 resolved by Martin (2026-11-21): fix it here. Was: `System.Title` is never passed to the tool, because `PopulateWorkItem` copies it directly. Under the SOAP rule a U+FFFE in a Title would still fail the save. Route Title through the tool here, or in a separate item? (for Martin) answered
- Q-3 resolved by Martin (2026-11-21): fix them here. Was: `configuration-default.json` and `configuration-classic.json` ship `[^( -~)\n\r\t]+`, which removes all non-ASCII text and far exceeds the SOAP rule. Change them here, or raise a separate issue? (for Martin) answered
- Q-4 resolved by the rule: keep the emoji sample as an opt-in, and reword it.
- Q-5 resolved by Martin (2026-11-23): bug fix in the next minor release, with a release note. Was: This changes migrated output for anyone on the built-in default. Release it as a bug fix? (for Martin) answered
- Q-6 resolved by Martin (2026-11-21): stop scrubbing them; the default removes only XML-invalid characters. Was: Should DEL and C1 (`\x7F-\x9F`) be scrubbed too? They are legal XML, and Azure DevOps Services REST would not seed them, so SOAP was not tested with them; an on-prem TFS source could hold them. Default proposal: keep scrubbing them, as main does now, to avoid a regression. (for Martin) openRun: 6ea6a085
Run: b8178864

## Spec

Working spec: `.specify/bugs/stringmanipulator-soap-scrub/assessment.md` (the bug assessment Martin approves).

Spec presented: .specify/bugs/stringmanipulator-soap-scrub/assessment.md sha256 c6af839f6accff3de3e24ab1b07b28add4f777bbd98d6b5b19c1bbe8dd8eb508

## Reproduction

On `main`, test orgs, scratch projects `amte2e-3258-*`, which were deleted afterwards. Full detail is in `.specify/wkr/3258.md`.

1. Enable `StringManipulatorTool` with `Manipulators: []`. Seed a Bug with `Microsoft.VSTS.Build.FoundIn` = `A𐓏B 𠀀 W⌚ E😀`. Migrate. The target has `AB  W⌚ E`, the run exits 0, and the loss is silent.
2. Replace the default with a no-op manipulator and seed one Bug per character. SOAP saved U+104CF, U+1F600, U+231A and U+FE0F unchanged. U+FFFE failed the save after the retry limit, and the run exited 1.
3. Unit level: applying the default pattern with .NET `Regex.Replace` turns `A𐓏B` and `A𠀀B` into `AB`, and leaves `A⌚B` and `A\uFFFEB` unchanged.

[NEEDS CLARIFICATION: whether SOAP rejects DEL and C1 (U+007F–U+009F). Azure DevOps Services REST refuses to seed them, so this can't be tested on the test orgs.]


## Cause

The default equates "surrogate pair" with "emoji". It doesn't target the characters that actually break a SOAP (XML 1.0) payload: C0 controls other than tab, LF and CR; lone surrogates; and U+FFFE/U+FFFF. So it over-scrubs valid text and under-scrubs the one class the test org proved fatal. Confidence: high.


## Proposed fix

**Preferred**: replace the default pattern with
`[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uFFFE\uFFFF]`.
This keeps DEL/C1 scrubbing as on `main` unless Martin decides otherwise (Q-6). Update `Description` to say it removes characters the SOAP API cannot save. The pattern stays a plain regex, so users can copy it into their own config.

**Alternatives**:
- Filter by rune in code instead of a regex. That is more exact, but it adds a code path and can't be copied into config.

**Files likely to change**:
- `src/MigrationTools/Tools/StringManipulatorTool.cs`
- `src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs`
- `docs/content/docs/reference/tools/stringmanipulatortool/index.md`
- `docs/data/classes/reference.tools.stringmanipulatortool.yaml` (generated by `GenerateDocs.ps1`)

**Tests to add or update**:
- Unchanged: `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café`, tab/LF/CR.
- Reduced to `AB`: U+0001, U+000B, U+001F, U+FFFE, U+FFFF, a lone high surrogate and a lone low surrogate.
- A user-supplied manipulator replaces the default.


## Questions

- Q-1 Q-2 route `System.Title` through the tool here or in a separate item? (for Martin) answered: fix it here
- Q-2 Q-3 change the shipped configs' ASCII-only pattern here, or in a separate issue? (for Martin) answered: fix them here
- Q-3 Q-5 release as a bug fix? (for Martin) answered: bug fix plus release note
- Q-4 Q-6 keep scrubbing DEL/C1? (for Martin) answered: stop scrubbing
- Q-5 whether SOAP rejects DEL and C1 (U+007F–U+009F). Azure DevOps Services REST refuses to seed them, so this can't be tested on the test orgs. (for Martin) answered: moot, DEL/C1 are no longer scrubbed
- Q-7 `Remove-E2EObjects.ps1 -Prefix amte2e-3258-` would derive field prefix `Custom.amte2e` and delete every e2e field. The clean-up uses Remove-E2EProject for the two scratch projects instead. Is that the clean-up you want? (for Martin) answered (2026-11-23): fix Remove-E2EObjects. Its field-prefix fix is covered by #3278, which owns tools/e2e/Remove-E2EObjects.ps1; #3258 does not edit that file and uses it once #3278 lands

Spec agreed: Approve (2026-11-21, review gate on .specify/bugs/stringmanipulator-soap-scrub/assessment.md); Title: Fix it here; Configs: Fix them here; DEL/C1: Stop scrubbing
Spec presented: .specify/bugs/stringmanipulator-soap-scrub/assessment.md sha256 ae708d85da7436fcb4fd77a7f5c733bfb55f2d945f6b5a9d6bc4cf22bde4e313
Spec agreed: Spec agreed, fix it as assessed
Spec version: .specify/bugs/stringmanipulator-soap-scrub/assessment.md sha256 ae708d85da7436fcb4fd77a7f5c733bfb55f2d945f6b5a9d6bc4cf22bde4e313

## Spec

Working spec: `specs/3258-stringmanipulatortool-default-pattern-re/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3258-stringmanipulatortool-default-pattern-re/assessment.md sha256 4b6a25b212ecee58c6db7c867c087c1dc70e702e59200481d1f321019b31899d

## Reproduction

On `main`, test orgs, scratch projects `amte2e-3258-*`, which were deleted afterwards. Full detail is in `.specify/wkr/3258.md`.

1. Enable `StringManipulatorTool` with `Manipulators: []`. Seed a Bug with `Microsoft.VSTS.Build.FoundIn` = `A𐓏B 𠀀 W⌚ E😀`. Migrate. The target has `AB  W⌚ E`, the run exits 0, and the loss is silent.
2. Replace the default with a no-op manipulator and seed one Bug per character. SOAP saved U+104CF, U+1F600, U+231A and U+FE0F unchanged. U+FFFE failed the save after the retry limit, and the run exited 1.
3. Unit level: applying the default pattern with .NET `Regex.Replace` turns `A𐓏B` and `A𠀀B` into `AB`, and leaves `A⌚B` and `A\uFFFEB` unchanged.

DEL and C1 (U+007F–U+009F) couldn't be tested, because Azure DevOps Services REST refuses to seed them. Martin decided not to scrub them.


## Cause

The default equates "surrogate pair" with "emoji". It doesn't target the characters that actually break a SOAP (XML 1.0) payload: C0 controls other than tab, LF and CR; lone surrogates; and U+FFFE/U+FFFF. So it over-scrubs valid text and under-scrubs the one class the test org proved fatal. Confidence: high.


## Proposed fix

**Preferred**: replace the default pattern with
`[\x00-\x08\x0B\x0C\x0E-\x1F]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uFFFE\uFFFF]`.
DEL and C1 are no longer scrubbed (Martin, Q-6). Update `Description` to say it removes characters the SOAP API cannot save. The pattern stays a plain regex, so users can copy it into their own config.

**Alternatives**:
- Filter by rune in code instead of a regex. That is more exact, but it adds a code path and can't be copied into config.

Martin also decided (2026-11-21): route `System.Title` through the tool (Q-2), and remove the ASCII-only manipulator from the shipped configs so the new default applies (Q-3).

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`
- `configuration-default.json`, `configuration-classic.json`
- `tools/e2e/scenarios/stringmanip-3258/**`
- `src/MigrationTools/Tools/StringManipulatorTool.cs`
- `src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs`
- `docs/content/docs/reference/tools/stringmanipulatortool/index.md`
- `docs/data/classes/reference.tools.stringmanipulatortool.yaml` (generated by `GenerateDocs.ps1`)

**Tests to add or update**:
- Unchanged: `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café`, tab/LF/CR.
- Reduced to `AB`: U+0001, U+000B, U+001F, U+FFFE, U+FFFF, a lone high surrogate and a lone low surrogate.
- A user-supplied manipulator replaces the default.


## Spec

Working spec: `specs/3258-stringmanipulatortool-default-pattern-re/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3258-stringmanipulatortool-default-pattern-re/assessment.md sha256 763b8a1d2974a635ae5e9824874c35d4b5907ed348af852e1a1b5ace019735dd

## Reproduction

On `main`, test orgs, scratch projects `amte2e-3258-*`, which were deleted afterwards. Full detail is in `item.md` in this folder.

1. Enable `StringManipulatorTool` with `Manipulators: []`. Seed a Bug with `Microsoft.VSTS.Build.FoundIn` = `A𐓏B 𠀀 W⌚ E😀`. Migrate. The target has `AB  W⌚ E`, the run exits 0, and the loss is silent.
2. Replace the default with a no-op manipulator and seed one Bug per character. SOAP saved U+104CF, U+1F600, U+231A and U+FE0F unchanged. U+FFFE failed the save after the retry limit, and the run exited 1.
3. Unit level: applying the default pattern with .NET `Regex.Replace` turns `A𐓏B` and `A𠀀B` into `AB`, and leaves `A⌚B` and `A\uFFFEB` unchanged.

DEL and C1 (U+007F–U+009F) couldn't be tested, because Azure DevOps Services REST refuses to seed them. Martin decided not to scrub them.


## Cause

The default equates "surrogate pair" with "emoji". It doesn't target the characters that actually break a SOAP (XML 1.0) payload: C0 controls other than tab, LF and CR; lone surrogates; and U+FFFE/U+FFFF. So it over-scrubs valid text and under-scrubs the one class the test org proved fatal. Confidence: high.


## Proposed fix

**Preferred**: replace the default pattern with
`[\x00-\x08\x0B\x0C\x0E-\x1F]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uFFFE\uFFFF]`.
DEL and C1 are no longer scrubbed (Martin, Q-6). Update `Description` to say it removes characters the SOAP API cannot save. The pattern stays a plain regex, so users can copy it into their own config.

**Alternatives**:
- Filter by rune in code instead of a regex. That is more exact, but it adds a code path and can't be copied into config.

Martin also decided (2026-11-21): route `System.Title` through the tool (Q-2), and remove the ASCII-only manipulator from the shipped configs so the new default applies (Q-3).

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`
- `configuration-default.json`, `configuration-classic.json`
- `tools/e2e/scenarios/stringmanip-3258/**`
- `src/MigrationTools/Tools/StringManipulatorTool.cs`
- `src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs`
- `docs/content/docs/reference/tools/stringmanipulatortool/index.md`
- `docs/data/classes/reference.tools.stringmanipulatortool.yaml` (generated by `GenerateDocs.ps1`)

**Tests to add or update**:
- Unchanged: `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café`, tab/LF/CR.
- Reduced to `AB`: U+0001, U+000B, U+001F, U+FFFE, U+FFFF, a lone high surrogate and a lone low surrogate.
- A user-supplied manipulator replaces the default.


## Spec

Working spec: `specs/3258-stringmanipulatortool-default-pattern-re/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3258-stringmanipulatortool-default-pattern-re/assessment.md sha256 763b8a1d2974a635ae5e9824874c35d4b5907ed348af852e1a1b5ace019735dd

## Reproduction

On `main`, test orgs, scratch projects `amte2e-3258-*`, which were deleted afterwards. Full detail is in `item.md` in this folder.

1. Enable `StringManipulatorTool` with `Manipulators: []`. Seed a Bug with `Microsoft.VSTS.Build.FoundIn` = `A𐓏B 𠀀 W⌚ E😀`. Migrate. The target has `AB  W⌚ E`, the run exits 0, and the loss is silent.
2. Replace the default with a no-op manipulator and seed one Bug per character. SOAP saved U+104CF, U+1F600, U+231A and U+FE0F unchanged. U+FFFE failed the save after the retry limit, and the run exited 1.
3. Unit level: applying the default pattern with .NET `Regex.Replace` turns `A𐓏B` and `A𠀀B` into `AB`, and leaves `A⌚B` and `A\uFFFEB` unchanged.

DEL and C1 (U+007F–U+009F) couldn't be tested, because Azure DevOps Services REST refuses to seed them. Martin decided not to scrub them.


## Cause

The default equates "surrogate pair" with "emoji". It doesn't target the characters that actually break a SOAP (XML 1.0) payload: C0 controls other than tab, LF and CR; lone surrogates; and U+FFFE/U+FFFF. So it over-scrubs valid text and under-scrubs the one class the test org proved fatal. Confidence: high.


## Proposed fix

**Preferred**: replace the default pattern with
`[\x00-\x08\x0B\x0C\x0E-\x1F]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uFFFE\uFFFF]`.
DEL and C1 are no longer scrubbed (Martin, Q-6). Update `Description` to say it removes characters the SOAP API cannot save. The pattern stays a plain regex, so users can copy it into their own config.

**Alternatives**:
- Filter by rune in code instead of a regex. That is more exact, but it adds a code path and can't be copied into config.

Martin also decided (2026-11-21): route `System.Title` through the tool (Q-2), and remove the ASCII-only manipulator from the shipped configs so the new default applies (Q-3).

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`
- `configuration-default.json`, `configuration-classic.json`
- `tools/e2e/scenarios/stringmanip-3258/**`
- `src/MigrationTools/Tools/StringManipulatorTool.cs`
- `src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs`
- `docs/content/docs/reference/tools/stringmanipulatortool/index.md`
- `docs/data/classes/reference.tools.stringmanipulatortool.yaml` (generated by `GenerateDocs.ps1`)

**Tests to add or update**:
- Unchanged: `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café`, tab/LF/CR.
- Reduced to `AB`: U+0001, U+000B, U+001F, U+FFFE, U+FFFF, a lone high surrogate and a lone low surrogate.
- A user-supplied manipulator replaces the default.


## Spec

Working spec: `specs/3258-stringmanipulatortool-default-pattern-re/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3258-stringmanipulatortool-default-pattern-re/assessment.md sha256 49664a93f47090866dddb6248c44ec5b615f48272cf1c5f88930e0971a42f709

## Reproduction

On `main`, test orgs, scratch projects `amte2e-3258-*`, which were deleted afterwards. Full detail is in `.specify/wkr/3258.md`.

1. Enable `StringManipulatorTool` with `Manipulators: []`. Seed a Bug with `Microsoft.VSTS.Build.FoundIn` = `A𐓏B 𠀀 W⌚ E😀`. Migrate. The target has `AB  W⌚ E`, the run exits 0, and the loss is silent.
2. Replace the default with a no-op manipulator and seed one Bug per character. SOAP saved U+104CF, U+1F600, U+231A and U+FE0F unchanged. U+FFFE failed the save after the retry limit, and the run exited 1.
3. Unit level: applying the default pattern with .NET `Regex.Replace` turns `A𐓏B` and `A𠀀B` into `AB`, and leaves `A⌚B` and `A\uFFFEB` unchanged.

DEL and C1 (U+007F–U+009F) couldn't be tested, because Azure DevOps Services REST refuses to seed them. Martin decided not to scrub them.


## Cause

The default equates "surrogate pair" with "emoji". It doesn't target the characters that actually break a SOAP (XML 1.0) payload: C0 controls other than tab, LF and CR; lone surrogates; and U+FFFE/U+FFFF. So it over-scrubs valid text and under-scrubs the one class the test org proved fatal. Confidence: high.


## Proposed fix

**Preferred**: replace the default pattern with
`[\x00-\x08\x0B\x0C\x0E-\x1F]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uFFFE\uFFFF]`.
DEL and C1 are no longer scrubbed (Martin, Q-6). Update `Description` to say it removes characters the SOAP API cannot save. The pattern stays a plain regex, so users can copy it into their own config.

**Alternatives**:
- Filter by rune in code instead of a regex. That is more exact, but it adds a code path and can't be copied into config.

Martin also decided (2026-11-21): route `System.Title` through the tool (Q-2), and remove the ASCII-only manipulator from the shipped configs so the new default applies (Q-3).

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`
- `configuration-default.json`, `configuration-classic.json`
- `tools/e2e/scenarios/stringmanip-3258/**`
- `src/MigrationTools/Tools/StringManipulatorTool.cs`
- `src/MigrationTools.Tests/ProcessorEnrichers/StringManipulatorEnricherTests.cs`
- `docs/content/docs/reference/tools/stringmanipulatortool/index.md`
- `docs/data/classes/reference.tools.stringmanipulatortool.yaml` (generated by `GenerateDocs.ps1`)

**Tests to add or update**:
- Unchanged: `A𐓏B`, `A𠀀B`, `A😀B`, `A⌚B`, `A❤️B`, `café`, tab/LF/CR.
- Reduced to `AB`: U+0001, U+000B, U+001F, U+FFFE, U+FFFF, a lone high surrogate and a lone low surrogate.
- A user-supplied manipulator replaces the default.

Spec fix: specs/3258-stringmanipulatortool-default-pattern-re/assessment.md sha256 ae708d85da7436fcb4fd77a7f5c733bfb55f2d945f6b5a9d6bc4cf22bde4e313 -> a7a69f040271a9c10880921ea10b385b9aaa2b1ede257bbbf790b1f9551b3817: wrote in Martin's answers to Q-5 and Q-7; corrected pattern escapes and an old path

## Docs updated
- core: `docs/content/docs/reference/tools/stringmanipulatortool/index.md`: "Remove only the characters the SOAP (TFS object model) API cannot save"
- core: `docs/data/classes/reference.tools.stringmanipulatortool.yaml` (generated by `GenerateDocs.ps1` from `appsettings.json`): "Remove characters the SOAP API cannot save"

`appsettings.json` added to paths: it holds the documented defaults `GenerateDocs.ps1` reads, so the generated page showed the old pattern until it changed.

## Validation (test orgs, 2026-11-23)
- Unit: `dotnet test src/MigrationTools.Tests --filter "FullyQualifiedName~StringManipulator"`: 46 passed, 0 failed.
- Branch, `config.json` (default): exit 0. Title and Found In kept `A𐓏B 𠀀 W⌚ E😀`; the U+FFFE Bug migrated with Found In `AB`.
- Branch, `config-user.json` (own manipulator): exit 1. The manipulator ran on Title and Found In; the U+FFFE Bug failed to save, as the default no longer applies.
- main, `config.json`: exit 1. Found In became `AB  W⌚ E`; the U+FFFE Bug failed to save.