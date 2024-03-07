using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class TfsUserMappingEnricherOptions : ProcessorEnricherOptions, ITfsUserMappingEnricherOptions
    {


        public override Type ToConfigure => typeof(TfsUserMappingEnricher);

        /// <summary>
        /// List the Itentity Fiells to chech for users that need mapped
        /// </summary>
        public List<string> IdentityFieldsToCheck { get; set; }

        /// <summary>
        /// This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
        /// </summary>
        public string UserMappingFile { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            IdentityFieldsToCheck = new List<string> {
                "System.AssignedTo",
        "System.ChangedBy",
        "System.CreatedBy",
        "Microsoft.VSTS.Common.ActivatedBy",
        "Microsoft.VSTS.Common.ResolvedBy",
        "Microsoft.VSTS.Common.ClosedBy" };
        }

        static public TfsUserMappingEnricherOptions GetDefaults()
        {
            var result = new TfsUserMappingEnricherOptions();
            result.SetDefaults();
            return result;
        }
    }

    public interface ITfsUserMappingEnricherOptions
    {

    }
}