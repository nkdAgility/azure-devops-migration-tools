using System;

namespace MigrationTools.EndpointEnrichers
{
    /// <summary>
    /// Configuration options for the Work Item Attachment Enricher that handles the download, processing, and migration of work item attachments between endpoints.
    /// </summary>
    public class WorkItemAttachmentEnricherOptions : EndpointEnricherOptions
    {
        /// <summary>
        /// Local file system path used as working directory for downloading and processing attachments during migration. Should have sufficient disk space and proper permissions.
        /// </summary>
        public string WorkingPath { get; set; }
        
        /// <summary>
        /// Maximum size in bytes for attachments that will be processed. Attachments larger than this size will be skipped to prevent performance issues.
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Gets the type of the enricher that this configuration applies to.
        /// </summary>
        public override Type ToConfigure => typeof(WorkItemAttachmentEnricher);

        /// <summary>
        /// Sets the default values for the work item attachment enricher configuration.
        /// </summary>
        public override void SetDefaults()
        {
            Enabled = true;
            WorkingPath = "c:\\temp\\WorkItemAttachmentWorkingFolder\\";
            MaxSize = 480000000;
        }
    }
}