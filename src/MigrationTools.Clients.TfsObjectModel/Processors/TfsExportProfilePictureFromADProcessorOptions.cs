using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the TFS Export Profile Picture from Active Directory Processor that exports user profile pictures from Active Directory for migration purposes.
    /// </summary>
    public class TfsExportProfilePictureFromADProcessorOptions : ProcessorOptions
    {


        /// <summary>
        /// The source domain where the pictures should be exported. 
        /// </summary>
        /// <default>String.Empty</default>
        public string Domain { get; set; }

        /// <summary>
        /// The user name of the user that is used to export the pictures.
        /// </summary>
        /// <default>String.Empty</default>
        public string Username { get; set; }

        /// <summary>
        /// The password of the user that is used to export the pictures.
        /// </summary>
        /// <default>String.Empty</default>
        public string Password { get; set; }

        /// <summary>
        /// TODO: You wpuld need to customise this for your system. Clone repo and run in Debug
        /// </summary>
        /// <default>String.Empty</default>
        public string PictureEmpIDFormat { get; set; }

    }
}