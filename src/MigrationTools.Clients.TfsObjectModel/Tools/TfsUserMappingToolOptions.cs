using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsUserMappingToolOptions : ToolOptions, ITfsUserMappingToolOptions
    {

        /// <summary>
        /// This is a list of the Identiy fields in the Source to check for user mapping purposes. You should list all identiy fields that you want to map.
        /// </summary>
        public List<string> IdentityFieldsToCheck { get; set; }

        /// <summary>
        /// This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
        /// </summary>
        public string UserMappingFile { get; set; }

        /// <summary>
        /// By default, users in source are mapped to target users by their display name. If this is set to true, then the
        /// users will be mapped by their email address first. If no match is found, then the display name will be used.
        /// </summary>
        public bool MatchUsersByEmail { get; set; }
    }

    public interface ITfsUserMappingToolOptions
    {
        List<string> IdentityFieldsToCheck { get; set; }
        string UserMappingFile { get; set; }
        bool MatchUsersByEmail { get; set; }
    }
}
