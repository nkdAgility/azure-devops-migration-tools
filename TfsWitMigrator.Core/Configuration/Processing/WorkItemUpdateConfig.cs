using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine.Configuration.Processing
{
    public class WorkItemUpdateConfig : ITfsProcessingConfig
    {
        public bool WhatIf { get; set; }

        public bool Disabled { get; set; }

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

