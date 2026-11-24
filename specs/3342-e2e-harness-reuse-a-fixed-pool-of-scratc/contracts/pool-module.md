# Contract: pool module functions

`tools/e2e/E2EPool.psm1`, imported by `E2EAzDo.psm1`. Every function takes `-Org` (default: the module's org) and calls `Assert-E2EName` on every project name it reads from the caller or from the org before it acts. A project whose name fails the guard (including `migrationSource1`/`migrationTarget1`) is never read for state beyond the listing and never modified.

## Public (exported from `E2EAzDo.psm1`, signatures unchanged)

### `New-E2EProject -Name <string> [-Process <string> = "Agile"] [-Org <string>]`

Takes a free pool project and returns it as the project `Name` on process `Process`.

1. `Assert-E2EName $Name`.
2. If a project `Name` exists: return it when its lease token is this run's owner; otherwise throw (it is not ours to adopt).
3. List pool projects (`amte2e-pool-NN`), then try each: claim by creating the lease team; on takeover see lease rules (research R2). On refusal, try the next.
4. Clear it; verify with the inventory; switch to `Process`; verify; rename to `Name`; renew the lease. Re-read the lease before each destructive step.
5. Return the project object. A `PoolSlot` note property carries `NN`.

Failure: throws with the reason; if the failure came after a claim, the lease is deleted (the project is free). If no project can be claimed: throws `No free pool project in <org>. Leases: <slot> held by <machine|pid|run> until <expiry>, ...` and creates no project.

### `Remove-E2EProject -Name <string> [-Org <string>]`

Releases a pool project that holds the lease of this run (or any, with `-Force`): clear, verify, switch to `amte2e-agile`, rename to `amte2e-pool-NN`, delete the lease team. Never deletes the project. If no project is named `Name`, returns without error (as today). If `Name` is a project that is not a leased pool project, throws.

## New, internal to the pool but exported for the scripts and tests

| Function | Behaviour |
|---|---|
| `Get-E2EPoolProject [-Owner <run>] [-All]` | Pool projects with their lease state (Free, Held, Expired, Orphaned) and claimant. `-Owner` filters to leases of that run. Reads leases by listing each project's teams. |
| `Get-E2EPoolLease -Project` | Parses the lease team's description to the fields in the data model; `$null` when there is no lease team. |
| `Set-E2EPoolLease -Project -Renew` | Moves `expires` forward 2 hours, and throws if the team's token is not this run's (lease lost). |
| `Clear-E2EPoolProject -Project` | Runs the clearing steps of research R3. Requires the lease to be this run's. |
| `Get-E2EPoolInventory -Project` | Reads the surfaces in the data model's inventory table. |
| `Test-E2EPoolClear -Project` | `$true` when the inventory equals a cleared project's; otherwise returns the differences. |
| `Switch-E2EPoolProcess -Project -Process` | `projectprocessmigration`, polled, then verified. |
| `Rename-E2EPoolProject -Project -Name` | `PATCH _apis/projects/{id}`, polled; throws if `Name` exists. |
| `Clear-E2EPool -Owner <run> \| -All` | Releases every project of the run, or every leased pool project. Collects errors and returns them rather than stopping at the first. |

Owner identity: `$env:AMTE2E_POOL_OWNER` (`<pid>:<run>`), else a new GUID run id with no pid.

## Invariants

- The number of projects in an org is the same after any call.
- A returned project has an empty inventory.
- Two callers never both return the same project.
- No call modifies a project that fails `Assert-E2EName`.
