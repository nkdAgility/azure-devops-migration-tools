---
redirectFrom: []
layout: page
toc: true
title: Work items are not migrated!
categories:
- Work Items
---

Maybe you see a `TF237124: Work Item is not ready to save` error when you attempt to do a migration.

A number of processors have a setting `"PrefixProjectToNodes": false`. If set to true this inserts the name of the source Team Project into the created structure e.g. Area path, Iteration path, or Work Item queries. It is also used by the migration processor. 

This setting **must** be consistent across all processors in a configuration file. If it not it can often cause migrations to fail as expected paths are not present.
