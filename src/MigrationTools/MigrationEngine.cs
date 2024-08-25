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
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;


namespace MigrationTools
{
    public class MigrationEngine : IMigrationEngine
    {
        private readonly ILogger<MigrationEngine> _logger;
        private readonly IServiceProvider _services;
        private IEndpoint _source;
        private IEndpoint _target;
        private ITelemetryLogger _telemetryLogger;
        private EngineConfiguration _engineConfiguration;

        public MigrationEngine(
            IServiceProvider services,
            IOptions<EngineConfiguration> config,
            ProcessorContainer processors,
            ITelemetryLogger telemetry,
            ILogger<MigrationEngine> logger)
        {
            _logger = logger;
            _logger.LogInformation("Creating Migration Engine {SessionId}", telemetry.SessionId);
            _services = services;
            Processors = processors;
            _telemetryLogger = telemetry;
            _engineConfiguration = config.Value;
        }
        

        public ProcessorContainer Processors { get; }

        public IEndpoint Source
        {
            get
            {
                if (_source is null)
                {
                    _source = _services.GetKeyedService<IEndpoint>("Source"); 
                }
                return _source;
            }
        }

        public IEndpoint Target
        {
            get
            {
                if (_target is null)
                {
                    _target = _services.GetKeyedService<IEndpoint>("Target");
                }
                return _target;
            }
        }
    
        public ProcessingStatus Run()
        {
            _telemetryLogger.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", Processors.Count }
                });
            Stopwatch engineTimer = Stopwatch.StartNew();

            _logger.LogInformation("Logging has been configured and is set to: {LogLevel}. ", _engineConfiguration.LogLevel);
            _logger.LogInformation("                              Max Logfile: {FileLogLevel}. ", "Verbose");
            _logger.LogInformation("                              Max Console: {ConsoleLogLevel}. ", "Debug");
            _logger.LogInformation("                 Max Application Insights: {AILogLevel}. ", "Error");
            _logger.LogInformation("The Max log levels above show where to go look for extra info. e.g. Even if you set the log level to Verbose you will only see that info in the Log File, however everything up to Debug will be in the Console.");

            ProcessingStatus ps = ProcessingStatus.Running;


            _logger.LogInformation("Beginning run of {ProcessorCount} processors", Processors.Count.ToString());
            foreach (IOldProcessor process in Processors.Processors)
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

        private NetworkCredential CheckForNetworkCredentials(NetworkCredentials credentials)
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
