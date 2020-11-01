namespace MigrationTools.Enrichers
{
    public class TfsWorkItemCreatedEnricherOptions : EndpointEnricherOptions
    {
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }
    }
}