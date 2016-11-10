using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemUpdateConfig : ITfsProcessingConfig
    {
        public bool WhatIf { get; set; }

        public bool Enabled { get; set; }

        public Type Processor
        {
            get
            {
                return typeof(WorkItemUpdate);
            }
        }

        public string QueryBit { get; set; }
    }
    }

