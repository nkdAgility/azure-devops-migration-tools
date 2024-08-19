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

        private readonly Lazy<List<_EngineV1.Containers.IOldProcessor>> _processorsLazy;

        public int Count { get { return _processorsLazy.Value.Count; } }

        public ReadOnlyCollection<_EngineV1.Containers.IOldProcessor> Processors
        {
            get
            {
                return new ReadOnlyCollection<_EngineV1.Containers.IOldProcessor>(_processorsLazy.Value);
            }
        }


        public ProcessorContainer(IOptions<ProcessorContainerOptions> options, IServiceProvider services, ILogger<ProcessorContainer> logger, ITelemetryLogger telemetry)
        {
            _services = services;
            _logger = logger;
            _Options = options.Value;
            // Initialize the lazy processor list
            _processorsLazy = new Lazy<List<_EngineV1.Containers.IOldProcessor>>(() => LoadProcessorsfromOptions(_Options));
        }

        private List<_EngineV1.Containers.IOldProcessor> LoadProcessorsfromOptions(ProcessorContainerOptions options)
        {
            var processors = new List<_EngineV1.Containers.IOldProcessor>();
            if (options.Processors != null)
            {
                var enabledProcessors = options.Processors.Where(x => x.Enabled).ToList();
                _logger.LogInformation("ProcessorContainer: Of {ProcessorCount} configured Processors only {EnabledProcessorCount} are enabled", options.Processors.Count, enabledProcessors.Count);
                var allTypes = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<_EngineV1.Containers.IOldProcessor>().ToList();

                foreach (IProcessorConfig processorConfig in enabledProcessors)
                {
                    if (processorConfig.IsProcessorCompatible(enabledProcessors))
                    {
                        _logger.LogInformation("ProcessorContainer: Adding Processor {ProcessorName}", processorConfig.Processor);


                        Type type = allTypes
                              .FirstOrDefault(t => t.Name.Equals(processorConfig.Processor));

                        if (type == null)
                        {
                            _logger.LogError("Type " + processorConfig.Processor + " not found.", processorConfig.Processor);
                            throw new Exception("Type " + processorConfig.Processor + " not found.");
                        }
                        _EngineV1.Containers.IOldProcessor pc = (_EngineV1.Containers.IOldProcessor)ActivatorUtilities.CreateInstance(_services, type);
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
            return processors;
        }
    }
}
