using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace MigrationTools.Enrichers
{
    public class TfsGitRepositoryEnricherOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:TfsGitRepositoryEnricher";
        public override Type ToConfigure => typeof(TfsGitRepositoryEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsGitRepositoryEnricherOptions GetDefaults()
        {
            var result = new TfsGitRepositoryEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }
}