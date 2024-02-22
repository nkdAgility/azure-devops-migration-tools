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
    /// <summary>
    /// Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
    /// </summary>
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
            if (!_options.Enabled)
            {
                Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Disabled", this.GetType().Name);
                return;
            }
            if (fieldItem.FieldType == "String" && fieldItem.Value !=null)
            {
                foreach (var manipulator in _options.Manipulators)
                {
                    if (manipulator.Enabled)
                    {
                        Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Running::{Description} with {pattern}", this.GetType().Name, manipulator.Description, manipulator.Pattern);
                        fieldItem.Value = Regex.Replace((string)fieldItem.Value, manipulator.Pattern, manipulator.Replacement);
                        if (fieldItem.Value.ToString().Length > 0 && fieldItem.Value.ToString().Length > _options.MaxStringLength)
                        {
                            fieldItem.Value = fieldItem.Value.ToString().Substring(0, Math.Min(fieldItem.Value.ToString().Length, _options.MaxStringLength));
                        }
                    }
                    else
                    {
                        Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Disabled::{Description}", this.GetType().Name, manipulator.Description);
                    }
                }
            }
            
        }
    }

}

