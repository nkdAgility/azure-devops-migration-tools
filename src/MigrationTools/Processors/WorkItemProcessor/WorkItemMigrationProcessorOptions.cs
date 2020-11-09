namespace MigrationTools.Processors
{
    public class WorkItemMigrationProcessorOptions : ProcessorOptions
    {
        public bool ReplayRevisions { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool CollapseRevisions { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }

        public override string Processor => nameof(WorkItemTrackingProcessor);

        public override IProcessorOptions GetDefault()
        {
            Enabled = true;
            CollapseRevisions = false;
            ReplayRevisions = true;
            WorkItemCreateRetryLimit = 5;
            PrefixProjectToNodes = false;
            //Endpoints = new System.Collections.Generic.List<IEndpointOptions>() {
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Source },
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Target }
            //    };
            return this;
        }
    }
}