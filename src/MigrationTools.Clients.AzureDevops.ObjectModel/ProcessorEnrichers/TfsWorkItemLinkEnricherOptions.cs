using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;

namespace MigrationTools.Enrichers
{
    public class TfsWorkItemLinkEnricherOptions : ProcessorEnricherOptions, ITfsWorkItemLinkEnricherOptions
    {
        public override Type ToConfigure => typeof(TfsNodeStructure);

        /// <summary>
        /// Skip validating links if the number of links in the source and the target matches!
        /// </summary>
        public bool FilterIfLinkCountMatches { get; set; }


        /// <summary>
        /// Save the work item after each link is added. This will slow the migration as it will cause many saves to the TFS database.
        /// </summary>
        /// <default>false</default>
        public bool SaveAfterEachLinkIsAdded { get; set; }


        public override void SetDefaults()
        {
            Enabled = true;
            FilterIfLinkCountMatches = true;
            SaveAfterEachLinkIsAdded = false;
        }

        static public TfsWorkItemLinkEnricherOptions GetDefaults()
        {
            var result = new TfsWorkItemLinkEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }

    public interface ITfsWorkItemLinkEnricherOptions
    {
        public bool FilterIfLinkCountMatches { get; set; }
        public bool SaveAfterEachLinkIsAdded { get; set; }
    }
}