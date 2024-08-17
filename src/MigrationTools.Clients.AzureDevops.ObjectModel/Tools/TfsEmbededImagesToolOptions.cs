using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools
{
    public class TfsEmbededImagesToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsEmbededImagesTool";
        public override Type ToConfigure => typeof(TfsEmbededImagesTool);

        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsEmbededImagesToolOptions GetDefaults()
        {
            var result = new TfsEmbededImagesToolOptions();
            result.SetDefaults();
            return result;
        }
    }
}