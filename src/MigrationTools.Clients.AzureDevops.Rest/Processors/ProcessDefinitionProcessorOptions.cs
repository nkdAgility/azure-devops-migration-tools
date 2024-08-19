using System;
using System.Collections.Generic;

namespace MigrationTools.Processors
{
    public class ProcessDefinitionProcessorOptions : ProcessorOptions
    {
        public Dictionary<string, List<string>> Processes { get; set; }
        public Dictionary<string, string> ProcessMaps { get; set; }


        public bool UpdateProcessDetails { get; set; }
        public int MaxDegreeOfParallelism { get; set; }

    }
}

