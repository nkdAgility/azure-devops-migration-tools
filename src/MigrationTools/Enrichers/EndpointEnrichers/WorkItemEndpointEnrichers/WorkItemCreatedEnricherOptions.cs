namespace MigrationTools.Enrichers
{
    public class WorkItemCreatedEnricherOptions : EndpointEnricherOptions
    {
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            UpdateCreatedDate = true;
            UpdateCreatedBy = true;
        }
    }
}