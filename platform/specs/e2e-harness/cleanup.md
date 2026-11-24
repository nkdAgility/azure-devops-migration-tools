# e2e harness: cleaning up scratch objects

`tools/e2e/Remove-E2EObjects.ps1` cleans up e2e scratch objects in the test organisations: it gives back the pool
projects first, then deletes the inherited processes they used, then `Custom.amte2e*` fields. Processes in use and
fields in use can't be deleted, which is why it works in that order. It lists whatever is left.

It never deletes a project. A deleted project stays in the organisation's recycle bin for about 28 days and counts
toward the 1,000-project limit all that time (hard delete is refused with `VS800076`), which is how the target org
filled up (#3342). Projects come from the scratch project pool instead.

## The scratch project pool

Each test org keeps a fixed set of projects named `amte2e-pool-NN`, on `amte2e-agile` while free. They live in
`tools/e2e/E2EPool.ps1`, used through `New-E2EProject` and `Remove-E2EProject` in `tools/e2e/E2EAzDo.psm1`.

- **Take.** `New-E2EProject -Name <name> -Process <process>` claims a free pool project by creating the team
  `amte2e-pool-lease` in it. Azure DevOps refuses a second team of that name, so exactly one run gets the project, on
  any machine. It then clears the project, switches it to the process (`projectprocessmigration`), renames it to the
  name (the default team and repo follow), and returns it. If no project can be claimed, it fails and names every
  lease's holder and expiry; it never creates a project.
- **Give back.** `Remove-E2EProject <name>` clears the project, switches it back to `amte2e-agile`, renames it back to
  `amte2e-pool-NN` and deletes the lease team last. If that fails part-way the lease is kept, so a half-cleared project
  is never handed out; the next take after the lease expires clears it first.
- **Lease.** The lease team's description holds `slot`, `machine`, `pid`, `run`, `item`, `claimed` and `expires`
  (UTC). It lasts two hours and is renewed on every take and give-back. The holder is the proof (`AMTE2E_POOL_OWNER`,
  `<pid>:<run>`), so seeds it starts share its leases. An expired lease, or one whose holder is a process on this
  machine that has exited, may be taken over; the taker deletes that lease team only if it is unchanged since it was
  read. The holder re-reads its lease before every destructive step and stops if it is no longer there.
- **Clear.** Work items are destroyed (not left in the recycle bin); test plans and runs, build and release
  definitions, variable groups, service connections, environments, wikis, extra dashboards, queries, extra teams, area
  and iteration children and tags are deleted; the repos are replaced by one new empty repo. "Cleared" is checked by
  an inventory of those surfaces read from the service (`Get-E2EPoolInventory`), not by the clearing code's own list.
- **Adding projects.** `tools/e2e/Initialize-E2EPool.ps1` renames idle scratch projects (no lease, nothing changed in
  24 hours, or named with `-Include` by their owner) to `amte2e-pool-NN`. It never touches `migrationSource1` or
  `migrationTarget1`, creates nothing, supports `-WhatIf`, and changes nothing on a second run.
- **The one exception.** `linkscope` needs a work item that can't be read, which only a deleted project gives (deleting
  the work item removes its links too). Its `*-gone` project is created and deleted with `New-E2EDisposableProject` /
  `Remove-E2EDisposableProject`, which refuse any org but `nkdagility-adomt-source` and any other name.

## Who owns which objects

Every scratch process name starts with a prefix, and a leased pool project carries the name its run asked for. Every
field a scenario creates is named `amte2e<issue>…` (reference `Custom.amte2e<issue>…`), carrying the number of the issue
it belongs to. For example, the `fields` and `procdef` scenarios came from #3227, so their fields are
`amte2e3227IssueOnly`, `amte2e3227Legacy` and `amte2e3227Ticket#`. A pool project belongs to whoever holds its lease.

| Run | Pool projects | Processes | Fields | Exit |
|---|---|---|---|---|
| `-Prefix amt<issue>-` or `-Prefix amte2e-<issue>-` | given back if leased under the prefix | those starting with the prefix | `Custom.amte2e<issue>…`, but not a longer number that starts with it | 0 |
| any other `-Prefix`, such as `amte2e-fields-` | given back if leased under the prefix | those starting with the prefix | none, with the warning "Cannot map prefix" | 1 |
| `-Prefix amte2e-`, `amte2e` or empty | refused before anything is changed | none | none | error |
| no `-Prefix`, `-All` or `-ClearPool` | refused before anything is changed | none | none | error |
| `-All` | every leased one given back | every `amte2e-*` | every `Custom.amte2e*` | 0 |
| `-ClearPool` | every leased one given back, whoever holds it (each holder is printed first) | none | none | 0, or 1 if one could not be given back |

A scratch project that is not in the pool is left and reported ("kept project …"); add it with `Initialize-E2EPool.ps1`.
The mapping is `Get-E2EFieldFilter` in `tools/e2e/E2EAzDo.psm1`, next to `Assert-E2EName`, which decides which names the
harness may touch at all.

## Dry run

`-WhatIf` lists every pool project it would give back ("would release …"), and every recycled project, process and
field it would delete ("would delete …"), and changes nothing. Every run first prints its field filter.

```powershell
./tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3278- -WhatIf
./tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3278-
./tools/e2e/Remove-E2EObjects.ps1 -All
./tools/e2e/Remove-E2EObjects.ps1 -ClearPool -WhatIf
```

Projects deleted before the pool existed stay in the recycle bin until the service purges them. They are reported as
leftovers.

## Tests

`tools/e2e/Remove-E2EObjects.Tests.ps1` and `tools/e2e/E2EPool.Tests.ps1` (Pester 5+) cover the mapping, the refusals,
`-WhatIf`, `-ClearPool`, leases, take and give-back, with every Azure DevOps call mocked.
`.github/workflows/e2e-harness-tests.yml` runs them on pull requests that change `tools/e2e/`.
`tools/e2e/Test-E2EPool.ps1 -Case Cycle | Race | Lease | Scenario` checks the pool live on the test orgs.
