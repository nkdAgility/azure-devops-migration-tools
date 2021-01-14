using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.
    /// </summary>
    public partial class AzureDevOpsPipelineProcessor : Processor
    {
        private AzureDevOpsPipelineProcessorOptions _Options;

        public AzureDevOpsPipelineProcessor(ProcessorEnricherContainer processorEnrichers, EndpointContainer endpoints, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnrichers, endpoints, services, telemetry, logger)
        {
        }

        public AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)Endpoints.Source;

        public AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)Endpoints.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("AzureDevOpsPipelineProcessor::Configure");
            _Options = (AzureDevOpsPipelineProcessorOptions)options;
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            MigratePipelines();
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (!(Endpoints.Source is Endpoint))
            {
                throw new Exception("The Source endpoint configured must be of type WorkItemEndpoint");
            }
            if (!(Endpoints.Target is Endpoint))
            {
                throw new Exception("The Target endpoint configured must be of type WorkItemEndpoint");
            }
        }

        /// <summary>
        /// Executes Method for migrating Taskgroups, Variablegroups or Pipelines, depinding on whhat is set in the config.
        /// </summary>
        private void MigratePipelines()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<Mapping> serviceConnectionMappings = null;
            IEnumerable<Mapping> taskGroupMappings = null;
            IEnumerable<Mapping> variableGroupMappings = null;
            if (_Options.MigrateServiceConnections)
            {
                serviceConnectionMappings = CreateServiceConnections();
            }
            if (_Options.MigrateTaskGroups)
            {
                taskGroupMappings = CreateTaskGroupDefinitions();
            }
            if (_Options.MigrateVariableGroups)
            {
                variableGroupMappings = CreateVariableGroupDefinitions();
            }
            if (_Options.MigrateBuildPipelines)
            {
                CreateBuildPipelines(taskGroupMappings, variableGroupMappings);
            }

            if (_Options.MigrateReleasePipelines)
            {
                CreateReleasePipelines(taskGroupMappings, variableGroupMappings);
            }
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        /// <summary>
        /// Map the taskgroups that are already migrated
        /// </summary>
        /// <typeparam name="DefintionType"></typeparam>
        /// <param name="sourceDefinitions"></param>
        /// <param name="targetDefinitions"></param>
        /// <param name="newMappings"></param>
        /// <returns>Mapping list</returns>
        private IEnumerable<Mapping> FindExistingMappings<DefintionType>(IEnumerable<DefintionType> sourceDefinitions, IEnumerable<DefintionType> targetDefinitions, List<Mapping> newMappings)
            where DefintionType : RestApiDefinition, new()
        {
            // This is not safe, because the target project can have a taskgroup with the same name but with different content
            // To make this save we must add a local storage option for the mappings (sid, tid)
            var alreadyMigratedMappings = new List<Mapping>();
            var alreadyMigratedDefintions = targetDefinitions.Where(t => newMappings.Any(m => m.TId == t.Id) == false).ToList();
            foreach (var item in alreadyMigratedDefintions)
            {
                var source = sourceDefinitions.FirstOrDefault(d => d.Name == item.Name);
                if (source == null)
                {
                    Log.LogInformation("The {DefinitionType} {DefinitionName}({DefinitionId}) doesn't exsist in the source collection.", typeof(DefintionType).Name, item.Name, item.Id);
                }
                else
                {
                    alreadyMigratedMappings.Add(new()
                    {
                        SId = source.Id,
                        TId = item.Id,
                        Name = item.Name
                    });
                }
            }
            return alreadyMigratedMappings;
        }

        /// <summary>
        /// Filter existing Definitions
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="sourceDefinitions"></param>
        /// <param name="targetDefinitions"></param>
        /// <returns>List of filtered Definitions</returns>
        private IEnumerable<DefinitionType> filteredDefinitions<DefinitionType>(IEnumerable<DefinitionType> sourceDefinitions, IEnumerable<DefinitionType> targetDefinitions)
            where DefinitionType : RestApiDefinition, new()
        {
            var objectsToMigrate = sourceDefinitions.Where(s => !targetDefinitions.Any(t => t.Name == s.Name));

            Log.LogInformation("{ObjectsToBeMigrated} of {TotalObjects} source {DefinitionType}(s) are going to be migrated..", objectsToMigrate.Count(), sourceDefinitions.Count(), typeof(DefinitionType).Name);

            return objectsToMigrate;
        }

        private IEnumerable<Mapping> CreateBuildPipelines(IEnumerable<Mapping> TaskGroupMapping = null, IEnumerable<Mapping> VariableGroupMapping = null)
        {
            Log.LogInformation("Processing Build Pipelines..");

            var sourceDefinitions = Source.GetApiDefinitions<BuildDefinition>();
            var targetDefinitions = Target.GetApiDefinitions<BuildDefinition>();
            var definitionsToBeMigrated = filteredDefinitions(sourceDefinitions, targetDefinitions);

            definitionsToBeMigrated = FilterAwayIfAnyMapsAreMissing(definitionsToBeMigrated, TaskGroupMapping, VariableGroupMapping);
            // Replace taskgroup and variablegroup sIds with tIds
            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                if (TaskGroupMapping != null)
                {
                    foreach (var phase in definitionToBeMigrated.Process.Phases)
                    {
                        foreach (var step in phase.Steps)
                        {
                            if (step.Task.DefinitionType.ToLower() != "metaTask".ToLower())
                            {
                                continue;
                            }
                            var mapping = TaskGroupMapping
                                .Where(d => d.SId == step.Task.Id).FirstOrDefault();
                            if (mapping == null)
                            {
                                Log.LogWarning("Can't find taskgroup {MissingTaskGroupId} in the target collection.", step.Task.Id);
                            }
                            else
                            {
                                step.Task.Id = mapping.TId;
                            }
                        }
                    }
                }

                if (VariableGroupMapping != null)
                {
                    foreach (var variableGroup in definitionToBeMigrated.VariableGroups)
                    {
                        if (variableGroup != null)
                        {
                            continue;
                        }
                        var mapping = VariableGroupMapping
                            .Where(d => d.SId == variableGroup.Id).FirstOrDefault();
                        if (mapping == null)
                        {
                            Log.LogWarning("Can't find variablegroup {MissingVariableGroupId} in the target collection.", variableGroup.Id);
                        }
                        else
                        {
                            variableGroup.Id = mapping.TId;
                        }
                    }
                }

            }
            var mappings = Target.CreateApiDefinitions<BuildDefinition>(definitionsToBeMigrated.ToList());
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }

        private IEnumerable<Mapping> CreatePoolMappings<DefinitionType>()
            where DefinitionType : RestApiDefinition, new()
        {
            var sourcePools = Source.GetApiDefinitions<DefinitionType>();
            var targetPools = Target.GetApiDefinitions<DefinitionType>();
            var mappings = new List<Mapping>();
            foreach (var sourcePool in sourcePools)
            {
                var targetPool = targetPools.FirstOrDefault(t => t.Name == sourcePool.Name);
                if (targetPool is not null)
                {
                    mappings.Add(new()
                    {
                        SId = sourcePool.Id,
                        TId = targetPool.Id,
                        Name = targetPool.Name
                    });
                }
            }
            return mappings;
        }

        private void UpdateQueueIdForPhase(DeployPhase phase, IEnumerable<Mapping> mappings)
        {
            var mapping = mappings.FirstOrDefault(a => a.SId == phase.DeploymentInput.QueueId.ToString());
            if (mapping is not null)
            {
                phase.DeploymentInput.QueueId = int.Parse(mapping.TId);
            }
            else
            {
                phase.DeploymentInput.QueueId = 0;
            }
        }

        private IEnumerable<Mapping> CreateReleasePipelines(IEnumerable<Mapping> TaskGroupMapping = null, IEnumerable<Mapping> VariableGroupMapping = null)
        {
            Log.LogInformation($"Processing Release Pipelines..");

            var sourceDefinitions = Source.GetApiDefinitions<ReleaseDefinition>();
            var targetDefinitions = Target.GetApiDefinitions<ReleaseDefinition>();

            var agentPoolMappings = CreatePoolMappings<TaskAgentPool>();
            var deploymentGroupMappings = CreatePoolMappings<DeploymentGroup>();

            var definitionsToBeMigrated = filteredDefinitions(sourceDefinitions, targetDefinitions);
            if (_Options.ReleasePipelines is not null)
            {
                definitionsToBeMigrated = definitionsToBeMigrated.Where(d => _Options.ReleasePipelines.Contains(d.Name));
            }

            definitionsToBeMigrated = FilterAwayIfAnyMapsAreMissing(definitionsToBeMigrated, TaskGroupMapping, VariableGroupMapping);

            // Replace taskgroup and variablegroup sIds with tIds
            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                UpdateQueueIdOnPhases(definitionToBeMigrated, agentPoolMappings, deploymentGroupMappings);

                UpdateTaskGroupId(definitionToBeMigrated, TaskGroupMapping);

                if (VariableGroupMapping is not null)
                {
                    UpdateVariableGroupId(definitionToBeMigrated.VariableGroups, VariableGroupMapping);

                    foreach (var environment in definitionToBeMigrated.Environments)
                    {
                        UpdateVariableGroupId(environment.VariableGroups, VariableGroupMapping);
                    }
                }
            }

            var mappings = Target.CreateApiDefinitions<ReleaseDefinition>(definitionsToBeMigrated);
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }

        private IEnumerable<DefinitionType> FilterAwayIfAnyMapsAreMissing<DefinitionType>(
                                                IEnumerable<DefinitionType> definitionsToBeMigrated,
                                                IEnumerable<Mapping> TaskGroupMapping,
                                                IEnumerable<Mapping> VariableGroupMapping)
            where DefinitionType : RestApiDefinition
        {
            //filter away definitions that contains task or variable groups if we dont have those mappings
            if (TaskGroupMapping is null)
            {
                var containsTaskGroup = definitionsToBeMigrated.Any(d => d.HasTaskGroups());
                if (containsTaskGroup)
                {
                    Log.LogWarning("You can't migrate pipelines that uses taskgroups if you didn't migrate taskgroups");
                    definitionsToBeMigrated = definitionsToBeMigrated.Where(d => d.HasTaskGroups() == false);
                }
            }
            if (VariableGroupMapping is null)
            {
                var containsVariableGroup = definitionsToBeMigrated.Any(d => d.HasVariableGroups());
                if (containsVariableGroup)
                {
                    Log.LogWarning("You can't migrate pipelines that uses variablegroups if you didn't migrate variablegroups");
                    definitionsToBeMigrated = definitionsToBeMigrated.Where(d => d.HasTaskGroups() == false);
                }
            }

            return definitionsToBeMigrated;
        }

        private void UpdateVariableGroupId(int[] variableGroupIds, IEnumerable<Mapping> VariableGroupMapping)
        {
            for (int i = 0; i < variableGroupIds.Length; i++)
            {
                var oldId = variableGroupIds[i].ToString();
                var mapping = VariableGroupMapping.Where(d => d.SId == oldId).FirstOrDefault();
                if (mapping is not null)
                {
                    variableGroupIds[i] = int.Parse(mapping.TId);
                }
                else
                {
                    //Not sure if we should exit hard in this case?
                    Log.LogWarning("Can't find variablegroups {OldVariableGroupId} in the target collection.", oldId);
                }
            }
        }

        private void UpdateTaskGroupId(ReleaseDefinition definitionToBeMigrated, IEnumerable<Mapping> TaskGroupMapping)
        {
            if (TaskGroupMapping != null)
            {
                var Environments = definitionToBeMigrated.Environments;
                foreach (var environment in Environments)
                {
                    foreach (var deployPhase in environment.DeployPhases)
                    {
                        foreach (var WorkflowTask in deployPhase.WorkflowTasks)
                        {
                            if (WorkflowTask.DefinitionType.ToLower() != "metaTask".ToLower())
                            {
                                continue;
                            }
                            var mapping = TaskGroupMapping
                                .Where(d => d.SId == WorkflowTask.TaskId.ToString()).FirstOrDefault();
                            if (mapping == null)
                            {
                                Log.LogWarning("Can't find taskgroup {TaskGroupId} in the target collection.", WorkflowTask.TaskId);
                            }
                            else
                            {
                                WorkflowTask.TaskId = Guid.Parse(mapping.TId);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateQueueIdOnPhases(ReleaseDefinition definitionToBeMigrated, IEnumerable<Mapping> agentPoolMappings, IEnumerable<Mapping> deploymentGroupMappings)
        {
            foreach (var environment in definitionToBeMigrated.Environments)
            {
                foreach (var phase in environment.DeployPhases)
                {
                    if (phase.PhaseType == "agentBasedDeployment")
                    {
                        UpdateQueueIdForPhase(phase, agentPoolMappings);
                    }
                    else if (phase.PhaseType == "machineGroupBasedDeployment")
                    {
                        UpdateQueueIdForPhase(phase, deploymentGroupMappings);
                    }
                }
            }
        }

        private IEnumerable<Mapping> CreateServiceConnections()
        {
            Log.LogInformation($"Processing Service Connections..");

            var sourceDefinitions = Source.GetApiDefinitions<ServiceConnection>();
            var targetDefinitions = Target.GetApiDefinitions<ServiceConnection>();
            var mappings = Target.CreateApiDefinitions(filteredDefinitions(sourceDefinitions, targetDefinitions));
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }

        private IEnumerable<Mapping> CreateTaskGroupDefinitions()
        {
            Log.LogInformation($"Processing Taskgroups..");

            var sourceDefinitions = Source.GetApiDefinitions<TaskGroup>();
            var targetDefinitions = Target.GetApiDefinitions<TaskGroup>();
            var mappings = Target.CreateApiDefinitions(filteredDefinitions(sourceDefinitions, targetDefinitions));
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }

        private IEnumerable<Mapping> CreateVariableGroupDefinitions()
        {
            Log.LogInformation($"Processing Variablegroups..");

            var sourceDefinitions = Source.GetApiDefinitions<VariableGroups>();
            var targetDefinitions = Target.GetApiDefinitions<VariableGroups>();
            var filteredDefinition = filteredDefinitions(sourceDefinitions, targetDefinitions);
            foreach (var variableGroup in filteredDefinition)
            {
                //was needed when now trying to migrated to azure devops services
                variableGroup.VariableGroupProjectReferences = new VariableGroupProjectReference[1];
                variableGroup.VariableGroupProjectReferences[0] = new VariableGroupProjectReference
                                {
                                    Name = variableGroup.Name,
                                    ProjectReference = new ProjectReference
                                                        {
                                                            Name = Target.Project
                                                        }
                                };
            }
            var mappings = Target.CreateApiDefinitions(filteredDefinition);
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }
    }
}