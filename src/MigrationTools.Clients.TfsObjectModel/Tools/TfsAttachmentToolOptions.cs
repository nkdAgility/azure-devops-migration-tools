﻿using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Configuration options for the TFS Attachment Tool that handles the migration of work item attachments between TFS/Azure DevOps systems.
    /// </summary>
    public class TfsAttachmentToolOptions : ToolOptions, ITfsAttachmentToolOptions
    {
        /// <summary>
        /// `AttachmentMigration` is set to true then you need to specify a working path for attachments to be saved locally.
        /// </summary>
        /// <default>C:\temp\Migration\</default>
        public string ExportBasePath { get; set; }

        /// <summary>
        /// `AttachmentMigration` is set to true then you need to specify a max file size for upload in bites.
        /// For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb).
        /// </summary>
        /// <default>480000000</default>
        public int MaxAttachmentSize { get; set; }
    }

    public interface ITfsAttachmentToolOptions
    {
        public string ExportBasePath { get; set; }
        public int MaxAttachmentSize { get; set; }
    }
}
