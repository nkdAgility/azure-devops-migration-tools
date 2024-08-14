using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers
{
    /// <summary>
    /// Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
    /// </summary>
    public class WorkItemTypeMappingEnricher : WorkItemProcessorEnricher
    {
        private Serilog.ILogger contextLog;
        private WorkItemTypeMappingEnricherOptions _options;

        public WorkItemTypeMappingEnricher(IOptions<WorkItemTypeMappingEnricherOptions> options, IServiceProvider services, ILogger<StringManipulatorEnricher> logger, ITelemetryLogger telemetryLogger)
           : base(services, logger, telemetryLogger)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            _options = options.Value;
            contextLog = Serilog.Log.ForContext<StringManipulatorEnricher>();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (!(options is StringManipulatorEnricherOptions))
            {
                throw new InvalidCastException(nameof(options));
            }
            _options = (WorkItemTypeMappingEnricherOptions)options;
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

