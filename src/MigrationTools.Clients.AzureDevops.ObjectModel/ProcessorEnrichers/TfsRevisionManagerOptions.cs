using System;

namespace MigrationTools.Enrichers
{
    public class TfsRevisionManagerOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(TfsRevisionManager);

        public bool ReplayRevisions { get; set; }
        public int MaxRevisions { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}