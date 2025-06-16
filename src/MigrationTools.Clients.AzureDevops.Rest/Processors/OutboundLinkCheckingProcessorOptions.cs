using System;
using System.ComponentModel.DataAnnotations;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    /// <summary>
    /// Configuration options for the Outbound Link Checking Processor that validates work item links and identifies broken or invalid references.
    /// </summary>
    public class OutboundLinkCheckingProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// WIQL (Work Item Query Language) query used to select the work items whose outbound links should be checked for validity.
        /// </summary>
        [Required]
        public string WIQLQuery { get; set; }
        
        /// <summary>
        /// File name where the results of the outbound link checking process will be saved, typically containing details of broken or invalid links.
        /// </summary>
        [Required]
        public string ResultFileName { get; set; }

    }
}
