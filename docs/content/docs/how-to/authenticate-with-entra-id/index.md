---
title: 'How-To: Authenticate with Microsoft Entra ID'
description: |
  How to sign in to Azure DevOps with Microsoft Entra ID instead of a PAT, as a user or as an app, with a different identity on each endpoint if you need one.
short_title: Entra ID sign-in
date: 2026-11-19T00:00:00Z

---
## Overview

With `AuthenticationMode` set to `EntraId`, no token goes in the configuration. The tool gets a Microsoft Entra ID token for Azure DevOps and renews it for as long as the migration runs.

### How It Works

Each endpoint (Source, Target, or any REST endpoint) has its own sign-in, its own token and its own renewal, so Source and Target can be different identities in different tenants.

- A token is reused until 5 minutes before it expires, then renewed.
- If Azure DevOps rejects a token (401), the tool gets a new one and sends the request again once.
- At connect, the log shows each endpoint's credential, identity, tenant and expiry. The token itself is never logged.

### Use Cases

Signing in as yourself is the recommended way to run a migration:

- **Recommended:** you are signed in with `az login`: use the one-line configuration below.
- You want to sign in as a specific user, in a browser or with a device code.
- Source and Target in different tenants, each with its own user.
- **Currently untested:** an unattended run as an app (certificate, client secret or managed identity).

> **Currently untested:** `Certificate`, `ClientSecret` and `ManagedIdentity` are built and unit-tested, and their failure messages have been checked, but no full migration has yet been run with an app identity. The run logs a warning when you use one. Try them on a test project first, and please report what you find.

## Configuration Structure

### Basic Examples

The recommended configuration. If you are signed in with `az login`, Visual Studio, Azure PowerShell or azd, this is all you need:

```json
"Authentication": {
  "AuthenticationMode": "EntraId"
}
```

The tool uses the first sign-in it finds on the machine. To use only one of them, set the `AZURE_TOKEN_CREDENTIALS` environment variable to its credential name, for example `AzureCliCredential`.

On the `AzureDevOpsEndpoint` (REST) the settings sit at the endpoint's top level: `"AuthenticationMode": "EntraId"` and, when needed, `"EntraId": { ... }` beside it.

### Options

Add an `EntraId` block only to choose a specific credential:

| Setting | Used by | Notes |
|---|---|---|
| `Credential` | all | `Default` (the first sign-in found, as above), `AzureCli`, `Interactive`, `DeviceCode`; currently untested: `Certificate`, `ClientSecret`, `ManagedIdentity` |
| `TenantId` | AzureCli, Interactive, DeviceCode (optional); Certificate, ClientSecret (required) | GUID or domain |
| `ClientId` | Certificate, ClientSecret (required); ManagedIdentity (user-assigned); Interactive, DeviceCode (your own public client app, optional) | GUID |
| `CertificateThumbprint` | Certificate | in `CurrentUser\My` or `LocalMachine\My`, with its private key |
| `CertificatePath`, `CertificatePassword` | Certificate | a PFX/PEM file instead of the store |
| `ClientSecret` | ClientSecret | |
| `LoginHint` | Interactive, DeviceCode | the account to sign in as |
| `AuthenticationRecordPath` | Interactive, DeviceCode | where the saved sign-in is kept |

A setting the chosen credential does not use stops the run before it starts, so a half-edited configuration cannot run as the wrong identity. `ClientSecret` and `CertificatePassword` are never written to logs or saved configurations; set them with environment variables, for example `MigrationTools__Endpoints__Target__Authentication__EntraId__ClientSecret`.

### Complex Examples

An app registration with a certificate on Target (currently untested), and the signed-in user on Source:

```json
"Source": {
  "EndpointType": "TfsTeamProjectEndpoint",
  "Collection": "https://dev.azure.com/contoso-source/",
  "Project": "Source",
  "Authentication": {
    "AuthenticationMode": "EntraId",
    "EntraId": { "Credential": "AzureCli" }
  }
},
"Target": {
  "EndpointType": "TfsTeamProjectEndpoint",
  "Collection": "https://dev.azure.com/contoso-target/",
  "Project": "Target",
  "Authentication": {
    "AuthenticationMode": "EntraId",
    "EntraId": {
      "Credential": "Certificate",
      "TenantId": "00000000-0000-0000-0000-000000000000",
      "ClientId": "00000000-0000-0000-0000-000000000000",
      "CertificateThumbprint": "0123456789ABCDEF0123456789ABCDEF01234567"
    }
  }
}
```

## Common Scenarios

### Sign in in a browser or with a device code

```json
"EntraId": { "Credential": "Interactive", "TenantId": "contoso.onmicrosoft.com", "LoginHint": "migrator@contoso.com" }
```

The first run opens the browser (`Interactive`) or prints a code to enter at the address shown (`DeviceCode`). After that:

- The sign-in record (account and tenant, no secret) is saved, by default to `%LOCALAPPDATA%\MigrationTools\auth\<endpoint>-<credential>-<tenant>.json`.
- The refresh token is kept in the operating system's protected store: DPAPI for the current user on Windows, Keychain on macOS, libsecret on Linux. It is never written unencrypted.
- Later runs sign in silently and renew for as long as they run. A run never opens a browser part way through; if the saved sign-in has expired or been revoked, it stops and tells you which file to delete.

### Run unattended with an app registration (currently untested)

Not yet run in a full migration; see the note at the top.

1. Create an app registration in Microsoft Entra ID and add a certificate (preferred) or a client secret.
2. In Azure DevOps, **Organization settings > Users > Add users**, add the app's service principal with a **Basic** access level.
3. Add it to the project with the permissions the migration needs. On the Target, for the TFS Object Model endpoints, make it a **Project Collection Administrator** or grant **Bypass rules on work item updates**, so revisions keep their original dates and authors.
4. Set `Credential` to `Certificate` or `ClientSecret` with `TenantId` and `ClientId`.

### Run on an Azure VM or container (currently untested)

Use `"Credential": "ManagedIdentity"`, with `ClientId` for a user-assigned identity. Add the identity to the organisation as above.

## Good Practices

- Prefer a certificate to a client secret, and keep secrets in environment variables.
- Pin the credential (`AzureCli`, `Certificate`, ...) for unattended runs, so the run cannot pick up another sign-in on the machine.
- Check the identity in the startup log line for each endpoint before a long run.

## Troubleshooting

Every sign-in problem stops the run with one message that names the endpoint and what to do:

| Message contains | Do this |
|---|---|
| `the Azure CLI is not signed in or not installed` | run `az login` (with `--tenant` if you set `TenantId`) |
| `the saved sign-in has expired or been revoked` | delete the file named in the message and run again to sign in |
| `AADSTS...` with Certificate or ClientSecret | check `TenantId`, `ClientId`, and that the certificate or secret is on the app and not expired |
| `no certificate with thumbprint` | import the certificate with its private key, or fix `CertificateThumbprint` |
| `no managed identity is available` | run on an Azure resource with a managed identity, or choose another credential |
| `does not use ...` / `needs ...` | fix the `EntraId` settings as the message says |

If Azure DevOps rejects the token after sign-in, the identity in the log is signed in but has no access: add it to the organisation and project.
