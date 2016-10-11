using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine.Configuration.Processing
{
    public class TestPlansAndSuitsMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }
        public bool PrefixProjectToNodes { get; set; }

        public Type Processor
        {
            get
            {
                return typeof(TestPlansAndSuitsMigrationContext);
            }
        }

    }
    }

