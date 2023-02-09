## Processors: WorkItemMigrationContext

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../././index.md) > [Reference](.././index.md) > [API v1](../index.md) > [Processors](./index.md)> **WorkItemMigrationContext**

WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If enabled then the processor will run | false |
| ReplayRevisions | Boolean | You can choose to migrate the tip only (a single write) or all of the revisions (many writes).
            If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true` | true |
| PrefixProjectToNodes | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |
| UpdateCreatedDate | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date.
            (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
            not the internal create date) | true |
| UpdateCreatedBy | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date.
            (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate,
            not the internal create date) | true |
| WIQLQueryBit | String | A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits) | AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') |
| WIQLOrderBit | String | A work item query to affect the order in which the work items are migrated. Don't leave this empty. | [System.ChangedDate] desc |
| LinkMigration | Boolean | If enabled this will migrate the Links for the work item at the same time as the whole work item. | true |
| AttachmentMigration | Boolean | If enabled this will migrate all of the attachments at the same time as the work item | true |
| AttachmentWorkingPath | String | `AttachmentMigration` is set to true then you need to specify a working path for attachments to be saved locally. | C:\temp\Migration\ |
| FixHtmlAttachmentLinks | Boolean | **beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML
            fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps;
            TFS should use integrated authentication. | ? |
| SkipToFinalRevisedWorkItemType | Boolean | **beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML fields as well as discussion comments. You must specify a
            PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication. | false |
| WorkItemCreateRetryLimit | Int32 | **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count.
            This allows for periodic network glitches not to end the process. | 5 |
| FilterWorkItemsThatAlreadyExistInTarget | Boolean | This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run.
            While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart. | true |
| PauseAfterEachWorkItem | Boolean | Pause after each work item is migrated | false |
| AttachmentMaxSize | Int32 | `AttachmentMigration` is set to true then you need to specify a max file size for upload in bites.
            For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb). | 480000000 |
| AttachRevisionHistory | Boolean | This will create a json file with the revision history and attach it to the work item. Best used with `MaxRevisions` or `ReplayRevisions`. | ? |
| LinkMigrationSaveEachAsAdded | Boolean | If you have changed parents before re-running a sync you may get a `TF26194: unable to change the value of the 'Parent' field` error.
            This will resolve it, but will slow migration. | false |
| GenerateMigrationComment | Boolean | If enabled, adds a comment recording the migration | false |
| WorkItemIDs | IList | A list of work items to import | [] |
| MaxRevisions | Int32 | Sets the maximum number of revisions that will be migrated. "First + Last N = Max".
            If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated. | 0 |
| NodeStructureEnricherEnabled | Nullable |  | ? |
| UseCommonNodeStructureEnricherConfig | Boolean |  | ? |
| StopMigrationOnMissingAreaIterationNodes | Boolean |  | ? |
| NodeBasePaths | String[] | The root paths of the Ares / Iterations you want migrate. See [NodeBasePath Configuration](#nodebasepath-configuration) | ["/"] |
| AreaMaps | Dictionary`2 | Remapping rules for area paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`,
            that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied. | {} |
| IterationMaps | Dictionary`2 | Remapping rules for iteration paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`,
            that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied. | {} |
| MaxGracefulFailures | Int32 | The maximum number of failures to tolerate before the migration fails. When set above zero, a work item migration error is logged but the migration will
            continue until the number of failed items reaches the configured value, after which the migration fails. | 0 |
| SkipRevisionWithInvalidIterationPath | Boolean | This will skip a revision if the source iteration has not been migrated i.e. it was deleted | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "WorkItemMigrationConfig",
  "Enabled": false,
  "ReplayRevisions": true,
  "PrefixProjectToNodes": false,
  "UpdateCreatedDate": true,
  "UpdateCreatedBy": true,
  "WIQLQueryBit": "AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request')",
  "WIQLOrderBit": "[System.ChangedDate] desc",
  "LinkMigration": true,
  "AttachmentMigration": true,
  "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
  "FixHtmlAttachmentLinks": false,
  "SkipToFinalRevisedWorkItemType": true,
  "WorkItemCreateRetryLimit": 5,
  "FilterWorkItemsThatAlreadyExistInTarget": true,
  "PauseAfterEachWorkItem": false,
  "AttachmentMaxSize": 480000000,
  "AttachRevisionHistory": false,
  "LinkMigrationSaveEachAsAdded": false,
  "GenerateMigrationComment": true,
  "WorkItemIDs": null,
  "MaxRevisions": 0,
  "NodeStructureEnricherEnabled": null,
  "UseCommonNodeStructureEnricherConfig": false,
  "StopMigrationOnMissingAreaIterationNodes": true,
  "NodeBasePaths": null,
  "AreaMaps": {
    "$type": "Dictionary`2"
  },
  "IterationMaps": {
    "$type": "Dictionary`2"
  },
  "MaxGracefulFailures": 0,
  "SkipRevisionWithInvalidIterationPath": false
}
```

## <a name="WIQLQueryBits"></a>WIQL Query Bits

The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax).

> Note: A useful Azure DevOps Extension to explore WIQL is the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor)

### Examples

You can use the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor) to craft a query in Azure DevOps.

Typical way that queries are built:

```
 var targetQuery =
     string.Format(
         @"SELECT [System.Id], [{ReflectedWorkItemIDFieldName}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {WIQLQueryBit} ORDER BY {WIQLOrderBit}",
         Engine.Target.Config.ReflectedWorkItemIDFieldName,
         _config.WIQLQueryBit,
         _config.WIQLOrderBit
      );
var targetFoundItems = Engine.Target.WorkItems.GetWorkItems(targetQuery);
```

A simple example config:

```
"WIQLQueryBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
"WIQLOrderBit": "[System.ChangedDate] desc",
```
Scope to Area Path (Team data):

```
"WIQLQueryBit": "AND [System.AreaPath] UNDER 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
"WIQLOrderBit": "[System.ChangedDate] desc",
```

```
"WIQLQueryBit": "AND [System.ChangedDate] > 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
"WIQLOrderBit": "[System.ChangedDate] desc",
```

## <a name="NodeBasePath"></a>NodeBasePath Configuration ##
The `NodeBasePaths` entry allows the filtering of the nodes to be replicated on the target projects. To try to explain the correct usage let us assume that we have a source team project `SourceProj` with the following node structures

- AreaPath
   - SourceProj
   - SourceProj\Team 1
   - SourceProj\Team 2
   - SourceProj\Team 2\Sub-Area
   - SourceProj\Team 3
- IterationPath
   - SourceProj
   - SourceProj\Sprint 1
   - SourceProj\Sprint 2
   - SourceProj\Sprint 2\Sub-Iteration
   - SourceProj\Sprint 3

Depending upon what node structures you wish to migrate you would need the following settings. Exclusions are also possible by prefixing a path with an exclamation mark `!`. Example are

| | |
|-|-|
| Intention    | Migrate all areas and iterations and all Work Items
| NodeBasePath | `[]`
| Comment      | The same AreaPath and Iteration Paths are created on the target as on the source. Hence, all migrated WI remain in their existing area and iteration paths
||
| Intention    | Only migrate area path `Team 2` and it associated Work Items, but all iteration paths
| NodeBasePath | `["Team 2", "Sprint"]`
| Comment      | Only the area path ending `Team 2` will be migrated. <br>The `WIQLQueryBit` should be edited to limit the WI migrated to this area path e.g. add `AND [System.AreaPath] UNDER 'SampleProject\\Team 2'` . <br> The migrated WI will have an area path of `TargetProj\Team 2` but retain their iteration paths matching the sprint name on the source
||
| Intention    | Only migrate iterations structure
| NodeBasePath | `["Sprint"]`
| Comment      | Only the area path ending `Team 2` will be migrated<br>All the iteration paths will be migrated. <br> The migrated WI will have the default area path of `TargetProj` as their source area path was not migrated i.e. `TargetProj`<br> The migrated WI will have an iteration path match the sprint name on the source
||
| Intention    | Move all WI to the existing area and iteration paths on the targetProj
| NodeBasePath | `["DUMMY VALUE"]`
| Comment      | As the `NodeBasePath` does not match any source area or iteration path no nodes are migrated. <br>Migrated WI will be assigned to any matching area or iteration paths. If no matching ones can be found they will default to the respective root values
||
| Intention    | Move the `Team 2` area, but not its `Sub-Area`
| NodeBasePath | `["Team 2", "!Team 2\\SubArea"]`
| Comment      | The Work Items will have to be restricted to the right areas, e.g. with `AND [System.AreaPath] UNDER 'SampleProject\\Team 2' AND [System.AreaPath] NOT UNDER 'SampleProject\\Team 2\\Sub-Area'`, otherwise their migratin will fail

# Iteration Maps and Area Maps

These two configuration elements apply after the `NodeBasePaths` selector, i.e.
only on Areas and Iterations that have been selected for migration. They allow
to change the area path, respectively the iteration path, of migrated work items.

These remapping rules are applied both while creating path nodes in the target
project and when migrating work items.

These remapping rules are applied with a higher priority than the
`PrefixProjectToNodes` option. This means that if no declared rule matches the
path and the `PrefixProjectToNodes` option is enabled, then the old behavior is
used.

The syntax is a dictionary of regular expressions and the replacement text.

*Warning*: These follow the
[.net regular expression language](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference).
The key in the dictionary is a regular expression search pattern, while the
value is a regular expression replacement pattern. It is therefore possible to
use back-references in the replacement string.

*Warning*: Special characters in the acceptation of regular expressions _and_
json both need to be escaped. For a key, this means, for example, that a
literal backslash must be escaped for the regular expression language `\\`
_and_ each of these backslashes must then be escaped for the json encoding:
`\\\\`. In the replacement string, a literal `$` must be escaped with an
additional `$` if it is followed by a number (due to the special meaning in
regular expression replacement strings), while a backslash must be escaped
(`\\`) due to the special meaning in json.

*Advice*: To avoid unexpected results, always match terminating backslashes in
the search pattern and replacement string: if a search pattern ends with a
backslash, you should also put one in the replacement string, and if the search
pattern does not include a terminating backslash, then none should be included
in the replacement string.

#### Examples explained

```json
"IterationMaps": {
  "^OriginalProject\\\\Path1(?=\\\\Sprint 2022)": "TargetProject\\AnotherPath\\NewTeam",
  "^OriginalProject\\\\Path1(?=\\\\Sprint 2020)": "TargetProject\\AnotherPath\\Archives\\Sprints 2020",
  "^OriginalProject\\\\Path2": "TargetProject\\YetAnotherPath\\Path2",
},
"AreaMaps": {
  "^OriginalProject\\\\(DescopeThis|DescopeThat)": "TargetProject\\Archive\\Descoped\\",
  "^OriginalProject\\\\(?!DescopeThis|DescopeThat)": "TargetProject\\NewArea\\",
}
```

- `"^OriginalProject\\\\Path1(?=\\\\Sprint 2022)": "TargetProject\\AnotherPath\\NewTeam",`

  In an iteration path, `OriginalProject\Path1` found at the beginning of the
  path, when followed by `\Sprint 2022`, will be replaced by
  `TargetProject\AnotherPath\NewTeam`.

  `OriginalProject\Path1\Sprint 2022\Sprint 01` will become
  `TargetProject\AnotherPath\NewTeam\Sprint 2022\Sprint 01` but
  `OriginalProject\Path1\Sprint 2020\Sprint 03` will _not_ be transformed by
  this rule.

- `"^OriginalProject\\\\Path1(?=\\\\Sprint 2020)": "TargetProject\\AnotherPath\\Archives\\Sprints 2020",`

  In an iteration path, `OriginalProject\Path1` found at the beginning of the
  path, when followed by `\Sprint 2020`, will be replaced by
  `TargetProject\AnotherPath\Archives\\Sprints 2020`.

  `OriginalProject\Path1\Sprint 2020\Sprint 01` will become
  `TargetProject\AnotherPath\Archives\Sprint 2020\Sprint 01` but
  `OriginalProject\Path1\Sprint 2021\Sprint 03` will _not_ be transformed by
  this rule.

- `"^OriginalProject\\\\Path2": "TargetProject\\YetAnotherPath\\Path2",`

  In an iteration path, `OriginalProject\Path2` will be replaced by
  `TargetProject\YetAnotherPath\Path2`.

- `"^OriginalProject\\\\(DescopeThis|DescopeThat)": "TargetProject\\Archive\\Descoped\\",`

  In an area path, `OriginalProject\` found at the beginning of the path, when
  followed by either `DescopeThis` or `DescopeThat` will be replaced by `TargetProject\Archive\Descoped\`.

  `OriginalProject\DescopeThis\Area` will be transformed to
  `TargetProject\Archive\Descoped\DescopeThis\Area`.
  `OriginalProject\DescopeThat\Product` will be transformed to
  `TargetProject\Archive\Descoped\DescopeThat\Product`.

- `"^OriginalProject\\\\(?!DescopeThis|DescopeThat)": "TargetProject\\NewArea\\",`

  In an area path, `OriginalProject\` found at the beginning of the path will be
  replaced by `TargetProject\NewArea\` unless it is followed by `DescopeThis` or
  `DescopeThat`.

  `OriginalProject\ValidArea\` would be replaced by
  `TargetProject\NewArea\ValidArea\` but `OriginalProject\DescopeThis` would not
  be modified by this rule.

## More Complex Team Migrations
The above options allow you to bring over a sub-set of the WIs (using the `WIQLQueryBit`) and move their area or iteration path to a default location. However you may wish to do something more complex e.g. re-map the team structure. This can be done with addition of a `FieldMaps` block to configuration in addition to the `NodeBasePaths`.

Using the above sample structure, if you wanted to map the source project `Team 1`  to target project `Team A` etc. you could add the field map as follows

```
 "FieldMaps": [
   {
      "$type": "FieldValueMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.AreaPath",
      "targetField": "System.AreaPath",
      "defaultValue": "TargetProg",
      "valueMapping": {
        "SampleProj\\Team 1": "TargetProg\\Team A",
        "SampleProj\\Team 2": "TargetProg\\Team B"
        "SampleProj\\Team 3": "TargetProg\\Team C"
      }
    },
  ],

```

> Note: This mappings could also be achieved with other forms of Field mapper e.g. `RegexFieldMapConfig`, but the value mapper as an example is easy to understand