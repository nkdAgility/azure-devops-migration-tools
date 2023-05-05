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

A complete list of [FieldMaps](../Reference/v1/FieldMaps/index.md) are available.

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