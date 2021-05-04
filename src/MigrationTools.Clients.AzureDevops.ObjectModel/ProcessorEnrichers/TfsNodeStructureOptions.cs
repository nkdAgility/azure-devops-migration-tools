using System;

namespace MigrationTools.Enrichers
{
    public class TfsNodeStructureOptions : ProcessorEnricherOptions, ITfsNodeStructureOptions
    {
        public override Type ToConfigure => typeof(TfsNodeStructure);

        public bool PrefixProjectToNodes { get; set; }
        public string[] NodeBasePaths { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }

    public interface ITfsNodeStructureOptions
    {
        public bool PrefixProjectToNodes { get; set; }
        public string[] NodeBasePaths { get; set; }
    }
}