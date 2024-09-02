---
title: Installation
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
redirect_from:
  - /installation/
---

Install the Azure DevOps Migration Tools on Windows.

These tools are available as a portable application and can be installed in a number of ways, including manually from a zip.
For a more detailed getting started guide please see the [documentation](https://nkdagility.com/learn/azure-devops-migration-tools/getting-started/).

## Option 1: Winget

We use [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/) to host the tools, and you can use the command `winget install nkdAgility.AzureDevOpsMigrationTools` to install them on Windows 10 and Windows 11. 

The tools will be installed to `%Localappdata%\Microsoft\WinGet\Packages\nkdAgility.AzureDevOpsMigrationTools_Microsoft.Winget.Source_XXXXXXXXXX` and a symbolic link to `devopsmigration.exe` that lets you run it from anywhere using `devopsmigration init`.

*NOTE: Do not install using an elevated command prompt!*

## Option 2: Chocolatey

We also deploy to [Chocolatey](https://chocolatey.org/packages/nkdagility.azuredevopsmigrationtools) and you can use the command `choco install vsts-sync-migrator` to install them on Windows Server. 

The tools will be installed to `C:\Tools\MigrationTools\` which should be added to the path. You can run `devopsmigration.exe`

## Option 3: Manual

You can download the [latest release](https://github.com/nkdAgility/azure-devops-migration-tools/releases/latest) and unzip it to a folder of your choice.

*NOTE: You may need to 'unlock' the downloaded archive before you unzop and run. Go to the file properties (click `show more options` on Win11) and click `Unblock`.*
