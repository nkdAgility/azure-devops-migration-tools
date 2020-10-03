using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public abstract class WorkItemEnricher : IWorkItemEnricher
    {

        protected IMigrationEngine Engine { get; }
       protected ILogger<IWorkItemEnricher> Log { get; }

        public WorkItemEnricher(IMigrationEngine engine, ILogger<WorkItemEnricher> logger)
        {
            Engine = engine;
            Log = logger;
        }

        public abstract void Configure(bool save = true, bool filter = true);
        public abstract int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);

    }
}
