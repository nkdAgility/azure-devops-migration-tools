using System;

namespace MigrationTools.Enrichers
{
    public class WorkItemFieldTableEnricherOptions : EndpointEnricherOptions
    {
        public override Type ToConfigure => typeof(WorkItemFieldTableEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}