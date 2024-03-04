using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Processors;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{

    /// <summary>
    /// The TfsRevisionManager manipulates the revisions of a work item to reduce the number of revisions that are migrated.
    /// </summary>
    public class TfsRevisionManager : WorkItemProcessorEnricher
    {
        public TfsRevisionManager(IServiceProvider services, ILogger<TfsRevisionManager> logger)
            : base(services, logger)
        {
        }

        public TfsRevisionManagerOptions Options { get; private set;}

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            Options = (TfsRevisionManagerOptions)options;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }


        public override void ProcessorExecutionBegin(IProcessor processor)
        {
            if (Options.Enabled)
            {
                Log.LogInformation("Filter Revisions.");
                EntryForProcessorType(processor);

                RefreshForProcessorType(processor);
            }
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            if (processor is null)
            {
                IMigrationEngine engine = Services.GetRequiredService<IMigrationEngine>();
                ((TfsWorkItemMigrationClient)engine.Target.WorkItems).Store?.RefreshCache(true);
            }
            else
            {
                TfsEndpoint target = (TfsEndpoint)processor.Target;
                target.TfsStore.RefreshCache(true);
            }
        }

        public List<RevisionItem> GetRevisionsToMigrate(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            // Revisions have been sorted already on object creation. Values of the Dictionary are sorted by RevisionItem.Number
            var sortedRevisions = sourceWorkItem.Revisions.Values.ToList();
            LogDebugCurrentSortedRevisions(sourceWorkItem, sortedRevisions);
            Log.LogDebug("TfsRevisionManager::GetRevisionsToMigrate: Raw Source {sourceWorkItem} Has {sortedRevisions} revisions", sourceWorkItem.Id, sortedRevisions.Count);

            sortedRevisions = RemoveRevisionsAlreadyOnTarget(targetWorkItem, sortedRevisions);

            RemoveRevisionsAllExceptLatest(sortedRevisions);

            RemoveRevisionsMoreThanMaxRevisions(sortedRevisions);

            LogDebugCurrentSortedRevisions(sourceWorkItem, sortedRevisions);

            return sortedRevisions;
        }

        private void LogDebugCurrentSortedRevisions(WorkItemData sourceWorkItem, List<RevisionItem> sortedRevisions)
        {
            Log.LogInformation("Found {RevisionsCount} revisions to migrate on  Work item:{sourceWorkItemId}", sortedRevisions.Count, sourceWorkItem.Id);
            Log.LogDebug("RevisionsToMigrate:----------------------------------------------------");
            foreach (RevisionItem item in sortedRevisions)
            {
                Log.LogDebug("RevisionsToMigrate: Index:{Index} - Number:{Number} - ChangedDate:{ChangedDate}", item.Index, item.Number, item.ChangedDate);
            }
            Log.LogDebug("RevisionsToMigrate:----------------------------------------------------");
        }

        private void RemoveRevisionsMoreThanMaxRevisions(List<RevisionItem> sortedRevisions)
        {
            if (Options.ReplayRevisions &&
                Options.MaxRevisions > 0 &&
                sortedRevisions.Count > 0 &&
                Options.MaxRevisions < sortedRevisions.Count)
            {
                var revisionsToRemove = sortedRevisions.Count - Options.MaxRevisions;
                sortedRevisions.RemoveRange(0, revisionsToRemove);
                Log.LogDebug("TfsRevisionManager::GetRevisionsToMigrate: MaxRevisions={MaxRevisions}! There are {sortedRevisionsCount} left", Options.MaxRevisions, sortedRevisions.Count);
            }
        }

        private void RemoveRevisionsAllExceptLatest(List<RevisionItem> sortedRevisions)
        {
            if (!Options.ReplayRevisions && sortedRevisions.Count > 0)
            {
                // Remove all but the latest revision if we are not replaying revisions
                sortedRevisions.RemoveRange(0, sortedRevisions.Count - 1);
                Log.LogDebug("TfsRevisionManager::GetRevisionsToMigrate: ReplayRevisions=false! There are {sortedRevisionsCount} left", sortedRevisions.Count);
            }
        }

        private List<RevisionItem> RemoveRevisionsAlreadyOnTarget(WorkItemData targetWorkItem, List<RevisionItem> sortedRevisions)
        {
            if (targetWorkItem != null)
            {
                Log.LogDebug("TfsRevisionManager::GetRevisionsToMigrate: Raw Target {targetWorkItemId} Has {targetWorkItemRevCount} revisions", targetWorkItem.Id, targetWorkItem.Revisions.Count);
                // Target exists so remove any Changed Date matches between them
                var targetChangedDates = (from RevisionItem x in targetWorkItem.Revisions.Values select x.ChangedDate).ToList();
                if (Options.ReplayRevisions)
                {
                    sortedRevisions = sortedRevisions.Where(x => !targetChangedDates.Contains(x.ChangedDate)).ToList();
                    Log.LogDebug("TfsRevisionManager::GetRevisionsToMigrate: After removing Date Matches there are {sortedRevisionsCount} left", sortedRevisions.Count);
                }
                // Find Max target date and remove all source revisions that are newer
                var targetLatestDate = targetChangedDates.Max();
                sortedRevisions = sortedRevisions.Where(x => x.ChangedDate > targetLatestDate).ToList();
                Log.LogDebug("TfsRevisionManager::GetRevisionsToMigrate: After removing revisions before target latest date {targetLatestDate} there are {sortedRevisionsCount} left", targetLatestDate, sortedRevisions.Count);
            }

            return sortedRevisions;
        }

        public void AttachSourceRevisionHistoryJsonToTarget(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            var fileData = JsonConvert.SerializeObject(sourceWorkItem.Revisions, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None });
            var filePath = Path.Combine(Path.GetTempPath(), $"{sourceWorkItem.ProjectName}-ID{sourceWorkItem.Id}-R{sourceWorkItem.Rev}-PreMigrationHistory.json");

            // todo: Delete this file after (!) WorkItem has been saved
            File.WriteAllText(filePath, fileData);

            if (targetWorkItem.internalObject != null)
            {
                targetWorkItem.ToWorkItem().Attachments.Add(new Attachment(filePath, "History has been consolidated into the attached file."));
            }

            Log.LogInformation("Attached a consolidated set of {RevisionCount} revisions.",
                new Dictionary<string, object>() {
                    {"RevisionCount", sourceWorkItem.Revisions.Count() }
                });
        }
    }
}