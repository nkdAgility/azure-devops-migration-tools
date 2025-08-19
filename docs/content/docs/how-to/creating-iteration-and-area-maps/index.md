---
title: 'How-To: Creating area and iteration maps'
description: |
  This guide explains how to create area and iteration maps for Azure DevOps Migration Tools, allowing you to adapt old locations to new ones during migration.
short_title: Creating area and iteration maps
discussionId: 1846
date: 2025-06-24T12:07:31Z

---
As per the [documentation]({{< ref "docs/reference/tools/tfsnodestructuretool" >}}), you need to add Iteration Maps and Area Maps that adapt the old locations to new ones that are valid in the Target.

**NOTE: It is NOT possible to migrate a work item if the Area or Iteration path does not exist on the target project. This is because the work item will be created with the same Area and Iteration path as the source work item. If the path does not exist, the work item will not be created. _There is no way around this!_**

Before your migration starts, it will validate that all of the Areas and Iterations from the **Source** work items revisions exist on the **Target**. Any that do not exist will be flagged in the logs, and if you have `"StopMigrationOnMissingAreaIterationNodes": true,` set, the migration will stop just after it outputs a list of the missing nodes.

Our algorithm that converts the Source nodes to Target nodes processes the [mappings]({{< ref "docs/reference/tools/tfsnodestructuretool" >}}) at that time. This means that any valid mapped nodes will never be caught by the `This path is not anchored in the source project` message, as they are already altered to be valid.

> We recently updated the logging for this part of the system to more easily debug both your mappings and to see what the system is doing with the nodes and their current state. You can set `"LogLevel": "Debug"` to see the details.

## Configuration Format

The modern configuration uses structured mappings with `Match` and `Replacement` properties, utilizing the new `Iterations.Mappings` and `Areas.Mappings` format:

```json
"Iterations": {
  "Mappings": [
    {
      "Match": "WorkItemMovedFromProjectName\\\\Iteration 1",
      "Replacement": "TargetProject\\Sprint 1"
    }
  ]
},
"Areas": {
  "Mappings": [
    {
      "Match": "WorkItemMovedFromProjectName\\\\Team 2",
      "Replacement": "TargetProject\\ProductA\\Team 2"
    }
  ]
}
```

### Legacy Format Support

_Note: The old `IterationMaps` and `AreaMaps` dictionary format is still supported for backward compatibility but is deprecated:_

```json
"IterationMaps": {
  "WorkItemMovedFromProjectName\\\\Iteration 1": "TargetProject\\Sprint 1"
},
"AreaMaps": {
   "WorkItemMovedFromProjectName\\\\Team 2": "TargetProject\\ProductA\\Team 2"
}
```

## Using Regular Expressions

You can use regular expressions to match and transform area or iteration paths. The syntax uses structured mappings with regular expression patterns:

```json
"Iterations": {
  "Mappings": [
    {
      "Match": "^OriginalProject\\\\Path1(?=\\\\Sprint 2022)(.*)$",
      "Replacement": "TargetProject\\AnotherPath\\NewTeam$1"
    },
    {
      "Match": "^OriginalProject\\\\Path1(?=\\\\Sprint 2020)(.*)$",
      "Replacement": "TargetProject\\AnotherPath\\Archives\\Sprints 2020$1"
    },
    {
      "Match": "^OriginalProject\\\\Path2(.*)$",
      "Replacement": "TargetProject\\YetAnotherPath\\Path2$1"
    }
  ]
},
"Areas": {
  "Mappings": [
    {
      "Match": "^OriginalProject\\\\(DescopeThis|DescopeThat)(.*)$",
      "Replacement": "TargetProject\\Archive\\Descoped\\$1$2"
    },
    {
      "Match": "^OriginalProject\\\\(?!DescopeThis|DescopeThat)(.*)$",
      "Replacement": "TargetProject\\NewArea\\$1"
    }
  ]
}
```

### Using Back-References in Replacements

If you want to use captured groups in the replacement, you can reference them using `$` followed by the group number:

```json
"Iterations": {
  "Mappings": [
    {
      "Match": "^\\\\oldproject1(?:\\\\([^\\\\]+))?\\\\([^\\\\]+)$",
      "Replacement": "TargetProject\\Q1\\$2"
    }
  ]
}
```

If the old iteration path was `\oldproject1\Custom Reporting\Sprint 13`, this would result in a match for each Iteration node after the project node. You would then be able to reference any of the nodes using "$" and the number of the match.

### Important Notes About Escaping

Regular expressions are much more difficult to build and debug, so it is a good idea to use a [regular expression tester](https://regex101.com/) to check that you are matching the right things and to build them in ChatGPT.

**Special Character Escaping Warning:** Special characters in the acceptation of regular expressions _and_ JSON both need to be escaped. For the Match property, this means, for example, that a literal backslash must be escaped for the regular expression language `\\` _and_ each of these backslashes must then be escaped for the JSON encoding: `\\\\`. In the Replacement property, a literal `$` must be escaped with an additional `$` if it is followed by a number (due to the special meaning in regular expression replacement strings), while a backslash must be escaped (`\\`) due to the special meaning in JSON.

**Advice:** To avoid unexpected results, always match terminating backslashes in the search pattern and replacement string: if a search pattern ends with a backslash, you should also put one in the replacement string, and if the search pattern does not include a terminating backslash, then none should be included in the replacement string.

_NOTE: You need `\\` to escape a `\` in the pattern, and `\\` to escape a `\` in JSON. Therefore, in the Match property you need 4 `\` to represent the `\\` for the pattern and only 2 `\` in the Replacement property._

## Some Useful Mapping Patterns

### Simple Project Rename with Path Preservation

```json
"Iterations": {
  "Mappings": [
    {
      "Match": "^OldProjectName([\\\\]?.*)$",
      "Replacement": "NewProjectName$1"
    }
  ]
},
"Areas": {
  "Mappings": [
    {
      "Match": "^OldProjectName([\\\\]?.*)$",
      "Replacement": "NewProjectName$1"
    }
  ]
}
```

This maps all `OldProjectName` area or iterations to a similar new node, preserving the entire path structure. If you have `ShouldCreateMissingRevisionPaths` enabled, it will create missing nodes automatically.

### Project Rename to Root Only

```json
"Iterations": {
  "Mappings": [
    {
      "Match": "^OldProjectName([\\\\]?.*)$",
      "Replacement": "NewProjectName"
    }
  ]
},
"Areas": {
  "Mappings": [
    {
      "Match": "^OldProjectName([\\\\]?.*)$",
      "Replacement": "NewProjectName"
    }
  ]
}
```

This maps all `OldProjectName` paths to just the new project name root, effectively flattening the hierarchy.

### Replacing PrefixProjectToNodes

The deprecated `PrefixProjectToNodes` option can be replaced with explicit mappings:

```json
"Iterations": {
  "Mappings": [
    {
      "Match": "^SourceServer\\\\(.*)$",
      "Replacement": "TargetServer\\SourceServer\\$1"
    }
  ]
},
"Areas": {
  "Mappings": [
    {
      "Match": "^SourceServer\\\\(.*)$",
      "Replacement": "TargetServer\\SourceServer\\$1"
    }
  ]
}
```

This prepends the source project name to the target set of nodes, useful when the target project already has nodes and you don't want to merge them together.

## Using Filters

You can also use `Filters` to control which nodes are migrated before applying mappings:

```json
"Iterations": {
  "Filters": ["*\\Sprint*"],
  "Mappings": [
    {
      "Match": "^OriginalProject\\\\(.*)$",
      "Replacement": "TargetProject\\$1"
    }
  ]
},
"Areas": {
  "Filters": ["*\\Team 2", "Team 2\\*"],
  "Mappings": [
    {
      "Match": "^OriginalProject\\\\(.*)$",
      "Replacement": "TargetProject\\$1"
    }
  ]
}
```

Filters use glob patterns and are applied before mappings. You can exclude specific paths by prefixing with `!`.

![Regular Expression Example](https://github.com/nkdAgility/azure-devops-migration-tools/assets/5205575/2cf50929-7ea9-4a71-beab-dd8ff3b5b2a8)

## Complete Example Configuration

Here's a complete example showing the TfsNodeStructureTool configuration with both Areas and Iterations mappings:

```json
{
  "$type": "TfsNodeStructureToolOptions",
  "Enabled": true,
  "Areas": {
    "Filters": [],
    "Mappings": [
      {
        "Match": "^Skypoint Cloud$",
        "Replacement": "MigrationTest5"
      }
    ]
  },
  "Iterations": {
    "Filters": [],
    "Mappings": [
      {
        "Match": "^Skypoint Cloud\\\\Sprint 1$",
        "Replacement": "MigrationTest5\\Sprint 1"
      }
    ]
  },
  "ShouldCreateMissingRevisionPaths": true,
  "ReplicateAllExistingNodes": true
}
```

For more detailed information and advanced configuration options, refer to the complete [TFS Node Structure Tool documentation]({{< ref "docs/reference/tools/tfsnodestructuretool" >}}).
