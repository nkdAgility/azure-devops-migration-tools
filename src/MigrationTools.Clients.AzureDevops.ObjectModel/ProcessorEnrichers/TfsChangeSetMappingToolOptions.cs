using System;
using MigrationTools.Enrichers;

namespace MigrationTools.ProcessorEnrichers
{
    public class TfsChangeSetMappingToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:ChangeSetMappingTool";
        public override Type ToConfigure => typeof(TfsChangeSetMappingTool);

        public string ChangeSetMappingFile { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            ChangeSetMappingFile = "changesetmapping.json";
        }
    }
}