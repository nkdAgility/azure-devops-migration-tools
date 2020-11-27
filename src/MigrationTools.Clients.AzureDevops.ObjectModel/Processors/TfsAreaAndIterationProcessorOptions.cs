using System;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    public class TfsAreaAndIterationProcessorOptions : ProcessorOptions
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
            var e1 = new TfsEndpointOptions();
            e1.SetDefaults();
            e1.Project = "sourceProject";
            Source = e1;
            var e2 = new TfsEndpointOptions();
            e2.SetDefaults();
            e2.Project = "targetProject";
            Target = e2;
        }
    }
}