namespace MigrationTools.Processors
{
    public class WorkItemMigrationProcessorOptions : ProcessorOptions
    {
        public bool ReplayRevisions { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool CollapseRevisions { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }

        public override string Processor => nameof(WorkItemMigrationProcessor);
    }
}