﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Exceptions;
using MigrationTools.Services;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class Processor : IProcessor
    {
        private IEndpoint _source;
        private IEndpoint _target;

        public Processor(
            IOptions<IProcessorOptions> options,
            CommonTools commonTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<Processor> logger)
        {
            Options = options.Value;
            Services = services;
            Telemetry = telemetry;
            Log = logger;
            ProcessorEnrichers = processorEnrichers;
            CommonTools = commonTools;
        }

        public CommonTools CommonTools { get; private set; }

        public IProcessorOptions Options { get; private set; }

        public IEndpoint Source { get { if (_source == null) { _source = GetEndpoint(Options.SourceName); } return _source; } }
        public IEndpoint Target { get { if (_target == null) { _target = GetEndpoint(Options.TargetName); } return _target; } }

        public ProcessorEnricherContainer ProcessorEnrichers { get; }

        public string Name { get { return GetType().Name; } }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<Processor> Log { get; }

        public bool SupportsProcessorEnrichers => false;

        public virtual ProcessorType Type => ProcessorType.AddHock;

        public Activity ProcessorActivity { get; private set; }

        public IEndpoint GetEndpoint(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Endpoint name cannot be null or empty", nameof(name));
            }
            // Assuming GetRequiredKeyedService throws an exception if the service is not found
            IEndpoint endpoint = Services.GetKeyedService<IEndpoint>(name);
            if (endpoint == null)
            {
                throw new ConfigurationValidationException(Options, ValidateOptionsResult.Fail($"The Endpoint '{name}' specified for `{this.GetType().Name}` was not found."));
            }
            return endpoint;
        }

        public void Execute()
        {
            using (ProcessorActivity = ActivitySourceProvider.ActivitySource.StartActivity($"{this.GetType().Name}", ActivityKind.Internal))
            {
                ProcessorActivity.SetTagsFromOptions(Options);


                Log.LogInformation("Migration Context Start: {MigrationContextname} ", Name);
                //////////////////////////////////////////////////
                try
                {
                    if (Options == null)
                    {
                        Log.LogError("Processor::Execute: Processer base has not been configured. Options does not exist!");
                        throw new InvalidOperationException("Processer base has not been configured.");
                    }
                    if (string.IsNullOrEmpty(Options.SourceName) || string.IsNullOrEmpty(Options.TargetName))
                    {
                        Log.LogCritical("Processor::Execute: Processer base has not been configured. Source or Target is null! You need to set both 'SourceName' and 'TargetName' on the processer to a valid 'Endpoint' entry.");
                        Environment.Exit(-200);
                    }
                    Status = ProcessingStatus.Running;
                    InternalExecute();
                    Status = ProcessingStatus.Complete;


                    Log.LogInformation(" Migration Processor Complete {MigrationContextname} ", Name);
                }
                catch (OptionsValidationException ex)
                {
                    Status = ProcessingStatus.Failed;
                    ProcessorActivity.SetStatus(ActivityStatusCode.Error);
                    Log.LogCritical(ex, "Validation of your configuration failed:");
                }
                catch (ConfigurationValidationException ex)
                {
                    Status = ProcessingStatus.Failed;
                    ProcessorActivity.SetStatus(ActivityStatusCode.Error);
                    Log.LogCritical(ex, "Validation of your configuration failed:");
                }
                catch (MigrationToolsException ex)
                {
                    Status = ProcessingStatus.Failed;
                    ProcessorActivity.SetStatus(ActivityStatusCode.Error);
                    switch (ex.ErrorSource)
                    {
                        case MigrationToolsException.ExceptionSource.Configuration:
                            Log.LogCritical(ex, "An error occurred in the Migration Tools causing it to stop! This is likley due to a configuration issue and is not being logged remotely. You can always ask on https://github.com/nkdAgility/azure-devops-migration-tools/discussions ");
                            break;
                        case MigrationToolsException.ExceptionSource.Internal:
                        default:
                            Log.LogCritical(ex, "An error occurred in the Migration Tools causing it to stop!");
                            Telemetry.TrackException(ex, ProcessorActivity.Tags);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Status = ProcessingStatus.Failed;
                    ProcessorActivity.SetStatus(ActivityStatusCode.Error);
                    Log.LogCritical(ex, "Error while running {MigrationContextname}", Name);
                    Telemetry.TrackException(ex, ProcessorActivity.Tags);
                }
                finally
                {
                    ProcessorActivity.Stop();
                    Log.LogInformation("{ProcessorName} completed in {ProcessorDuration} ", Name, ProcessorActivity.Duration.ToString("c"));
                }

            }
        }

        protected abstract void InternalExecute();

        protected Type GetTypeFromName(string name)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                 .Where(a => !a.IsDynamic)
                 .SelectMany(a => a.GetTypes())
                 .FirstOrDefault(t => t.Name.Equals(name) || t.FullName.Equals(name));
            if (type is null)
            {
                var e = new InvalidOperationException("The Type cant be found");
                Log.LogError(e, "Unable to find the type {ObjectToLoadName} needed by the processor {ProfessorName}", name, nameof(this.GetType));
                throw e;
            }
            return type;
        }

        protected static void AddMetric(string name, IDictionary<string, double> store, double value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }

        protected static void AddParameter(string name, IDictionary<string, string> store, string value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }
    }
}
