using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.Processors
{
    public class ProcessorContainer
    {
        private IServiceProvider _services;
        private ILogger<ProcessorContainer> _logger;
        private ProcessorContainerOptions _Options;

        private List<IProcessor> processors;

        public int Count { get { return processors.Count; } }

        public ReadOnlyCollection<IProcessor> Processors
        {
            get
            {
                return new ReadOnlyCollection<IProcessor>(processors);
            }
        }


        public ProcessorContainer(IOptions<ProcessorContainerOptions> options, IServiceProvider services, ILogger<ProcessorContainer> logger, ITelemetryLogger telemetry)
        {
            _services = services;
            _logger = logger;
            _Options = options.Value;
            LoadProcessorsfromOptions(_Options);
        }

        private void LoadProcessorsfromOptions(ProcessorContainerOptions options)
        {
            if (options.Processors != null)
            {
                var enabledProcessors = options.Processors.Where(x => x.Enabled).ToList();
                _logger.LogInformation("ProcessorContainer: Of {ProcessorCount} configured Processors only {EnabledProcessorCount} are enabled", options.Processors.Count, enabledProcessors.Count);
                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes()).ToList();

                foreach (IProcessorConfig processorConfig in enabledProcessors)
                {
                    if (processorConfig.IsProcessorCompatible(enabledProcessors))
                    {
                        _logger.LogInformation("ProcessorContainer: Adding Processor {ProcessorName}", processorConfig.Processor);
                        string typePattern = $"VstsSyncMigrator.Engine.{processorConfig.Processor}";

                        Type type = allTypes
                              .FirstOrDefault(t => t.Name.Equals(processorConfig.Processor) || t.FullName.Equals(typePattern));

                        if (type == null)
                        {
                            _logger.LogError("Type " + typePattern + " not found.", typePattern);
                            throw new Exception("Type " + typePattern + " not found.");
                        }

                        IProcessor pc = (IProcessor)_services.GetRequiredService(type);
                        pc.Configure(processorConfig);
                        processors.Add(pc);
                    }
                    else
                    {
                        var message = "ProcessorContainer: Cannot add Processor {ProcessorName}. Processor is not compatible with other enabled processors in configuration.";
                        _logger.LogError(message, processorConfig.Processor);
                        throw new InvalidOperationException(string.Format(message, processorConfig.Processor, "ProcessorContainer"));
                    }
                }
            }
        }
    }
}
