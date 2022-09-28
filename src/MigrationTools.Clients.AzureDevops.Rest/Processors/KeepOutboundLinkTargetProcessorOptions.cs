using System;
using MigrationTools.Processors;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    public class KeepOutboundLinkTargetProcessorOptions : ProcessorOptions
    {
        public override Type ToConfigure => typeof(KeepOutboundLinkTargetProcessor);

        public string WIQLQueryBit { get; set; }

        public string TargetLinksToKeepOrganization { get; set; }
        public string TargetLinksToKeepProject { get; set; }

        public override IProcessorOptions GetDefault()
        {
            SetDefaults();
            return this;
        }

        public override void SetDefaults()
        {
            WIQLQueryBit = "Select [System.Id] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'";
        }
    }
}
