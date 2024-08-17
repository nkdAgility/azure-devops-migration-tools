using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds.Users;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
    /// </summary>
    public class GitRepoMappingTool : WorkItemProcessorEnricher
    {
        private Serilog.ILogger contextLog;
        private GitRepoMappingToolOptions _options;

        public ReadOnlyDictionary<string, string> Mappings { get; private set; }

        public GitRepoMappingTool(IOptions<GitRepoMappingToolOptions> options, IServiceProvider services, ILogger<GitRepoMappingTool> logger, ITelemetryLogger telemetryLogger)
           : base(services, logger, telemetryLogger)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options.Value;
            Mappings = new ReadOnlyDictionary<string, string>(_options.Mappings);
            contextLog = Serilog.Log.ForContext<GitRepoMappingTool>();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }
        public override void ProcessorExecutionWithFieldItem(IProcessor processor, FieldItem fieldItem)
        {


        }


    }

}

