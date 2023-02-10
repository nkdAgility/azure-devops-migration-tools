## FieldMaps

[Overview](.././index.md) > [Reference](../index.md) > *FieldMaps*

These fieldMaps are provided to allow you to mondify the data as you do the migration.

| FieldMaps | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [FieldBlankMapConfig](/docs/Reference/v1/FieldMaps/FieldBlankMapConfig.md) | ready | Work Item Field | Allows you to blank an already populated field |
| [FieldLiteralMapConfig](/docs/Reference/v1/FieldMaps/FieldLiteralMapConfig.md) | ready | Work Item Field | Sets a field on the `target` to b a specific value. |
| [FieldMergeMapConfig](/docs/Reference/v1/FieldMaps/FieldMergeMapConfig.md) | ready | Work Item Field | Ever wanted to merge two or three fields? This mapping will let you do just that. |
| [FieldtoFieldMapConfig](/docs/Reference/v1/FieldMaps/FieldtoFieldMapConfig.md) | ready | Work Item Field | Just want to map one field to another? This is the one for you. |
| [FieldtoFieldMultiMapConfig](/docs/Reference/v1/FieldMaps/FieldtoFieldMultiMapConfig.md) | ready | Work Item Field | Want to setup a bunch of field maps in a single go. Use this shortcut! |
| [FieldtoTagMapConfig](/docs/Reference/v1/FieldMaps/FieldtoTagMapConfig.md) | ready | Work Item Field | Want to take a field and convert its value to a tag? Done... |
| [FieldValueMapConfig](/docs/Reference/v1/FieldMaps/FieldValueMapConfig.md) | ready | Work Item Field | Need to map not just the field but also values? This is the default value mapper. |
| [FieldValuetoTagMapConfig](/docs/Reference/v1/FieldMaps/FieldValuetoTagMapConfig.md) | ready | Work Item Field | Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target. |
| [MultiValueConditionalMapConfig](/docs/Reference/v1/FieldMaps/MultiValueConditionalMapConfig.md) | ready | Work Item Field | ??? If you know how to use this please send a PR :) |
| [RegexFieldMapConfig](/docs/Reference/v1/FieldMaps/RegexFieldMapConfig.md) | ready | Work Item Field | I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done. |
| [TreeToTagMapConfig](/docs/Reference/v1/FieldMaps/TreeToTagMapConfig.md) | ready | Work Item Field | Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path... |



