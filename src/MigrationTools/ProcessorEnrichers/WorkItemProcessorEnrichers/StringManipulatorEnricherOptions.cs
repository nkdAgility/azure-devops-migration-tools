using System;
using System.Collections.Generic;
using MigrationTools.Options;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class StringManipulatorEnricherOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:StringManipulatorEnricher";

        public override Type ToConfigure => typeof(StringManipulatorEnricher);

        /// <summary>
        /// Max number of chars in a string. Applied last, and set to 1000000 by default.
        /// </summary>
        /// <default>1000000</default>
        public int MaxStringLength { get; set; }

        /// <summary>
        /// List of regex based string manipulations to apply to all string fields. Each regex replacement is applied in order and can be enabled or disabled.
        /// </summary>
        /// <default>{}</default>
        public List<RegexStringManipulator> Manipulators { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            MaxStringLength = 1000000;
            Manipulators = new List<RegexStringManipulator> {
                new RegexStringManipulator()
                {
                    Enabled = false,
                    Pattern = @"[^( -~)\n\r\t]+",
                    Replacement = "",
                    Description = "Remove all non-ASKI characters between ^ and ~."
                }
            };
        }
    }

    public class RegexStringManipulator
    {
        public bool Enabled { get; set; }
        public string Pattern { get; set; }
        public string Replacement { get; set; }
        public string Description { get; set; }
    }
}