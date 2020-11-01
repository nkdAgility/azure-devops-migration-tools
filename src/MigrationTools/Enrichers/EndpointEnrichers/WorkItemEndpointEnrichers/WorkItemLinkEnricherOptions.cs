namespace MigrationTools.Enrichers
{
    internal class WorkItemLinkEnricherOptions : EndpointEnricherOptions
    {
        public bool SaveEachAsAdded { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            SaveEachAsAdded = false;
        }
    }
}