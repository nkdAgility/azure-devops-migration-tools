---
title: Setting up the Azure DevOps Migration Tools
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
redirect_from: 
 - /preview/setup/
---

To use these tools you will need to install them and configure your target environment (TFS or Azure DevOps). The following pages will guide you through the process:

1. [Installation](installation.md)

The tools are run using `devopsmigration` from the command line. You can use the `--help` option to see the available commands.

```shell
devopsmigration --help
```

2. [Permissions](permissions.md)

The tools require specific permissions on TFS or Azure DevOps to be able to run. This page will guide you through the minimum permissions required to run the tools.

3. [ReflectedWorkItemId](reflectedworkitemid.md)

We use a field on the work item to track the migration of work items. This field is always referred to in the docs as `ReflectedWorkItemId` and is used to track the work item in the target. It enables the ability to resume migrations as well as to be able to scope the work items based on a query and have multiple runs overlap.

1. [Getting Started](../getstarted/)

