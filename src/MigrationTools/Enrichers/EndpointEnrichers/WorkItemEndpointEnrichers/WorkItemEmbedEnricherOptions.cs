namespace MigrationTools.Enrichers
{
    internal class WorkItemEmbedEnricherOptions : EndpointEnricherOptions
    {
        public string WorkingPath { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            WorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
        }
    }
}