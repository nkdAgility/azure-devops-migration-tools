---
title: User Agent and Run Id
description: How the tool identifies itself, its version and the run on every request it sends to Azure DevOps, and how to add a label for your migration.
weight: 30
date: 2026-11-19T00:00:00Z

---
## Overview

Every request the tool sends to Azure DevOps or TFS carries a user agent that names the tool, its version and the run:

```text
AzureDevOpsMigrationTools/16.2.1 (run 3f2a9c41d0b7; +https://devopsmigration.io)
```

With a label set it becomes:

```text
AzureDevOpsMigrationTools/16.2.1 (run 3f2a9c41d0b7; wave-1; +https://devopsmigration.io)
```

### How It Works

- **Run id:** generated once each time the tool starts, and written near the top of the log as `Run Id: 3f2a9c41d0b7`, followed by the full `User Agent:`. Use it to match what the server recorded to a run log.
- **TFS object model:** the value is added to the front of the client library's own user agent before the first connection is opened, so it covers work items, attachments, test plans, queries and every other object model call.
- **REST endpoints:** the value is added to the HTTP client of every `AzureDevOpsEndpoint` request.
- **Embedded images:** the value is added to the image downloads made by the `TfsEmbededImagesTool`.

The user agent never contains customer, project or user names, URLs or tokens. The only free text is the label, and only you set it.

### Use Cases

- Telling migration traffic apart from other traffic on the organisation's **Usage** page (**Organization settings > Usage**), where rate limiting is shown.
- Giving Microsoft support the run id and version when you raise a rate limiting or throttling case.
- Telling several migration runs apart, for example one label for each wave.

## Configuration Structure

### Options

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `UserAgentLabel` | string | _none_ | Optional name for this migration, added after the run id. Only letters, digits, `-`, `.` and `_` are kept; spaces become `-`; other characters are removed. The schema allows up to 32 characters; a longer value set through an environment variable is cut to 32. |

### Sample

`UserAgentLabel` sits directly under `MigrationTools`, next to `Version`:

```json
{
  "$schema": "https://devopsmigration.io/schema/configuration.schema.json",
  "MigrationTools": {
    "Version": "16.0",
    "UserAgentLabel": "wave-1",
    "Endpoints": { },
    "Processors": [ ]
  }
}
```

You can also set it without changing the file, through the environment variable `MigrationTools__UserAgentLabel`.

## Good Practices

- Use a short, neutral label such as `wave-1` or `dry-run-2`. Do not put customer, project or user names in it: it is sent to the server with every request.
- Keep the log from each run. The `Run Id` line is what links the server's records to that log.

## Troubleshooting

- **The label is not what I set:** characters other than letters, digits, `-`, `.` and `_` are removed, and spaces become `-`. Check the `User Agent:` line near the top of the log for the value that was sent.
- **The Usage page does not show the whole value:** the Usage page shows the user agent as Azure DevOps recorded it, which may be shortened. The tool name, version and run id come first, so they are the part most likely to be shown.
