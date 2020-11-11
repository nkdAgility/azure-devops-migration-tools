namespace MigrationTools.Processors
{
    public class TfsTeamSettingsProcessorOptions : ProcessorOptions
    {
        public bool MigrateTeamSettings { get; set; }
        public bool UpdateTeamSettings { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public override string Processor => nameof(WorkItemTrackingProcessor);

        public override IProcessorOptions GetDefault()
        {
            MigrateTeamSettings = true;
            UpdateTeamSettings = true;
            PrefixProjectToNodes = false;
            //Endpoints = new System.Collections.Generic.List<IEndpointOptions>() {
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Source },
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Target }
            //    };
            return this;
        }
    }
}