using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsAttachmentToolOptions : ToolOptions, ITfsAttachmentToolOptions
    {

        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsAttachmentTool";

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