using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Services;

namespace MigrationTools
{
    public class MigrationEngine : IMigrationEngine
    {
        private readonly ILogger<MigrationEngine> _logger;
        private readonly IServiceProvider _services;
        private IEndpoint _source;
        private IEndpoint _target;
        private ITelemetryLogger _telemetryLogger;
        private ProcessorMetrics processorMetrics;

        public MigrationEngine(
            IServiceProvider services,
            ProcessorContainer processors,
            ITelemetryLogger telemetry,
            ILogger<MigrationEngine> logger)
        {
            _logger = logger;
            _logger.LogInformation("Creating Migration Engine {SessionId}", telemetry.SessionId);
            _services = services;
            Processors = processors;
            _telemetryLogger = telemetry;
            processorMetrics = services.GetRequiredService<ProcessorMetrics>();
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
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("MigrationEngine:Run", ActivityKind.Internal))
            {
                activity?.SetTag("migrationtools.engine.processors", Processors.Count);
                activity?.Start();

                _logger.LogInformation("Logging has been configured and is set to: {LogLevel}. ", "unknown");
                _logger.LogInformation("                              Max Logfile: {FileLogLevel}. ", "Verbose");
                _logger.LogInformation("                              Max Console: {ConsoleLogLevel}. ", "Debug");
                _logger.LogInformation("                 Max Application Insights: {AILogLevel}. ", "Error");
                _logger.LogInformation("The Max log levels above show where to go look for extra info. e.g. Even if you set the log level to Verbose you will only see that info in the Log File, however everything up to Debug will be in the Console.");
                ProcessingStatus ps = ProcessingStatus.Running;
                _logger.LogInformation("Beginning run of {ProcessorCount} processors", Processors.Count.ToString());
               
                foreach (IOldProcessor process in Processors.Processors)
                {                   
                    using (var activityForProcessor = ActivitySourceProvider.ActivitySource.StartActivity($"Processor[{process.GetType().Name}]", ActivityKind.Internal))
                    {
                        activityForProcessor.AddTag("ProcessorName", process.Name);
                        activityForProcessor.AddTag("Status", process.Status.ToString());
                        processorMetrics.QueueSize.Add(1);
                        _logger.LogInformation("Processor: {ProcessorName}", process.Name);
                        Stopwatch processorTimer = Stopwatch.StartNew();
                        process.Execute();
                        processorTimer.Stop();
                       
                        processorMetrics.Duration.Record(processorTimer.ElapsedMilliseconds, new KeyValuePair<string, object?>("processor", nameof(process)));
                        if (process.Status == ProcessingStatus.Failed)
                        {
                            activity.SetStatus(ActivityStatusCode.Error);
                            ps = ProcessingStatus.Failed;
                            _logger.LogError("{Context} The Processor {ProcessorName} entered the failed state...stopping run", process.Name, "MigrationEngine");
                            break;
                        }
                    }
                    processorMetrics.QueueSize.Add(-1);
                }
                activity?.Stop();
                return ps;
            }
        }

    }
}
