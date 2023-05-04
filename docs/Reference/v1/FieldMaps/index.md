---
title: FieldMaps
layout: page
template: index-template.md
pageType: index
toc: true
pageStatus: generated
discussionId: 
---

These fieldMaps are provided to allow you to mondify the data as you do the migration.

| FieldMaps | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [FieldClearMapConfig](FieldClearMapConfig.md) | ready | Work Item | Allows you to set an already populated field to Null. This will only work with fields that support null. |
| [FieldLiteralMapConfig](FieldLiteralMapConfig.md) | ready | Work Item Field | Sets a field on the `target` to b a specific value. |
| [FieldMergeMapConfig](FieldMergeMapConfig.md) | ready | Work Item Field | Ever wanted to merge two or three fields? This mapping will let you do just that. |
| [FieldSkipMapConfig](FieldSkipMapConfig.md) | ready | Work Item | Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue. |
| [FieldtoFieldMapConfig](FieldtoFieldMapConfig.md) | ready | Work Item Field | Just want to map one field to another? This is the one for you. |
| [FieldtoFieldMultiMapConfig](FieldtoFieldMultiMapConfig.md) | ready | Work Item Field | Want to setup a bunch of field maps in a single go. Use this shortcut! |
| [FieldtoTagMapConfig](FieldtoTagMapConfig.md) | ready | Work Item Field | Want to take a field and convert its value to a tag? Done... |
| [FieldValueMapConfig](FieldValueMapConfig.md) | ready | Work Item Field | Need to map not just the field but also values? This is the default value mapper. |
| [FieldValuetoTagMapConfig](FieldValuetoTagMapConfig.md) | ready | Work Item Field | Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target. |
| [MultiValueConditionalMapConfig](MultiValueConditionalMapConfig.md) | ready | Work Item Field | ??? If you know how to use this please send a PR :) |
| [RegexFieldMapConfig](RegexFieldMapConfig.md) | ready | Work Item Field | I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done. |
| [TreeToTagMapConfig](TreeToTagMapConfig.md) | ready | Work Item Field | Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path... |



