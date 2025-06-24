---
title: "How-To: Creating area and iteration maps"
short_title: Creating area and iteration maps
description: |
  This guide explains how to create area and iteration maps for Azure DevOps Migration Tools, allowing you to adapt old locations to new ones during migration.
discussionId: 1846
date: 2025-06-24T12:07:31Z
---

As per the [documentation](/Reference/Tools/TfsNodeStructureTool/), you need to add Iteration Maps and Area Maps that adapt the old locations to new ones that are valid in the Target.

Before your migration starts, it will validate that all of the Areas and Iterations from the **Source** work items revisions exist on the **Target**. Any that do not exist will be flagged in the logs, and if you have `"StopMigrationOnMissingAreaIterationNodes": true,` set, the migration will stop just after it outputs a list of the missing nodes.

Our algorithm that converts the Source nodes to Target nodes processes the [mappings](/Reference/Tools/TfsNodeStructureTool/) at that time. This means that any valid mapped nodes will never be caught by the `This path is not anchored in the source project` message, as they are already altered to be valid.

> We recently updated the logging for this part of the system to more easily debug both your mappings and to see what the system is doing with the nodes and their current state. You can set `"LogLevel": "Debug"` to see the details.

To add a mapping, you can follow [the documentation](/Reference/Tools/TfsNodeStructureTool/) with this being the simplest way:

```json
"IterationMaps": {
  "WorkItemMovedFromProjectName\\\\Iteration 1": "TargetProject\\Sprint 1"
},
"AreaMaps": {
   "WorkItemMovedFromProjectName\\\\Team 2": "TargetProject\\ProductA\\Team 2"
}
```

Or you can use regular expressions to match the missing area or iteration paths:

```json
"IterationMaps": {
  "^OriginalProject\\\\Path1(?=\\\\Sprint 2022)": "TargetProject\\AnotherPath\\NewTeam",
  "^OriginalProject\\\\Path1(?=\\\\Sprint 2020)": "TargetProject\\AnotherPath\\Archives\\Sprints 2020",
  "^OriginalProject\\\\Path2": "TargetProject\\YetAnotherPath\\Path2"
},
"AreaMaps": {
  "^OriginalProject\\\\(DescopeThis|DescopeThat)": "TargetProject\\Archive\\Descoped\\",
  "^OriginalProject\\\\(?!DescopeThis|DescopeThat)": "TargetProject\\NewArea\\"
}
```

If you want to use the matches in the replacement, you can use the following:

```json
"IterationMaps": {
  "^\\\\oldproject1(?:\\\\([^\\\\]+))?\\\\([^\\\\]+)$": "TargetProject\\Q1$2"
}
```

If the old iteration path was `\oldproject1\Custom Reporting\Sprint 13`, this would result in a match for each Iteration node after the project node. You would then be able to reference any of the nodes using "$" and the number of the match.

Regular expressions are much more difficult to build and debug, so it is a good idea to use a [regular expression tester](https://regex101.com/) to check that you are matching the right things and to build them in ChatGPT.

_NOTE: You need `\\` to escape a `\` in the pattern, and `\\` to escape a `\` in JSON. Therefore, on the left of the match, you need 4 `\` to represent the `\\` for the pattern and only 2 `\` in the match._

## Some pretty cool mappings

```json
"^OldProjectName([\\\\]?.*)$": "NewProjectName$1"
```

or

```json
"^OldProjectName([\\\\]?.*)$": "NewProjectName"
```

The first one maps all `OldProjectName` area or iterations to a similar new node. If you have `CreateMissingNodes` enabled, it will create that. The second will just map all `OldProjectName` to the new project name root.

![image](https://github.com/nkdAgility/azure-devops-migration-tools/assets/5205575/2cf50929-7ea9-4a71-beab-dd8ff3b5b2a8)

```

```
