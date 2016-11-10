using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TestRunsMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }
        public string Status { get { return "Experimental"; } }

        public Type Processor
        {
            get
            {
                return typeof(TestRunsMigrationContext);
            }
        }

    }
    }

