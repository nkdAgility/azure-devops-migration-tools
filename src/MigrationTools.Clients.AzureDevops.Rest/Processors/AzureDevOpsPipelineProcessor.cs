using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Pipelines</processingtarget>
    public partial class AzureDevOpsPipelineProcessor : Processor
    {
        public AzureDevOpsPipelineProcessor(IOptions<AzureDevOpsPipelineProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        public new AzureDevOpsPipelineProcessorOptions Options => (AzureDevOpsPipelineProcessorOptions)base.Options;

        public new AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)base.Source;

        public new AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)base.Target;


        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            MigratePipelinesAsync().GetAwaiter().GetResult();
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (Source is not AzureDevOpsEndpoint)
            {
                throw new Exception("The Source endpoint configured must be of type AzureDevOpsEndpoint");
            }
            if (Target is not AzureDevOpsEndpoint)
            {
                throw new Exception("The Target endpoint configured must be of type AzureDevOpsEndpoint");
            }
        }

        /// <summary>
        /// Executes Method for migrating Taskgroups, Variablegroups or Pipelines, depinding on what
        /// is set in the config.
        /// </summary>
        private async System.Threading.Tasks.Task MigratePipelinesAsync()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<Mapping> serviceConnectionMappings = null;
            IEnumerable<Mapping> taskGroupMappings = null;
            IEnumerable<Mapping> variableGroupMappings = null;
            if (Options.MigrateServiceConnections)
            {
                serviceConnectionMappings = await CreateServiceConnectionsAsync();
            }
            if (Options.MigrateVariableGroups)
            {
                variableGroupMappings = await CreateVariableGroupDefinitionsAsync();
            }
            if (Options.MigrateTaskGroups)
            {
                taskGroupMappings = await CreateTaskGroupDefinitionsAsync();
            }
            if (Options.MigrateBuildPipelines)
            {
                await CreateBuildPipelinesAsync(taskGroupMappings, variableGroupMappings, serviceConnectionMappings);
            }

            if (Options.MigrateReleasePipelines)
            {
                await CreateReleasePipelinesAsync(taskGroupMappings, variableGroupMappings, serviceConnectionMappings);
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
            // This is not safe, because the target project can have a taskgroup with the same name
            // but with different content To make this save we must add a local storage option for
            // the mappings (sid, tid)
            var alreadyMigratedMappings = new List<Mapping>();
            var alreadyMigratedDefintions = targetDefinitions.Where(t => newMappings.Any(m => m.TargetId == t.Id) == false).ToList();
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
                        SourceId = source.Id,
                        TargetId = item.Id,
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
        private IEnumerable<DefinitionType> FilterOutExistingDefinitions<DefinitionType>(IEnumerable<DefinitionType> sourceDefinitions, IEnumerable<DefinitionType> targetDefinitions)
            where DefinitionType : RestApiDefinition, new()
        {
            var objectsToMigrate = sourceDefinitions.Where(s => !targetDefinitions.Any(t => t.Name == s.Name));

            Log.LogInformation("{ObjectsToBeMigrated} of {TotalObjects} source {DefinitionType}(s) are going to be migrated..", objectsToMigrate.Count(), sourceDefinitions.Count(), typeof(DefinitionType).Name);

            return objectsToMigrate;
        }

        /// <summary>
        /// Filter incompatible TaskGroups
        /// </summary>
        /// <param name="sourceDefinitions"></param>
        /// <param name="availableTasks"></param>
        /// <param name="taskGroupMapping"></param>
        /// <returns>List of filtered Definitions</returns>
        private IEnumerable<BuildDefinition> FilterOutIncompatibleBuildDefinitions(IEnumerable<BuildDefinition> sourceDefinitions, IEnumerable<TaskDefinition> availableTasks, IEnumerable<Mapping> taskGroupMapping)
        {
            var objectsToMigrate = sourceDefinitions.Where(g =>
            {
                var missingTasksNames = new List<string>();
                var allTasksAreAvailable = g.Process.Phases.Select(p => p.Steps).SelectMany(s => s).All(t =>
                {
                    if (availableTasks.Any(a => a.Id == t.Task.Id) || taskGroupMapping.Any(m => m.SourceId == t.Task.Id))
                    {
                        return true;
                    }
                    missingTasksNames.Add(t.DisplayName);
                    return false;
                });

                if (!allTasksAreAvailable)
                {
                    Log.LogWarning(
                        @"{DefinitionType} ""{DefinitionName}"" cannot be migrated because the Task(s) ""{MissingTaskNames}"" are not available. This usually happens if the extension for the task is not installed.",
                        typeof(BuildDefinition).Name, g.Name, string.Join(",", missingTasksNames));
                    return false;
                }
                return true;
            });
            return objectsToMigrate;
        }

        /// <summary>
        /// Filter existing TaskGroups
        /// </summary>
        /// <param name="sourceDefinitions"></param>
        /// <param name="targetDefinitions"></param>
        /// <returns>List of filtered Definitions</returns>
        private IEnumerable<TaskGroup> FilterOutExistingTaskGroups(IEnumerable<TaskGroup> sourceDefinitions, IEnumerable<TaskGroup> targetDefinitions)
        {
            var objectsToMigrate = sourceDefinitions.Where(s => !targetDefinitions.Any(t => t.Name == s.Name));
            var rootSourceDefinitions = SortDefinitionsByVersion(objectsToMigrate).First();
            Log.LogInformation("{ObjectsToBeMigrated} of {TotalObjects} source {DefinitionType}(s) are going to be migrated..", objectsToMigrate.GroupBy(o => o.Name).Where(o => o.Count() >= 1).Count(), rootSourceDefinitions.Count(), typeof(TaskGroup).Name);
            return objectsToMigrate;
        }

        /// <summary>
        /// Filter incompatible TaskGroups
        /// </summary>
        /// <param name="filteredTaskGroups"></param>
        /// <param name="availableTasks"></param>
        /// <returns>List of filtered Definitions</returns>
        private IEnumerable<TaskGroup> FilterOutIncompatibleTaskGroups(IEnumerable<TaskGroup> filteredTaskGroups, IEnumerable<TaskDefinition> availableTasks)
        {
            var objectsToMigrate = filteredTaskGroups.Where(g =>
            {
                var missingTasksNames = new List<string>();
                var allTasksAreAvailable = g.Tasks.All(t =>
                {
                    if (availableTasks.Any(a => a.Id == t.Task.Id))
                    {
                        return true;
                    }
                    missingTasksNames.Add(t.DisplayName);
                    return false;
                });

                if (!allTasksAreAvailable)
                {
                    Log.LogWarning(
                        @"{DefinitionType} ""{DefinitionName}"" cannot be migrated because the Task(s) ""{MissingTaskNames}"" are not available. This usually happens if the extension for the task is not installed.",
                        typeof(TaskGroup).Name, g.Name, string.Join(",", missingTasksNames));
                    return false;
                }
                return true;
            });
            return objectsToMigrate;
        }

        /// <summary>
        /// Group and Sort Definitions by Version numer
        /// </summary>
        /// <param name="sourceDefinitions"></param>
        /// <returns>List of sorted Definitions</returns>
        private IEnumerable<IEnumerable<TaskGroup>> SortDefinitionsByVersion(IEnumerable<TaskGroup> sourceDefinitions)
        {
            var groupList = new List<IEnumerable<TaskGroup>>();
            sourceDefinitions.OrderBy(d => d.Version.Major);
            var rootGroups = sourceDefinitions.Where(d => d.Version.Major == 1);
            var updatedGroups = sourceDefinitions.Where(d => d.Version.Major > 1);
            groupList.Add(rootGroups);
            groupList.Add(updatedGroups);

            return groupList;
        }

        /// <summary>
        /// Retrieve the selected pipeline definitions from the Azure DevOps Endpoint for the <typeparamref name="DefinitionType"/> type.
        /// </summary>
        /// <typeparam name="DefinitionType">The type of Pipeline definition to query. The type must inherit from <see cref="RestApiDefinition"/>.</typeparam>
        /// <param name="endpoint">The <see cref="AzureDevOpsEndpoint"/> to query against.</param>
        /// <param name="definitionNames">The list of definitions to query for. If the value is <c>null</c> or an empty list, all definitions will be queried.</param>
        /// <returns></returns>
        private async Task<IEnumerable<DefinitionType>> GetSelectedDefinitionsFromEndpointAsync<DefinitionType>(AzureDevOpsEndpoint endpoint, List<string> definitionNames)
            where DefinitionType : RestApiDefinition, new()
        {
            IEnumerable<Task<IEnumerable<DefinitionType>>> GetDefinitionListTasks(AzureDevOpsEndpoint endpoint, List<string> definitionNames) =>
                definitionNames switch
                {
                    null or { Count: 0 } => new List<Task<IEnumerable<DefinitionType>>> { endpoint.GetApiDefinitionsAsync<DefinitionType>() },
                    not null when typeof(DefinitionType) == typeof(BuildDefinition) => definitionNames.ConvertAll(d => endpoint.GetApiDefinitionsAsync<DefinitionType>(queryString: $"name={d}")),
                    not null when typeof(DefinitionType) == typeof(ReleaseDefinition) => definitionNames.ConvertAll(d => endpoint.GetApiDefinitionsAsync<DefinitionType>(queryString: $"searchText={d}&isExactNameMatch=true")),
                    _ => new List<Task<IEnumerable<DefinitionType>>>()
                };

            Log.LogInformation("Querying definitions in the project: {ProjectName}", endpoint.Options.Project);
            Log.LogInformation("Configured {Definition} definitions: {DefinitionList}",
                typeof(DefinitionType).Name,
                definitionNames == null || definitionNames.Count == 0 ? "All" : String.Join(";", definitionNames));

            var listTasks = GetDefinitionListTasks(endpoint, definitionNames);
            var executedTasks = await System.Threading.Tasks.Task.WhenAll(listTasks).ConfigureAwait(false);

            return executedTasks
                .SelectMany(t => t)
                .ToList();
        }

        private async Task<IEnumerable<Mapping>> CreateBuildPipelinesAsync(IEnumerable<Mapping> TaskGroupMapping = null, IEnumerable<Mapping> VariableGroupMapping = null, IEnumerable<Mapping> serviceConnectionMappings = null)
        {
            Log.LogInformation("Processing Build Pipelines..");

            var sourceDefinitions = await GetSelectedDefinitionsFromEndpointAsync<BuildDefinition>(Source, Options.BuildPipelines);
            var targetDefinitions = await GetSelectedDefinitionsFromEndpointAsync<BuildDefinition>(Target, Options.BuildPipelines);
            var availableTasks = await Target.GetApiDefinitionsAsync<TaskDefinition>(queryForDetails: false);
            var sourceServiceConnections = await Source.GetApiDefinitionsAsync<ServiceConnection>();
            var targetServiceConnections = await Target.GetApiDefinitionsAsync<ServiceConnection>();
            var sourceRepositories = await Source.GetApiDefinitionsAsync<GitRepository>(queryForDetails: false);
            var targetRepositories = await Target.GetApiDefinitionsAsync<GitRepository>(queryForDetails: false);
            var definitionsToBeMigrated = FilterOutExistingDefinitions(sourceDefinitions, targetDefinitions);
            definitionsToBeMigrated = FilterOutIncompatibleBuildDefinitions(definitionsToBeMigrated, availableTasks, TaskGroupMapping).ToList();
            definitionsToBeMigrated = FilterAwayIfAnyMapsAreMissing(definitionsToBeMigrated, TaskGroupMapping, VariableGroupMapping);

            // Replace taskgroup and variablegroup sIds with tIds
            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                var sourceConnectedServiceId = definitionToBeMigrated.Repository?.Properties?.ConnectedServiceId;
                var targetConnectedServiceId = targetServiceConnections.FirstOrDefault(s => sourceServiceConnections
                    .FirstOrDefault(c => c.Id == sourceConnectedServiceId)?.Name == s.Name)?.Id;

                if (definitionToBeMigrated.Repository?.Properties != null)
                {
                    definitionToBeMigrated.Repository.Properties.ConnectedServiceId = targetConnectedServiceId;
                    MapRepositoriesInBuidDefinition(sourceRepositories, targetRepositories, definitionToBeMigrated);
                }

                if (TaskGroupMapping is not null)
                {
                    foreach (var phase in definitionToBeMigrated.Process.Phases)
                    {
                        foreach (var step in phase.Steps)
                        {
                            if (step.Task.DefinitionType.ToLower() != "metaTask".ToLower())
                            {
                                continue;
                            }
                            var mapping = TaskGroupMapping.FirstOrDefault(d => d.SourceId == step.Task.Id);
                            if (mapping == null)
                            {
                                Log.LogWarning("Can't find taskgroup {MissingTaskGroupId} in the target collection.", step.Task.Id);
                            }
                            else
                            {
                                step.Task.Id = mapping.TargetId;
                            }
                        }
                    }
                }

                if (VariableGroupMapping is not null && definitionToBeMigrated.VariableGroups is not null)
                {
                    foreach (var variableGroup in definitionToBeMigrated.VariableGroups)
                    {
                        if (variableGroup == null)
                        {
                            continue;
                        }
                        var mapping = VariableGroupMapping.FirstOrDefault(d => d.SourceId == variableGroup.Id);
                        if (mapping == null)
                        {
                            Log.LogWarning("Can't find variablegroup {MissingVariableGroupId} in the target collection.", variableGroup.Id);
                        }
                        else
                        {
                            variableGroup.Id = mapping.TargetId;
                        }
                    }
                }

                if (serviceConnectionMappings is not null)
                {
                    foreach (var phase in definitionToBeMigrated.Process.Phases)
                    {
                        foreach (var step in phase.Steps)
                        {
                            var newInputs = new Dictionary<string, object>();
                            foreach (var input in (IDictionary<String, Object>)step.Inputs)
                            {
                                var mapping = serviceConnectionMappings.FirstOrDefault(d => d.SourceId == input.Value.ToString());
                                if (mapping != null)
                                {
                                    newInputs.Add(input.Key, mapping.TargetId);
                                }
                            }

                            foreach (var input in newInputs)
                            {
                                ((IDictionary<String, Object>)step.Inputs).Remove(input.Key);
                                ((IDictionary<String, Object>)step.Inputs).Add(input.Key, input.Value);
                            }
                        }
                    }
                }
            }
            var mappings = await Target.CreateApiDefinitionsAsync<BuildDefinition>(definitionsToBeMigrated.ToList());
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }

        private void MapRepositoriesInBuidDefinition(IEnumerable<GitRepository> sourceRepositories, IEnumerable<GitRepository> targetRepositories, BuildDefinition definitionToBeMigrated)
        {
            var sourceRepoId = definitionToBeMigrated.Repository.Id;
            string sourceRepositoryName = sourceRepositories.FirstOrDefault(s => s.Id == sourceRepoId)?.Name ?? string.Empty;
            string targetRepoId;

            if (Options.RepositoryNameMaps.ContainsKey(sourceRepositoryName))  //Map repository name if configured
            {
                targetRepoId = targetRepositories.FirstOrDefault(r => Options.RepositoryNameMaps[sourceRepositoryName] == r.Name)?.Id;
            }
            else
            {
                targetRepoId = targetRepositories.FirstOrDefault(r => sourceRepositoryName == r.Name)?.Id;
            }
            definitionToBeMigrated.Repository.Id = targetRepoId;
        }

        private async Task<IEnumerable<Mapping>> CreatePoolMappingsAsync<DefinitionType>()
            where DefinitionType : RestApiDefinition, new()
        {
            var sourcePools = await Source.GetApiDefinitionsAsync<DefinitionType>();
            var targetPools = await Target.GetApiDefinitionsAsync<DefinitionType>();
            var mappings = new List<Mapping>();
            foreach (var sourcePool in sourcePools)
            {
                var targetPool = targetPools.FirstOrDefault(t => t.Name == sourcePool.Name);
                if (targetPool is not null)
                {
                    mappings.Add(new()
                    {
                        SourceId = sourcePool.Id,
                        TargetId = targetPool.Id,
                        Name = targetPool.Name
                    });
                }
            }
            return mappings;
        }

        private void UpdateQueueIdForPhase(DeployPhase phase, IEnumerable<Mapping> mappings)
        {
            var mapping = mappings.FirstOrDefault(a => a.SourceId == phase.DeploymentInput.QueueId.ToString());
            if (mapping is not null)
            {
                phase.DeploymentInput.QueueId = int.Parse(mapping.TargetId);
            }
            else
            {
                phase.DeploymentInput.QueueId = 0;
            }
        }

        private async Task<IEnumerable<Mapping>> CreateReleasePipelinesAsync(IEnumerable<Mapping> TaskGroupMapping = null, IEnumerable<Mapping> VariableGroupMapping = null, IEnumerable<Mapping> ServiceConnectionMappings = null)
        {
            Log.LogInformation($"Processing Release Pipelines..");

            var sourceDefinitions = await GetSelectedDefinitionsFromEndpointAsync<ReleaseDefinition>(Source, Options.ReleasePipelines);
            var targetDefinitions = await GetSelectedDefinitionsFromEndpointAsync<ReleaseDefinition>(Target, Options.ReleasePipelines);

            var agentPoolMappings = await CreatePoolMappingsAsync<TaskAgentPool>();
            var deploymentGroupMappings = await CreatePoolMappingsAsync<DeploymentGroup>();

            var definitionsToBeMigrated = FilterOutExistingDefinitions(sourceDefinitions, targetDefinitions);
            definitionsToBeMigrated = FilterAwayIfAnyMapsAreMissing(definitionsToBeMigrated, TaskGroupMapping, VariableGroupMapping);

            // Replace queue, taskgroup and variablegroup sourceIds with targetIds
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

                UpdateServiceConnectionId(definitionToBeMigrated, ServiceConnectionMappings);
            }

            var mappings = await Target.CreateApiDefinitionsAsync<ReleaseDefinition>(definitionsToBeMigrated);
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
                var mapping = VariableGroupMapping.FirstOrDefault(d => d.SourceId == oldId);
                if (mapping is not null)
                {
                    variableGroupIds[i] = int.Parse(mapping.TargetId);
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
            if (TaskGroupMapping is null)
            {
                return;
            }
            foreach (var environment in definitionToBeMigrated.Environments)
            {
                foreach (var deployPhase in environment.DeployPhases)
                {
                    foreach (var WorkflowTask in deployPhase.WorkflowTasks)
                    {
                        if (WorkflowTask.DefinitionType != null && WorkflowTask.DefinitionType.ToLower() != "metaTask".ToLower())
                        {
                            continue;
                        }
                        var mapping = TaskGroupMapping.FirstOrDefault(d => d.SourceId == WorkflowTask.TaskId.ToString());
                        if (mapping == null)
                        {
                            Log.LogWarning("Can't find taskgroup {TaskGroupName} in the target collection.", WorkflowTask.Name);
                        }
                        else
                        {
                            WorkflowTask.TaskId = Guid.Parse(mapping.TargetId);
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

        private void UpdateServiceConnectionId(ReleaseDefinition definitionToBeMigrated, IEnumerable<Mapping> ServiceConnectionMappings)
        {
            if (ServiceConnectionMappings is null)
            {
                return;
            }

            foreach (var environment in definitionToBeMigrated.Environments)
            {
                foreach (var deployPhase in environment.DeployPhases)
                {
                    foreach (var workflowTask in deployPhase.WorkflowTasks)
                    {
                        bool hasFoundInputWhichNeedsReplacement = false;
                        string inputNameWhichNeedsValueReplacement = "azureSubscription";
                        string valueOfInputThatNeedsToBeMapped = string.Empty;

                        foreach (var input in workflowTask.Inputs)
                        {
                            if (input.Key == inputNameWhichNeedsValueReplacement)
                            {
                                valueOfInputThatNeedsToBeMapped = input.Value.ToString();
                                hasFoundInputWhichNeedsReplacement = true;
                                break;
                            }
                        }

                        if (hasFoundInputWhichNeedsReplacement)
                        {
                            Mapping scMapping = ServiceConnectionMappings.FirstOrDefault(sc => sc.SourceId == valueOfInputThatNeedsToBeMapped);

                            IDictionary<string, object> workflowTaskInputs = workflowTask.Inputs;
                            workflowTaskInputs.Remove(inputNameWhichNeedsValueReplacement);
                            workflowTaskInputs.Add(inputNameWhichNeedsValueReplacement, scMapping.TargetId);
                            workflowTask.Inputs = (System.Dynamic.ExpandoObject)workflowTaskInputs;
                        }
                    }
                }
            }
        }

        private async Task<IEnumerable<Mapping>> CreateServiceConnectionsAsync()
        {
            Log.LogInformation($"Processing Service Connections..");

            var sourceDefinitions = await Source.GetApiDefinitionsAsync<ServiceConnection>();
            var targetDefinitions = await Target.GetApiDefinitionsAsync<ServiceConnection>();
            var mappings = await Target.CreateApiDefinitionsAsync(FilterOutExistingDefinitions(sourceDefinitions, targetDefinitions));
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }

        private async Task<IEnumerable<Mapping>> CreateTaskGroupDefinitionsAsync()
        {
            Log.LogInformation($"Processing Taskgroups..");

            var sourceDefinitions = await Source.GetApiDefinitionsAsync<TaskGroup>(queryForDetails: false);
            var targetDefinitions = await Target.GetApiDefinitionsAsync<TaskGroup>(queryForDetails: false);
            var availableTasks = await Target.GetApiDefinitionsAsync<TaskDefinition>(queryForDetails: false);
            var filteredTaskGroups = FilterOutExistingTaskGroups(sourceDefinitions, targetDefinitions);
            filteredTaskGroups = FilterOutIncompatibleTaskGroups(filteredTaskGroups, availableTasks).ToList();

            var rootSourceDefinitions = SortDefinitionsByVersion(filteredTaskGroups).First();
            var updatedSourceDefinitions = SortDefinitionsByVersion(filteredTaskGroups).Last();

            var mappings = await Target.CreateApiDefinitionsAsync(rootSourceDefinitions);

            targetDefinitions = await Target.GetApiDefinitionsAsync<TaskGroup>(queryForDetails: false);
            var rootTargetDefinitions = SortDefinitionsByVersion(targetDefinitions).First();
            await Target.UpdateTaskGroupsAsync(rootTargetDefinitions, updatedSourceDefinitions);

            targetDefinitions = await Target.GetApiDefinitionsAsync<TaskGroup>(queryForDetails: false);
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions.Where(d => d.Name != null), mappings));
            return mappings;
        }

        private async Task<IEnumerable<Mapping>> CreateVariableGroupDefinitionsAsync()
        {
            Log.LogInformation($"Processing Variablegroups..");

            var sourceDefinitions = await Source.GetApiDefinitionsAsync<VariableGroups>();
            var targetDefinitions = await Target.GetApiDefinitionsAsync<VariableGroups>();
            var filteredDefinition = FilterOutExistingDefinitions(sourceDefinitions, targetDefinitions);
            foreach (var variableGroup in filteredDefinition)
            {
                //was needed when now trying to migrated to azure devops services
                variableGroup.VariableGroupProjectReferences = new VariableGroupProjectReference[1];
                variableGroup.VariableGroupProjectReferences[0] = new VariableGroupProjectReference
                {
                    Name = variableGroup.Name,
                    ProjectReference = new ProjectReference
                    {
                        Name = Target.Options.Project
                    }
                };
            }
            var mappings = await Target.CreateApiDefinitionsAsync(filteredDefinition);
            mappings.AddRange(FindExistingMappings(sourceDefinitions, targetDefinitions, mappings));
            return mappings;
        }
    }
}
