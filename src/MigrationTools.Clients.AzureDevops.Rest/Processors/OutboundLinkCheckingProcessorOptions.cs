using System;
using System.ComponentModel.DataAnnotations;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    public class OutboundLinkCheckingProcessorOptions : ProcessorOptions
    {
        [Required]
        public string WIQLQuery { get; set; }
        [Required]
        public string ResultFileName { get; set; }

    }
}
