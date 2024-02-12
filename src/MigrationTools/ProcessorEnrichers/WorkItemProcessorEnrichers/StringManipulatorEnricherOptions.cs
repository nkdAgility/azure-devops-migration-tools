using System;
using System.Collections.Generic;
using MigrationTools.Options;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class StringManipulatorEnricherOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(StringManipulatorEnricher);

        public int MaxStringLength { get; set; }
        public List<RegexStringManipulator> Manipulators { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            MaxStringLength = 1000000;
            Manipulators = new List<RegexStringManipulator> {
                new RegexStringManipulator()
                {
                    Enabled = true,
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