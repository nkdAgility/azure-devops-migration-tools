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
        private IMigrationClient _source;
        private IMigrationClient _target;
        private NetworkCredentialsOptions _networkCredentials;
        private ITelemetryLogger _telemetryLogger;
        private EngineConfiguration _engineConfiguration;

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
            _logger.LogInformation("Creating Migration Engine Session[{SessionId}] on {datetime}", telemetry.SessionId, DateTime.Now);
            _services = services;
            FieldMaps = fieldMaps;
            _networkCredentials = networkCredentials.Value;
            TypeDefinitionMaps = typeDefinitionMaps;
            Processors = processors;
            GitRepoMaps = gitRepoMaps;
            ChangeSetMapps = changeSetMapps;
            _telemetryLogger = telemetry;
            _engineConfiguration = config.Value;
        }

        public ChangeSetMappingContainer ChangeSetMapps { get; }
        
        public FieldMapContainer FieldMaps { get; }

        public GitRepoMapContainer GitRepoMaps { get; }

        public ProcessorContainer Processors { get; }

        public IMigrationClient Source
        {
            get
            {
                if (_source is null)
                {
                    _source = GetMigrationClient(_engineConfiguration.Source, _networkCredentials.Source);
                }
                return _source;
            }
        }

        public IMigrationClient Target
        {
            get
            {
                if (_target is null)
                {
                    _target = GetMigrationClient(_engineConfiguration.Target, _networkCredentials.Target);
                }
                return _target;
            }
        }
        
        public TypeDefinitionMapContainer TypeDefinitionMaps { get; }

        public ProcessingStatus Run()
        {
            _telemetryLogger.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", Processors.Count },
                    { "Mappings", FieldMaps.Count }
                });
            Stopwatch engineTimer = Stopwatch.StartNew();

            _logger.LogInformation("Logging has been configured and is set to: {LogLevel}. ", _engineConfiguration.LogLevel);
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
                _telemetryLogger.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });

                if (process.Status == ProcessingStatus.Failed)
                {
                    ps = ProcessingStatus.Failed;
                    _logger.LogError("{Context} The Processor {ProcessorName} entered the failed state...stopping run", process.Name, "MigrationEngine");
                    break;
                }
            }
            engineTimer.Stop();
            _telemetryLogger.TrackEvent("EngineComplete",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "EngineTime", engineTimer.ElapsedMilliseconds }
                });
            return ps;
        }

        private IMigrationClient GetMigrationClient(IMigrationClientConfig config, Credentials networkCredentials)
        {
            var credentials = CheckForNetworkCredentials(networkCredentials);
            var client = _services.GetRequiredService<IMigrationClient>();
            client.Configure(config, credentials);
            return client;
        }

        private NetworkCredential CheckForNetworkCredentials(Credentials credentials)
        {
            NetworkCredential networkCredentials = null;
            if (!string.IsNullOrWhiteSpace(credentials?.UserName) && !string.IsNullOrWhiteSpace(credentials?.Password))
            {
                networkCredentials = new NetworkCredential(credentials.UserName, credentials.Password, credentials.Domain);
            }
            return networkCredentials;
        }
    }
}
