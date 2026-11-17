using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Keeps only the approved tags (`includeTags`) on the target work item, always drops the blocked ones (`excludeTags`), and logs each tag it drops.
    /// </summary>
    public class TagFilterMap : FieldMapBase
    {
        public TagFilterMap(ILogger<TagFilterMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => "System.Tags";
        private TagFilterMapOptions Config { get { return (TagFilterMapOptions)_Config; } }

        public override void Configure(IFieldMapOptions config)
        {
            base.Configure(config);
            TagRules.Validate(Config.includeTags, $"{nameof(TagFilterMap)} {nameof(TagFilterMapOptions.includeTags)}");
            TagRules.Validate(Config.excludeTags, $"{nameof(TagFilterMap)} {nameof(TagFilterMapOptions.excludeTags)}");
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            var tags = TagRules.Split(target.Tags);
            var result = TagRules.Filter(tags, Config.includeTags, Config.excludeTags);
            if (result.Dropped.Count == 0)
            {
                return;
            }
            target.Tags = TagRules.Join(result.Kept);
            foreach (var tag in result.Dropped)
            {
                Log.LogInformation("TagFilterMap: dropped tag '{Tag}' from source work item {SourceId} revision {Revision}", tag, source.Id, source.Rev);
            }
        }
    }
}
