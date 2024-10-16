using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
    /// </summary>
    public class StringManipulatorTool : Tool<StringManipulatorToolOptions>, IStringManipulatorTool
    {

        public StringManipulatorTool(IOptions<StringManipulatorToolOptions> options, IServiceProvider services, ILogger<StringManipulatorTool> logger, ITelemetryLogger telemetryLogger)
           : base(options, services, logger, telemetryLogger)
        {
        }

        public void ProcessorExecutionWithFieldItem(IProcessor processor, FieldItem fieldItem)
        {
            Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem", GetType().Name);
            if (!Options.Enabled)
            {
                Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Disabled", GetType().Name);
                return;
            }
            if (fieldItem.FieldType == "String" && fieldItem.Value != null)
            {
                if (!HasManipulators())
                {
                    AddDefaultManipulator();
                }
                foreach (var manipulator in Options.Manipulators)
                {
                    if (manipulator.Enabled)
                    {
                        Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Running::{Description} with {pattern}", GetType().Name, manipulator.Description, manipulator.Pattern);
                        var originalValue = fieldItem.Value;
                        fieldItem.Value = Regex.Replace((string)fieldItem.Value, manipulator.Pattern, manipulator.Replacement);
                        Log.LogTrace("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Running::{Description}::Original::{@OriginalValue}", GetType().Name, manipulator.Description, originalValue);
                        Log.LogTrace("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Running::{Description}::New::{@fieldItemValue}", GetType().Name, manipulator.Description, fieldItem.Value);
                    }
                    else
                    {
                        Log.LogDebug("{WorkItemProcessorEnricher}::ProcessorExecutionWithFieldItem::Disabled::{Description}", GetType().Name, manipulator.Description);
                    }
                }

                if (HasStringTooLong(fieldItem))
                {
                    fieldItem.Value = fieldItem.Value.ToString().Substring(0, Math.Min(fieldItem.Value.ToString().Length, Options.MaxStringLength));
                }
            }

        }

        private void AddDefaultManipulator()
        {
            if (Options.Manipulators == null)
            {
                Options.Manipulators = new List<RegexStringManipulator>();
            }
            Options.Manipulators.Add(new RegexStringManipulator() { Enabled = true, Description = "Default: Removes invalid chars!", Pattern = "[^( -~)\n\r\t]+", Replacement = "" });
        }

        private bool HasStringTooLong(FieldItem fieldItem)
        {
            return fieldItem.Value.ToString().Length > 0 && fieldItem.Value.ToString().Length > Options.MaxStringLength;
        }

        private bool HasManipulators()
        {
            return Options.Manipulators != null && Options.Manipulators.Count > 0;
        }
    }

}

