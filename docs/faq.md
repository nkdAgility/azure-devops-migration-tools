# VSTS Sync Migration Tools FAQ

## What is this tool for?

Have a look at the [why page](why.md) for a description as to why this tool exists.

## What processors are available?

As this is an active project the list of processors is ever changing. The current list can be seen on this [documentation's home page](index.md)

## Can this tool move source code too?

No, this tools has no processors for source code migration. You need to use other tools to do that.

## I run 'vstssyncmigrator execute -c VstsBulkEditor.json` but nothing happens

If you have generated the default `VstsBulkEditor.json` configuration file using the `vstssyncmigrator init` command then the chances are nothing is happening because all the processors are set to `"Enabled": false`. 

You need to enable at least one processor before anything will happen. 

## Wow that configuration file looks complex!

Chances are you will not need all the section in the default generated `VstsBulkEditor.json` configuration file.

It is often best to use this file as ane example and to build up your own configuration file up from scratch with just the settings you need.

## The processors are not running in the order I need.

If you find processors are running in an incorrect order for your needs just re-order their declarations in the `VstsBulkEditor.json` configuration file. The processsors are run in the order they are shown.

## Every test migration I do is slow

You can often speed up your testing by disabling processors that have previous run successfully by editing processors  `"Enabled": true` settings.

You do have to be careful with this process, it does assume that you are not clearing down the target system between tests, and that what has been migrated in past test runs is correct for the processor you are testing.

That said it is often a good idea to build up your configuration processor by processor. Proving each one works as required before adding the next one.

## No work items get migrate

Maybe you see a `TF237124: Work Item is not ready to save` error when you atempt to do a migration.

A number of processors have a setting `"PrefixProjectToNodes": false`. If set to true this inserts the name of the source Team Project into the created structure e.g. Area path, Iteration path, or Work Item queries. It is also used by the migration processor. 

This setting **must** be consistent across all processors in a configuration file. If it not it can often cause migrations to fail as expected paths are not present.

## My test migration are skipping items

This tools is designed to do sync and/or migrations. This means it detects if an item has already been migrated. If it finds a migrated item it will be skipped. 

If you want to fully test a migration it is often easiest to delete the target Team Project and recreate it, rather than try to delete all the previously migrated items.

Also remember that the `ReflectedWorkItemId` value will have been set on the source system. This can mean the work item select queries may not give the expect results, unless this field is cleared between test runs.

## I don't understand what the 'ReflectedWorkItemId' is for.

To provide sync as well as migration it is important that the tools knows which items have already been migrated. This is the purpose of the `ReflectedWorkItemId` field. 

This field needs to be added to both the source and target team projects. On both system the URL pointing to the migrated item on the other system is stored here. This means there is an easy way for a user to trace work items between the source and target systems (and vice versa).

How the `ReflectedWorkItemId` field is added depends on whether the system, there are three options

- TFS
- VSTS using the new process customisation process
- VSTS when the instance has been imported using the  [VSTS Migration Tool](https://blogs.msdn.microsoft.com/visualstudioalm/2016/11/16/import-your-tfs-database-into-visual-studio-team-services/)

For details on how to add the field in each case, and trouble shooting check the [server configuration page](server-configuration.md)