using System;

namespace MigrationTools.Enrichers
{
    public class TfsRevisionManagerOptions : ProcessorEnricherOptions, ITfsRevisionManagerOptions
    {
        public override Type ToConfigure => typeof(TfsRevisionManager);

        public bool ReplayRevisions { get; set; }
        public int MaxRevisions { get; set; }
        public bool CollapseRevisions { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }

    public interface ITfsRevisionManagerOptions
    {
        public int MaxRevisions { get; set; }
        public bool CollapseRevisions { get; set; }
        public bool ReplayRevisions { get; set; }
    }


}