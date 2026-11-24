# Research: scratch project pool (#3342)

Facts marked "live" are in `item.md` or `spec.md` (checked on the target org, 2026-11-24). Items marked **verify at build** are small unknowns the first live run settles; each has a fallback so none blocks the design.

## R1 Claim: lease team

- **Decision**: The claim is creating the team `amte2e-pool-lease` in the project (`POST _apis/projects/{id}/teams`). A duplicate name is refused, so exactly one taker wins (live). The team description holds `claimant=<machine>|<pid>|<run>;expires=<UTC ISO 8601>`.
- **Rationale**: Project properties have no compare-and-set (`test` op refused, `VS850022`). Team name uniqueness is enforced by the service.
- **Alternatives**: Project property (no CAS, rejected). A file or queue in the repo (the clearing deletes repos). A lock held on the local machine only (the existing queue; does not cross machines).

## R2 Takeover and renewal

- **Decision**: Expiry is 2 hours after the claim and is renewed on every take, release and between scenarios by updating the team description. A taker may take over a lease when (a) `expires` is in the past, or (b) the claimant machine is this machine and its pid is not running. Takeover deletes the lease team by id only if its description still equals the one just read, then creates its own lease team; a failed create means another taker won, so the next project is tried. Before each destructive step (clear, process switch, rename) the holder re-reads the lease and stops if its token is not there.
- **Rationale**: There is no compare-and-delete in the API, so the re-read narrows the race to a window of one REST call, and the create is still the atomic decision. The worst case (two holders believing they own a project) needs an expired lease read, a renewal by the old holder and a delete, within one call, which the destructive-step re-read then also catches.
- **The pid in the token**: seeds and asserts run as child `pwsh` processes of the proof, which exit before the run does. The token's pid is therefore the owner pid from `$env:AMTE2E_POOL_OWNER` (`<pid>:<run>`), which `proof.ps1` sets to its own pid. Without it (a seed run by hand) the token has no pid and only the expiry applies, so a hand run is never taken over as "dead" while it is still in use.
- **Clock**: Expiry is written by the claimant in UTC. Machines keep time by NTP and the 2-hour window is large against skew.
- **Alternatives**: 10-minute leases with a heartbeat (needs a background job per run; rejected as more moving parts). No expiry (a crash would hold the project forever).

## R3 Clearing

- **Decision**: Destroy, not just delete, every item in the FR-004 list, in this order, one project at a time:
  1. Work items: WIQL `SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project` in pages (the query returns at most 20,000), `DELETE wit/workitems/{id}`, then `DELETE wit/recyclebin/{id}` (destroy). Batch with `wit/$batch` of 200.
  2. Test plans (and so their suites, points and runs), shared steps are work items so are covered in 1.
  3. Build definitions, release definitions (`vsrm.dev.azure.com`), variable groups, service connections, environments, wikis, and dashboards except the default.
  4. Queries: delete all folders and queries under Shared Queries, keeping the two root folders.
  5. Teams: delete every team except the default team and `amte2e-pool-lease`.
  6. Area and iteration child nodes of the root (`DELETE wit/classificationnodes/{group}/{path}`, reclassifying to the root).
  7. Work item tag definitions (`DELETE wit/tags/{id}`).
  8. Repos: create one new empty repo named after the project, delete the other repos, then purge them from the git recycle bin (`DELETE git/recycleBin/repositories/{id}`). Refs and pull requests go with the repos.
- **Rationale**: Deleted work items keep their ids and rows and later queries trip on them. Replacing repos is quicker and more certain than force-pushing refs and abandoning pull requests, and a repo with no commits is what a fresh project has.
- **Verification**: FR-004a. `Get-E2EPoolInventory` reads the same surfaces by REST and `Test-E2EPoolClear` compares the counts with `{ workItems = 0; teams = 2 (default, lease); childNodes = 0; queries = 0; repos = 1 (empty); pipelines = 0; ... }`. The inventory lists surfaces from REST, not from the clearing function's own list, so a surface the clearing misses shows up as a failure in the Cycle test.
- **Verify at build**: the harness identity may lack permission to destroy work items or purge repos. If so, the take fails visibly naming the permission (spec edge case), and the spec changes.
- **Alternatives**: Delete-only work items (ids and recycle bin entries stay; rejected in item Q-4). Delete and recreate the project (the thing being removed).

## R4 Process switch

- **Decision**: `POST {project}/_apis/wit/projectprocessmigration?api-version=7.1-preview.1` with `{ typeId }`, polled with `Wait-E2EOperation`. Confirm with `System.ProcessTemplateType` and by checking that the work item types the seed uses exist (`wit/workitemtypes`). Done after clearing, so there are no work items to map. Works between processes with the same base (live); all 60 `amte2e*` processes derive from Agile.
- **Rationale**: One pool of projects serves every process, which one pool per process could not (more projects than exist).
- **Rule on refusal**: a refused switch fails the take, releases the lease, and marks that scenario not poolable (SC-002 allows 3).
- **Verify at build**: processes a seed creates for itself (`amte2e-fields-old`, `amte2e-3297-*proc`) must exist before the take. In those seeds the process is created before the project today, so the order holds; the seeds are checked one by one.

## R5 Rename

- **Decision**: `PATCH _apis/projects/{id}` with `{ name }`, polled. The default team and the default repo follow (live). Take renames to the seed's name, release renames to `amte2e-pool-NN`. If the name already exists as a project, the take fails visibly and releases its lease.
- **Same name asked twice in one run** (seeds call `New-E2EProject` for a name that a prior step made): if a project with that name exists and its lease token is this run's owner, return it unchanged (spec scenario 3). Any other project with that name is an error, not a project to adopt.
- **Verify at build**: whether a repo the harness creates in clearing (R3 step 8) follows a rename the way the original default repo does. Fallback: create the new repo after the rename, in the take, named after the requested project. The Cycle test asserts `repos == [project name]`.

## R6 Project names the proof asks for

- **Decision**: The proof keeps per-run names internally (`amte2e-<item>-p<run>-<scenario>-<param>`), because two proofs on two machines running the same scenario would otherwise ask for one name and the second rename would collide. The names exist only while leased and are never printed as projects "left for clean-up". The proof output lists the pool slots it used (`amte2e-pool-NN`) and that each was released. SC-006's "no per-run project name" is met by the output; the names are only the rename target.
- **Alternatives**: Fixed names per scenario (a collision across machines fails the take, with no benefit).

## R7 Release

- **Decision**: Clear, switch to `amte2e-agile`, rename to `amte2e-pool-NN`, delete the lease team (last, so the project stays claimed while it is being cleared). The slot number comes from the lease team description (`slot=NN`) written at take time, so a project can be released by its current name. `proof.ps1` finds its projects through `Get-E2EPoolProject -Owner`, not through a list of its own, so it releases everything it holds in a `finally`, including projects a crashed seed took.
- **Release fails part-way**: the lease is kept, so the project is never offered half-cleared; it becomes takeable when the lease expires, and the take clears it first.

## R8 Pool size and adopt

- **Decision**: The pool is every eligible live scratch project per org: no lease team, none changed (project, work item, repo) in the last 24 hours, name `amte2e-*` or `amt<issue>-*`, not `migrationSource1`/`migrationTarget1` (those fail `Assert-E2EName` and are also skipped explicitly before any call). About 45 per org against a largest scenario of 6, so about seven proofs at once. Adopt renames to `amte2e-pool-NN`, filling the lowest free numbers, and creates nothing. `-WhatIf` lists renames and each skip with its reason. A second run finds the pool already named and changes nothing. A project on a per-run process is not switched at adopt; it is switched on its first take (R4).
- **Last-changed**: project `lastUpdateTime`, the newest work item `System.ChangedDate` by WIQL (`ORDER BY [System.ChangedDate] DESC`, top 1) and the repo's newest push (`git/repositories/{id}/pushes?$top=1`).
- **Alternatives**: A fixed small pool, for example 10 (leaves 35 idle projects that still count to the limit); creating new projects (impossible at the limit).

## R9 `linkscope`

- **Decision**: The missing-project case uses the name `amte2e-<item>-p<run>-linkscope-gone`, which the seed never creates and nothing renames to. The seed stops calling `New-E2EProject $Gone` and `Remove-E2EProject $Gone`. The config still points at the name, which resolves to no project.
- **Rationale**: The test is the project being absent (item gap 5). It needs no pool project.

## R10 Cleanup script and proof

- **Decision**:
  - `Remove-E2EObjects.ps1` gains `-ClearPool`. It lists each pool project's lease, then clears and releases every pool project (an operator override, so it also takes leases that have not expired and prints each claimant first). `-All` and `-Prefix` stop deleting projects: the project loop is removed, the recycle-bin loop stays only for projects that are already deleted, and the process and field steps are unchanged. With projects released, per-run processes have no users and delete cleanly.
  - `proof.ps1` sets `AMTE2E_POOL_OWNER`, drops the scratch project list, wraps the scenario loop in a `finally` that releases everything held by its owner, renews the lease between scenarios, and prints `pool projects used: <slot> (<org>), ...` and `released: ...`.
- **Rationale**: Matches item gaps 9 and 10.

## R11 Scenarios outside the pool

- **Decision**: None are known to be outside it today (every process has the Agile base, live). The plan records the not-poolable rule and the place to list them (`scenarios.md`), and the Scenario test run (`wicore` twice) plus a full `-All` proof show any that fail.

## R12 Tests

- **Decision**: Two layers. Pester with REST mocked covers lease parsing, expiry and takeover, token re-read, the refuse-to-delete rules in `Remove-E2EObjects.ps1`, adopt eligibility, and name guarding. `Test-E2EPool.ps1` is the live harness for the item's Outcome checks and prints PASS/FAIL per case with exit 0 only if all pass. The Race case starts two takes as separate `pwsh` processes and checks the slots differ over 10 trials.
