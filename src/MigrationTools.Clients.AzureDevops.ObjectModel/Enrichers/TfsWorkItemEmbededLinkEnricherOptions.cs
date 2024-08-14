using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace MigrationTools.Enrichers
{
    public class TfsWorkItemEmbededLinkEnricherOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:TfsWorkItemEmbededLinkEnricher";
        public override Type ToConfigure => typeof(TfsWorkItemEmbededLinkEnricher);

        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsWorkItemLinkEnricherOptions GetDefaults()
        {
            var result = new TfsWorkItemLinkEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }
}