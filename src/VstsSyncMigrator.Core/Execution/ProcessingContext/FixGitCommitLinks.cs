﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using VstsSyncMigrator._EngineV1.Processors;

namespace VstsSyncMigrator.Engine
{
    public class FixGitCommitLinks : StaticProcessorBase
    {
        private FixGitCommitLinksConfig _config;
        private TfsGitRepositoryEnricher _GitRepositoryEnricher;

        public FixGitCommitLinks(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<FixGitCommitLinks> logger) : base(services, me, telemetry, logger)
        {
            Logger = logger;
        }

        public override string Name
        {
            get
            {
                return "FixGitCommitLinks";
            }
        }

        public ILogger<FixGitCommitLinks> Logger { get; }

        public override void Configure(IProcessorConfig config)
        {
            _config = (FixGitCommitLinksConfig)config;
            _GitRepositoryEnricher = Services.GetRequiredService<TfsGitRepositoryEnricher>();
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            var query =
                string.Format(
                    @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY {1}",
                    _config.QueryBit,
                    _config.OrderBit
                    );
            List<WorkItemData> workitems = Engine.Target.WorkItems.GetWorkItems(query);
            Log.LogInformation("Update {0} work items?", workitems.Count);
            /////////////////////////////////////////////////
            int current = workitems.Count;
            int count = 0;
            long elapsedms = 0;
            int noteFound = 0;
            foreach (WorkItemData workitem in workitems)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
                workitem.ToWorkItem().Open();

                _GitRepositoryEnricher.Enrich(null, workitem);

                if (workitem.ToWorkItem().IsDirty)
                {
                    Log.LogInformation("Saving {workitemId}", workitem.Id);

                    workitem.SaveToAzureDevOps();
                }

                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Log.LogInformation("Average time of {0} per work item and {1} estimated to completion",
                    string.Format(@"{0:s\:fff} seconds", average),
                    string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining));
            }
            Log.LogInformation("Did not find old repo for {0} links?", noteFound);
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds", stopwatch.Elapsed.ToString("c"));
        }
    }
}