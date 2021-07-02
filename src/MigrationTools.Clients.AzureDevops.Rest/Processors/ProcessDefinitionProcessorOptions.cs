using System;
using System.Collections.Generic;

namespace MigrationTools.Processors
{
    public class ProcessDefinitionProcessorOptions : ProcessorOptions
    {
        public Dictionary<string, List<string>> Processes { get; set; }
        public Dictionary<string, string> ProcessMaps { get; set; }

        public override Type ToConfigure => typeof(ProcessDefinitionProcessor);

        public bool UpdateProcessDetails { get; set; }
        public int MaxDegreeOfParallelism { get; set; }

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            Processes = new Dictionary<string, List<string>> { { "*", new List<string>() { "*" } } };
            UpdateProcessDetails = true;
            MaxDegreeOfParallelism = 1;
            ProcessMaps = new Dictionary<string, string>();
        }
    }
}

