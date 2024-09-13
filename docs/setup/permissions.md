---
title: Permissions
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
redirect_from:
  - /permissions/
---

The permissions are currently governed by the needs of the [TFS Client Object Model](https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops) from Microsoft. Microsoft [Announced the deprecation of the WIT and Test Client OM](https://devblogs.microsoft.com/devops/announcing-the-deprecation-of-the-wit-and-test-client-om-at-jan-1-2020-2/) in 2020, however this is still the only way to interact with versions of TFS from 2010 to 2018 with any consistency. We are working on moving our tools to the REST API, but this is a large task and will take some time.

 The Azure DevOps Migration Tools use a flag to Bypass the Work Item rules engine and write data into TFS\VSTS that may not comply with the rules. For example you can write data directly into the `Closed` state without starting at `New`! This is very useful for migrations but requires some pre-requisites.

_Note: I have been informed by the Azure DevOps product team that *ObjectModel API only works with full scoped PATs*, so it won't work with any PAT that has specific scopes._

## Source Permissions

At this time the documented minimum required permissions for running the tools are:

- An account in the "Project Collection Administrator" group  - This will override any 'denied' permissions enabling a smooth migration
- PAT with "full access"

Note: We never write to the source system, but still require a PAT with full access as specified above.

## Target permissions

At this time the documented minimum required permissions for running the tools are:

- An account in the "Project Collection Administrator" group - This will override any 'denied' permissions enabling a smooth migration and allow the tools to bypass the Work Item rules engine.
- An account in the "Project Collection Automation" group -  This grants "Make requests on behalf of others"
- PAT with "full access"

### Unsupported Permission Options for Scoped PATs

We have seen that the tools may work with less permissions however the following has not been full tested and is not currently supported:

- Project and Team (Read, write, & manage)
- Work Items (Read, Write & Manage)
- Identity (Read & Manage)
- Security (Manage)

If you do try this out then please let us know how you get on!

### Granting "Make requests on behalf of others" on older TFS versions

In order to have the "Changed by" field set to a user other than the user that is running the migration, you need to grant the user the "Make requests on behalf of others" permission. This is not granted by default for "Project Collection Administrator" users and on TFS you can only get it by adding the user to the "Project Collection Service Accounts" group.

```cmd
tfssecurity /g+ "Project Collection Service Accounts" n:domainusername ALLOW /server:http://myserver:8080/tfs
```

This is not required for Azure DevOps Service targets as `tfssecurity` is not available. For Azure DevOps 