using System;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class TfsAreaAndIterationProcessorOptions : ProcessorOptions, ITfsNodeStructureOptions
    {
        /// <summary>
        /// Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        public string[] NodeBasePaths { get; set; }

        public override Type ToConfigure => typeof(TfsAreaAndIterationProcessor);

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            PrefixProjectToNodes = false;
            SourceName = "sourceName";
            TargetName = "targetName";
        }
    }
}