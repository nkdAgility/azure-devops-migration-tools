using System;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemLinkEnricherOptions : EndpointEnricherOptions
    {
        public bool SaveEachAsAdded { get; set; }

        public override Type ToConfigure => typeof(WorkItemLinkEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
            SaveEachAsAdded = false;
        }
    }
}