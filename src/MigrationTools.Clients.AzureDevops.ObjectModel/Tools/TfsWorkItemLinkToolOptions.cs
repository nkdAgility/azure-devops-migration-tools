using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsWorkItemLinkToolOptions : ToolOptions, ITfsWorkItemLinkToolOptions
    {

        /// <summary>
        /// Skip validating links if the number of links in the source and the target matches!
        /// </summary>
        public bool FilterIfLinkCountMatches { get; set; }


        /// <summary>
        /// Save the work item after each link is added. This will slow the migration as it will cause many saves to the TFS database.
        /// </summary>
        /// <default>false</default>
        public bool SaveAfterEachLinkIsAdded { get; set; }
    }

    public interface ITfsWorkItemLinkToolOptions
    {
        public bool FilterIfLinkCountMatches { get; set; }
        public bool SaveAfterEachLinkIsAdded { get; set; }
    }
}