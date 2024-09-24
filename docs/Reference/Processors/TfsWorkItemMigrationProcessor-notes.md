## <a name="WIQLQuery"></a>WIQL Query

The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax). We only support flat quereis that have `FROM WorkItems` in the query.    

> Note: A useful Azure DevOps Extension to explore WIQL is the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor)

### Examples

You can use the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor) to craft a query in Azure DevOps.

A simple example config:

```
"WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
```
Scope to Area Path (Team data):

```
"WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] UNDER 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
```

## <a name="NodeBasePath"></a>NodeBasePath Configuration 

Moved to the TfsNodeStructure

# Iteration Maps and Area Maps

Moved to the TfsNodeStructure



## More Complex Team Migrations
The above options allow you to bring over a sub-set of the WIs (using the `WIQLQueryBit`) and move their area or iteration path to a default location. However you may wish to do something more complex e.g. re-map the team structure. This can be done with addition of a `FieldMaps` block to configuration in addition to the `NodeBasePaths`.

Using the above sample structure, if you wanted to map the source project `Team 1`  to target project `Team A` etc. you could add the field map as follows

A complete list of [FieldMaps](../Reference/FieldMaps/index.md) are available.

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

# Removed Properties

- PrefixProjectToNodes - This option was removed in favour of the Area and Iteration Maps on [TfsNodeStructure](/Reference/v2/ProcessorEnrichers/TfsNodeStructure/)
