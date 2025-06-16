using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the TFS Export Users for Mapping Processor that extracts user information from work items to create user mapping files for migration scenarios.
    /// </summary>
    public class TfsExportUsersForMappingProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// WIQL (Work Item Query Language) query used to select the work items from which user information should be extracted.
        /// </summary>
        public string WIQLQuery { get; set; }

        /// <summary>
        /// `OnlyListUsersInWorkItems`
        /// </summary>
        /// <default>true</default>
        public bool OnlyListUsersInWorkItems { get; set; } = true;

        /// <summary>
        /// Set to <see langword="true"/>, if you want to export all users in source and target server.
        /// The lists of user can be useful, if you need tu manually edit mapping file.
        /// Users will be exported to file set in <see cref="UserExportFile"/>.
        /// </summary>
        public bool ExportAllUsers { get; set; }

        /// <summary>
        /// Path to export file where all source and target servers' users will be exported.
        /// Users are exported only if <see cref="ExportAllUsers"/> is set to <see langword="true"/>.
        /// </summary>
        public string UserExportFile { get; set; }
    }
}
