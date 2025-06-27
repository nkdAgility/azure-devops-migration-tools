/**
 * Azure DevOps Migration Tools Configuration Wizard
 * Interactive configuration builder for migration tools using JSON Schema
 */

class ConfigurationWizard {
  constructor() {
    this.currentStep = 1;
    this.totalSteps = 6;
    this.configuration = {
      "$schema": "https://devopsmigration.io/schema/configuration.schema.json",
      Serilog: {
        MinimumLevel: "Information"
      },
      MigrationTools: {
        Version: "16.0",
        Endpoints: {},
        Processors: [],
        CommonTools: {}
      }
    };
    this.selectedMigrationType = null;
    this.selectedProcessors = [];
    this.savedAdvancedOptions = null;
    this.schema = null;
    this.availableProcessors = [];
    this.availableEndpoints = [];
    this.init();
  }

  async init() {
    this.render();
    this.attachEventListeners();
    await this.loadSchema();
    this.loadStep1();
  }

  async loadSchema() {
    try {
      // Load the JSON schema
      const schemaURL = window.migrationToolsData?.schemaURL || '/schema/configuration.schema.json';
      console.log('Loading schema from:', schemaURL);
      
      const response = await fetch(schemaURL);
      if (!response.ok) {
        throw new Error(`Failed to fetch schema: ${response.status} ${response.statusText}`);
      }
      
      this.schema = await response.json();
      console.log('Schema loaded successfully, structure:', {
        hasProperties: !!this.schema.properties,
        hasMigrationTools: !!this.schema.properties?.MigrationTools,
        hasProcessors: !!this.schema.properties?.MigrationTools?.properties?.processors,
        hasEndpoints: !!this.schema.properties?.MigrationTools?.properties?.endpoints
      });
      
      // Extract processors and endpoints from schema
      this.extractAvailableOptions();
      
      console.log('Schema extraction completed:', {
        processors: this.availableProcessors.length,
        endpoints: this.availableEndpoints.length
      });
    } catch (error) {
      console.error('Failed to load schema:', error);
      // Fallback to legacy data if schema loading fails
      console.log('Falling back to legacy data');
      this.extractLegacyData();
    }
  }

  extractAvailableOptions() {
    if (!this.schema?.properties?.MigrationTools?.properties) {
      console.warn('Schema structure not as expected');
      return;
    }

    // Extract processors from schema
    const processorsSchema = this.schema.properties.MigrationTools.properties.processors;
    if (processorsSchema?.prefixItems?.anyOf) {
      this.availableProcessors = processorsSchema.prefixItems.anyOf.map(processor => ({
        name: processor.title,
        description: processor.description || 'No description available',
        properties: processor.properties || {},
        schema: processor
      }));
    }

    // Extract endpoints from schema
    const endpointsSchema = this.schema.properties.MigrationTools.properties.endpoints;
    if (endpointsSchema?.properties) {
      this.availableEndpoints = Object.entries(endpointsSchema.properties).map(([key, endpoint]) => ({
        name: endpoint.title,
        key: key,
        description: endpoint.description || 'No description available',
        properties: endpoint.properties || {},
        schema: endpoint
      }));
    }

    console.log('Schema extraction results:', {
      processors: this.availableProcessors.length,
      processorNames: this.availableProcessors.map(p => p.name),
      endpoints: this.availableEndpoints.length,
      endpointNames: this.availableEndpoints.map(e => e.name)
    });
  }

  extractLegacyData() {
    console.log('Using legacy data fallback');
    
    // Fallback to existing Hugo data structure
    if (window.migrationToolsData?.processors) {
      this.availableProcessors = Object.entries(window.migrationToolsData.processors).map(([key, processor]) => ({
        name: key,
        description: processor.description || 'No description available',
        properties: processor.options || {},
        schema: processor
      }));
      console.log('Legacy processors loaded:', this.availableProcessors.length);
    }

    if (window.migrationToolsData?.endpoints) {
      this.availableEndpoints = Object.entries(window.migrationToolsData.endpoints).map(([key, endpoint]) => ({
        name: key,
        key: key,
        description: endpoint.description || 'No description available',
        properties: endpoint.options || {},
        schema: endpoint
      }));
      console.log('Legacy endpoints loaded:', this.availableEndpoints.length);
    }

    // If we still don't have processors, create some fallback ones
    if (this.availableProcessors.length === 0) {
      console.warn('No processors found in legacy data, creating fallback processors');
      this.availableProcessors = [
        {
          name: 'TfsWorkItemMigrationProcessor',
          description: 'Migrates work items from source to target with their history, attachments, and links.',
          properties: { enabled: { type: 'boolean' } },
          schema: {}
        },
        {
          name: 'AzureDevOpsPipelineProcessor', 
          description: 'Migrates Azure DevOps pipelines including build and release definitions.',
          properties: { enabled: { type: 'boolean' } },
          schema: {}
        },
        {
          name: 'TfsTeamSettingsProcessor',
          description: 'Migrates team settings including areas and iterations.',
          properties: { enabled: { type: 'boolean' } },
          schema: {}
        }
      ];
    }

    // If we still don't have endpoints, create some fallback ones
    if (this.availableEndpoints.length === 0) {
      console.warn('No endpoints found in legacy data, creating fallback endpoints');
      this.availableEndpoints = [
        {
          name: 'TfsTeamProjectEndpoint',
          key: 'tfsTeamProjectEndpoint',
          description: 'Connects to TFS or Azure DevOps for work item and team project operations.',
          properties: { 
            Collection: { type: 'string' },
            Project: { type: 'string' },
            AccessToken: { type: 'string' }
          },
          schema: {}
        },
        {
          name: 'AzureDevOpsEndpoint',
          key: 'azureDevOpsEndpoint', 
          description: 'Connects to Azure DevOps for pipeline and REST API operations.',
          properties: {
            Organization: { type: 'string' },
            Project: { type: 'string' }, 
            AccessToken: { type: 'string' }
          },
          schema: {}
        }
      ];
    }

    console.log('Final fallback data:', {
      processors: this.availableProcessors.length,
      endpoints: this.availableEndpoints.length
    });
  }

  render() {
    const app = document.getElementById('config-wizard-app');
    app.innerHTML = `
      <div class="config-wizard">
        <!-- Progress bar -->
        <div class="wizard-progress">
          <div class="progress">
            <div class="progress-bar bg-primary" role="progressbar" style="width: ${(this.currentStep / this.totalSteps) * 100}%"></div>
          </div>
          <div class="d-flex justify-content-between mt-2">
            <small class="text-muted">Step ${this.currentStep} of ${this.totalSteps}</small>
            <small class="text-muted">${Math.round((this.currentStep / this.totalSteps) * 100)}% Complete</small>
          </div>
        </div>

        <!-- Step indicators -->
        <ol class="wizard-steps">
          ${this.renderStepIndicators()}
        </ol>

        <!-- Wizard content -->
        <div class="wizard-content">
          <div id="wizard-step-content">
            <!-- Dynamic content will be loaded here -->
          </div>
          
          <!-- Navigation -->
          <div class="wizard-navigation">
            <button type="button" class="btn btn-outline-secondary wizard-nav-button" id="prevBtn" ${this.currentStep === 1 ? 'disabled' : ''}>
              <i class="fas fa-arrow-left me-2"></i>Previous
            </button>
            <div class="text-center">
              <small class="text-muted">Step ${this.currentStep} of ${this.totalSteps}</small>
            </div>
            <button type="button" class="btn btn-primary wizard-nav-button" id="nextBtn">
              ${this.currentStep === this.totalSteps ? 'Finish' : 'Next'} <i class="fas fa-arrow-right ms-2"></i>
            </button>
          </div>
        </div>
      </div>
    `;
  }

  renderStepIndicators() {
    const steps = [
      'Migration Type',
      'Endpoints',
      'Processors',
      'Field Mapping',
      'Advanced',
      'Review'
    ];

    return steps.map((title, index) => {
      const stepNumber = index + 1;
      let stepClass = '';
      
      if (stepNumber < this.currentStep) {
        stepClass = 'completed';
      } else if (stepNumber === this.currentStep) {
        stepClass = 'active';
      }

      return `
        <li class="wizard-step ${stepClass}">
          <span class="wizard-step-circle">
            ${stepNumber < this.currentStep ? '<i class="fas fa-check"></i>' : stepNumber}
          </span>
          <span class="wizard-step-label">${title}</span>
        </li>
      `;
    }).join('');
  }

  attachEventListeners() {
    document.addEventListener('click', (e) => {
      if (e.target.id === 'nextBtn') {
        this.nextStep();
      } else if (e.target.id === 'prevBtn') {
        this.prevStep();
      }
    });
  }

  nextStep() {
    if (this.validateCurrentStep()) {
      // Save current step data before moving to next step
      this.saveCurrentStepData();
      
      if (this.currentStep < this.totalSteps) {
        this.currentStep++;
        this.render();
        this.loadCurrentStep();
      } else {
        this.finishWizard();
      }
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      // Save current step data before moving to previous step
      this.saveCurrentStepData();
      
      this.currentStep--;
      this.render();
      this.loadCurrentStep();
    }
  }

  loadCurrentStep() {
    switch (this.currentStep) {
      case 1:
        this.loadStep1();
        break;
      case 2:
        this.loadStep2();
        break;
      case 3:
        this.loadStep3();
        break;
      case 4:
        this.loadStep4();
        break;
      case 5:
        this.loadStep5();
        break;
      case 6:
        this.loadStep6();
        break;
    }
  }

  saveCurrentStepData() {
    switch (this.currentStep) {
      case 5:
        // Save advanced configuration options
        this.savedAdvancedOptions = {
          logLevel: document.getElementById('logLevel')?.value || 'Information',
          batchSize: document.getElementById('batchSize')?.value || '100',
          pauseAfterEach: document.getElementById('pauseAfterEach')?.checked || false,
          maxFailures: document.getElementById('maxFailures')?.value || '0',
          skipInvalidPaths: document.getElementById('skipInvalidPaths')?.checked || true,
          generateComment: document.getElementById('generateComment')?.checked || true
        };
        break;
    }
  }

  loadStep1() {
    const content = document.getElementById('wizard-step-content');
    
    // Create migration type options based on available processors
    const migrationTypes = this.getMigrationTypes();
    console.log('Step 1 - Migration types generated:', migrationTypes);
    
    if (migrationTypes.length === 0) {
      content.innerHTML = `
        <h2 class="text-primary mb-4">Choose Your Migration Type</h2>
        <div class="alert alert-warning">
          <i class="fas fa-exclamation-triangle me-2"></i>
          <strong>No migration types available.</strong> 
          This might be due to a schema loading issue. Please check the browser console for details.
        </div>
        <div class="alert alert-info">
          <strong>Debug Info:</strong>
          <ul>
            <li>Available Processors: ${this.availableProcessors.length}</li>
            <li>Available Endpoints: ${this.availableEndpoints.length}</li>
            <li>Schema Loaded: ${this.schema ? 'Yes' : 'No'}</li>
          </ul>
        </div>
      `;
      return;
    }
    
    content.innerHTML = `
      <h2 class="text-primary mb-4">Choose Your Migration Type</h2>
      <p class="lead mb-4">What would you like to migrate? This will determine which processors and endpoint configurations are needed.</p>
      
      <div class="config-section">
        <div class="row">
          ${migrationTypes.map(type => `
            <div class="col-md-6 mb-3">
              <div class="config-option" data-migration-type="${type.key}">
                <div class="config-option-title">
                  <i class="${type.icon} text-primary me-2"></i>${type.name}
                </div>
                <div class="config-option-description">
                  ${type.description}
                </div>
                <div class="config-option-processors">
                  <small class="text-muted">Processors: ${type.processors.join(', ')}</small>
                </div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
    `;

    // Attach click handlers for migration type selection
    document.querySelectorAll('.config-option[data-migration-type]').forEach(option => {
      option.addEventListener('click', () => {
        // Remove previous selection
        document.querySelectorAll('.config-option').forEach(opt => 
          opt.classList.remove('selected'));
        
        // Mark current selection
        option.classList.add('selected');
        this.selectedMigrationType = option.dataset.migrationType;
        
        // Enable next button
        const nextBtn = document.getElementById('nextBtn');
        nextBtn.disabled = false;
      });
    });
  }

  getMigrationTypes() {
    // Define the three main migration type templates
    const migrationTypes = [];

    // Work Items category
    const workItemProcessors = this.availableProcessors.filter(p => 
      p.name.toLowerCase().includes('workitem') || 
      p.name.toLowerCase().includes('tfsworkitem') ||
      p.name.toLowerCase().includes('workitemtracking')
    );
    
    migrationTypes.push({
      key: 'work-items',
      name: 'Work Items',
      icon: 'fas fa-tasks',
      description: 'Migrate work items (User Stories, Bugs, Tasks, etc.) with their history, attachments, and links. Includes related processors for work item migration.',
      processors: workItemProcessors.length > 0 ? workItemProcessors.map(p => p.name) : ['TfsWorkItemMigrationProcessor', 'WorkItemTrackingProcessor'],
      recommendedEndpoints: ['TfsTeamProjectEndpoint', 'TfsWorkItemEndpoint']
    });

    // Pipelines category
    const pipelineProcessors = this.availableProcessors.filter(p => 
      p.name.toLowerCase().includes('pipeline')
    );
    
    migrationTypes.push({
      key: 'pipelines',
      name: 'Pipelines',
      icon: 'fas fa-code-branch',
      description: 'Migrate Azure DevOps pipelines including Build and Release definitions, Task Groups, Variable Groups, and Service Connections.',
      processors: pipelineProcessors.length > 0 ? pipelineProcessors.map(p => p.name) : ['AzureDevOpsPipelineProcessor'],
      recommendedEndpoints: ['AzureDevOpsEndpoint']
    });

    // Custom migration option
    migrationTypes.push({
      key: 'custom',
      name: 'Custom',
      icon: 'fas fa-cogs',
      description: 'Create a custom migration configuration by selecting specific processors and endpoints. Perfect for specialized migration scenarios or when you need fine-grained control.',
      processors: this.availableProcessors.map(p => p.name),
      recommendedEndpoints: ['TfsTeamProjectEndpoint', 'AzureDevOpsEndpoint']
    });

    return migrationTypes;
  }

  loadStep2() {
    const content = document.getElementById('wizard-step-content');
    const recommendedEndpoints = this.getRecommendedEndpoints();
    
    content.innerHTML = `
      <h2 class="text-primary mb-4">Configure Source and Target Endpoints</h2>
      <p class="lead mb-4">Set up connections to your source and target instances.</p>
      
      <div class="alert alert-info mb-4">
        <i class="fas fa-info-circle me-2"></i>
        <strong>Recommended Endpoints:</strong> Based on your migration type, we recommend using these endpoint types.
      </div>
      
      <div class="row">
        <div class="col-md-6">
          <div class="config-section">
            <h3><i class="fas fa-upload text-success me-2"></i>Source Endpoint</h3>
            ${this.renderEndpointSelection('source', recommendedEndpoints)}
          </div>
        </div>
        <div class="col-md-6">
          <div class="config-section">
            <h3><i class="fas fa-download text-info me-2"></i>Target Endpoint</h3>
            ${this.renderEndpointSelection('target', recommendedEndpoints)}
          </div>
        </div>
      </div>
      
      <div class="alert alert-warning">
        <i class="fas fa-shield-alt me-2"></i>
        <strong>Security Note:</strong> Your credentials are only used to generate the configuration file. 
        They are not stored or transmitted anywhere.
      </div>
    `;

    // Add event handlers for endpoint type changes
    this.attachEndpointFormHandlers();
  }

  getRecommendedEndpoints() {
    // Get the migration type and return its recommended endpoints
    const migrationTypes = this.getMigrationTypes();
    const selectedType = migrationTypes.find(t => t.key === this.selectedMigrationType);
    return selectedType?.recommendedEndpoints || ['TfsTeamProjectEndpoint'];
  }

  renderEndpointSelection(type, recommendedEndpoints) {
    // Get available endpoints that match the recommendations
    const availableEndpoints = this.availableEndpoints.filter(endpoint => 
      recommendedEndpoints.some(rec => endpoint.name.includes(rec))
    );

    // Default to first recommended if no matches found
    if (availableEndpoints.length === 0 && this.availableEndpoints.length > 0) {
      availableEndpoints.push(this.availableEndpoints[0]);
    }

    return `
      <div class="wizard-form-group">
        <label class="wizard-form-label">Endpoint Type</label>
        <select class="form-control" id="${type}EndpointType" onchange="wizard.updateEndpointForm('${type}')">
          ${availableEndpoints.map(endpoint => `
            <option value="${endpoint.name}" ${recommendedEndpoints.includes(endpoint.name) ? 'selected' : ''}>
              ${endpoint.name}
            </option>
          `).join('')}
        </select>
        <div class="wizard-form-help">
          ${availableEndpoints.find(e => recommendedEndpoints.includes(e.name))?.description || 'Select an endpoint type'}
        </div>
      </div>
      <div id="${type}EndpointForm">
        ${this.renderEndpointForm(type, availableEndpoints[0]?.name)}
      </div>
    `;
  }

  updateEndpointForm(type) {
    const select = document.getElementById(`${type}EndpointType`);
    const endpointType = select.value;
    const formContainer = document.getElementById(`${type}EndpointForm`);
    formContainer.innerHTML = this.renderEndpointForm(type, endpointType);
  }

  renderEndpointForm(type, endpointTypeName) {
    // Find the endpoint schema
    const endpointSchema = this.availableEndpoints.find(e => e.name === endpointTypeName);
    if (!endpointSchema) {
      return '<p class="text-muted">No configuration required for this endpoint type.</p>';
    }

    // Generate form fields based on the schema properties
    const properties = endpointSchema.properties || {};
    const formFields = [];

    Object.entries(properties).forEach(([propName, propSchema]) => {
      // Skip internal properties
      if (propName === 'EndpointType' || propName === 'enabled') {
        return;
      }

      const fieldId = `${type}${propName}`;
      const fieldType = this.getInputTypeFromSchema(propSchema);
      const placeholder = this.getPlaceholderFromProperty(propName, type);
      const description = propSchema.description || '';

      formFields.push(`
        <div class="wizard-form-group">
          <label class="wizard-form-label">${this.formatPropertyName(propName)}</label>
          <input type="${fieldType}" class="form-control" id="${fieldId}" placeholder="${placeholder}" />
          ${description ? `<div class="wizard-form-help">${description}</div>` : ''}
        </div>
      `);
    });

    return formFields.join('');
  }

  getInputTypeFromSchema(propSchema) {
    if (propSchema.type === 'string') {
      const propName = propSchema.description?.toLowerCase() || '';
      if (propName.includes('url') || propName.includes('organization')) {
        return 'url';
      }
      if (propName.includes('token') || propName.includes('password')) {
        return 'password';
      }
      if (propName.includes('email')) {
        return 'email';
      }
    }
    return 'text';
  }

  getPlaceholderFromProperty(propName, type) {
    const lowerName = propName.toLowerCase();
    
    if (lowerName.includes('organization') || lowerName.includes('organisation')) {
      return 'https://dev.azure.com/yourorg/';
    }
    if (lowerName.includes('collection')) {
      return 'https://dev.azure.com/yourorg or https://tfs.company.com/tfs/DefaultCollection';
    }
    if (lowerName.includes('project')) {
      return `${type === 'source' ? 'Source' : 'Target'} project name`;
    }
    if (lowerName.includes('token')) {
      return 'Your PAT token';
    }
    if (lowerName.includes('url')) {
      return 'https://';
    }
    
    return `Enter ${this.formatPropertyName(propName)}`;
  }

  formatPropertyName(propName) {
    // Convert camelCase to Title Case
    return propName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  attachEndpointFormHandlers() {
    // Make the wizard instance globally available for the onchange handlers
    window.wizard = this;
  }

  getEndpointTypeForMigration() {
    switch (this.selectedMigrationType) {
      case 'pipelines':
        return 'AzureDevOpsEndpoint';
      case 'work-items':
      case 'test-plans':
      case 'work-items-test-plans':
      default:
        return 'TfsTeamProjectEndpoint';
    }
  }

  getEndpointDescription() {
    const endpointType = this.getEndpointTypeForMigration();
    if (endpointType === 'AzureDevOpsEndpoint') {
      return 'AzureDevOpsEndpoint is used for pipeline migrations and provides REST API access to Azure DevOps services.';
    } else {
      return 'TfsTeamProjectEndpoint is used for work item and test plan migrations, providing access to TFS Object Model functionality.';
    }
  }

  renderEndpointForm(type, endpointType) {
    const isAzureDevOps = endpointType === 'AzureDevOpsEndpoint';
    
    if (isAzureDevOps) {
      return `
        <div class="wizard-form-group">
          <label class="wizard-form-label">Organization URL</label>
          <input type="url" class="form-control" id="${type}Organization" placeholder="https://dev.azure.com/yourorg" />
          <div class="wizard-form-help">The URL of your Azure DevOps organization</div>
        </div>
        <div class="wizard-form-group">
          <label class="wizard-form-label">Personal Access Token</label>
          <input type="password" class="form-control" id="${type}AccessToken" placeholder="Your PAT token" />
          <div class="wizard-form-help">PAT token with Build & Release permissions</div>
        </div>
        <div class="wizard-form-group">
          <label class="wizard-form-label">Project Name</label>
          <input type="text" class="form-control" id="${type}Project" placeholder="${type === 'source' ? 'Source' : 'Target'} project name" />
        </div>
      `;
    } else {
      return `
        <div class="wizard-form-group">
          <label class="wizard-form-label">Collection URL</label>
          <input type="url" class="form-control" id="${type}Collection" placeholder="https://dev.azure.com/yourorg or https://tfs.company.com/tfs/DefaultCollection" />
          <div class="wizard-form-help">TFS Collection or Azure DevOps organization URL</div>
        </div>
        <div class="wizard-form-group">
          <label class="wizard-form-label">Personal Access Token</label>
          <input type="password" class="form-control" id="${type}AccessToken" placeholder="Your PAT token" />
          <div class="wizard-form-help">PAT token with Work Items read/write permissions</div>
        </div>
        <div class="wizard-form-group">
          <label class="wizard-form-label">Project Name</label>
          <input type="text" class="form-control" id="${type}Project" placeholder="${type === 'source' ? 'Source' : 'Target'} project name" />
        </div>
        <div class="wizard-form-group">
          <label class="wizard-form-label">Authentication Mode</label>
          <select class="form-select" id="${type}AuthMode">
            <option value="AccessToken">Personal Access Token</option>
            <option value="Prompt">Windows Authentication (Prompt)</option>
          </select>
          <div class="wizard-form-help">Choose authentication method for TFS/Azure DevOps</div>
        </div>
        ${type === 'target' ? `
        <div class="wizard-form-group">
          <label class="wizard-form-label">Reflected Work Item ID Field</label>
          <input type="text" class="form-control" id="${type}ReflectedField" value="Custom.ReflectedWorkItemId" />
          <div class="wizard-form-help">Custom field to track migrated work items</div>
        </div>
        ` : ''}
      `;
    }
  }

  loadStep3() {
    const content = document.getElementById('wizard-step-content');
    const processors = this.getProcessorsForMigrationType();
    
    content.innerHTML = `
      <h2 class="text-primary mb-4">Select Processors</h2>
      <p class="lead mb-4">Choose which processors to include based on your migration type.</p>
      
      <div class="config-section">
        <h3>Recommended Processors</h3>
        <p class="text-muted mb-3">These processors are recommended for your migration type:</p>
        <div class="row">
          ${processors.recommended.map(proc => `
            <div class="col-md-6 mb-3">
              <div class="config-option selected" data-processor="${proc.name}">
                <div class="config-option-title">
                  <i class="fas fa-check-circle text-success me-2"></i>${proc.name}
                </div>
                <div class="config-option-description">${proc.description}</div>
                <div class="config-option-meta">
                  <small class="text-muted">Properties: ${Object.keys(proc.properties).length}</small>
                </div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
      
      <div class="config-section">
        <h3>Additional Processors</h3>
        <p class="text-muted mb-3">Select additional processors if needed:</p>
        <div class="row">
          ${processors.additional.map(proc => `
            <div class="col-md-6 mb-3">
              <div class="config-option" data-processor="${proc.name}">
                <div class="config-option-title">
                  <i class="fas fa-plus-circle text-primary me-2"></i>${proc.name}
                </div>
                <div class="config-option-description">${proc.description}</div>
                <div class="config-option-meta">
                  <small class="text-muted">Properties: ${Object.keys(proc.properties).length}</small>
                </div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
      
      ${processors.additional.length === 0 ? 
        '<div class="alert alert-info"><i class="fas fa-info-circle me-2"></i>All available processors for your migration type are already recommended above.</div>' : 
        ''
      }
    `;

    // Add click handlers for all processors
    content.querySelectorAll('.config-option[data-processor]').forEach(option => {
      option.addEventListener('click', () => {
        option.classList.toggle('selected');
        const processorName = option.dataset.processor;
        const index = this.selectedProcessors.findIndex(p => p === processorName);
        if (index === -1) {
          this.selectedProcessors.push(processorName);
        } else {
          this.selectedProcessors.splice(index, 1);
        }
      });
    });

    // Pre-select recommended processors
    processors.recommended.forEach(proc => {
      if (!this.selectedProcessors.includes(proc.name)) {
        this.selectedProcessors.push(proc.name);
      }
    });
  }

  getProcessorsForMigrationType() {
    // Get the migration type configuration
    const migrationTypes = this.getMigrationTypes();
    const selectedType = migrationTypes.find(t => t.key === this.selectedMigrationType);
    
    if (!selectedType) {
      return { recommended: [], additional: [] };
    }

    // Get recommended processors for this migration type
    const recommendedProcessorNames = selectedType.processors;
    const recommended = this.availableProcessors.filter(proc => 
      recommendedProcessorNames.includes(proc.name)
    );

    // Get additional processors (all others not in recommended)
    const additional = this.availableProcessors.filter(proc => 
      !recommendedProcessorNames.includes(proc.name)
    );

    return { recommended, additional };
  }

  loadStep4() {
    const content = document.getElementById('wizard-step-content');
    content.innerHTML = `
      <h2 class="text-primary mb-4">Field Mapping Configuration</h2>
      <p class="lead mb-4">Configure how fields should be mapped between source and target.</p>
      
      <div class="config-section">
        <h3>Mapping Strategy</h3>
        <div class="row">
          <div class="col-md-6 mb-3">
            <div class="config-option" data-mapping-strategy="auto">
              <div class="config-option-title">
                <i class="fas fa-magic text-primary me-2"></i>Automatic Mapping
              </div>
              <div class="config-option-description">
                Let the tool automatically map fields with the same names and compatible types.
              </div>
            </div>
          </div>
          <div class="col-md-6 mb-3">
            <div class="config-option" data-mapping-strategy="manual">
              <div class="config-option-title">
                <i class="fas fa-cogs text-primary me-2"></i>Manual Mapping
              </div>
              <div class="config-option-description">
                Define custom field mappings and transformations.
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="config-section" id="custom-mappings" style="display: none;">
        <h3>Custom Field Mappings</h3>
        <div class="alert alert-info">
          <i class="fas fa-lightbulb me-2"></i>
          You can configure detailed field mappings in the generated configuration file.
        </div>
      </div>
    `;

    // Add mapping strategy selection
    content.querySelectorAll('.config-option[data-mapping-strategy]').forEach(option => {
      option.addEventListener('click', () => {
        content.querySelectorAll('.config-option').forEach(opt => opt.classList.remove('selected'));
        option.classList.add('selected');
        
        const customMappings = document.getElementById('custom-mappings');
        if (option.dataset.mappingStrategy === 'manual') {
          customMappings.style.display = 'block';
        } else {
          customMappings.style.display = 'none';
        }
      });
    });
  }

  loadStep5() {
    const content = document.getElementById('wizard-step-content');
    content.innerHTML = `
      <h2 class="text-primary mb-4">Advanced Configuration</h2>
      <p class="lead mb-4">Configure advanced options for performance and error handling.</p>
      
      <div class="row">
        <div class="col-md-6">
          <div class="config-section">
            <h3><i class="fas fa-tachometer-alt text-primary me-2"></i>Performance</h3>
            <div class="wizard-form-group">
              <label class="wizard-form-label">Batch Size</label>
              <select class="form-select" id="batchSize">
                <option value="100">100 (Default)</option>
                <option value="50">50 (Conservative)</option>
                <option value="200">200 (Aggressive)</option>
                <option value="500">500 (Large datasets)</option>
              </select>
              <div class="wizard-form-help">Number of work items to process in each batch</div>
            </div>
            <div class="form-check">
              <input class="form-check-input" type="checkbox" id="pauseAfterEach">
              <label class="form-check-label" for="pauseAfterEach">
                Pause after each work item
              </label>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="config-section">
            <h3><i class="fas fa-shield-alt text-primary me-2"></i>Error Handling</h3>
            <div class="wizard-form-group">
              <label class="wizard-form-label">Max Graceful Failures</label>
              <input type="number" class="form-control" id="maxFailures" value="0" min="0" max="100">
              <div class="wizard-form-help">Number of failures before stopping migration</div>
            </div>
            <div class="form-check">
              <input class="form-check-input" type="checkbox" id="skipInvalidPaths" checked>
              <label class="form-check-label" for="skipInvalidPaths">
                Skip items with invalid area/iteration paths
              </label>
            </div>
          </div>
        </div>
      </div>
      
      <div class="config-section">
        <h3><i class="fas fa-file-alt text-primary me-2"></i>Logging</h3>
        <div class="row">
          <div class="col-md-6">
            <div class="wizard-form-group">
              <label class="wizard-form-label">Log Level</label>
              <select class="form-select" id="logLevel">
                <option value="Information">Information</option>
                <option value="Debug">Debug</option>
                <option value="Warning">Warning</option>
                <option value="Error">Error</option>
              </select>
            </div>
          </div>
          <div class="col-md-6">
            <div class="form-check mt-4">
              <input class="form-check-input" type="checkbox" id="generateComment" checked>
              <label class="form-check-label" for="generateComment">
                Generate migration comments
              </label>
            </div>
          </div>
        </div>
      </div>
    `;
    
    // Restore saved values if available
    if (this.savedAdvancedOptions) {
      const logLevelSelect = document.getElementById('logLevel');
      const batchSizeSelect = document.getElementById('batchSize');
      const pauseAfterEachCheck = document.getElementById('pauseAfterEach');
      const maxFailuresInput = document.getElementById('maxFailures');
      const skipInvalidPathsCheck = document.getElementById('skipInvalidPaths');
      const generateCommentCheck = document.getElementById('generateComment');
      
      if (logLevelSelect) logLevelSelect.value = this.savedAdvancedOptions.logLevel;
      if (batchSizeSelect) batchSizeSelect.value = this.savedAdvancedOptions.batchSize;
      if (pauseAfterEachCheck) pauseAfterEachCheck.checked = this.savedAdvancedOptions.pauseAfterEach;
      if (maxFailuresInput) maxFailuresInput.value = this.savedAdvancedOptions.maxFailures;
      if (skipInvalidPathsCheck) skipInvalidPathsCheck.checked = this.savedAdvancedOptions.skipInvalidPaths;
      if (generateCommentCheck) generateCommentCheck.checked = this.savedAdvancedOptions.generateComment;
    }
  }

  loadStep6() {
    this.generateConfiguration();
    const content = document.getElementById('wizard-step-content');
    content.innerHTML = `
      <h2 class="text-primary mb-4">Review and Download Configuration</h2>
      <p class="lead mb-4">Review your configuration and download the JSON file.</p>
      
      <div class="config-section">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h3>Generated Configuration</h3>
          <div>
            <button type="button" class="btn btn-outline-primary btn-sm me-2" onclick="copyToClipboard()">
              <i class="fas fa-copy me-1"></i>Copy
            </button>
            <button type="button" class="btn btn-primary btn-sm" onclick="downloadConfig()">
              <i class="fas fa-download me-1"></i>Download
            </button>
          </div>
        </div>
        <pre class="config-preview" id="configPreview">${JSON.stringify(this.configuration, null, 2)}</pre>
      </div>
      
      <div class="alert alert-success">
        <i class="fas fa-check-circle me-2"></i>
        <strong>Next Steps:</strong>
        <ol class="mb-0 mt-2">
          <li>Download or copy the configuration file</li>
          <li>Save it as <code>configuration.json</code></li>
          <li>Run the migration tool: <code>devopsmigration execute --config configuration.json</code></li>
        </ol>
      </div>
    `;
  }

  validateCurrentStep() {
    switch (this.currentStep) {
      case 1:
        if (!this.selectedMigrationType) {
          alert('Please select a migration type to continue.');
          return false;
        }
        break;
      case 2:
        const endpointType = this.getEndpointTypeForMigration();
        if (endpointType === 'AzureDevOpsEndpoint') {
          const sourceOrg = document.getElementById('sourceOrganization')?.value;
          const targetOrg = document.getElementById('targetOrganization')?.value;
          if (!sourceOrg || !targetOrg) {
            alert('Please fill in both source and target organization URLs.');
            return false;
          }
        } else {
          const sourceCollection = document.getElementById('sourceCollection')?.value;
          const targetCollection = document.getElementById('targetCollection')?.value;
          if (!sourceCollection || !targetCollection) {
            alert('Please fill in both source and target collection URLs.');
            return false;
          }
        }
        break;
      // Add validation for other steps as needed
    }
    return true;
  }

  generateConfiguration() {
    // Start with base configuration that includes $schema
    this.configuration = {
      "$schema": "https://devopsmigration.io/schema/configuration.schema.json",
      Serilog: {
        MinimumLevel: this.savedAdvancedOptions?.logLevel || 'Information'
      },
      MigrationTools: {
        Version: "16.0",
        Endpoints: {},
        Processors: [],
        CommonTools: {}
      }
    };

    // Generate endpoints based on user selections
    this.generateEndpointsConfiguration();
    
    // Generate processors based on user selections
    this.generateProcessorsConfiguration();
  }

  generateEndpointsConfiguration() {
    const sourceEndpointType = document.getElementById('sourceEndpointType')?.value;
    const targetEndpointType = document.getElementById('targetEndpointType')?.value;

    // Generate source endpoint
    if (sourceEndpointType) {
      this.configuration.MigrationTools.Endpoints.Source = this.generateEndpointConfig('source', sourceEndpointType);
    }

    // Generate target endpoint  
    if (targetEndpointType) {
      this.configuration.MigrationTools.Endpoints.Target = this.generateEndpointConfig('target', targetEndpointType);
    }
  }

  generateEndpointConfig(type, endpointTypeName) {
    const endpointSchema = this.availableEndpoints.find(e => e.name === endpointTypeName);
    if (!endpointSchema) {
      return { EndpointType: endpointTypeName };
    }

    const config = { EndpointType: endpointTypeName };
    const properties = endpointSchema.properties || {};

    // Generate configuration values from form inputs
    Object.keys(properties).forEach(propName => {
      if (propName === 'EndpointType') return;

      const inputElement = document.getElementById(`${type}${propName}`);
      if (inputElement) {
        let value = inputElement.value;
        
        // Redact sensitive information
        if (propName.toLowerCase().includes('token') || propName.toLowerCase().includes('password')) {
          value = value ? '**redacted**' : '';
        }
        
        // Set the value if provided
        if (value) {
          config[propName] = value;
        }
      }
    });

    return config;
  }

  generateProcessorsConfiguration() {
    // Generate processor configurations based on selected processors
    this.configuration.MigrationTools.Processors = this.selectedProcessors.map(processorName => {
      const processorSchema = this.availableProcessors.find(p => p.name === processorName);
      if (!processorSchema) {
        return { ProcessorType: processorName, Enabled: true };
      }

      const config = { 
        ProcessorType: processorName,
        Enabled: true
      };

      // Add default values for processor properties based on schema
      const properties = processorSchema.properties || {};
      Object.entries(properties).forEach(([propName, propSchema]) => {
        if (propName === 'ProcessorType' || propName === 'Enabled') return;

        // Add default values based on schema
        if (propSchema.default !== undefined) {
          config[propName] = propSchema.default;
        } else {
          // Generate sensible defaults based on property type
          config[propName] = this.getDefaultValueForProperty(propName, propSchema);
        }
      });

      return config;
    });
  }

  getDefaultValueForProperty(propName, propSchema) {
    const type = propSchema.type;
    const lowerName = propName.toLowerCase();

    if (type === 'boolean') {
      return lowerName.includes('enabled') ? true : false;
    } else if (type === 'string') {
      if (lowerName.includes('source')) return 'Source';
      if (lowerName.includes('target')) return 'Target';
      return null;
    } else if (type === 'integer' || type === 'number') {
      return 0;
    } else if (type === 'array') {
      return [];
    } else if (type === 'object') {
      return {};
    }
    
    return null;
  }

  getProcessorsForMigrationType() {
    const processors = {
      'work-items': {
        required: [
          { id: 'TfsWorkItemMigrationProcessor', name: 'Work Item Migration', description: 'Migrates work items with history, attachments, and links' }
        ],
        optional: [
          { id: 'TfsWorkItemBulkEditProcessor', name: 'Bulk Edit', description: 'Bulk edit work items after migration' },
          { id: 'TfsSharedQueryProcessor', name: 'Shared Queries', description: 'Migrate shared queries' },
          { id: 'TfsTeamSettingsProcessor', name: 'Team Settings', description: 'Migrate team settings and configurations' }
        ]
      },
      'test-plans': {
        required: [
          { id: 'TfsTestPlansAndSuitesMigrationProcessor', name: 'Test Plans Migration', description: 'Migrates test plans and suites' }
        ],
        optional: [
          { id: 'TfsTestConfigurationsMigrationProcessor', name: 'Test Configurations', description: 'Migrate test configurations' },
          { id: 'TfsTestVariablesMigrationProcessor', name: 'Test Variables', description: 'Migrate test variables' }
        ]
      },
      'pipelines': {
        required: [
          { id: 'AzureDevOpsPipelineProcessor', name: 'Pipeline Migration', description: 'Migrates build and release pipelines' }
        ],
        optional: [
          { id: 'ProcessDefinitionProcessor', name: 'Process Definition', description: 'Migrate process definitions' }
        ]
      },
      'work-items-test-plans': {
        required: [
          { id: 'TfsWorkItemMigrationProcessor', name: 'Work Item Migration', description: 'Migrates work items with history, attachments, and links' },
          { id: 'TfsTestPlansAndSuitesMigrationProcessor', name: 'Test Plans Migration', description: 'Migrates test plans and suites' }
        ],
        optional: [
          { id: 'TfsTestConfigurationsMigrationProcessor', name: 'Test Configurations', description: 'Migrate test configurations' },
          { id: 'TfsTestVariablesMigrationProcessor', name: 'Test Variables', description: 'Migrate test variables' },
          { id: 'TfsSharedQueryProcessor', name: 'Shared Queries', description: 'Migrate shared queries' },
          { id: 'TfsTeamSettingsProcessor', name: 'Team Settings', description: 'Migrate team settings and configurations' }
        ]
      }
    };

    return processors[this.selectedMigrationType] || processors['work-items'];
  }

  getSelectedProcessorsConfig() {
    // This would generate the actual processor configurations
    // For now, return a basic work item migration processor
    return [
      {
        ProcessorType: "TfsWorkItemMigrationProcessor",
        Enabled: true,
        SourceName: "Source",
        TargetName: "Target",
        WIQLQuery: "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
      }
    ];
  }

  finishWizard() {
    // Handle wizard completion
    console.log('Wizard completed!', this.configuration);
  }
}

// Global functions for the UI
window.copyToClipboard = function() {
  const configText = document.getElementById('configPreview').textContent;
  navigator.clipboard.writeText(configText).then(() => {
    alert('Configuration copied to clipboard!');
  });
};

window.downloadConfig = function() {
  const configText = document.getElementById('configPreview').textContent;
  const blob = new Blob([configText], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'migration-configuration.json';
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
};

// Initialize the wizard when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
  // Check if we have the wizard data available
  if (typeof window.migrationToolsData !== 'undefined') {
    new ConfigurationWizard();
  } else {
    console.error('Migration tools data not available');
  }
});
