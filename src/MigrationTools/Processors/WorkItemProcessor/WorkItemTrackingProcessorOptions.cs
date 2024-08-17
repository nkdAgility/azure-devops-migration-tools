using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class WorkItemTrackingProcessorOptions : ProcessorOptions
    {
        public bool ReplayRevisions { get; set; }
        public bool CollapseRevisions { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }
        public override Type ToConfigure => typeof(WorkItemTrackingProcessor);

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            Enabled = true;
            CollapseRevisions = false;
            ReplayRevisions = true;
            WorkItemCreateRetryLimit = 5;
            //Endpoints = new System.Collections.Generic.List<IEndpointOptions>() {
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Source },
            //    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Target }
            //    };
            //ProcessorEnrichers = new List<IProcessorEnricherOptions>() {
            //    { new PauseAfterEachItemOptions { Enabled = true } },
            //    { new AppendMigrationToolSignatureFooterOptions { Enabled = true } }
            //};
        }
    }
}