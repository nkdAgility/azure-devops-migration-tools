using System;

namespace MigrationTools.Enrichers
{
    public class TfsRevisionManagerOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:TfsRevisionManager";
        public override Type ToConfigure => typeof(TfsRevisionManager);

        /// <summary>
        /// You can choose to migrate the tip only (a single write) or all of the revisions (many writes).
        /// If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true`.
        /// </summary>
        /// <default>true</default>
        public bool ReplayRevisions { get; set; }

        /// <summary>
        /// Sets the maximum number of revisions that will be migrated. "First + Last N = Max".
        /// If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated.
        /// </summary>
        /// <default>0</default>
        public int MaxRevisions { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            ReplayRevisions = true;
            MaxRevisions = 0;
        }

        static public TfsRevisionManagerOptions GetDefaults()
        {
            var result = new TfsRevisionManagerOptions();
            result.SetDefaults();
            return result;
        }
    }
}