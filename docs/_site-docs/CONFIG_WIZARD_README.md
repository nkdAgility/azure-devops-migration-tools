# Interactive Configuration Tool Implementation

## Design Constraints

### 🎯 **Migration Type Architecture**

The wizard is built around three primary migration types with specific endpoint and processor constraints:

#### **1. WorkItems Migration**

- **Required Endpoint**: `TfsWorkItemEndpoint`
- **Required Processor**: `TfsWorkItemMigrationProcessor`
- **Behavior**: Endpoint and processor are locked/pre-selected. Only processor options are configurable.
- **Use Case**: Standard work item migration between Azure DevOps organizations or TFS instances.

#### **2. Pipelines Migration**

- **Required Endpoint**: `AzureDevOpsEndpoint`
- **Required Processor**: `AzureDevOpsPipelineProcessor`
- **Behavior**: Endpoint and processor are locked/pre-selected. Only processor options are configurable.
- **Use Case**: Migration of build and release pipelines between Azure DevOps organizations.

#### **3. Custom Migration**

- **Endpoints**: User can add multiple named endpoints of any valid type
- **Processors**: User can select any number of processors with full configuration flexibility
- **Behavior**: Complete freedom to configure complex migration scenarios
- **Constraints**: Type validation ensures only compatible named endpoints are available for each processor type
- **Use Case**: Advanced scenarios requiring multiple processors, custom field mapping, or specialized migration patterns.

### 🔧 **Processor Configuration Behavior**

#### **Constrained Modes (WorkItems & Pipelines)**

- Processor selection is **locked** based on migration type
- Only processor-specific **options** are editable (e.g., WIQL queries, field mappings, performance settings)
- Simplified UI focuses on configuration rather than selection

#### **Custom Mode**

- **Full processor selection**: Choose from all available processors
- **Multiple processor support**: Add/remove processors as needed
- **Named endpoint association**: Each processor can reference named endpoints by name
- **Type validation**: Only compatible endpoints appear in dropdown for each processor

## Overview

This implementation provides a **comprehensive interactive configuration wizard** for the Azure DevOps Migration Tools. The wizard guides users through a step-by-step process to build their migration configuration without requiring deep knowledge of the underlying JSON structure.

## Architecture & Features

### 🎯 **Core Design Principles**

1. **Progressive Disclosure**: Information is revealed step-by-step to avoid overwhelming users
2. **Smart Defaults**: Pre-configured sensible defaults based on migration type
3. **Real-time Validation**: Immediate feedback on configuration choices
4. **Data-Driven**: Leverages existing YAML configuration data from the documentation system
5. **Responsive Design**: Works seamlessly on desktop, tablet, and mobile devices

### 🚀 **Key Features Implemented**

#### **Multi-Step Wizard Flow**

```text
Step 1: Migration Type Selection
├── WorkItems (TfsWorkItemEndpoint + TfsWorkItemMigrationProcessor)
├── Pipelines (AzureDevOpsEndpoint + AzureDevOpsPipelineProcessor)
└── Custom (User-defined endpoints and processors)

Step 2: Endpoint Configuration
├── Pre-configured endpoints (WorkItems/Pipelines modes)
├── Named endpoint creation (Custom mode)
├── Authentication (PAT tokens)
└── Connection validation

Step 3: Processor Configuration
├── Locked processor options (WorkItems/Pipelines modes)
├── Processor selection and addition (Custom mode)
├── Processor-specific configuration
└── Named endpoint assignment per processor

Step 4: Field Mapping Strategy
├── Automatic mapping
├── Manual mapping options
└── Custom field transformations

Step 5: Advanced Options
├── Performance tuning
├── Error handling
└── Logging configuration

Step 6: Review & Export
├── Generated configuration preview
├── Validation warnings
└── Download/copy functionality
```

#### **User Experience Features**

- **Visual Progress Indicator**: Shows completion percentage and current step
- **Step Navigation**: Previous/Next buttons with validation
- **Smart Selections**: Migration type determines available processors
- **Real-time Preview**: Live configuration generation
- **Copy/Download Options**: Easy export of generated configuration
- **Responsive Design**: Mobile-friendly interface
- **Dark/Light Theme Support**: Integrates with existing theme system

#### **Technical Features**

- **Hugo Integration**: Leverages existing YAML data files
- **Client-side Processing**: No server-side dependencies
- **Security Conscious**: Credentials only used for config generation
- **Validation System**: Input validation and error handling
- **Bootstrap 5**: Consistent with existing documentation styling

## Implementation Details

### **File Structure**

```
docs/
├── content/docs/config-wizard.md          # Wizard page content
├── layouts/config-wizard.html             # Custom Hugo layout
├── static/css/config-wizard.css           # Wizard-specific styles
└── static/js/config-wizard.js             # Main application logic
```

### **Data Integration**

The wizard dynamically loads configuration data from the existing YAML files:

- `reference.processors.*.yaml` - Processor configurations
- `reference.endpoints.*.yaml` - Endpoint options
- `reference.fieldmaps.*.yaml` - Field mapping options
- `reference.tools.*.yaml` - Tool configurations

### **Technology Stack**

- **Frontend**: Vanilla JavaScript (ES6+), HTML5, CSS3
- **Styling**: Bootstrap 5.3.6, Custom CSS with CSS Variables
- **Icons**: Font Awesome 6
- **Build System**: Hugo static site generator
- **Data Format**: YAML → JSON conversion

## Usage Examples

### **For Beginners**

1. Visit `/docs/config-wizard/`
2. Select "WorkItems" migration type
3. Configure TfsWorkItemEndpoint with source and target organization URLs
4. Accept default TfsWorkItemMigrationProcessor settings
5. Choose "Automatic Mapping" for field mapping
6. Use default advanced settings
7. Download generated `configuration.json`

### **For Advanced Users**

1. Select "Custom" migration type for comprehensive setup
2. Add multiple named endpoints (TfsWorkItemEndpoint, AzureDevOpsEndpoint, etc.)
3. Configure multiple processors with custom settings
4. Define manual field mapping strategies
5. Tune performance and error handling parameters
6. Generate complex configurations with custom WIQL queries and processor combinations

## Benefits

### **User Benefits**

- **Reduced Learning Curve**: No need to understand complex JSON structure
- **Faster Setup**: 5-10 minutes vs hours of manual configuration
- **Error Prevention**: Validation prevents common configuration mistakes
- **Educational**: Learn about available options through guided explanations
- **Confidence Building**: Visual progress and validation provide reassurance

### **Project Benefits**

- **Lower Support Burden**: Fewer configuration-related support requests
- **Better Adoption**: Easier onboarding leads to higher tool adoption
- **Documentation Synergy**: Leverages existing documentation infrastructure
- **Maintainability**: Auto-updates as processors/options change

## Future Enhancements

### **Phase 2 Features**

- **Configuration Templates**: Save and reuse common configurations
- **Validation Engine**: Real-time connection testing
- **WIQL Query Builder**: Visual query construction interface
- **Batch Operations**: Configure multiple migration jobs
- **Import/Export**: Load existing configurations for modification

### **Advanced Features**

- **Migration Scenarios**: Pre-built configurations for common patterns
- **Performance Estimator**: Predict migration time based on data size
- **Dependency Checker**: Validate prerequisites and permissions
- **Progress Tracking**: Real-time migration monitoring integration

## Implementation Strategy

### **How to Build an Interactive Configuration Tool**

Based on this implementation, here's the recommended approach for similar tools:

#### **1. Data Architecture**

```javascript
// Leverage existing structured data
const configData = {
  processors: loadFromYAML(),
  endpoints: loadFromYAML(),
  fieldMaps: loadFromYAML(),
  templates: loadFromYAML(),
};
```

#### **2. Progressive UI Design**

```javascript
class ConfigWizard {
  constructor() {
    this.steps = [
      { id: "type", title: "Choose Type", validator: this.validateType },
      { id: "endpoints", title: "Configure Endpoints", validator: this.validateEndpoints },
      // ... more steps
    ];
  }
}
```

#### **3. Smart Defaults System**

```javascript
getProcessorsForType(migrationType) {
  const mapping = {
    'workitems': {
      processors: ['TfsWorkItemMigrationProcessor'],
      endpoints: ['TfsWorkItemEndpoint'],
      locked: true
    },
    'pipelines': {
      processors: ['AzureDevOpsPipelineProcessor'],
      endpoints: ['AzureDevOpsEndpoint'],
      locked: true
    },
    'custom': {
      processors: [], // User selectable
      endpoints: [], // User configurable
      locked: false
    }
  };
  return mapping[migrationType] || { processors: [], endpoints: [], locked: false };
}
```

#### **4. Real-time Validation**

```javascript
validateStep(stepData) {
  const validators = {
    endpoints: this.validateEndpoints,
    processors: this.validateProcessors
  };
  return validators[this.currentStep](stepData);
}
```

#### **5. Configuration Generation**

```javascript
generateConfig() {
  return {
    MigrationTools: {
      Version: "16.0",
      Endpoints: this.buildEndpoints(),
      Processors: this.buildProcessors(),
      // ... other sections
    }
  };
}
```

## Integration Points

The wizard integrates seamlessly with the existing documentation:

- **Navigation**: Linked from main docs page and getting started guide
- **Styling**: Uses existing Bootstrap theme and color schemes
- **Data**: Leverages auto-generated YAML configuration data
- **Comments**: Supports Giscus comments for user feedback
- **Editing**: "Edit this page" links to GitHub for improvements

This implementation demonstrates how to build a sophisticated, user-friendly configuration tool that significantly improves the user experience while leveraging existing documentation infrastructure.
