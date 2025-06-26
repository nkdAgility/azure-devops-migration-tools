/**
 * Azure DevOps Migration Tools Configuration Wizard
 * Interactive configuration builder for migration tools
 */

class ConfigurationWizard {
  constructor() {
    this.currentStep = 1;
    this.totalSteps = 6;
    this.configuration = {
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
    this.init();
  }

  init() {
    this.render();
    this.attachEventListeners();
    this.loadStep1();
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
    content.innerHTML = `
      <h2 class="text-primary mb-4">Choose Your Migration Type</h2>
      <p class="lead mb-4">What would you like to migrate? This will determine which processors and endpoint configurations are needed.</p>
      
      <div class="config-section">
        <div class="row">
          <div class="col-md-6 mb-3">
            <div class="config-option" data-migration-type="work-items">
              <div class="config-option-title">
                <i class="fas fa-tasks text-primary me-2"></i>Work Items
              </div>
              <div class="config-option-description">
                Migrate work items (User Stories, Bugs, Tasks, etc.) with their history, attachments, and links. Uses TfsWorkItemEndpoint.
              </div>
            </div>
          </div>
          <div class="col-md-6 mb-3">
            <div class="config-option" data-migration-type="test-plans">
              <div class="config-option-title">
                <i class="fas fa-clipboard-check text-primary me-2"></i>Test Plans
              </div>
              <div class="config-option-description">
                Migrate test plans, test suites, and test cases. Uses TfsWorkItemEndpoint for test artifacts.
              </div>
            </div>
          </div>
          <div class="col-md-6 mb-3">
            <div class="config-option" data-migration-type="pipelines">
              <div class="config-option-title">
                <i class="fas fa-code-branch text-primary me-2"></i>Pipelines
              </div>
              <div class="config-option-description">
                Migrate Azure DevOps pipelines (Build and Release definitions). Uses AzureDevOpsEndpoint.
              </div>
            </div>
          </div>
          <div class="col-md-6 mb-3">
            <div class="config-option" data-migration-type="work-items-test-plans">
              <div class="config-option-title">
                <i class="fas fa-layer-group text-primary me-2"></i>Work Items + Test Plans
              </div>
              <div class="config-option-description">
                Migrate both work items and test plans together. Uses TfsWorkItemEndpoint for both.
              </div>
            </div>
          </div>
        </div>
      </div>
    `;

    // Add click handlers for migration type selection
    content.querySelectorAll('.config-option').forEach(option => {
      option.addEventListener('click', () => {
        // Remove previous selection
        content.querySelectorAll('.config-option').forEach(opt => opt.classList.remove('selected'));
        // Add selection to clicked option
        option.classList.add('selected');
        this.selectedMigrationType = option.dataset.migrationType;
      });
    });
  }

  loadStep2() {
    const content = document.getElementById('wizard-step-content');
    const endpointType = this.getEndpointTypeForMigration();
    const endpointDescription = this.getEndpointDescription();
    
    content.innerHTML = `
      <h2 class="text-primary mb-4">Configure Source and Target Endpoints</h2>
      <p class="lead mb-4">Set up connections to your source and target instances.</p>
      
      <div class="alert alert-info mb-4">
        <i class="fas fa-info-circle me-2"></i>
        <strong>Endpoint Type:</strong> Your migration type requires <code>${endpointType}</code> endpoints.
        <br><small>${endpointDescription}</small>
      </div>
      
      <div class="row">
        <div class="col-md-6">
          <div class="config-section">
            <h3><i class="fas fa-upload text-success me-2"></i>Source Endpoint</h3>
            ${this.renderEndpointForm('source', endpointType)}
          </div>
        </div>
        <div class="col-md-6">
          <div class="config-section">
            <h3><i class="fas fa-download text-info me-2"></i>Target Endpoint</h3>
            ${this.renderEndpointForm('target', endpointType)}
          </div>
        </div>
      </div>
      
      <div class="alert alert-warning">
        <i class="fas fa-shield-alt me-2"></i>
        <strong>Security Note:</strong> Your credentials are only used to generate the configuration file. 
        They are not stored or transmitted anywhere.
      </div>
    `;
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
        <h3>Required Processors</h3>
        <p class="text-muted mb-3">These processors are automatically included for your migration type:</p>
        <div class="row">
          ${processors.required.map(proc => `
            <div class="col-md-6 mb-3">
              <div class="config-option selected">
                <div class="config-option-title">
                  <i class="fas fa-check-circle text-success me-2"></i>${proc.name}
                </div>
                <div class="config-option-description">${proc.description}</div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
      
      <div class="config-section">
        <h3>Optional Processors</h3>
        <p class="text-muted mb-3">Select additional processors you might need:</p>
        <div class="row">
          ${processors.optional.map(proc => `
            <div class="col-md-6 mb-3">
              <div class="config-option" data-processor="${proc.id}">
                <div class="config-option-title">
                  <i class="fas fa-plus-circle text-primary me-2"></i>${proc.name}
                </div>
                <div class="config-option-description">${proc.description}</div>
              </div>
            </div>
          `).join('')}
        </div>
      </div>
    `;

    // Add click handlers for optional processors
    content.querySelectorAll('.config-option[data-processor]').forEach(option => {
      option.addEventListener('click', () => {
        option.classList.toggle('selected');
        const processorId = option.dataset.processor;
        const index = this.selectedProcessors.indexOf(processorId);
        if (index === -1) {
          this.selectedProcessors.push(processorId);
        } else {
          this.selectedProcessors.splice(index, 1);
        }
      });
    });
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
    const endpointType = this.getEndpointTypeForMigration();
    
    // Update logging level from saved step 5 data
    const logLevel = this.savedAdvancedOptions?.logLevel || 'Information';
    this.configuration.Serilog.MinimumLevel = logLevel;
    
    // Generate the complete configuration based on user selections
    if (endpointType === 'AzureDevOpsEndpoint') {
      // For pipeline migrations
      this.configuration.MigrationTools.Endpoints = {
        Source: {
          EndpointType: "AzureDevOpsEndpoint",
          Organization: document.getElementById('sourceOrganization')?.value || "",
          Project: document.getElementById('sourceProject')?.value || "",
          Authentication: {
            AuthenticationMode: "AccessToken",
            AccessToken: "**redacted**"
          }
        },
        Target: {
          EndpointType: "AzureDevOpsEndpoint", 
          Organization: document.getElementById('targetOrganization')?.value || "",
          Project: document.getElementById('targetProject')?.value || "",
          Authentication: {
            AuthenticationMode: "AccessToken",
            AccessToken: "**redacted**"
          }
        }
      };
    } else {
      // For work item and test plan migrations
      this.configuration.MigrationTools.Endpoints = {
        Source: {
          EndpointType: "TfsTeamProjectEndpoint",
          Collection: document.getElementById('sourceCollection')?.value || "",
          Project: document.getElementById('sourceProject')?.value || "",
          Authentication: {
            AuthenticationMode: document.getElementById('sourceAuthMode')?.value || "AccessToken",
            AccessToken: "**redacted**"
          }
        },
        Target: {
          EndpointType: "TfsTeamProjectEndpoint",
          Collection: document.getElementById('targetCollection')?.value || "",
          Project: document.getElementById('targetProject')?.value || "",
          Authentication: {
            AuthenticationMode: document.getElementById('targetAuthMode')?.value || "AccessToken",
            AccessToken: "**redacted**"
          },
          ReflectedWorkItemIdField: document.getElementById('targetReflectedField')?.value || "Custom.ReflectedWorkItemId"
        }
      };
    }

    // Add processors based on migration type and selections
    this.configuration.MigrationTools.Processors = this.getSelectedProcessorsConfig();
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
