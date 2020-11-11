namespace MigrationTools.Processors
{
    public class TfsTeamSettingsProcessorOptions : ProcessorOptions
    {
        public override string Processor => nameof(WorkItemTrackingProcessor);

        public override IProcessorOptions GetDefault()
        {
            //Endpoints = new System.Collections.Generic.List<IEndpointOptions>() {
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Source },
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Target }
            //    };
            return this;
        }
    }
}