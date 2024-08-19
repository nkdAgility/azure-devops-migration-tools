using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class ExportProfilePictureFromADProcessorOptions : ProcessorOptions
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