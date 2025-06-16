---
optionsClassName: TfsGitRepositoryToolOptions
optionsClassFullName: MigrationTools.Tools.TfsGitRepositoryToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsGitRepositoryToolOptions",
      "Enabled": false,
      "ShouldDropChangedSetLinks": false,
      "Mappings": {}
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
description: missing XML code comments
className: TfsGitRepositoryTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: Mappings
  type: Dictionary
  description: Dictionary mapping source repository names to target repository names. Used to update Git repository links and references in work items during migration.
  defaultValue: '{}'
- parameterName: ShouldDropChangedSetLinks
  type: Boolean
  description: When set to true, changeset links in work items will be removed during migration to prevent broken links when repositories are not migrated.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsGitRepositoryTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsGitRepositoryToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsGitRepositoryToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsGitRepositoryTool/
title: TfsGitRepositoryTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/TfsGitRepositoryTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsGitRepositoryTool-introduction.md
  exists: false
  markdown: ''

---