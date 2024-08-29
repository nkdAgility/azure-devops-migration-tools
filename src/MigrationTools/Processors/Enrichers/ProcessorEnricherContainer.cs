﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Enrichers
{
    public class ProcessorEnricherContainer : List<IProcessorEnricher>
    {
        private bool _Configured;

        public ProcessorEnricherContainer(IServiceProvider services, ITelemetryLogger telemetry, ILogger<ProcessorEnricherContainer> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        protected ILogger<ProcessorEnricherContainer> Log { get; }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        public void ConfigureEnrichers(List<IProcessorEnricherOptions> enrichers)
        {
            Log.LogDebug("ProcessorEnricherContainer::ConfigureEnrichers");
            if (_Configured)
            {
                Log.LogError("ProcessorEnricherContainer::ConfigureEnrichers: You cant configure enrichers twice");
                throw new Exception("You cant configure enrichers twice");
            }
            if (enrichers is null)
            {
                Log.LogWarning("No Enrichers have been Configured");
            }
            else
            {
                foreach (IProcessorEnricherOptions item in enrichers)
                {
                    var peType = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithNameString(item.GetType().Name.Replace("Options", ""));
                    var pe = (WorkItemProcessorEnricher)Services.GetRequiredService(peType);
                    Add(pe);
                    Log.LogInformation("Loading Processor Enricher: {ProcessorEnricherName} {ProcessorEnricherEnabled}", pe.GetType().Name, item.Enabled);
                }
            }
            _Configured = true;
        }

        internal void ProcessorExecutionAfterProcessWorkItem(IProcessor processor, WorkItemData workitem)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionBeforeProcessWorkItem");
            foreach (var enricher in this)
                enricher.ProcessorExecutionAfterProcessWorkItem(processor, workitem);
        }

        internal void ProcessorExecutionBeforeProcessWorkItem(IProcessor processor, WorkItemData workitem)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionBeforeProcessWorkItem");
            foreach (var enricher in this)
                enricher.ProcessorExecutionBeforeProcessWorkItem(processor, workitem);
        }

        internal void ProcessorExecutionAfterSource(IProcessor processor, List<WorkItemData> workItems)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionAfterSource");
            foreach (var enricher in this)
                enricher.ProcessorExecutionAfterSource(processor, workItems);
        }

        public void ProcessorExecutionBegin(IProcessor processor)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionBegin");
            foreach (var enricher in this)
                enricher.ProcessorExecutionBegin(processor);
        }

        public void ProcessorExecutionEnd(IProcessor processor)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionEnd");
            foreach (var enricher in this)
                enricher.ProcessorExecutionEnd(processor);
        }
    }
}