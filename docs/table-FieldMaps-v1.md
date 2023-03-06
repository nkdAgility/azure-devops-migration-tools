| FieldMaps | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [FieldBlankMapConfig](Reference/v1/FieldMaps/FieldBlankMapConfig.md) | ready | Work Item | Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue. |
| [FieldClearMapConfig](Reference/v1/FieldMaps/FieldClearMapConfig.md) | ready | Work Item | Allows you to set an already populated field to Null. This will only work with fields that support null. |
| [FieldLiteralMapConfig](Reference/v1/FieldMaps/FieldLiteralMapConfig.md) | ready | Work Item Field | Sets a field on the `target` to b a specific value. |
| [FieldMergeMapConfig](Reference/v1/FieldMaps/FieldMergeMapConfig.md) | ready | Work Item Field | Ever wanted to merge two or three fields? This mapping will let you do just that. |
| [FieldtoFieldMapConfig](Reference/v1/FieldMaps/FieldtoFieldMapConfig.md) | ready | Work Item Field | Just want to map one field to another? This is the one for you. |
| [FieldtoFieldMultiMapConfig](Reference/v1/FieldMaps/FieldtoFieldMultiMapConfig.md) | ready | Work Item Field | Want to setup a bunch of field maps in a single go. Use this shortcut! |
| [FieldtoTagMapConfig](Reference/v1/FieldMaps/FieldtoTagMapConfig.md) | ready | Work Item Field | Want to take a field and convert its value to a tag? Done... |
| [FieldValueMapConfig](Reference/v1/FieldMaps/FieldValueMapConfig.md) | ready | Work Item Field | Need to map not just the field but also values? This is the default value mapper. |
| [FieldValuetoTagMapConfig](Reference/v1/FieldMaps/FieldValuetoTagMapConfig.md) | ready | Work Item Field | Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target. |
| [MultiValueConditionalMapConfig](Reference/v1/FieldMaps/MultiValueConditionalMapConfig.md) | ready | Work Item Field | ??? If you know how to use this please send a PR :) |
| [RegexFieldMapConfig](Reference/v1/FieldMaps/RegexFieldMapConfig.md) | ready | Work Item Field | I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done. |
| [TreeToTagMapConfig](Reference/v1/FieldMaps/TreeToTagMapConfig.md) | ready | Work Item Field | Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path... |
