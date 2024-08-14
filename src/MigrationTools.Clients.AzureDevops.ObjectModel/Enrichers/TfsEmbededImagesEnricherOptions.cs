using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace MigrationTools.Enrichers
{
    public class TfsEmbededImagesEnricherOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:TfsEmbededImagesEnricher";
        public override Type ToConfigure => typeof(TfsEmbededImagesEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsEmbededImagesEnricherOptions GetDefaults()
        {
            var result = new TfsEmbededImagesEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }
}