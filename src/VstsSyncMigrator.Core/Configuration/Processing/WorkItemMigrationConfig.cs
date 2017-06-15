using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }
        public bool PrefixProjectToNodes { get; set; }
        public bool UpdateCreatedDate { get; set; }
        public bool UpdateCreatedBy { get; set; }
        public bool UpdateTargetReflectedId { get; set; }
        public bool UpdateSoureReflectedId { get; set; }
        public Type Processor
        {
            get
            {
                return typeof(WorkItemMigrationContext);
            }
        }

        public string QueryBit { get; set; }
    }
    }

