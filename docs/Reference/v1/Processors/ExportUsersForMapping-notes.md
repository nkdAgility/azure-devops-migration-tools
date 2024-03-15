There was a request to have the ability to map users to try and maintain integrity across different systems. We added a `TfsUserMappingEnricher` that allows you to map users from Source to Target.

##How it works

1. Run `ExportUsersForMappingConfig` which will export all of the Users in Source Mapped or not to target.
2. Run `WorkItemMigrationConfig` which will run a validator by detail to warn you of missing users. If it finds a mapping it will convert the field... 

## ExportUsersForMappingConfig

Running the `ExportUsersForMappingConfig` to get the list of users will produce something like:

```
[
  {
    "Source": {
      "FriendlyName": "Martin Hinshelwood nkdAgility.com",
      "AccountName": "martin@nkdagility.com"
    },
    "target": {
      "FriendlyName": "Hinshelwood, Martin",
      "AccountName": "martin@othercompany.com"
    }
  },
  {
    "Source": {
      "FriendlyName": "Rollup Bot",
      "AccountName": "Bot@nkdagility.com"
    },
    "target": {
      "FriendlyName": "Service Account 4",
      "AccountName": "randoaccount@somecompany.com"
    }
  },
  {
    "Source": {
      "FriendlyName": "Another non mapped Account",
      "AccountName": "not-mapped@nkdagility.com"
    },
    "target": null
  }
]
```

Any `null` in the target field means that the user is not mapped. You can then use this to create a mapping file will all of your users.

IMPORTANT: The Friendly name in Azure DevOps / TFS is not nessesarily the AAD Friendly name as users can change this in the tool. We load all of the users from both systems, and match on "email" to ensure we only assume mapping for the same user. Non mapped users, or users listed as null, will not be mapped.

### Notes

- On `ExportUsersForMappingConfig` you can set `OnlyListUsersInWorkItems` to filter the mapping based on the scope of the query. This is greater if you have many users.
- Configured using the `TfsUserMappingEnricherOptions` setting in `CommonEnrichersConfig`

## WorkItemMigrationConfig

When you run the `WorkItemMigrationContext`


```
...
  "LogLevel": "Debug",
  "CommonEnrichersConfig": [
    {
      "$type": "TfsUserMappingEnricherOptions",
      "Enabled": true,
      "UserMappingFile": "C:\\temp\\userExport.json",
      "IdentityFieldsToCheck": [
        "System.AssignedTo",
        "System.ChangedBy",
        "System.CreatedBy",
        "Microsoft.VSTS.Common.ActivatedBy",
        "Microsoft.VSTS.Common.ResolvedBy",
        "Microsoft.VSTS.Common.ClosedBy"
      ]
    }
  ],
...
```


### Notes

- Configured using the `TfsUserMappingEnricherOptions` setting in `CommonEnrichersConfig`
- Applies to all identity fields specified in the list