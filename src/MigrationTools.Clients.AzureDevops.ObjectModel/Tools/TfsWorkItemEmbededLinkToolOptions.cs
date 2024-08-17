using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools
{
    public class TfsWorkItemEmbededLinkToolOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsWorkItemEmbededLinkTool";
        public override Type ToConfigure => typeof(TfsWorkItemEmbededLinkTool);

        public override void SetDefaults()
        {
            Enabled = true;
        }

        static public TfsWorkItemLinkToolOptions GetDefaults()
        {
            var result = new TfsWorkItemLinkToolOptions();
            result.SetDefaults();
            return result;
        }
    }
}