using System;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    public class OutboundLinkCheckingProcessorOptions : ProcessorOptions
    {
        public string WIQLQuery { get; set; }
        public string ResultFileName { get; set; }

    }
}
