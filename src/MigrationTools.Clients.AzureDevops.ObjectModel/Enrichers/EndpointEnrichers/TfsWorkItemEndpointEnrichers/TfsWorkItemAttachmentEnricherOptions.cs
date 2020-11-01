namespace MigrationTools.Enrichers
{
    public class TfsWorkItemAttachmentEnricherOptions : EndpointEnricherOptions
    {
        public string WorkingPath { get; set; }
        public int MaxSize { get; set; }
    }
}