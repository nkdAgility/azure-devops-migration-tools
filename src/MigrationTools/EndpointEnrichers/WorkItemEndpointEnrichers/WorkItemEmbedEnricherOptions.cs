using System;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemEmbedEnricherOptions : EndpointEnricherOptions
    {
        public string WorkingPath { get; set; }

        public override Type ToConfigure => typeof(WorkItemEmbedEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
            WorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
        }
    }
}