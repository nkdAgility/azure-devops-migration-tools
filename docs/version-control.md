---
title: Version Control
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
redirect_from: 
 - /preview/version-control/
---

While we do not migrate source control for you, we do have tools to maintain the links between work items and source code. 

## Git Version Control (GIT)

While we do not have any tools to migrate Git repositories, we do have tools to maintain the links between work items and source code. This is done by using the [TfsGitRepositoryTool](/_reference/reference.tools.tfsgitrepositorytool.md) configuration section.

As long as the Git repos exist and have the same name, just enabling [TfsGitRepositoryTool](/_reference/reference.tools.tfsgitrepositorytool.md) is enough. If you have diferent names for the repositories, you can use the [TfsGitRepositoryTool](/_reference/reference.tools.tfsgitrepositorytool.md) configuration section to map the source repository to the target repository.

## Team Foundation Version Control (TFVC)

Although TFVC has been depricated for quite some time there are still many folks that use it. We dont support migrating it directly, but if you have migrated your TFVC repository to Git, you can use the [TfsChangesetMappingTool](/_reference/reference.tools.tfschangesetmappingtool.md) to maintain the links between work items and changesets.

Azure DevOps Migration Tools enable migrating work item changesets with cross project links from tfvc or git.

### Migrate Changesets from TFVC to Git

For a successful migration of work items changesets from TFVC to Git you need a mapping file with ChangeSetId - CommitId values that you can had to the [TfsChangesetMappingTool](/_reference/reference.tools.tfschangesetmappingtool.md). If you 
have migrated your TFVC repository with [git-tfs](https://github.com/git-tfs/git-tfs) you can generate this file by using the following
[command](https://github.com/git-tfs/git-tfs/blob/master/doc/commands/exportmap.md). Then follow the documentation on [TfsChangesetMappingTool](/_reference/reference.tools.tfschangesetmappingtool.md) to configure the tool with this file.

#### Migrate Changesets from TFVC to Git with external project links

If your changesets contain cross project links make sure to update the "GitRepoMapping" configuration section.

##### Use Cases

Lets assume you have Project A with a TFVC repository and a Project B with Work Items, which changesets link to Project A. 
You wish to migrate Project A to a Git Repository in Project C and move the work Items including changesets from Project B to Project C and all
changesets from Project B needs to move to the Git Repository of Project C. To achieve this follow the steps:

1. Migrate your repository and make sure to create the mapping file. 
2. Edit the configuration file with following settings in [TfsGitRepositoryTool](/_reference/reference.tools.tfsgitrepositorytool.md): 

"GitRepoMapping": {"A" :"C","B" :"C"}

3. Run the migration with configuration file that contain set in [TfsChangesetMappingTool](/_reference/reference.tools.tfschangesetmappingtool.md).

"ChangeSetMappingFile": "C:\\git-tfs\\ChangeSetId-to-CommitId\\{mappingFile}",
