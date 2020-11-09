using System;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemAttachmentEnricherOptions : EndpointEnricherOptions
    {
        public string WorkingPath { get; set; }
        public int MaxSize { get; set; }

        public override Type ToConfigure => typeof(WorkItemAttachmentEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
            WorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
            MaxSize = 480000000;
        }
    }
}