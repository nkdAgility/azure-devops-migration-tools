using System;
using MigrationTools.Processors;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    public class OutboundLinkCheckingProcessorOptions : ProcessorOptions
    {
        public string WIQLQuery { get; set; }
        public string ResultFileName { get; set; }

    }
}
