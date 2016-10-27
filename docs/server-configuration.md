# Server Configuration and Requirements

There are some requirements that you will need to meet in order to use the tool against your TFS or VSTS server. The VSTS Bulk Data Editor uses a flag to Bypass the Work Item rules engine and write data into TFS\VSTS that may not comply with the rules. For example you can write data directly into the `Closed` state without starting at `New`. This is very usefull for migrations but requires some pre-requisits.

## Bypass Rules

You need to be part of the `Project Collection Service Accounts` group. You can do this by calling the following command:

`tfssecurity /g+ "Project Collection Service Accounts" n:domainusername ALLOW /server:http://myserver:8080/tfs`

## Migration State

### Team Foundation Server (TFS)

In order to store the state for the migration you need to use a custom field. Use the `witadmin exportwitd` command to export each work item adn add:

```xml
<FIELD name="ReflectedWorkItemId" refname="TfsMigrationTool.ReflectedWorkItemId" type="String" />
```

to the Fields section, then use `witadmin importwitd` to re-import the customised work item type template.

### Visual Studio Team Services (VSTS)

You need to [add a custom field through the VSTS UI](https://blogs.msdn.microsoft.com/visualstudioalm/2015/12/10/adding-a-custom-field-to-a-work-item/) to be able to use the tool.

