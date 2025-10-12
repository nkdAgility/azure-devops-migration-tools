using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
    /// </summary>
    public class StringManipulatorTool : Tool<StringManipulatorToolOptions>, IStringManipulatorTool
    {

        public StringManipulatorTool(
            IOptions<StringManipulatorToolOptions> options,
            IServiceProvider services,
            ILogger<StringManipulatorTool> logger,
            ITelemetryLogger telemetryLogger)
            : base(options, services, logger, telemetryLogger)
        {
        }

        public string? ProcessString(string? value)
        {
            const string logPrefix = nameof(StringManipulatorTool) + "::" + nameof(ProcessString);
            Log.LogDebug(logPrefix);

            string result = value;
            if (!Options.Enabled)
            {
                Log.LogDebug(logPrefix + "::Disabled");
                return result;
            }
            if (value is not null)
            {
                if (!HasManipulators())
                {
                    AddDefaultManipulator();
                }
                foreach (var manipulator in Options.Manipulators)
                {
                    if (manipulator.Enabled)
                    {
                        Log.LogDebug(logPrefix + "::Running::{Description} with {pattern}", manipulator.Description, manipulator.Pattern);
                        string oldValue = result;
                        result = Regex.Replace(result, manipulator.Pattern, manipulator.Replacement);
                        Log.LogTrace(logPrefix + "::Running::{Description}::Original::{@oldValue}", manipulator.Description, oldValue);
                        Log.LogTrace(logPrefix + "::Running::{Description}::New::{@newValue}", manipulator.Description, result);
                    }
                    else
                    {
                        Log.LogDebug(logPrefix + "::Disabled::{Description}", manipulator.Description);
                    }
                }

                if (IsStringTooLong(result))
                {
                    result = result.Substring(0, Options.MaxStringLength);
                }
            }
            return result;
        }

        private void AddDefaultManipulator()
        {
            if (Options.Manipulators is null)
            {
                Options.Manipulators = new List<RegexStringManipulator>();
            }
            Options.Manipulators.Add(new RegexStringManipulator()
            {
                Enabled = true,
                Description = "Default: Removes control characters and emojis but preserves Unicode letters and symbols",
                Pattern = @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F]|[\uD800-\uDBFF][\uDC00-\uDFFF]|\uFE0F",
                Replacement = ""
            });
        }

        private bool IsStringTooLong(string value) => (value.Length > 0) && (value.Length > Options.MaxStringLength);

        private bool HasManipulators() => Options.Manipulators?.Count > 0;
    }
}
