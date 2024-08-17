using System;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools
{
    public class TfsChangeSetMappingToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsChangeSetMappingTool";
        public override Type ToConfigure => typeof(TfsChangeSetMappingTool);

        public string ChangeSetMappingFile { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            ChangeSetMappingFile = "changesetmapping.json";
        }
    }
}