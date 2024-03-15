## Additional Samples & Info

```
{
    "$type": "ExportUsersForMappingConfig",
    "Enabled": false,
    "LocalExportJsonFile": "c:\\temp\\ExportUsersForMappingConfig.json",
    "WIQLQuery": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc",
    "IdentityFieldsToCheck": [
    "System.AssignedTo",
    "System.ChangedBy",
    "System.CreatedBy",
    "Microsoft.VSTS.Common.ActivatedBy",
    "Microsoft.VSTS.Common.ResolvedBy",
    "Microsoft.VSTS.Common.ClosedBy"
    ]
}
```