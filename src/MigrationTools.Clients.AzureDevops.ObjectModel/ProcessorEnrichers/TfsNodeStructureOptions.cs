using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace MigrationTools.Enrichers
{
    public class TfsNodeStructureOptions : ProcessorEnricherOptions, ITfsNodeStructureOptions
    {
        public override Type ToConfigure => typeof(TfsNodeStructure);

        public bool PrefixProjectToNodes { get; set; }
        public string[] NodeBasePaths { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }
        public bool ShouldCreateMissingRevisionPaths { get; set; }
        public bool ReplicateAllExistingNodes { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            PrefixProjectToNodes = false;
            AreaMaps = new Dictionary<string, string>();
            IterationMaps = new Dictionary<string, string>();
            ShouldCreateMissingRevisionPaths = true;
            ReplicateAllExistingNodes = false;
        }

        static public TfsNodeStructureOptions GetDefaults()
        {
            var result = new TfsNodeStructureOptions();
            result.SetDefaults();
            return result;
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