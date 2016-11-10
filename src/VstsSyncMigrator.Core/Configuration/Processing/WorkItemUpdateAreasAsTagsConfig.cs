using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemUpdateAreasAsTagsConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }
        public string AreaIterationPath { get; set; }

        public Type Processor
        {
            get
            {
                return typeof(WorkItemUpdateAreasAsTagsContext);
            }
        }

    }
    }

