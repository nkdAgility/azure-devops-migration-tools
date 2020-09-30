using System;
using System.Collections.Generic;
using MigrationTools.Core.Clients;
using MigrationTools.Core.DataContracts;
using Serilog;

namespace MigrationTools.Core.Clients
{
    public abstract class WorkItemQueryBase : IWorkItemQuery
    {
        private string _Query;
        private Dictionary<string, string> _Parameters;
        private IMigrationClient _MigrationClient;

        public WorkItemQueryBase(IServiceProvider services, ITelemetryLogger telemetry)
        {
            Services = services;
            Telemetry = telemetry;
        }

        protected string Query { get { return _Query; } }
        protected Dictionary<string, string> Parameters { get { return _Parameters; } }
        protected IMigrationClient MigrationClient { get { return _MigrationClient; } }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        public void Configure(IMigrationClient migrationClient, string query, Dictionary<string, string> parameters)
        {
            _MigrationClient = migrationClient ?? throw new ArgumentNullException(nameof(migrationClient));
            _Query = query ?? throw new ArgumentNullException(nameof(query));
            _Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public abstract List<WorkItemData> GetWorkItems();

    }
}