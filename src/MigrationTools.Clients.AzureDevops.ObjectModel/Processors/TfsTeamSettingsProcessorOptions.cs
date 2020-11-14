using System;
using System.Collections.Generic;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    public class TfsTeamSettingsProcessorOptions : ProcessorOptions
    {
        public bool MigrateTeamSettings { get; set; }
        public bool UpdateTeamSettings { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public List<string> Teams { get; set; }

        public override string Processor => nameof(ToConfigure);

        public override Type ToConfigure => typeof(WorkItemTrackingProcessor);

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            MigrateTeamSettings = true;
            UpdateTeamSettings = true;
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