using System;

namespace MigrationTools.EndpointEnrichers
{
    /// <summary>
    /// Configuration options for the Work Item Link Enricher that handles the migration and processing of work item links between different endpoints.
    /// </summary>
    public class WorkItemLinkEnricherOptions : EndpointEnricherOptions
    {
        /// <summary>
        /// When true, saves each work item after adding links to ensure immediate persistence. When false, saves in batches for better performance.
        /// </summary>
        public bool SaveEachAsAdded { get; set; }

        /// <summary>
        /// Gets the type of the enricher that this configuration applies to.
        /// </summary>
        public override Type ToConfigure => typeof(WorkItemLinkEnricher);

        /// <summary>
        /// Sets the default values for the work item link enricher configuration.
        /// </summary>
        public override void SetDefaults()
        {
            Enabled = true;
            SaveEachAsAdded = false;
        }
    }
}