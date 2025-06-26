---
title: "JSON Schemas"
description: "JSON Schema definitions for Azure DevOps Migration Tools configuration"
weight: 10
outputs: ["html", "schema-catalog"]
---

The Azure DevOps Migration Tools provide JSON Schema definitions for configuration validation and IDE support.

## Main Configuration Schema

- **[Configuration Schema](configuration.schema.json)** - Complete configuration schema for Azure DevOps Migration Tools

## Component Schemas

### Processors

- [Azure DevOps Pipeline Processor](schema.processors.azuredevopspipelineprocessor.json)
- [Keep Outbound Link Target Processor](schema.processors.keepoutboundlinktargetprocessor.json)
- [Outbound Link Checking Processor](schema.processors.outboundlinkcheckingprocessor.json)
- [Process Definition Processor](schema.processors.processdefinitionprocessor.json)
- [TFS Export Profile Picture From AD Processor](schema.processors.tfsexportprofilepicturefromadprocessor.json)
- [TFS Export Users For Mapping Processor](schema.processors.tfsexportusersformappingprocessor.json)
- [TFS Import Profile Picture Processor](schema.processors.tfsimportprofilepictureprocessor.json)
- [TFS Shared Query Processor](schema.processors.tfssharedqueryprocessor.json)
- [TFS Team Settings Processor](schema.processors.tfsteamsettingsprocessor.json)
- [TFS Test Configurations Migration Processor](schema.processors.tfstestconfigurationsmigrationprocessor.json)
- [TFS Test Plans And Suites Migration Processor](schema.processors.tfstestplansandsuitesmigrationprocessor.json)
- [TFS Test Variables Migration Processor](schema.processors.tfstestvariablesmigrationprocessor.json)
- [TFS Work Item Bulk Edit Processor](schema.processors.tfsworkitembulkeditprocessor.json)
- [TFS Work Item Delete Processor](schema.processors.tfsworkitemdeleteprocessor.json)
- [TFS Work Item Migration Processor](schema.processors.tfsworkitemmigrationprocessor.json)
- [TFS Work Item Overwrite Areas As Tags Processor](schema.processors.tfsworkitemoverwriteareasastagsprocessor.json)
- [TFS Work Item Overwrite Processor](schema.processors.tfsworkitemoverwriteprocessor.json)
- [Work Item Tracking Processor](schema.processors.workitemtrackingprocessor.json)

### Tools

- [Field Mapping Tool](schema.tools.fieldmappingtool.json)
- [String Manipulator Tool](schema.tools.stringmanipulatortool.json)
- [TFS Attachment Tool](schema.tools.tfsattachmenttool.json)
- [TFS Change Set Mapping Tool](schema.tools.tfschangesetmappingtool.json)
- [TFS Embedded Images Tool](schema.tools.tfsembededimagestool.json)
- [TFS Git Repository Tool](schema.tools.tfsgitrepositorytool.json)
- [TFS Node Structure Tool](schema.tools.tfsnodestructuretool.json)
- [TFS Revision Manager Tool](schema.tools.tfsrevisionmanagertool.json)
- [TFS Team Settings Tool](schema.tools.tfsteamsettingstool.json)
- [TFS User Mapping Tool](schema.tools.tfsusermappingtool.json)
- [TFS Validate Required Field Tool](schema.tools.tfsvalidaterequiredfieldtool.json)
- [TFS Work Item Embedded Link Tool](schema.tools.tfsworkitemembededlinktool.json)
- [TFS Work Item Link Tool](schema.tools.tfsworkitemlinktool.json)
- [Work Item Type Mapping Tool](schema.tools.workitemtypemappingtool.json)

### Field Maps

- [Field Calculation Map](schema.fieldmaps.fieldcalculationmap.json)
- [Field Clear Map](schema.fieldmaps.fieldclearmap.json)
- [Field Literal Map](schema.fieldmaps.fieldliteralmap.json)
- [Field Merge Map](schema.fieldmaps.fieldmergemap.json)
- [Field Skip Map](schema.fieldmaps.fieldskipmap.json)
- [Field To Field Map](schema.fieldmaps.fieldtofieldmap.json)
- [Field To Field Multi Map](schema.fieldmaps.fieldtofieldmultimap.json)
- [Field To Tag Field Map](schema.fieldmaps.fieldtotagfieldmap.json)
- [Field Value Map](schema.fieldmaps.fieldvaluemap.json)
- [Multi Value Conditional Map](schema.fieldmaps.multivalueconditionalmap.json)
- [Regex Field Map](schema.fieldmaps.regexfieldmap.json)
- [Tree To Tag Field Map](schema.fieldmaps.treetotagfieldmap.json)

### Endpoints

- [Azure DevOps Endpoint](schema.endpoints.azuredevopsendpoint.json)
- [File System Work Item Endpoint](schema.endpoints.filesystemworkitemendpoint.json)
- [TFS Endpoint](schema.endpoints.tfsendpoint.json)
- [TFS Team Project Endpoint](schema.endpoints.tfsteamprojectendpoint.json)
- [TFS Team Settings Endpoint](schema.endpoints.tfsteamsettingsendpoint.json)
- [TFS Work Item Endpoint](schema.endpoints.tfsworkitemendpoint.json)

### Processor Enrichers

- [Pause After Each Item](schema.processorenrichers.pauseaftereachitem.json)

## Usage

You can reference these schemas in your JSON configuration files:

```json
{
  "$schema": "https://devopsmigration.io/schema/configuration.schema.json",
  "MigrationTools": {
    // your configuration here
  }
}
```

## Schema Validation

Most modern IDEs and editors support JSON Schema validation. Simply add the `$schema` property to your configuration files to enable:

- Auto-completion
- Validation
- Documentation on hover
- Error highlighting
