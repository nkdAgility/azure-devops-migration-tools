using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Renames tags on the target work item from a list of source → target pairs; tags that match no pair are kept unchanged.
    /// </summary>
    public class TagMappingMap : FieldMapBase
    {
        public TagMappingMap(ILogger<TagMappingMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => "System.Tags";
        private TagMappingMapOptions Config { get { return (TagMappingMapOptions)_Config; } }

        public override void Configure(IFieldMapOptions config)
        {
            base.Configure(config);
            var mappings = Config.mappings ?? new List<TagMapping>();
            TagRules.Validate(mappings.Select(m => m?.source), $"{nameof(TagMappingMap)} {nameof(TagMappingMapOptions.mappings)} source");
            if (mappings.Any(m => string.IsNullOrWhiteSpace(m?.target)))
            {
                throw new ArgumentException($"{nameof(TagMappingMap)} {nameof(TagMappingMapOptions.mappings)} has an entry with an empty target; use {nameof(TagFilterMap)} to drop tags.");
            }
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            var tags = TagRules.Split(target.Tags);
            var result = TagRules.Map(tags, Config.mappings);
            if (result.Renamed.Count == 0)
            {
                return;
            }
            target.Tags = TagRules.Join(result.Tags);
            foreach (var rename in result.Renamed)
            {
                Log.LogDebug("TagMappingMap: renamed tag '{From}' to '{To}' on work item {SourceId} (target {TargetId})", rename.Key, rename.Value, source.Id, target.Id);
            }
        }
    }
}
