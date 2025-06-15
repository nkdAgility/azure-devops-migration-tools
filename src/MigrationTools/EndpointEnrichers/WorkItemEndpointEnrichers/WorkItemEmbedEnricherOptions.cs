using System;

namespace MigrationTools.EndpointEnrichers
{
    /// <summary>
    /// Configuration options for the Work Item Embed Enricher that handles embedded content and images within work item fields during migration.
    /// </summary>
    public class WorkItemEmbedEnricherOptions : EndpointEnricherOptions
    {
        /// <summary>
        /// Local file system path used as working directory for processing embedded content and images during migration. Should have sufficient disk space and proper permissions.
        /// </summary>
        public string WorkingPath { get; set; }

        /// <summary>
        /// Gets the type of the enricher that this configuration applies to.
        /// </summary>
        public override Type ToConfigure => typeof(WorkItemEmbedEnricher);

        /// <summary>
        /// Sets the default values for the work item embed enricher configuration.
        /// </summary>
        public override void SetDefaults()
        {
            Enabled = true;
            WorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
        }
    }
}