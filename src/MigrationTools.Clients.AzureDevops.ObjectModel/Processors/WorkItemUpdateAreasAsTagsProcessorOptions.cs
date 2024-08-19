using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools.Processors
{
    public class WorkItemUpdateAreasAsTagsProcessorOptions : ProcessorOptions
    {

        /// <summary>
        /// This is a required parameter. That define the root path of the iteration. To get the full path use `\` 
        /// </summary>
        /// <default>\</default>
        public string AreaIterationPath { get; set; }

    }
}