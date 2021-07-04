using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Processors;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{

    public class TfsRevisionManager : WorkItemProcessorEnricher
    {

        private TfsRevisionManagerOptions _Options;

        public TfsRevisionManager(IServiceProvider services, ILogger<WorkItemProcessorEnricher> logger) : base(services, logger)
        {
        }

        public TfsRevisionManagerOptions Options
        {
            get { return _Options; }
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new System.NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (TfsRevisionManagerOptions)options;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new System.NotImplementedException();
        }


        public override void ProcessorExecutionBegin(IProcessor processor)
        {
            if (Options.Enabled)
            {
                Log.LogInformation("Filter Revisions .");
                EntryForProcessorType(processor);

                RefreshForProcessorType(processor);
            }
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            if (processor is null)
            {
                Log.LogInformation("EntryForProcessorType:v1 (No-Endpoints)");
                IMigrationEngine engine = Services.GetRequiredService<IMigrationEngine>();
            }
            else
            {
                Log.LogInformation("EntryForProcessorType:v2 (Endpoints)");
                TfsEndpoint source = (TfsEndpoint)processor.Source;
                TfsEndpoint target = (TfsEndpoint)processor.Target;
            }
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

            if (targetWorkItem != null)
            {
                // Target exists so remove any Changed Date matches between them
                var targetChangedDates = (from Revision x in targetWorkItem.ToWorkItem().Revisions select Convert.ToDateTime(x.Fields["System.ChangedDate"].Value)).ToList();
                if (_Options.ReplayRevisions)
                {
                    sortedRevisions = sortedRevisions.Where(x => !targetChangedDates.Contains(x.ChangedDate)).ToList();
                }
                // Find Max target date and remove all source revisions that are newer
                var targetLatestDate = targetChangedDates.Max();
                sortedRevisions = sortedRevisions.Where(x => x.ChangedDate > targetLatestDate).ToList();
            }

            if (!_Options.ReplayRevisions && sortedRevisions.Count > 0)
            {
                // Remove all but the latest revision if we are not replaying revisions
                sortedRevisions.RemoveRange(0, sortedRevisions.Count - 1);
            }

            //if (_Options.MaxRevisions > 0 && sortedRevisions.Count > 0)
            //{
            //    // Keep the first revission, and the latest up to [MaxRevisions]
            //    // _config.MaxRevisions = 10?
            //    var revisionsToRemove = sortedRevisions.Count - 1; // all except latest

            //    sortedRevisions.RemoveRange(1, revisionsToRemove);
            //}

            Log.LogInformation("Found {RevisionsCount} revisions to migrate on  Work item:{sourceWorkItemId}",
                new Dictionary<string, object>() {
                    {"RevisionsCount", sortedRevisions.Count},
                    {"sourceWorkItemId", sourceWorkItem.Id}
                });
            Log.LogDebug("RevisionsToMigrate:----------------------------------------------------");
            foreach (RevisionItem item in sortedRevisions)
            {
                Log.LogDebug("RevisionsToMigrate: Index:{Index} - Number:{Number} - ChangedDate:{ChangedDate}", item.Index, item.Number, item.ChangedDate);
            }
            Log.LogDebug("RevisionsToMigrate:----------------------------------------------------");

            return sortedRevisions;
        }

        public List<RevisionItem> CollapseRevisions(List<RevisionItem> revisionsToMigrate, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (_Options.CollapseRevisions)
            {
                var data = revisionsToMigrate.Select(rev =>
                {
                    var revWi = sourceWorkItem.GetRevision(rev.Number);

                    return new
                    {
                        revWi.Id,
                        Rev = revWi.Rev,
                        RevisedDate = revWi.ChangedDate,
                        revWi.Fields
                    };
                });

                var fileData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None });
                var filePath = Path.Combine(Path.GetTempPath(), $"{sourceWorkItem.Id}_PreMigrationHistory.json");

                // todo: Delete this file after (!) WorkItem has been saved
                File.WriteAllText(filePath, fileData);
                targetWorkItem.ToWorkItem().Attachments.Add(new Attachment(filePath, "History has been consolidated into the attached file."));

                revisionsToMigrate = revisionsToMigrate.GetRange(revisionsToMigrate.Count - 1, 1);

                Log.LogInformation(" Attached a consolidated set of {RevisionCount} revisions.",
                    new Dictionary<string, object>() {
                            {"RevisionCount", data.Count() }
                    });
            }

            return revisionsToMigrate;
        }

    }
}