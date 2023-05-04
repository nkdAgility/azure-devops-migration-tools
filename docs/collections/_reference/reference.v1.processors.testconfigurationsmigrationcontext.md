---
classData:
  optionsClassName: TestConfigurationsMigrationConfig
  optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestConfigurationsMigrationConfig
  configurationSamples:
  - name: default
    description: 
    sample: >-
      {
        "$type": "TestConfigurationsMigrationConfig",
        "Enabled": false
      }
    sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestConfigurationsMigrationConfig
  description: This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`.
  className: TestConfigurationsMigrationContext
  typeName: Processors
  architecture: v1
  options:
  - parameterName: Enabled
    type: Boolean
    description: missng XML code comments
    defaultValue: missng XML code comments
jekyllData:
  redirectFrom:
  - /Reference/v1/Processors/TestConfigurationsMigrationConfig/
  permalink: /Reference/v1/Processors/TestConfigurationsMigrationContext/

---