---
title: Permissions
short_title: Permissions
description: |
  The current permissions are governed by the requirements of the TFS Client Object Model from Microsoft.
  While Microsoft announced the deprecation of the WIT and Test Client OM in 2020, it remains the only consistent method for interacting with versions of TFS from 2010 to 2018.
date: 2025-06-24T12:07:31Z
discussionId:
aliases:
  - /permissions/
---

The current permissions are governed by the requirements of the [TFS Client Object Model](https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops) from Microsoft. While Microsoft [announced the deprecation of the WIT and Test Client OM](https://devblogs.microsoft.com/devops/announcing-the-deprecation-of-the-wit-and-test-client-om-at-jan-1-2020-2/) in 2020, it remains the only consistent method for interacting with versions of TFS from 2010 to 2018. We are in the process of migrating our tools to the REST API, but this is a large effort and will take some time to complete.

The Azure DevOps Migration Tools use a flag to bypass the Work Item rules engine, allowing data to be written into TFS/VSTS in ways that might not comply with the usual rules. For example, you can directly transition an item into the `Closed` state without starting at `New`. This is highly beneficial for migrations but requires specific pre-requisites.

_Note: According to the Azure DevOps product team, the Object Model API only works with full-scoped PATs, meaning it is incompatible with PATs that have limited scopes._

## Source Permissions

The current minimum required permissions for running the tools are:

- Membership in the "Project Collection Administrator" group – This will override any 'denied' permissions, ensuring a smooth migration.
- A PAT (Personal Access Token) with "full access."

_Note: Although we do not write data to the source system, we still require a PAT with full access._

## Target Permissions

The current minimum required permissions for running the tools are:

- Membership in the "Project Collection Administrator" group – This overrides any 'denied' permissions and allows the tools to bypass the Work Item rules engine.
- Membership in the "Project Collection Automation" group – This grants the "Make requests on behalf of others" permission.
- A PAT with "full access."

### Unsupported Permissions for Scoped PATs

In some cases, the tools may function with fewer permissions, but the following configurations have not been fully tested and are not officially supported:

- Project and Team (Read, Write, and Manage)
- Work Items (Read, Write, and Manage)
- Identity (Read and Manage)
- Security (Manage)

If you try these settings, please share your results with us!

### Granting "Make requests on behalf of others" in Older TFS Versions

To set the "Changed by" field to a user other than the one running the migration, you must grant the user the "Make requests on behalf of others" permission. This permission is not included by default for "Project Collection Administrator" users. In older versions of TFS, it can only be assigned by adding the user to the "Project Collection Service Accounts" group.

You can use the following command to do this:

```cmd
tfssecurity /g+ "Project Collection Service Accounts" n:domainusername ALLOW /server:http://myserver:8080/tfs
```

This step is not required for Azure DevOps Service targets, as `tfssecurity` is not available in that environment.
