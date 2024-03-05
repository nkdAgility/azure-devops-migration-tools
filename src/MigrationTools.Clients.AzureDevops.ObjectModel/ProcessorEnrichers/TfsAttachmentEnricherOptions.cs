using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class TfsAttachmentEnricherOptions : ProcessorEnricherOptions, ITfsAttachmentEnricherOptions
    {
        

        public override Type ToConfigure => typeof(TfsAttachmentEnricher);

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

        public override void SetDefaults()
        {
            Enabled = true;
            ExportBasePath = @"c:\temp\WorkItemAttachmentExport";
            MaxAttachmentSize = 480000000;
        }

        static public TfsAttachmentEnricherOptions GetDefaults()
        {
            var result = new TfsAttachmentEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }

    public interface ITfsAttachmentEnricherOptions
    {
        public string ExportBasePath { get; set; }
    public int MaxAttachmentSize { get; set; }

    }
}