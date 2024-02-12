using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers
{
    public class StringManipulatorEnricher : WorkItemProcessorEnricher
    {
        private Serilog.ILogger contextLog;
        private StringManipulatorEnricherOptions _options;

        public StringManipulatorEnricher(IServiceProvider services, ILogger<StringManipulatorEnricher> logger)
           : base(services, logger)
        {
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
           _options = (StringManipulatorEnricherOptions)options;
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
          Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem", this.GetType().Name);
          if (fieldItem.FieldType == "String")
            {
                foreach (var manipulator in _options.Manipulators)
                {
                    if (manipulator.Enabled)
                    {
                        fieldItem.Value = Regex.Replace((string)fieldItem.Value, manipulator.Pattern, manipulator.Replacement);
                    }
                }
                fieldItem.Value.ToString().Substring(0, Math.Min(fieldItem.Value.ToString().Length, _options.MaxStringLength));
            }
        }

    }
}
