---
classData:
  optionsClassName: FieldSkipMapConfig
  optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldSkipMapConfig
  configurationSamples:
  - name: default
    description: 
    sample: >-
      {
        "$type": "FieldSkipMapConfig",
        "WorkItemTypeName": "*",
        "targetField": "System.Description"
      }
    sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldSkipMapConfig
  description: Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.
  className: FieldSkipMapConfig
  typeName: FieldMaps
  architecture: v1
  options:
  - parameterName: targetField
    type: String
    description: missng XML code comments
    defaultValue: missng XML code comments
  - parameterName: WorkItemTypeName
    type: String
    description: missng XML code comments
    defaultValue: missng XML code comments
jekyllData:
  redirectFrom: []
  permalink: /Reference/v1/FieldMaps/FieldSkipMapConfig/

---