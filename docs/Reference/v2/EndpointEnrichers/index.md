---
title: Endpoint Enrichers
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
---

Endpoint Enrichers are run within the context of the Endpoint that they are configured for. Many endpoints are flexible, however there are also enrichers that only work with certain Endpoints.

{% include content-collection-table.html collection = "reference" typeName = "EndpointEnrichers" architecture = "v2" %}


### Endpoint Enricher Options

All Endpoint Enrichers have a minimum set of options that are required to run. 

#### Minimum Options to run

The `Enabled` options is common to all Endpoint Enrichers.


```JSON
    {
      "ObjectType": "EndpointEnrichersOptions",
      "Enabled": true,
    }
```
