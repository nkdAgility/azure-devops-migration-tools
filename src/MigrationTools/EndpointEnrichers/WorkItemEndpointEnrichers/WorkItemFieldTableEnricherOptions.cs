using System;

namespace MigrationTools.EndpointEnrichers
{
    /// <summary>
    /// Configuration options for the Work Item Field Table Enricher that processes and transforms table data within work item fields during migration.
    /// </summary>
    public class WorkItemFieldTableEnricherOptions : EndpointEnricherOptions
    {
        /// <summary>
        /// Gets the type of the enricher that this configuration applies to.
        /// </summary>
        public override Type ToConfigure => typeof(WorkItemFieldTableEnricher);

        /// <summary>
        /// Sets the default values for the work item field table enricher configuration.
        /// </summary>
        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}