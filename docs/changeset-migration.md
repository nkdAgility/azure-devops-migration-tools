---
title: Changeset Migration
layout: page
template: default
pageType: index
toc: true
pageStatus: production
discussionId: 
redirect_from: /changeset-migration.html
---

Azure DevOps Migration Tools enable migrating work item changesets with cross project links from tfvc or git.

## Migrate Changesets from TFVC to Git

For a successful migration of work items changesets from TFVC to Git you need a mapping file with ChangeSetId - CommitId values. If you 
have migrated your TFVC repository with [git-tfs](https://github.com/git-tfs/git-tfs) you can generate this file by using the following
[command](https://github.com/git-tfs/git-tfs/blob/master/doc/commands/exportmap.md). Then you need to add following line to the config json file:

"ChangeSetMappingFile": "C:\\git-tfs\\ChangeSetId-to-CommitId\\{mappingFile}",

### Migrate Changesets from TFVC to Git with external project links

If your changesets contain cross project links make sure to update the "GitRepoMapping" configuration section.

## Use Cases

Lets assume you have Project A with a TFVC repository and a Project B with Work Items, which changesets link to Project A. 
You wish to migrate Project A to a Git Repository in Project C and move the work Items including changesets from Project B to Project C and all
changesets from Project B needs to move to the Git Repository of Project C. To achieve this follow the steps:

1. Migrate your repository and make sure to create the mapping file. 
2. Edit the configuration file with following settings: 

"GitRepoMapping": {"A" :"C","B" :"C"}

3. Run the migration with configuration file that contain

"ChangeSetMappingFile": "C:\\git-tfs\\ChangeSetId-to-CommitId\\{mappingFile}",
