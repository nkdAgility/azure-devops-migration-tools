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

The permissions are courently governed by the needs of the [TFS Client Object Model](https://learn.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops) from Microsoft. Microsoft [Announced the deprecation of the WIT and Test Client OM](https://devblogs.microsoft.com/devops/announcing-the-deprecation-of-the-wit-and-test-client-om-at-jan-1-2020-2/) in 2020, however this is still the only way to interact with versions of TFS from 2010 to 2018 with any consistantcy. We are working on moving our tools to the REST API, but this is a large task and will take some time.

## Minimum Permission Requirements

At this time the documented minimum required permissions for running the tools are:

- Account in both the source and target projects with "Project Collection Administrator" rights
- PAT with "full access" for both the Source and the Target

Note: I have been informed by the Azure DevOps product team information that ObjectModel API only works with full scoped PATs, so it won't work with any PAT that has specific scopes. 

### However! Unsupported Permission Options

We have seen that the tools may work with less permissions however the following has not been full tested and is not currently supported:

- Project and Team (Read, write, & manage)
- Work Items (Read, Write & Manage)
- Identity (Read & Manage)
- Security (Manage)

If you do try this out then please let us know how you get on!

## TFS, VSTS, & Azure DevOps Server Configuration (On-Premises Only)

There are some additional requirements that you will need to meet in order to use the tool against your TFS or VSTS server. The Azure DevOps Migration Tools use a flag to Bypass the Work Item rules engine and write data into TFS\VSTS that may not comply with the rules. For example you can write data directly into the `Closed` state without starting at `New`. This is very useful for migrations but requires some pre-requisites.

## Bypass Rules

For on-premises instances you need to be part of the `Project Collection Service Accounts` group. You can do this by calling the following command:

`tfssecurity /g+ "Project Collection Service Accounts" n:domainusername ALLOW /server:http://myserver:8080/tfs`

This is not required for Azure DevOps Service targets, but you do need to be a Collection Admin on the Target.