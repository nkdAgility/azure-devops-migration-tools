using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Configuration options for the TFS User Mapping Tool that handles the mapping of user identities between source and target systems during work item migration.
    /// </summary>
    public class TfsUserMappingToolOptions : ToolOptions, ITfsUserMappingToolOptions
    {

        /// <summary>
        /// This is a list of the Identiy fields in the Source to check for user mapping purposes. You should list all identiy fields that you want to map.
        /// </summary>
        /// <default>none</default>
        public List<string> IdentityFieldsToCheck { get; set; }

        /// <summary>
        /// This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
        /// By default the file is a JSON object mapping source display names to target display names. When UseIdentityMapDataFormat is
        /// enabled, the format is detected from the content: a JSON array of objects with "Source" and "Target" identities is read as
        /// detailed IdentityMapData, and a JSON object is read as the simple dictionary format.
        /// In the dictionary format a key or value may be written as the display name followed by the UPN in angle brackets to identify one of several users that share a display name.
        /// </summary>
        /// <default>none</default>
        public string UserMappingFile { get; set; }

        /// <summary>
        /// By default, users in source are mapped to target users by their display name. If this is set to true, then the
        /// users will be mapped by their email address first. If no match is found, then the display name will be used.
        /// </summary>
        /// <default>false</default>
        public bool MatchUsersByEmail { get; set; }

        /// <summary>
        /// Used with MatchUsersByEmail to handle a tenant or domain change. Maps source email domains to target email domains,
        /// for example { "old.com": "new.com" } matches first.last@old.com to first.last@new.com. Applied when no exact email match is found.
        /// </summary>
        /// <default>none</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> EmailDomainMappings { get; set; }

        /// <summary>
        /// Used with MatchUsersByEmail. When true and neither the exact email nor EmailDomainMappings produce a match, users are matched
        /// on the email local part (the part before @), ignoring the domain. Only a single unambiguous match is used.
        /// </summary>
        /// <default>false</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool MatchUsersByEmailLocalPart { get; set; } = false;

        /// <summary>
        /// When set to true, this setting will skip a validation that all users exists or mapped
        /// </summary>
        /// <default>false</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool SkipValidateAllUsersExistOrAreMapped { get; set; } = false;

        /// <summary>
        /// This is the regionalized "Project Collection Valid Users" group name. Default is "Project Collection Valid Users".
        /// </summary>
        /// <default>Project Collection Valid Users</default>
        public string ProjectCollectionValidUsersGroupName { get; set; } = "Project Collection Valid Users";

        /// <summary>
        /// When set to true, the ExportUsersForMapping processor exports a JSON array of detailed IdentityMapData entries (Source and Target identities)
        /// instead of simple display name mappings, and the UserMappingFile format is detected on import so either format can be used.
        /// Detailed entries match work item values against the source display name or account name, ignoring case.
        /// </summary>
        /// <default>false</default>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool UseIdentityMapDataFormat { get; set; } = false;

    }

    public interface ITfsUserMappingToolOptions
    {
        List<string> IdentityFieldsToCheck { get; set; }
        string UserMappingFile { get; set; }
        bool MatchUsersByEmail { get; set; }
        bool SkipValidateAllUsersExistOrAreMapped { get; set; }
        bool UseIdentityMapDataFormat { get; set; }
    }   
}
