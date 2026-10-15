# End-to-end regression pass: October 2026

**Code:** `main` at `3e93cfd5`. This includes about 30 PRs merged on 2026-10-11/12/13, ending with #3221 (Entra ID, new test orgs).
**Environment:** `nkdagility-adomt-source` → `nkdagility-adomt-target`, `MigrationTools.ConsoleFull` Debug build, run through `tools/e2e/Invoke-MigrationE2E.ps1` with Microsoft Entra ID (`az login`). Every scenario used its own freshly created `amte2e-<scenario>-src`/`-tgt` projects and, where needed, its own inherited processes. Nothing in `migrationSource1`, `migrationTarget1` or any other existing project was touched.
**Date:** 2026-10-13.

## Result

| | |
|---|---|
| Unit tests (`dotnet test MigrationTools.sln`) | **Pass**: 473 passed, 0 failed, 4 skipped (Entra, new orgs) |
| E2E scenarios | 12 of 14 areas pass; **users** and **process definitions** fail |
| Issues filed | [#3223](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3223), [#3224](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3224), [#3225](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3225), [#3226](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3226). Existing [#3211](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3211) reproduced. |
| Harness runs | 40 with Entra ID; 2 failed with a transient 401 and passed on re-run (#3225) |

### Issues filed

| Issue | Area | Severity | Regression from the Oct PRs? |
|---|---|---|---|
| [#3223](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3223) `IdentityFieldsToCheck` is index-merged with the shipped defaults, so unlisted identity fields (ResolvedBy, ClosedBy, …) are still mapped | user mapping (#3187, #3195/#3206) | High: identities silently changed | No. A config-binding problem that makes #3187's exact matching impossible to configure. |
| [#3224](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3224) `ProcessDefinitionProcessor` POSTs the inherited "Stories" behaviour as new, the service rejects it, then `ArgumentNullException` aborts the sync | process definitions | High: the processor can't sync a customised inherited process | No. Same failure as the closed #3086; still reproduces. |
| [#3225](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3225) EntraId: intermittent `TF30063 401` at the first connection (2/40 runs) | auth (#3221) | Medium: a run fails, re-run passes | New code path (#3221) |
| [#3226](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3226) `TfsChangeSetMappingTool` samples and defaults use `File`, but the option is `ChangeSetMappingFile` | config/docs | Medium: setting silently ignored | No |
| [#3211](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3211) (existing) VS403115 on a locked system type | process definitions | | Reproduced on the second procdef run |

## Scenarios

Each row lists what the scenario checks, the result, and the evidence: exit codes and log lines are quoted from the run, and target state was checked over REST. Wherever a run failed because of my own setup, the row says so and the result shown is after the setup was fixed.

### Work items

| Scenario | PRs | Result | Evidence |
|---|---|---|---|
| **wicore**: full history, links, attachments, area/iteration | regression, #3196, #3180, #3205 | **PASS** | Exit 0, 5/5 "Saved as". `assert.ps1`: every source revision matched (date, ChangedBy, state). The story has 6 source revisions → 9 target, of which 3 are same-date fix-up saves (Activated/Resolved/ClosedDate), each logged `[WRN] ... applying fix-up save`. No "Migration" revisions. Nothing dated after the source history. The attachment's `authorizedDate` resolves to the target revision dated as source rev 3. Parent/child and related links all point at the target org (8 link ends). Areas `Team A`, `Team A\Sub`, sprints and sprint dates created. Closed/Resolved/Activated dates equal the source. |
| **rerun**: idempotency and resume | regression | **PASS** | Same config re-run: exit 0, 5× "Skipping as work item exists and no revisions to sync detected", revision counts unchanged, 5 items. After adding one source revision: only that item replayed ("Saved as 7"); title, state and ResolvedDate match. |
| **errors**: continue past item failures, exit code | #3181 | **PASS** | Bug mapped to a type disabled in the target (VS403417). `StopOnFirstError: true`: task 15 saved, bug 16 failed, tasks 17/18 not attempted, exit 1. Default: `Continuing with the remaining work items (1 failed so far)...`, 17/18 saved, `1 work item(s) could not be migrated: 16`, `The migration run failed ... Exiting with code 1.` |
| **typechange**: first type disabled in the target | #3179 | **PASS** | Exit 0. `[WRN] Source work item 19 changed type from 'Bug' to 'User Story'. Target type 'Bug' does not exist or is disabled in the target project, so the work item will be created as 'User Story' instead.` Target is a User Story; no VS403417. |
| **fields**: history-only fields, validator, reflected id | #3186, #3202, #3151, #3209 | **PASS** | History-only field reproduced by moving an item from a project whose process has `Custom.amte2eLegacy`. `StopIfHistoryOnlyFieldsFound: true`: `Missing target field: Custom.amte2eLegacy ... 2 revision(s) across 1 work item(s) [21]`, then `History-only fields were found and 'StopIfHistoryOnlyFieldsFound' is true, so the migration will stop now.`, exit 1, nothing saved. `false`: same warning, 3 saved, end-of-run summary lists the field, exit 0. #3151 control without a map: `Missing field 'Custom.amte2eIssueOnly' in 'Task'` and validation fails. With `ApplyTo ["Issue"]` (source WIT): `Source field ... is mapped`, and the value lands in the target. #3209: a single-revision Task has a TargetToTarget copy of `Custom.ReflectedWorkItemId` at rev 1; re-run creates no duplicate. |
| **identity**: identity fields, dates, "set to current user" rule | #3190, #3196, #3180, #3205, #3187 (nulls) | **PASS** | Target process rule `whenChanged System.State → copyFromCurrentUser System.AssignedTo`. Source used an unresolved identity "Ghost Person <ghost.person@example.invalid>", because the orgs have one real user. Exit 0, no TF26194. Per-revision assert, fix-ups collapsed at ≤1 ms: AssignedTo/ResolvedBy/ClosedBy keep the ghost on all 8 revisions, the rule never replaced it, ClosedDate 2026-09-15 kept. A null AssignedTo stays null. Reactivation (Active→Resolved→Active): ActivatedDate equal at every revision. |
| **users**: mapping formats, exact fields, @mentions | #3165, #3187, #3201 | **FAIL** → [#3223](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3223) | Dictionary and IdentityMapData (`Detected format is IdentityMapData`) both map the ghost AssignedTo to the real user. Null passes through. Source items unchanged. **Fail:** ResolvedBy is mapped although it's not in `IdentityFieldsToCheck`. Controls: an empty mapping file keeps the ghost; a second unmapped ghost is kept; a list padded to 6 entries keeps the ghost. #3201: inconclusive here, because the mention kept the same identity id, which is the same in both orgs, and no mention log line appeared. |

### Links, images, git

| Scenario | PRs | Result | Evidence |
|---|---|---|---|
| **git**: commit links | #3185, #3204, #3198, #3070 | **PASS** | The same commits were pushed to both orgs. C1 rewritten to the target repo. S1 in another project's repo was resolved by name to the target's same-named repo (#3185). C2, missing from the target, was left on the source: `... not found in target repo amte2e-git-tgt/amte2e-git-tgt; leaving link ... pointing at the source.` plus a summary (#3204). Malformed legacy link: `Invalid Git external link format (legacy) ... The link will be skipped.` (#3198). New format `projectName/repoName/commit` rewritten to the target (#3070). Exit 0. |
| **images**: embedded images | #3200, #3203 | **PASS** | Exit 0. `Placeholder work item 22 created ...`, then `Destroyed placeholder work item 22 in the target.` No "DELETE THIS" items; #22 is not in the recycle bin. `?fileName=b.png?fileName=b.png` becomes `...?fileName=b.png`. Both target images download as valid PNGs from the target org. |

### Other processors

| Scenario | PRs | Result | Evidence |
|---|---|---|---|
| **queries + teams** | #3219; regression: shared queries, team settings, capacity | **PASS** | Exit 0. `Queries Found:3 Skipped:0 Migrated:3 Failed:0`. `@CurrentIteration('[src]\Squad A <id>') + 1` becomes `@CurrentIteration('[amte2e-qt-tgt]\Squad A') + 1`, and `@TeamAreas` becomes the target team. All three queries run in the target. Squad A: sprints subscribed, team area with children. Capacity (Development 4h/day, day off) identical. |
| **testplans**: four-step guide | regression, #3209 guide | **PASS** | All 4 steps exit 0. Plan with root, requirement suite (target US 49, case 51) and "Suite A" [51, 56]. A Test Case `compref` now points at the migrated Shared Steps (#50). TestedBy link migrated. Re-running step 4 adds new cases without duplicating the plan. |
| **pipelines** | #3207, #3220; regression: build/release, variable groups | **PASS** | `MigrateVariableGroups: false`: `1 of 1 source TaskGroup(s) are going to be migrated (0 already exist in the target, 2 version(s) in total)`; build and release `were skipped ... marking the run as failed`, exit 1 (#3220). Target task group has v1 and v2 under one id (#3207). All on: variable group, release and build migrated; build points at the target repo, variable group and target task group 2.*; exit 0. Re-run: `0 of 1` for everything, exit 0. The "rejected by the target" path also hit (TF401019, see observations): `0 of 1 ... skipped and 1 were rejected ... marking the run as failed`, exit 1. |
| **procdef**: process definitions | #3208; regression | **FAIL** → [#3224](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3224), [#3211](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3211) | #3208 partly verified: `Created target WorkItemField entry for [amte2eTicket#]`. Then `Error migrating WorkItemBehavior: Stories`, the service says `You must provide a value for the behavior parameter.`, then `ArgumentNullException (key)`, exit 1. Re-run: VS403115 on the locked User Story, exit 1. The WIT-level `#` field step was never reached. |
| **config**: upgrade and docs generation | #3199, #3197 | **PASS** | `upgrade` on the shipped classic config and on a v16 config with lists: exit 0, no duplicated list entries, no null properties, no `ChangeSetMappingFile: null`, lists kept as written. ConsoleDataGenerator: exit 0, no changes to `docs` (in sync), `parsingFallback` default `RaiseError`. Found [#3226](https://github.com/nkdAgility/azure-devops-migration-tools/issues/3226) along the way. |

### Unit tests

| Run | Result |
|---|---|
| `34e69928` (before #3221) | 463 passed, 0 failed, 4 skipped |
| `3e93cfd5` with the machine's stale `AZDEVOPS_SYSTEM_TEST_PAT` set | 11 failed, all live tests `TF30063 401` against `nkdagility-preview`: the old PAT is used as the alias token and the default org. Environmental; #3221 asks for that variable to be cleared. |
| `3e93cfd5` with `MigrationTools__Tests__UseEntraId=true`, new orgs, alias cleared for the process | **473 passed, 0 failed, 4 skipped** |

## Observations (not filed)
- **Pipelines:** a build definition with no `repository.properties` (for example one created through the REST API) skips repository mapping (`AzureDevOpsPipelineProcessor.cs:249`) and is rejected by the target with TF401019, with no warning. Designer-created definitions have properties.
- **Pipelines:** `BuildPipelines: null` / `ReleasePipelines: null` (the processor's own defaults) fail schema validation, and the run exits -1.
- **Test plans:** a Test Case whose steps contain a `compref` but no "Shared Steps" link keeps the source id. In the target that id belonged to an unrelated User Story in another project, and nothing was logged.
- **Fields:** `[WRN] PopulateWorkItem:FieldUpdate: Missing field in target workitem ... Custom.amte2eIssueOnly` is logged even though a field map migrated the value. The end-of-run summary gets it right.
- **Identity:** the `copies the current user into System.AssignedTo ... migrated revisions will show the migration account` warning fired, but no revision was affected in this run.
- **Teams:** the source default team is created in the target as a separate team named after the source project, not mapped to the target default team.
- **Git:** the malformed-link warning is logged 4 times per item. Enrichment warnings are re-logged for items skipped on re-run. A `/`-format link that uses ids lands in a generic `unknown repo id (none)` summary that doesn't name the link.
- **Docs:** the endpoint reference sample still contains a token-shaped placeholder (`jklsadhjks…`), and the plans-and-suites guide still points at `nkdagility-preview`.

## Not tested, and why
- **#3184** (email domain change) and **#3195/#3206** (duplicate display names): each test org has one real user, and creating accounts is out of scope. Unit tests cover these paths (`TfsUserEmailMatcher`, `TfsUserMappingToolTests`).
- **#3201** @mentions: inconclusive, because the only user has the same identity id in both orgs (see users).
- Long-running Entra token refresh (>1 h): all runs were under 2 minutes.
- **Before/after runs on the pre-PR code:** builds before #3221 can only authenticate with a PAT, and none is available any more, so each scenario ran on `main` only. Unit tests ran on both commits.

## Setup mistakes found and corrected (for whoever re-runs this)
- The target process needs `Custom.ReflectedWorkItemId` on Test Plan and Test Suite too; the validator says so clearly.
- `FixHtmlAttachmentLinks: true` is required for `TfsEmbededImagesTool`.
- `TfsTeamSettingsProcessor` needs `TfsTeamSettingsEndpoint` endpoints, and `TfsSharedQueryProcessor` needs `TfsEndpoint`.
- A team created through REST has no backlog iteration, and every team-settings write fails with TF400497 until one is set.
- The new git link format is `projectName/repoName/commitId`, by name.
- Shared Steps references need the "Shared Steps" link next to the `compref`.
- PowerShell: `$S`/`$s` are the same variable, and returning `byte[]` through an `if` expression unrolls it.

## Left in the organisations
Nothing live. All `amte2e-*` projects (32 in the new orgs plus 2 in `nkdagility-preview`), 9 inherited processes (8 in the new orgs, 1 in preview) and 6 `Custom.amte2e*` fields were deleted (`tools/e2e/Remove-E2EObjects.ps1`). The 34 deleted projects remain **in the orgs' recycle bins** until Azure DevOps purges them; hard delete isn't allowed on Azure DevOps Services (`VS800076`):
- source: amte2e-errors-src, -fields-old, -fields-src, -git-other, -git-src, -identity-src, -images-src, -images2-src, -pipe-src, -procdef-src, -qt-src, -tp-src, -typechange-src, -users-src, -wicore-src
- target: amte2e-errors-tgt, -fields-tgt, -git-tgt, -identity-tgt, -images-tgt, -images2-tgt, -pipe-tgt, -procdef-tgt, -qt-tgt, -tp-tgt, -typechange-tgt, -users-tgt, -users2-tgt … -users5-tgt, -wicore-tgt
- nkdagility-preview: amte2e-wicore-src, amte2e-wicore-tgt (from the first attempt, before the move to the new orgs)

Not created by this pass, but noticed in `nkdagility-preview`: inherited processes `amt3186Base`, `amt3186Tgt2`, `amt3186Hist` and `amt3187Agile`, apparently left by earlier PR validations.

## Re-running
```powershell
az login                                                   # an identity with Project Collection Administrator in both test orgs
dotnet build MigrationTools.sln
./tools/e2e/Initialize-E2EEnvironment.ps1                  # creates the shared amte2e-agile process in both orgs (needed again after cleanup)
./tools/e2e/scenarios/wicore/seed.ps1                      # every scenario has a seed.ps1
./tools/e2e/Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/wicore/config.json
./tools/e2e/scenarios/wicore/assert.ps1                    # exit code = number of failed checks
./tools/e2e/Remove-E2EObjects.ps1                          # removes every amte2e-* project/process and Custom.amte2e* field; lists leftovers
```
- **Executable assertions exist only for `wicore` and `identity`.** For the other scenarios, the evidence in this report came from log lines and REST checks run by hand during the pass. Re-running those scenarios repeats the migration, but checking the target state is still manual. Adding an `assert.ps1` for each is a follow-up.
- `users`: run `scenarios/users/New-UsersConfigs.ps1` after seeding. `TfsUserMappingTool` needs an absolute path to the mapping file, so the configs are generated for the current checkout and are not committed.
- `procdef`: `seed.ps1` creates the `#` field, the customised `amte2e-procdef` process and the projects. The target org must not already have that process.
- The helpers sign in like the harness (`az` Entra token, or `AMT_TEST_PAT`). They only call the four Azure DevOps hosts for the test orgs, check a work item's project before updating it by id, and refuse to create or delete anything outside the `amte2e` prefix. Seeded ids are stored in git-ignored `state.local.json` files.