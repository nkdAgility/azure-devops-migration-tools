using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Build.Client;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class TfsUserMappingEnricherOptions : ProcessorEnricherOptions, ITfsUserMappingEnricherOptions
    {

        public const string ConfigurationSectionName = "MigrationTools:CommonEnrichers:TfsUserMappingEnricher";
        public override Type ToConfigure => typeof(TfsUserMappingEnricher);

        /// <summary>
        /// This is a list of the Identiy fields in the Source to check for user mapping purposes. You should list all identiy fields that you wan to map.
        /// </summary>
        public List<string> IdentityFieldsToCheck { get; set; }

        /// <summary>
        /// This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
        /// </summary>
        public string UserMappingFile { get; set; }

        public override void SetDefaults()
        {
            Enabled = false;
            UserMappingFile = "usermapping.json";
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
         List<string> IdentityFieldsToCheck { get; set; }
        string UserMappingFile { get; set; }
    }
}