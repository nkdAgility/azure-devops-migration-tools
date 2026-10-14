using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.DataContracts.Process;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Task = System.Threading.Tasks.Task;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Options;
using Microsoft.Extensions.Options;
using MigrationTools.Tools;

namespace MigrationTools.Processors
{

    internal class ProcessorModel
    {
        public Dictionary<string, ProcessDefinitionModel> ProcessDefinitions { get; set; } = new Dictionary<string, ProcessDefinitionModel>();
        public Dictionary<string, WorkItemTypeModel> WorkItemTypes { get; set; } = new Dictionary<string, WorkItemTypeModel>();
        public Dictionary<string, WorkItemTypeField> WorkItemFields { get; set; } = new Dictionary<string, WorkItemTypeField>();
        public Dictionary<string, WorkItemBehavior> WorkItemBehaviors { get; set; } = new Dictionary<string, WorkItemBehavior>();
        public Dictionary<string, WorkItemPage> WorkItemPages { get; set; } = new Dictionary<string, WorkItemPage>();
        public Dictionary<string, WorkItemGroup> WorkItemGroups { get; set; } = new Dictionary<string, WorkItemGroup>();
        public Dictionary<string, WorkItemControl> WorkItemControls { get; set; } = new Dictionary<string, WorkItemControl>();
    }
    /// <summary>
    /// Model that carries meta and state of an individual process definition
    /// </summary>
    public class ProcessDefinitionModel
    {
        public string MappedFrom { get; set; }
        public ProcessDefinition Process { get; set; }
        public List<WorkItemTypeModel> WorkItemTypes { get; set; }
        public List<WorkItemBehavior> WorkItemBehaviors { get; set; }
    }
    /// <summary>
    /// Model that carries meta and state of an individual work item type
    /// </summary>
    public class WorkItemTypeModel
    {
        public WorkItemType WorkItemType { get; set; }
        public List<WorkItemTypeField> Fields { get; set; } = new List<WorkItemTypeField>();
        public Dictionary<string, WorkItemState> States { get; set; } = new Dictionary<string, WorkItemState>();
        public Dictionary<string, WorkItemRule> Rules { get; set; } = new Dictionary<string, WorkItemRule>();
        public Dictionary<string, WorkItemTypeBehavior> Behaviors { get; set; } = new Dictionary<string, WorkItemTypeBehavior>();
        public WorkItemLayout Layout { get; internal set; }
    }

    /// <summary>
    /// Process definition processor used to keep processes between two orgs in sync
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Pipelines</processingtarget>
    public partial class ProcessDefinitionProcessor : Processor
    {
        private ProcessorModel SourceModel;
        private ProcessorModel TargetModel;
        private Dictionary<string, WorkItemField> SourceFields = new Dictionary<string, WorkItemField>();

        private WorkItemLayout SourceLayout { get; set; }

        private Dictionary<string, WorkItemField> TargetFields = new Dictionary<string, WorkItemField>();

        public ProcessDefinitionProcessor(IOptions<ProcessDefinitionProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
            SourceModel = new ProcessorModel();
            TargetModel = new ProcessorModel();
        }

        private WorkItemLayout TargetLayout { get; set; }



        public new ProcessDefinitionProcessorOptions Options => (ProcessDefinitionProcessorOptions)base.Options;

        public new AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)base.Source;

        public new AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)base.Target;

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            Synchronize().GetAwaiter().GetResult();
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

            if (Options.Processes.Count == 0) Options.Processes.Add("*", new List<string>() { "*" });

            if (Source.Options.Name == Target.Options.Name)
            {
                throw new Exception("Source and Target need to be defined independently for parallel processing to work.");
            }
        }
        private async Task Synchronize()
        {
            try
            {
                // Lets first start by taking inventory of what we have in source and target, in parallel.
                await Task.WhenAll(
                    BuildModel(SourceModel, Source, true),
                    BuildModel(TargetModel, Target, false));
                /*
                // Comment out the below for Synchronous model loading - Usually for debugging purposes
                await BuildModel(TargetModel, Target, false);
                await BuildModel(SourceModel, Source, true);
                */
                Log.LogInformation("Source and target data models established.");

                Log.LogInformation("Synchronizing organization level fields.");
                // We've got all the target and sources data.. let's start syncronizing
                await SourceFields.Values.ParallelForEachAsync(Math.Max(1, Options.MaxDegreeOfParallelism), async (sourceField) =>
                {
                    if (sourceField.ReferenceName.StartsWith("System.") || sourceField.ReferenceName.StartsWith("Microsoft."))
                    {
                        return;
                    }
                    WorkItemField targetField = null;
                    if (!TargetFields.ContainsKey(sourceField.ReferenceName))
                    {
                        await SyncDefinitionType<WorkItemField>(TargetFields, sourceField, targetField);
                    }
                });

                await SourceModel.ProcessDefinitions.Values.ParallelForEachAsync(Math.Max(1,Options.MaxDegreeOfParallelism), SyncProcess);

                if (Target.FailedSyncWrites > 0)
                {
                    Log.LogError("{FailedWrites} process definition change(s) were rejected by [{Target}]; see the errors above. Marking the run as failed.", Target.FailedSyncWrites, Target.Options.Name);
                    CompletedWithErrors = true;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Failed to synchronize processes.");
                throw;
            }
        }

        private async Task SyncProcess(ProcessDefinitionModel sourceProc)
        {
            Log.LogInformation($"Starting sync of process [{sourceProc.Process.Name}] in [{Source.Options.Name}].");

            var targetProc = TargetModel.ProcessDefinitions.FirstOrDefault(tgtProc => tgtProc.Value.MappedFrom.Equals(sourceProc.Process.Name, StringComparison.OrdinalIgnoreCase)).Value ?? new ProcessDefinitionModel()
            {
                MappedFrom = sourceProc.Process.Name
            };
            if (targetProc.Process == null)
            {
                var targetProcessName = ProcessDefinitionSync.ResolveTargetProcessName(Options.ProcessMaps, sourceProc.Process.Name);
                var newProcess = sourceProc.Process.CloneAsNew();
                if (!targetProcessName.Equals(sourceProc.Process.Name, StringComparison.OrdinalIgnoreCase))
                {
                    newProcess.Name = targetProcessName;
                    newProcess.ReferenceName = null;
                }
                targetProc.Process = await Target.SyncDefinition(newProcess, null);
            }
            else
            {
                targetProc.Process = await Target.SyncDefinition(sourceProc.Process, targetProc.Process);
            }
            if (string.IsNullOrEmpty(targetProc.Process?.Id))
            {
                Log.LogError("Unable to create or find the target process for [{SourceProcess}]. Skipping its behaviors and work item types.", sourceProc.Process.Name);
                return;
            }

            // A newly created process already has its system and inherited behaviors.
            targetProc.WorkItemBehaviors ??= (await Target.GetApiDefinitionsAsync<WorkItemBehavior>(new string[] { targetProc.Process.Id })).ToList();

            foreach (var sourceBehavior in sourceProc.WorkItemBehaviors.Where(b => !b.IsSystem))
            {
                var targetBehavior = ProcessDefinitionSync.FindTargetBehavior(targetProc.WorkItemBehaviors, sourceBehavior);
                if (targetBehavior == null && sourceBehavior.IsInherited)
                {
                    Log.LogWarning("Inherited behavior [{Behavior}] ({ReferenceName}) does not exist in the target process [{TargetProcess}] and cannot be created. Skipping it.", sourceBehavior.Name, sourceBehavior.ReferenceName, targetProc.Process.Name);
                    continue;
                }
                var syncedBehavior = await SyncDefinitionType(TargetModel.WorkItemBehaviors, sourceBehavior, targetBehavior, targetProc.Process.Id);
                if (targetBehavior == null && !string.IsNullOrEmpty(syncedBehavior?.Id))
                {
                    targetProc.WorkItemBehaviors.Add(syncedBehavior);
                }
            }

            foreach (var sourceWit in sourceProc.WorkItemTypes)
            {
                await SyncWorkItemType(sourceWit, targetProc.Process.Id);
            }

            Log.LogInformation($"Completed sync of process [{Source.Options.Name}::{sourceProc.Process.Name}] in [{Target.Options.Name}::{targetProc.Process.Name}].");
        }

        private async Task SyncWorkItemType(WorkItemTypeModel sourceWit, string processId)
        {
            if (sourceWit.WorkItemType.IsSystem)
            {
                Log.LogInformation("Work item type [{WorkItemType}] is not customised in [{Source}]. Nothing to sync.", sourceWit.WorkItemType.Name, Source.Options.Name);
                return;
            }

            TargetModel.WorkItemTypes.TryGetValue(sourceWit.WorkItemType.Name, out var targetWit);
            if (ProcessDefinitionSync.NeedsTargetWorkItemTypeCreated(sourceWit.WorkItemType, targetWit?.WorkItemType))
            {
                targetWit = new WorkItemTypeModel()
                {
                    WorkItemType = await Target.SyncDefinition(sourceWit.WorkItemType, null, processId)
                };
                if (string.IsNullOrEmpty(targetWit.WorkItemType.Id))
                {
                    Log.LogError("Unable to create work item type [{WorkItemType}] in [{Target}]. Skipping its states, fields, rules and layout.", sourceWit.WorkItemType.Name, Target.Options.Name);
                    return;
                }
                TargetModel.WorkItemTypes[sourceWit.WorkItemType.Name] = targetWit;
                await LoadWorkItemTypeDetails(TargetModel, targetWit, Target, processId);
            }
            else
            {
                targetWit.WorkItemType = await Target.SyncDefinition(sourceWit.WorkItemType, targetWit.WorkItemType, processId);
            }

            foreach (var state in sourceWit.States.Where(x => x.Value.CustomizationType == "custom"))
            {
                if (state.Value.StateCategory == "Completed")
                {
                    Log.LogWarning($"Cannot modify [Completed] category on work item state [{state.Value.Name}] on wit type [{sourceWit.WorkItemType.ReferenceName}].");
                }
                else
                {
                    await SyncDefinitionType<WorkItemState>(
                        targetWit.States, state.Value,
                        targetWit.States.Values.FirstOrDefault(x => x.Name == state.Value.Name),
                        processId, targetWit.WorkItemType.ReferenceName);
                }
            }
            // System fields come with the parent type and reject writes (VS403307), like system rules (#3210).
            foreach (var field in sourceWit.Fields.Where(f => !f.IsSystem))
            {
                var existingField = targetWit.Fields.FirstOrDefault(x => x.ReferenceName == field.ReferenceName);
                await SyncDefinitionType<WorkItemTypeField>(
                    TargetModel.WorkItemFields,
                    field,
                    existingField,
                    processId, targetWit.WorkItemType.ReferenceName);
            }
            foreach (var rule in sourceWit.Rules.Values.Where(r => r.IsCustom))
            {
                await SyncDefinitionType<WorkItemRule>(
                    targetWit.Rules,
                    rule,
                    targetWit.Rules.Values.FirstOrDefault(x => x.IsCustom && string.Equals(x.Name, rule.Name, StringComparison.OrdinalIgnoreCase)),
                    processId, targetWit.WorkItemType.ReferenceName);
            }
            foreach (var behavior in sourceWit.Behaviors)
            {
                var targetBehavior = targetWit.Behaviors.Values.FirstOrDefault(x => x.Id == behavior.Value.Id);
                // The service fails with TF401349 when re-applying an unchanged inherited association.
                if (targetBehavior != null && targetBehavior.IsDefault == behavior.Value.IsDefault)
                {
                    continue;
                }
                await SyncDefinitionType<WorkItemTypeBehavior>(
                    targetWit.Behaviors,
                    behavior.Value,
                    targetBehavior,
                    processId, targetWit.WorkItemType.ReferenceName);
            }

            #region Sync Pages ...
            // Making sure the pages themselves are in sync is not too hard.. let's do them first
            foreach (var page in sourceWit.Layout.Pages)
            {
                var targetPage = targetWit.Layout.Pages
                    .FirstOrDefault(x => x.Label.Equals(page.Label, StringComparison.OrdinalIgnoreCase));

                var syncedPage = await SyncDefinitionType<WorkItemPage>(
                    TargetModel.WorkItemPages,
                    page, targetPage,
                    processId, targetWit.WorkItemType.ReferenceName);
                if (targetPage == null && !string.IsNullOrEmpty(syncedPage?.Id))
                {
                    targetWit.Layout.Pages.Add(syncedPage);
                }
            }
            #endregion

            #region Sync Sections and Groups ...
            // Now that we know all the target pages are present we can iterate over what resides on pages..
            // A page's constituent parts is a little more difficult because you need to support an existing
            // group or control that has moved pages, sections or groups

            foreach (var sourcePage in sourceWit.Layout.Pages)
            {
                var targetPage = targetWit.Layout.Pages
                    .FirstOrDefault(x => x.Label.Equals(sourcePage.Label, StringComparison.OrdinalIgnoreCase));
                if (targetPage == null)
                {
                    Log.LogError("Page [{Page}] is missing from work item type [{WorkItemType}] in [{Target}]. Skipping its groups.", sourcePage.Label, targetWit.WorkItemType.Name, Target.Options.Name);
                    continue;
                }

                foreach (var sourceSection in sourcePage.Sections ?? Enumerable.Empty<WorkItemSection>())
                {
                    foreach (var sourceGroup in sourceSection.Groups ?? Enumerable.Empty<WorkItemGroup>())
                    {
                        var sourceControls = sourceGroup.Controls ?? new List<WorkItemControl>();
                        var sourceGroupKey = $"{sourceWit.WorkItemType.Name}::{sourcePage.Label}::{sourceSection.Id}::{sourceGroup.Label}";
                        // first let's see if the target has any inherited group for this source group ..
                        // It will have a group.inherits != null/"" if it is inherited.. you can edit "system" groups
                        if (!sourceGroup.Inherited) // It's a custom group
                        {
                            // look for the group on the flattened set of groups.. remember flat groups are keyed on $"{wit.Name}::{page.Label}::{section.Id}::{group.Label}"
                            var existingGroup = TargetModel.WorkItemGroups.Select(x => new { x.Key, x.Value })
                                .FirstOrDefault(x =>
                                    x.Key.StartsWith($"{targetWit.WorkItemType.Name}::") &&
                                    string.Equals(x.Value.Label, sourceGroup.Label, StringComparison.OrdinalIgnoreCase));

                            if (existingGroup != null)
                            {
                                WorkItemGroup finalTargetGroup = null;
                                WorkItemPage finalTargetPage = null;
                                // here we know the group exists.. we need to check if its on the same page
                                if (sourceGroupKey.Equals(existingGroup.Key, StringComparison.OrdinalIgnoreCase))
                                {
                                    // It's on the same page/section.. no need to move
                                    Log.LogInformation($"Target group [{targetPage.Label}:{existingGroup.Value.Label}] located on same page/section. Skipping group location sync..");
                                    finalTargetPage = targetPage;
                                    finalTargetGroup = existingGroup.Value;
                                }
                                else
                                {
                                    // It's on a different page or section.. we need to move it .. but how can we tell
                                    // if it moved to a different page or just a different section? We split and compare
                                    var sourceSplit = sourceGroupKey.Split("::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    var existingSplit = existingGroup.Key.Split("::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    var existingPage = targetWit.Layout.Pages.FirstOrDefault(p => p.Label.Equals(existingSplit[1]));

                                    if (sourceSplit[1].Equals(existingSplit[1], StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Its on the same page.. it must be section change.. lets move it to the new section
                                        var tempTargetGroup = existingGroup.Value.CloneAsNew();
                                        tempTargetGroup.Id = existingGroup.Value.Id;
                                        if (await Target.MoveWorkItemGroupWithinPage(
                                            tempTargetGroup, processId, targetWit.WorkItemType.ReferenceName,
                                            targetPage.Id, sourceSplit[2], existingSplit[2]))
                                        {
                                            Log.LogInformation($"Target group [{sourceGroup.Label}] located on same page but different section. Moved from [{sourceSplit[2]}] to [{existingSplit[2]}] ..");
                                        }
                                        else
                                        {
                                            Log.LogError($"Target group [{sourceGroup.Label}] located on same page but different section. Unable to move from [{sourceSplit[2]}] to [{existingSplit[2]}] ..");
                                        }
                                        finalTargetPage = existingPage;
                                        finalTargetGroup = tempTargetGroup;
                                    }
                                    else
                                    {

                                        // Its on a different page .. lets move pages
                                        var tempTargetGroup = existingGroup.Value.CloneAsNew();
                                        tempTargetGroup.Id = existingGroup.Value.Id;
                                        if (await Target.MoveWorkItemGroupToNewPage(
                                            tempTargetGroup, processId, targetWit.WorkItemType.ReferenceName,
                                            targetPage.Id, sourceSplit[2], existingPage.Id, existingSplit[2]))
                                        {
                                            Log.LogInformation($"Target group located on different page. Moved from [{sourceSplit[1]}:{sourceSplit[2]}] to [{existingSplit[1]}:{existingSplit[2]}] ..");
                                        }
                                        else
                                        {
                                            Log.LogError($"Target group located on different page. Unable to move from [{existingSplit[1]}:{existingSplit[2]}] to [{targetPage.Label}:{sourceSplit[2]}]!");
                                        }
                                        finalTargetPage = existingPage;
                                        finalTargetGroup = tempTargetGroup;
                                    }
                                }

                                // Iterate through the source controls and make sure the target has all the controls from source
                                foreach (var sourceControl in sourceControls)
                                {
                                    if (WorkItemGroup.IsHtmlControl(sourceControl))
                                    {
                                        Log.LogWarning($"Skipped HTML control sync [{sourceControl.Label}] as it should have already been migrated as part of the group sync.");
                                    }
                                    else
                                    {
                                        var targetControl = finalTargetGroup.Controls?.FirstOrDefault(ctl => string.Equals(ctl.Id, sourceControl.Id, StringComparison.OrdinalIgnoreCase));

                                        if (targetControl == null)
                                        {
                                            // The page object we hold does not carry its sections, so search every target group of this type (#3218).
                                            var targetWitGroups = TargetModel.WorkItemGroups
                                                .Where(x => x.Key.StartsWith($"{targetWit.WorkItemType.Name}::"))
                                                .Select(x => x.Value);
                                            var oldGroup = ProcessDefinitionSync.FindGroupContainingControl(targetWitGroups, sourceControl.Id, finalTargetGroup);

                                            if (oldGroup == null) // It must be a new control
                                            {
                                                if (await Target.AddWorkItemControlToGroup(sourceControl.CloneAsNew(), processId, targetWit.WorkItemType.ReferenceName, finalTargetGroup.Id, sourceControl.Id))
                                                {
                                                    Log.LogInformation($"Attached control [{sourceControl.Label}] to group [{finalTargetGroup.Label}].");
                                                }
                                                else
                                                {
                                                    Log.LogError($"Failed to attach control [{sourceControl.Label}] to new group [{finalTargetGroup.Label}]!");
                                                }
                                            }
                                            else
                                            {
                                                // It must be control movement between groups
                                                if (await Target.MoveWorkItemControlToOtherGroup(sourceControl.CloneAsNew(), processId, targetWit.WorkItemType.ReferenceName, finalTargetGroup.Id, sourceControl.Id, oldGroup.Id))
                                                {
                                                    Log.LogInformation($"Moved control [{sourceControl.Id}] from [{oldGroup.Label}] to existing group [{finalTargetGroup.Label}].");
                                                }
                                                else
                                                {
                                                    Log.LogError($"Failed to move control [{sourceControl.Id}] from [{oldGroup.Label}] to existing group [{finalTargetGroup.Label}].");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            {
                                                Log.LogInformation($"Target already contains control [{sourceControl.Label}] in proper group [{finalTargetGroup.Label}].");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Target doesn't have the group at all
                                WorkItemGroup newGroup = await SyncDefinitionType<WorkItemGroup>(TargetModel.WorkItemGroups, sourceGroup,
                                    null, processId, targetWit.WorkItemType.ReferenceName, targetPage.Id, sourceSection.Id);
                                if (string.IsNullOrEmpty(newGroup?.Id))
                                {
                                    Log.LogError("Unable to create group [{Group}] on [{WorkItemType}]. Skipping its controls.", sourceGroup.Label, targetWit.WorkItemType.Name);
                                    continue;
                                }

                                foreach (var sourceControl in sourceControls.Where(c => !WorkItemGroup.IsHtmlControl(c)))
                                {
                                    if (await Target.AddWorkItemControlToGroup(sourceControl.CloneAsNew(), processId, targetWit.WorkItemType.ReferenceName, newGroup.Id, sourceControl.Id))
                                    {
                                        Log.LogInformation($"Attached control [{sourceControl.Label}] to new group [{newGroup.Label}].");
                                    }
                                    else
                                    {
                                        Log.LogError($"Failed to attach control [{sourceControl.Label}] to new group [{newGroup.Label}]!");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            // Refresh the target layout so the model reflects the groups and controls added or moved above.
            await LoadLayout(TargetModel, targetWit, Target, processId);
            Log.LogInformation($"Completed sync of work item type [{Source.Options.Name}::{sourceWit.WorkItemType.Name}] in [{Target.Options.Name}::{targetWit.WorkItemType.Name}].");
        }



        private async Task<DefinitionType> SyncDefinitionType<DefinitionType>(Dictionary<string, DefinitionType> DataDictionary, DefinitionType sourceDef, DefinitionType targetDef, params string[] routeParams)
            where DefinitionType : RestApiDefinition, ISynchronizeable<DefinitionType>, new()
        {
            targetDef = await Target.SyncDefinition(sourceDef, targetDef, routeParams);

            if (string.IsNullOrEmpty(targetDef?.Id))
            {
                // The endpoint has already logged why the create failed; carry on with the rest of the sync (#3224).
                return targetDef;
            }
            if (!DataDictionary.ContainsKey(targetDef.Id))
            {
                DataDictionary.Add(targetDef.Id, targetDef);
            }
            else
            {
                DataDictionary[targetDef.Id] = targetDef;
            }
            return targetDef;
        }


        private async Task BuildModel(ProcessorModel model, AzureDevOpsEndpoint endpoint, bool warnOnMissing)
        {
            // Grab all the procs, then iterate over them looking for procs user has configured to be
            // sync'd. Then grab all Work Item Types for the given process and filter those by the ones user
            // wants to sync.

            Log.LogDebug($"Loading model for [{endpoint.Options.Name}].");

            var rootFields = (await endpoint.GetApiDefinitionsAsync<WorkItemField>(queryForDetails: false))
                .ToDictionary(x => x.ReferenceName, x => x);

            if (endpoint.Options.Name == Source.Options.Name)
            {
                SourceFields = rootFields;
            }
            else
            {
                TargetFields = rootFields;
            }


            var procs = await endpoint.GetApiDefinitionsAsync<ProcessDefinition>();
            foreach (var processFilter in Options.Processes.Keys)
            {
                string mappedProcName = processFilter;
                if (model == TargetModel && Options.ProcessMaps.ContainsKey(processFilter))
                {
                    mappedProcName = Options.ProcessMaps[processFilter];
                }

                var proc = procs.FirstOrDefault(p => processFilter == "*"
                    || p.Name.Equals(mappedProcName, StringComparison.OrdinalIgnoreCase));
                if (proc != null)
                {
                    //if (!isMapped) mappedProcName = proc.Name;
                    if (!model.ProcessDefinitions.ContainsKey(proc.Id))
                    {
                        model.ProcessDefinitions.Add(mappedProcName, new ProcessDefinitionModel()
                        {
                            Process = proc,
                            MappedFrom = processFilter,
                            WorkItemTypes = new List<WorkItemTypeModel>()
                        });
                    }

                    model.ProcessDefinitions[mappedProcName].WorkItemBehaviors = (await endpoint.GetApiDefinitionsAsync<WorkItemBehavior>(new string[] { proc.Id })).ToList();
                    model.ProcessDefinitions[mappedProcName].WorkItemBehaviors.ForEach(b => {
                        if (!model.WorkItemBehaviors.ContainsKey(b.Id)) {
                            model.WorkItemBehaviors.Add(b.Id, b);
                        }
                    });

                    #region Build Work Item Types data ...

                    var procWits = (await endpoint.GetApiDefinitionsAsync<WorkItemType>(new string[] { proc.Id }, singleDefinitionQueryString: "$expand=All"));
                    procWits = procWits.Where(x => Options.Processes[processFilter].Any(a => a == "*"
                            || x.Name.Equals(a, StringComparison.OrdinalIgnoreCase)));
                    if (procWits != null && procWits.Count() > 0)
                    {
                        foreach (var wit in procWits)
                        {
                            if (!model.WorkItemTypes.ContainsKey(wit.Name))
                            {
                                model.WorkItemTypes.Add(wit.Name, new WorkItemTypeModel()
                                {
                                    WorkItemType = wit
                                });
                                model.ProcessDefinitions[mappedProcName].WorkItemTypes.Add(model.WorkItemTypes[wit.Name]);
                            }
                            else
                            {
                                model.WorkItemTypes[wit.Name] = new WorkItemTypeModel()
                                {
                                    WorkItemType = wit
                                };
                            }

                            // The layout of a locked system type cannot be read (VS403115), and there is nothing customised to sync from it (#3211).
                            if (!wit.IsSystem)
                            {
                                await LoadWorkItemTypeDetails(model, model.WorkItemTypes[wit.Name], endpoint, proc.Id);
                            }
                        }
                    }
                    #endregion
                }
                else if (warnOnMissing)
                {
                    Log.LogWarning($"Unable to locate {processFilter} process in {endpoint.Options.Name} organization.");
                }
            }
        }

        private async Task LoadWorkItemTypeDetails(ProcessorModel model, WorkItemTypeModel witModel, AzureDevOpsEndpoint endpoint, string processId)
        {
            var witId = witModel.WorkItemType.Id;
            await Task.WhenAll(
                Task.Run(async () => await LoadLayout(model, witModel, endpoint, processId)),
                Task.Run(async () => await LoadWorkItemFields(model, witModel.WorkItemType, endpoint, processId)),
                Task.Run(async () =>
                {
                    witModel.States = (await endpoint.GetApiDefinitionsAsync<WorkItemState>(new string[] { processId, witId })).ToDictionary(x => x.Id, x => x);
                }),
                Task.Run(async () =>
                {
                    witModel.Rules = (await endpoint.GetApiDefinitionsAsync<WorkItemRule>(new string[] { processId, witId })).ToDictionary(x => x.Id, x => x);
                }),
                Task.Run(async () =>
                {
                    witModel.Behaviors = (await endpoint.GetApiDefinitionsAsync<WorkItemTypeBehavior>(new string[] { processId, witId })).ToDictionary(x => x.Id, x => x);
                })
            );
        }

        private async Task LoadWorkItemFields(ProcessorModel model, WorkItemType wit, AzureDevOpsEndpoint endpoint, string processId)
        {

            model.WorkItemTypes[wit.Name].Fields =
                (await endpoint.GetApiDefinitionsAsync<WorkItemTypeField>(new string[] { processId, wit.Id }, singleDefinitionQueryString: "$expand=All")).ToList();
            model.WorkItemTypes[wit.Name].Fields.ForEach(field =>
            {
                if (!model.WorkItemFields.ContainsKey(field.Id))
                {
                    model.WorkItemFields.Add(field.Id, field);
                }
            });

        }
        private async Task LoadLayout(ProcessorModel model, WorkItemTypeModel wit, AzureDevOpsEndpoint endpoint, string processId)
        {
            wit.Layout = (await endpoint.GetApiDefinitionAsync<WorkItemLayout>(new string[] { processId, wit.WorkItemType.ReferenceName }));
            wit.Layout.Pages ??= new List<WorkItemPage>();
            foreach (var page in wit.Layout.Pages)
            {
                var pageKey = $"{wit.WorkItemType.Name}::{page.Label}";
                if (model.WorkItemPages.ContainsKey(pageKey))
                {
                    model.WorkItemPages[pageKey] = page;
                }
                else
                {
                    model.WorkItemPages.Add(pageKey, page);
                }
                foreach (var section in page.Sections ?? Enumerable.Empty<WorkItemSection>())
                {
                    foreach (var group in section.Groups ?? Enumerable.Empty<WorkItemGroup>())
                    {
                        var groupKey = $"{wit.WorkItemType.Name}::{page.Label}::{section.Id}::{group.Label}";
                        if (model.WorkItemGroups.ContainsKey(groupKey))
                        {
                            model.WorkItemGroups[groupKey] = group;
                        }
                        else
                        {
                            model.WorkItemGroups.Add(groupKey, group);
                        }
                        foreach (var control in group.Controls ?? Enumerable.Empty<WorkItemControl>())
                        {
                            var controlKey = $"{wit.WorkItemType.Name}::{page.Label}::{section.Id}::{group.Label}::{control.Id}";
                            if (model.WorkItemControls.ContainsKey(controlKey))
                            {
                                model.WorkItemControls[controlKey] = control;
                            }
                            else
                            {
                                model.WorkItemControls.Add(controlKey, control);
                            }
                        }
                    }
                }
            }
        }
    }

    public static class AsyncExt
    {
        public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    { await body(partition.Current); }
                }
            }
            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(dop)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
        }
    }
}

