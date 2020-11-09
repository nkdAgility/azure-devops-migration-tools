using System;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemCreatedEnricherOptions : EndpointEnricherOptions
    {
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }

        public override Type ToConfigure => typeof(WorkItemCreatedEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
            UpdateCreatedDate = true;
            UpdateCreatedBy = true;
        }
    }
}