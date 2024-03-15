﻿using System;
using MigrationTools.Processors;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    public class KeepOutboundLinkTargetProcessorOptions : ProcessorOptions
    {
        public override Type ToConfigure => typeof(KeepOutboundLinkTargetProcessor);

        public string WIQLQuery { get; set; }

        public string TargetLinksToKeepOrganization { get; set; }
        public string TargetLinksToKeepProject { get; set; }
        public string CleanupFileName { get; set; }
        public string PrependCommand { get; set; }

        public bool DryRun { get; set; }

        public override IProcessorOptions GetDefault()
        {
            SetDefaults();
            return this;
        }

        public override void SetDefaults()
        {
            WIQLQuery = "Select [System.Id] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'";
            TargetLinksToKeepOrganization = "https://dev.azure.com/nkdagility";
            TargetLinksToKeepProject = Guid.NewGuid().ToString();
            DryRun = true;
            CleanupFileName = "c:/temp/OutboundLinkTargets.bat";
            PrependCommand = "start";
        }
    }
}
