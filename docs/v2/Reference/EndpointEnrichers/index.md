## Endpoint Enrichers
**_This documentation is for a preview version of the Azure DevOps Migration Tools._**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](../Endpoints/index.md) > *Endpoint Enrichers*

Endpoint Enrichers are run within the context of the Endpoint that they are configured for. Many enpoints are flexable, however there are allso enrichers that only work with certain Endpoints.

Endpoint Enricher | Data Target | Description
----------|-----------|------------
WorkItemAttachmentEnricher | Attachments | TBA
WorkItemLinkEnricher | Links | TBA
WorkItemCreatedEnricher | CreatedDate | TBA
WorkItemEmbedEnricher | HTML Fileds | TBA
WorkItemFieldTableEnricher | History | TBA


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
