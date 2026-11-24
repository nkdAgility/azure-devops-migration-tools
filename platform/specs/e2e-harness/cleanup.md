# e2e harness: cleaning up scratch objects

`tools/e2e/Remove-E2EObjects.ps1` deletes e2e scratch objects from the test organisations: projects first, then the
inherited processes they used, then `Custom.amte2e*` fields. Processes in use and fields in use can't be deleted, which
is why it works in that order. It lists whatever is left.

## Who owns which objects

Every scratch project and process name starts with a prefix. Every field a scenario creates is named
`amte2e<issue>…` (reference `Custom.amte2e<issue>…`), carrying the number of the issue it belongs to. For example, the
`fields` and `procdef` scenarios came from #3227, so their fields are `amte2e3227IssueOnly`, `amte2e3227Legacy` and
`amte2e3227Ticket#`.

| Run | Projects and processes | Fields | Exit |
|---|---|---|---|
| `-Prefix amt<issue>-` or `-Prefix amte2e-<issue>-` | those starting with the prefix | `Custom.amte2e<issue>…`, but not a longer number that starts with it | 0 |
| any other `-Prefix`, such as `amte2e-fields-` | those starting with the prefix | none, with the warning "Cannot map prefix" | 1 |
| `-Prefix amte2e-`, `amte2e` or empty | refused before anything is deleted | none | error |
| no `-Prefix` and no `-All` | refused before anything is deleted | none | error |
| `-All` | every `amte2e-*` | every `Custom.amte2e*` | 0 |

The mapping is `Get-E2EFieldFilter` in `tools/e2e/E2EAzDo.psm1`, next to `Assert-E2EName`, which decides which names the
harness may touch at all.

## Dry run

`-WhatIf` lists every project, recycled project, process and field the run would delete ("would delete …"), and deletes
nothing. Every run first prints its field filter.

```powershell
./tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3278- -WhatIf
./tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3278-
./tools/e2e/Remove-E2EObjects.ps1 -All
```

Projects deleted on Azure DevOps Services stay in the organisation's recycle bin until the service purges them (hard
delete is refused with `VS800076`). They are reported as leftovers.

## Tests

`tools/e2e/Remove-E2EObjects.Tests.ps1` (Pester 5+) covers the mapping, the refusals and `-WhatIf`, with every Azure
DevOps call mocked. `.github/workflows/e2e-harness-tests.yml` runs it on pull requests that change `tools/e2e/`.
