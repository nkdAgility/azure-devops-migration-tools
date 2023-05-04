---
title: Endpoint Enrichers
layout: page
template: index-template.md
pageType: index
toc: true
pageStatus: generated
discussionId: 
---

Endpoint Enrichers are run within the context of the Endpoint that they are configured for. Many endpoints are flexible, however there are also enrichers that only work with certain Endpoints.

| Endpoint Enricher          | Data Target | Description |
| -------------------------- | ----------- | ----------- |
| WorkItemAttachmentEnricher | Attachments | TBA         |
| WorkItemLinkEnricher       | Links       | TBA         |
| WorkItemCreatedEnricher    | CreatedDate | TBA         |
| WorkItemEmbedEnricher      | HTML Fields | TBA         |
| WorkItemFieldTableEnricher | History     | TBA         |


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
