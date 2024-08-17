
### Example Full Migration from v12.0

The following file is an example that can be used in your `configuration.json` file to migrate Azure DevOps pipelines.
```json
{
    "GitRepoMapping": null,
    "LogLevel": "Information",
    "Processors": [
      {
        "$type": "AzureDevOpsPipelineProcessorOptions",
        "Enabled": true,
        "MigrateBuildPipelines": true,
        "MigrateReleasePipelines": true,
        "MigrateTaskGroups": true,
        "MigrateVariableGroups": true,
        "MigrateServiceConnections": true,
        "BuildPipelines": null,
        "ReleasePipelines": null,
        "RefName": null,
        "SourceName": "Source",
        "TargetName": "Target",
        "RepositoryNameMaps": {}
      }
    ],
    "Version": "12.0",
    "Endpoints": {
      "AzureDevOpsEndpoints": [
        {
          "name": "Source",
          "$type": "AzureDevOpsEndpointOptions",
          "Organisation": "https://dev.azure.com/source-org/",
          "Project": "Awesome project",
          "AuthenticationMode": "AccessToken",
          "AccessToken": "xxxxxx",
          "EndpointEnrichers": null
        },
        {
          "Name": "Target",
          "$type": "AzureDevOpsEndpointOptions",
          "Organisation": "https://dev.azure.com/target-org/",
          "Project": "Cool project",
          "AuthenticationMode": "AccessToken",
          "AccessToken": "xxxxxx",
          "EndpointEnrichers": null
        }
      ]
    }
  }
```

If the repository in the target has a different name from the one that was used in the source project, you should map it.
In the example above replace `"RepositoryNameMaps": {}` with the following:
```json
"RepositoryNameMaps": {
    "Awesome project": "Cool project"
}
```

# Important note
When the application is creating service connections make sure you have proper permissions on Azure Active Directory and you can grant Contributor role to the subscription that was chosen.