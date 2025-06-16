using System;
using System.ComponentModel.DataAnnotations;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    /// <summary>
    /// Configuration options for the Keep Outbound Link Target Processor that preserves external links to specific Azure DevOps organizations and projects during migration cleanup operations.
    /// </summary>
    public class KeepOutboundLinkTargetProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Initializes a new instance of the KeepOutboundLinkTargetProcessorOptions class with default settings.
        /// </summary>
        public KeepOutboundLinkTargetProcessorOptions()
        {
            WIQLQuery = "Select [System.Id] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'";
            TargetLinksToKeepOrganization = "https://dev.azure.com/nkdagility";
            TargetLinksToKeepProject = Guid.NewGuid().ToString();
            DryRun = true;
            CleanupFileName = "c:/temp/OutboundLinkTargets.bat";
            PrependCommand = "start";
        }

        /// <summary>
        /// WIQL (Work Item Query Language) query used to select the work items whose outbound links should be processed for preservation.
        /// </summary>
        [Required]
        public string WIQLQuery { get; set; }

        /// <summary>
        /// URL of the Azure DevOps organization whose links should be preserved during cleanup operations.
        /// </summary>
        public string TargetLinksToKeepOrganization { get; set; }
        
        /// <summary>
        /// Project name or GUID within the target organization whose links should be preserved.
        /// </summary>
        public string TargetLinksToKeepProject { get; set; }
        
        /// <summary>
        /// File path where the cleanup script or batch file will be generated for removing unwanted outbound links.
        /// </summary>
        public string CleanupFileName { get; set; }
        
        /// <summary>
        /// Command to prepend to each line in the cleanup script, such as "start" for Windows batch files.
        /// </summary>
        public string PrependCommand { get; set; }

        /// <summary>
        /// When true, performs a dry run without making actual changes, only generating the cleanup script for review.
        /// </summary>
        public bool DryRun { get; set; }


    }
}
