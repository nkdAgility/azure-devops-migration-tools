# Data model: scratch project pool (#3342)

Nothing is stored outside the test orgs. The state is the project's name, its lease team and the project's contents.

## Pool project

| Field | Value |
|---|---|
| Org | `nkdagility-adomt-source` or `nkdagility-adomt-target` |
| Slot | `NN`, two digits, 01 to 99, unique per org |
| Name | `amte2e-pool-NN` when free; the seed's requested name while leased |
| Process | `amte2e-agile` when free; the requested process while leased |
| Lease | the team `amte2e-pool-lease`, present only while leased |

Never deleted. Never `migrationSource1` or `migrationTarget1`.

## Lease team

Team name `amte2e-pool-lease` (fixed). Description, one line:

```text
slot=NN;machine=<name>;pid=<owner pid or empty>;run=<run id>;item=<issue>;claimed=<UTC>;expires=<UTC>
```

| Field | Meaning |
|---|---|
| `slot` | Pool slot, so release can restore `amte2e-pool-NN` from the current name |
| `machine`, `pid` | Claimant machine and owner process (the proof's pid, from `AMTE2E_POOL_OWNER`; empty when run by hand) |
| `run` | The proof's run id, or a new GUID for a hand run |
| `item` | The issue number, when known |
| `claimed`, `expires` | UTC, ISO 8601. `expires` is claim time plus 2 hours, moved forward by renewal |

The token is `machine|pid|run`.

## Lease state

| State | Condition | Taker may |
|---|---|---|
| Free | no lease team | claim it |
| Held | team exists, `expires` in the future, claimant not a dead local pid | leave it alone |
| Expired | `expires` in the past | take over |
| Orphaned | `machine` is this machine and `pid` is set and not running | take over at once |

## Transitions

```text
Free ──take──▶ Claimed ──clear──▶ Cleared ──switch process──▶ Switched ──rename──▶ Leased (returned)
Leased ──release──▶ Clearing ──▶ Reset (agile) ──▶ Renamed (pool-NN) ──▶ Free   (lease team deleted last)
Held ──expiry or dead pid──▶ Expired/Orphaned ──takeover──▶ Claimed
```

A failure at any step before "Leased" releases the lease (the project is then free but may be half-cleared; the next take clears first). A failure during release keeps the lease so the project is not offered half-cleared.

## Seed request

| Field | Value |
|---|---|
| Name | passes `Assert-E2EName` |
| Process | an existing process name in the org, with the Agile base |
| Org | defaults to the module's default org |

## Inventory (what "cleared" is measured against)

Read by REST from the project, independent of the clearing code:

| Surface | Cleared value |
|---|---|
| Work items (WIQL by `System.TeamProject`) and recycle bin entries | 0 |
| Teams | exactly the default team and the lease team |
| Area and iteration children of the root | 0 |
| Queries under Shared Queries and My Queries | 0 |
| Work item tags | 0 |
| Repos (and their refs and pull requests) | 1 repo, empty |
| Repo recycle bin entries for the project | 0 |
| Build definitions, release definitions | 0 |
| Variable groups, service connections, environments, wikis | 0 |
| Test plans, test runs | 0 |
| Dashboards | the default only |
| Process type | the requested process |
