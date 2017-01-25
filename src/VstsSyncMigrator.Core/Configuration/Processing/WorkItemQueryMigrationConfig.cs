using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemQueryMigrationConfig : ITfsProcessingConfig
    {
        /// <summary>
        /// Is this processor enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Do we add the source project name into the folder path
        /// </summary>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        ///  The name of the shared folder, setting the default name
        /// </summary>
        private string sharedFolderName = "Shared Queries";

        /// <summary>
        ///  The name of the shared folder, made a parameter incase it every needs to be edited
        /// </summary>
        public string SharedFolderName
        {
            get
            {
                return this.sharedFolderName;
            }
            set
            {
                this.sharedFolderName = value;
            }
        }

        public Type Processor
        {
            get
            {
                return typeof(WorkItemQueryMigrationContext);
            }
        }

    }
}

