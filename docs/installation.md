---
title: Installation
layout: page
pageType: index
template: getting-started-template.md
toc: true
pageStatus: published
discussionId: 
redirect_from: /getting-started.html
---

## Installing and running the tools

These tools are available as a portable application and can be installed in a number of ways, including manually from a zip.
For a more detailed getting started guide please see the [documentation](https://nkdagility.com/docs/azure-devops-migration-tools/getting-started.html).

### Option 1: Winget

We use [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/) to host the tools, and you can use the command `winget install nkdAgility.AzureDevOpsMigrationTools` to install them on Windows 10 and Windows 11. 

The tools will be installed to `%Localappdata%\Microsoft\WinGet\Packages\nkdAgility.AzureDevOpsMigrationTools_Microsoft.Winget.Source_XXXXXXXXXX` and a symbolic link to `devopsmigration.exe` that lets you run it from anywhere using `devopsmigration init`.

**NOTE: Do not install using an elevated command prompt!**

### Option 2: Chocolatey

We also deploy to [Chocolatey](https://chocolatey.org/packages/nkdagility.azuredevopsmigrationtools) and you can use the command `choco install vsts-sync-migrator` to install them on Windows Server. 

The tools will be installed to `C:\Tools\MigrationTools\` which should be added to the path. You can run `devopsmigration.exe`

### Option 3: Manual

You can download the [latest release](https://github.com/nkdAgility/azure-devops-migration-tools/releases/latest) and unzip it to a folder of your choice.

## Minimum Permission Requirements

At this time the documented minimum required permissions for running the tools are:

- Account in both the source and target projects with "Project Collection Administrator" rights
- PAT with "full access" for both the Source and the Target

Note: I have been informed by the Azure DevOps product team information that ObjectModel API only works with full scoped PATs, so it won't work with any PAT that has specific scopes. 
