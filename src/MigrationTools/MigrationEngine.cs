using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Options;
using MigrationTools.Processors;

namespace MigrationTools
{
    public class MigrationEngine : IMigrationEngine
    {
        private readonly ILogger<MigrationEngine> _logger;
        private readonly IServiceProvider _services;
        private IMigrationClient _Source;
        private IMigrationClient _Target;
        private NetworkCredentialsOptions _networkCredentials;


        public MigrationEngine(
            IServiceProvider services,
            IOptions<NetworkCredentialsOptions> networkCredentials,
            IOptions<EngineConfiguration> config,
            TypeDefinitionMapContainer typeDefinitionMaps,
            ProcessorContainer processors,
            GitRepoMapContainer gitRepoMaps,
            ChangeSetMappingContainer changeSetMapps,
            FieldMapContainer fieldMaps,
            ITelemetryLogger telemetry,
            ILogger<MigrationEngine> logger)
        {
            _logger = logger;
            _logger.LogInformation("Creating Migration Engine {SessionId}", telemetry.SessionId);
            _services = services;
            FieldMaps = fieldMaps;
            _networkCredentials = networkCredentials.Value;
            TypeDefinitionMaps = typeDefinitionMaps;
            Processors = processors;
            GitRepoMaps = gitRepoMaps;
            ChangeSetMapps = changeSetMapps;
            Telemetry = telemetry;
            Config = config.Value;
        }

        public ChangeSetMappingContainer ChangeSetMapps { get; }
        public EngineConfiguration Config { get; }
        public FieldMapContainer FieldMaps { get; }
        public GitRepoMapContainer GitRepoMaps { get; }
        public ProcessorContainer Processors { get; }

        public IMigrationClient Source
        {
            get
            {
                return GetSource();
            }
        }

        public IMigrationClient Target
        {
            get
            {
                return GetTarget();
            }
        }

        public ITelemetryLogger Telemetry { get; }
        public TypeDefinitionMapContainer TypeDefinitionMaps { get; }

        public NetworkCredential CheckForNetworkCredentials(Credentials credentials)
        {
            NetworkCredential networkCredentials = null;
            if (!string.IsNullOrWhiteSpace(credentials.UserName) && !string.IsNullOrWhiteSpace(credentials.Password))
            {
                networkCredentials = new NetworkCredential(credentials.UserName, credentials.Password, credentials.Domain);
            }
            return networkCredentials;
        }

        public ProcessingStatus Run()
        {
            Telemetry.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", Processors.Count },
                    { "Mappings", FieldMaps.Count }
                });
            Stopwatch engineTimer = Stopwatch.StartNew();

            _logger.LogInformation("Logging has been configured and is set to: {LogLevel}. ", Config.LogLevel.ToString());
            _logger.LogInformation("                              Max Logfile: {FileLogLevel}. ", "Verbose");
            _logger.LogInformation("                              Max Console: {ConsoleLogLevel}. ", "Debug");
            _logger.LogInformation("                 Max Application Insights: {AILogLevel}. ", "Error");
            _logger.LogInformation("The Max log levels above show where to go look for extra info. e.g. Even if you set the log level to Verbose you will only see that info in the Log File, however everything up to Debug will be in the Console.");

            ProcessingStatus ps = ProcessingStatus.Running;

            Processors.EnsureConfigured();
            TypeDefinitionMaps.EnsureConfigured();
            GitRepoMaps.EnsureConfigured();
            ChangeSetMapps.EnsureConfigured();
            FieldMaps.EnsureConfigured();

            _logger.LogInformation("Beginning run of {ProcessorCount} processors", Processors.Count.ToString());
            foreach (_EngineV1.Containers.IProcessor process in Processors.Items)
            {
                _logger.LogInformation("Processor: {ProcessorName}", process.Name);
                Stopwatch processorTimer = Stopwatch.StartNew();
                process.Execute();
                processorTimer.Stop();
                Telemetry.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });

                if (process.Status == ProcessingStatus.Failed)
                {
                    ps = ProcessingStatus.Failed;
                    _logger.LogError("{Context} The Processor {ProcessorName} entered the failed state...stopping run", process.Name, "MigrationEngine");
                    break;
                }
            }
            engineTimer.Stop();
            Telemetry.TrackEvent("EngineComplete",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "EngineTime", engineTimer.ElapsedMilliseconds }
                });
            return ps;
        }

        private IMigrationClient GetSource()
        {
            if (_Source is null)
            {
                var credentials = CheckForNetworkCredentials(_networkCredentials.Source);
                _Source = _services.GetRequiredService<IMigrationClient>();
                _Source.Configure(Config.Source, credentials);
            }
            return _Source;
        }

        private IMigrationClient GetTarget()
        {
            if (_Target is null)
            {
                var credentials = CheckForNetworkCredentials(_networkCredentials.Target);
                _Target = _services.GetRequiredService<IMigrationClient>();
                _Target.Configure(Config.Target, credentials);
            }
            return _Target;
        }
    }
}