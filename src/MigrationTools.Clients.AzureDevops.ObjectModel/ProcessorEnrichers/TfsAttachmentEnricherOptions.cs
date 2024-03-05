using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class TfsAttachmentEnricherOptions : ProcessorEnricherOptions, ITfsAttachmentEnricherOptions
    {
        public override Type ToConfigure => typeof(TfsAttachmentEnricher);

        public string ExportBasePath { get; set; }
        public string MaxAttachmentSize { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            ExportBasePath = @"c:\temp\WorkItemAttachmentExport";
            MaxAttachmentSize = "480000000";
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
    public string MaxAttachmentSize { get; set; }

    }
}