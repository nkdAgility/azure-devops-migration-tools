using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Core;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Core.Sinks;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationTools
{
    public class MigrationEngineCore
    {
        private readonly IHost _Host;
        private readonly ILogger<MigrationEngineCore> _Log;
        private readonly TelemetryClient _Telemetry;
        private readonly EngineConfiguration _Config;

        ProcessorContainer _pContainer;
        TypeDefinitionMapContainer _witdContainer;
        GitRepoMapContainer _grmContainer;

        ITeamProjectContext _Source;
        ITeamProjectContext _Target;
        NetworkCredential _SourceCreds;
        NetworkCredential _TargetCreds;

        Dictionary<string, IWitdMapper> _workItemTypeDefinitions = new Dictionary<string, IWitdMapper>();
        Dictionary<string, string> _gitRepoMapping = new Dictionary<string, string>();
        public readonly Dictionary<int, string> _ChangeSetMapping = new Dictionary<int, string>();

        public MigrationEngineCore(IHost host, ILogger<MigrationEngineCore> log, TelemetryClient telemetry, IEngineConfigurationBuilder configBuilder)
        {
            _Host = host;
            _Log = log;
            _Telemetry = telemetry;
            _Config = configBuilder.BuildFromFile();
            _witdContainer = _Host.Services.GetRequiredService<TypeDefinitionMapContainer>();
            _pContainer = _Host.Services.GetRequiredService<ProcessorContainer>();
            _grmContainer = _Host.Services.GetRequiredService<GitRepoMapContainer>();
            ProcessConfiguration();
        }

        private void ProcessConfiguration()
        {
            Telemetry.EnableTrace = _Config.TelemetryEnableTrace;
            if (_Config.Source != null)
            {
                ITeamProjectContext tpc= _Host.Services.GetRequiredService<ITeamProjectContext>();
                if (_SourceCreds == null)
                    tpc.Connect(_Config.Source);
                else
                    tpc.Connect(_Config.Source, _SourceCreds);
                _Source = tpc;
            }
            if (_Config.Target != null)
            {
                ITeamProjectContext tpc = _Host.Services.GetRequiredService<ITeamProjectContext>();
                if (_TargetCreds == null)
                    tpc.Connect(_Config.Target);
                else
                    tpc.Connect(_Config.Target, _TargetCreds);
                _Source = tpc;
            }
        }


        public void Run()
        {

            Log.Error("Running but no implementation :) ");
        }
        
    }
}
