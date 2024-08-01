using System;
using MigrationTools.Enrichers;

namespace MigrationTools.ProcessorEnrichers
{
    public class TfsValidateRequiredFieldOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:TfsValidateRequiredField";
        public override Type ToConfigure => typeof(TfsValidateRequiredField);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}