using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.DataContracts;
using MigrationTools.Services;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public abstract class FieldMapBase : IFieldMap, IRecordsWorkItemReview
    {
        public WorkItemReviewLog ReviewLog { protected get; set; }

        // The map's identity in the review log: a failure resolves only when this same map runs cleanly again. The
        // hash of its configuration tells apart two instances of one map type, which can share a display name.
        private string ReviewKey => $"Field map {Name} ({MappingDisplayName}) [{ConfigHash}]";

        private string _configHash;

        private string ConfigHash => _configHash ??= ComputeConfigHash(_Config);

        internal static string ComputeConfigHash(IFieldMapOptions config)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(config);
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json ?? string.Empty));
            return BitConverter.ToString(hash, 0, 4).Replace("-", string.Empty).ToLowerInvariant();
        }

        protected IFieldMapOptions _Config;
        protected ITelemetryLogger Telemetry;

        public FieldMapBase(ILogger<FieldMapBase> logger, ITelemetryLogger telemetryLogger)
        {
            Log = logger;
            Telemetry = telemetryLogger;
        }

        public virtual void Configure(IFieldMapOptions config)
        {
            _Config = config;
            _configHash = null;
        }

        public void Execute(WorkItemData source, WorkItemData target)
        {
            try
            {
                if (RefactoredToUseWorkItemData)
                {
                    InternalExecute(source, target);
                }
                else
                {
                    InternalExecute(source.ToWorkItem(), target.ToWorkItem());
                }
                ReviewLog?.MarkFieldMapRan(ReviewKey);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Field map fault",
                       new Dictionary<string, string> {
                            { "Source", source.ToWorkItem().Id.ToString() },
                            { "Target",  target.ToWorkItem().Id.ToString()}
                       });
                Telemetry.TrackException(ex, null);
                // On a catch-up save (no revision replayed) the map ran on the current values, not on a revision.
                ReviewLog?.Record(WorkItemReviewCategory.FieldMapFailed, withoutRevision: ReviewLog.CurrentItemReplayedRevisions != true, reason:
                    $"{ReviewKey} failed: {ex.Message}",
                    action: "Fix the field map configuration or the source value, then re-run.");
            }
        }

        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public abstract string MappingDisplayName { get; }
        public ILogger<FieldMapBase> Log { get; }

        internal abstract void InternalExecute(WorkItem source, WorkItem target);

        internal virtual bool RefactoredToUseWorkItemData => false;

        internal virtual void InternalExecute(WorkItemData source, WorkItemData target)
        {

        }
    }
}