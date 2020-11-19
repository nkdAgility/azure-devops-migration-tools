using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Enrichers
{
    public class TfsNodeStructureOptions : ProcessorEnricherOptions
    {
        public override Type ToConfigure => typeof(TfsNodeStructure);

        public bool PrefixProjectToNodes { get; set; }
        public string[] NodeBasePaths { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
        }
    }
}
