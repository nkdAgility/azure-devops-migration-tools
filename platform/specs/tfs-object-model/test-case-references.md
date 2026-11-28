# Test Case references

`TfsWorkItemLinkTool.MigrateSharedSteps` and `MigrateSharedParameters` remap a Test Case's references to Shared Steps (`ref="<id>"` in `Microsoft.VSTS.TCM.Steps`) and Shared Parameters (`dataSetId` and `sharedParameterDataSetIds` in `Microsoft.VSTS.TCM.LocalDataSource`) from source ids to the ids of the migrated copies, found by their reflected work item id.

- A referenced item that is missing from the source, or cannot be read (TF26198), is reported as `link-skipped`.
- A referenced item that exists in the source but has no match in the target is reported by `UnresolvedTestCaseReference.Report`: a `[SKIP] ... not remapped` warning naming the Shared Steps or Shared Parameter, the Test Case and its target, and a `reference-left-on-source` review row. The reference keeps the source id; it is not cleared. (#3256)
- Both methods mark the `reference-left-on-source` check as done, so a re-run that remaps the reference resolves the earlier row. A work item migrated with no source links left marks it too (`UnresolvedTestCaseReference.MarkCheckedWithoutLinks`), so removing the reference and its link from the source also resolves the row.
- Everything uses the TFS object model; no REST call is made.
