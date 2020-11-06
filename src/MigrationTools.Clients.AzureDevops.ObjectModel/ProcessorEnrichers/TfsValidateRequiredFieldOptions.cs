using System;
using MigrationTools.Enrichers;

namespace MigrationTools.ProcessorEnrichers
{
    public class TfsValidateRequiredFieldOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(TfsValidateRequiredField);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}