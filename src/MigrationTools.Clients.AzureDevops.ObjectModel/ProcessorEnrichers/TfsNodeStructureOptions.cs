using System;
using System.Collections.Generic;

namespace MigrationTools.Enrichers
{
    public class TfsNodeStructureOptions : ProcessorEnricherOptions, ITfsNodeStructureOptions
    {
        public override Type ToConfigure => typeof(TfsNodeStructure);

        public bool PrefixProjectToNodes { get; set; }
        public string[] NodeBasePaths { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            AreaMaps = new Dictionary<string, string>();
            IterationMaps = new Dictionary<string, string>();
        }
    }

    public interface ITfsNodeStructureOptions
    {
        public bool PrefixProjectToNodes { get; set; }
        public string[] NodeBasePaths { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }
    }
}