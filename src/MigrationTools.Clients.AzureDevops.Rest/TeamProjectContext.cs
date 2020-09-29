using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Sinks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MigrationTools.Clients.AzureDevops.Rest
{
    public class TeamProjectContext : ITeamProjectContext
    {
        private readonly IHost _Host;
        private readonly ILogger<MigrationEngineCore> _Log;
        private readonly TelemetryClient _Telemetry;
        private readonly EngineConfiguration _Config;

        public TeamProjectContext(IHost host, ILogger<MigrationEngineCore> log, TelemetryClient telemetry, EngineConfiguration config)
        {
            _Host = host;
            _Log = log;
            _Telemetry = telemetry;
            _Config = config;
        }

        public TeamProjectConfig Config => throw new NotImplementedException();

        public void Connect(TeamProjectConfig config)
        {
            throw new NotImplementedException();
        }

        public void Connect(TeamProjectConfig config, NetworkCredential credentials)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }
    }
}
