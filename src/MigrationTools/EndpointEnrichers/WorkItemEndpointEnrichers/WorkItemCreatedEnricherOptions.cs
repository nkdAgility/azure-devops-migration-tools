using System;

namespace MigrationTools.EndpointEnrichers
{
    /// <summary>
    /// Configuration options for the Work Item Created Enricher that preserves original creation metadata when migrating work items between endpoints.
    /// </summary>
    public class WorkItemCreatedEnricherOptions : EndpointEnricherOptions
    {
        /// <summary>
        /// When true, updates the created date of migrated work items to match the original creation date from the source system.
        /// </summary>
        public bool UpdateCreatedDate { get; set; }
        
        /// <summary>
        /// When true, updates the created by field of migrated work items to match the original creator from the source system.
        /// </summary>
        public bool UpdateCreatedBy { get; set; }

        /// <summary>
        /// Gets the type of the enricher that this configuration applies to.
        /// </summary>
        public override Type ToConfigure => typeof(WorkItemCreatedEnricher);

        /// <summary>
        /// Sets the default values for the work item created enricher configuration.
        /// </summary>
        public override void SetDefaults()
        {
            Enabled = true;
            UpdateCreatedDate = true;
            UpdateCreatedBy = true;
        }
    }
}