using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Processors;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    public class LinkCheckingOptions : ProcessorOptions
    {
        public override Type ToConfigure => typeof(LinkChecking);

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            
        }
    }
}
