using System;

namespace MigrationTools.Enrichers
{
    public class PauseAfterEachItemOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(PauseAfterEachItem);

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}