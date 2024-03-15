---
title: v1 Reference Overview
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
---

The system works by setting one or more [Processors](../v1/Processors/index.md) in the json 
configuration file. You can configure one or more [FieldMaps](../v1/FieldMaps/index.md) to 
manipulate the data during a migration.

### What types of things do we have

- **[Processors](../v1/Processors/index.md)** - Processors allow you to move different types of data between `Endpoints` and does not care what `Endpoint` you have on each end.
- **[FieldMaps](../v1/FieldMaps/index.md)** - 

## Full Config File

This config is for reference only. It has things configured that you will not need, and that may conflict with each other.

{% highlight JSON %}
{% include sampleConfig/configration-demo-v15.0.json %}
{% endhighlight %}
