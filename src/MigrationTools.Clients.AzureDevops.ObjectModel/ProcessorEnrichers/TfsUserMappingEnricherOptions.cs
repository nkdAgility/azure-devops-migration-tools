using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class TfsUserMappingEnricherOptions : ProcessorEnricherOptions, ITfsUserMappingEnricherOptions
    {
        

        public override Type ToConfigure => typeof(TfsUserMappingEnricher);


        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsAttachmentEnricherOptions GetDefaults()
        {
            var result = new TfsAttachmentEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }

    public interface ITfsUserMappingEnricherOptions
    {

    }
}