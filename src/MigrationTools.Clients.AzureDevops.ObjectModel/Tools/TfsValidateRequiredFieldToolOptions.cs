using System;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools
{
    public class TfsValidateRequiredFieldToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsValidateRequiredFieldTool";
        public override Type ToConfigure => typeof(TfsValidateRequiredFieldTool);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}