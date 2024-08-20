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

namespace MigrationTools.Processors.Infrastructure
{
    public class ProcessorContainer
    {
        private IServiceProvider _services;
        private ILogger<ProcessorContainer> _logger;
        private ProcessorContainerOptions _Options;

        private readonly Lazy<List<IOldProcessor>> _processorsLazy;

        public int Count { get { return _processorsLazy.Value.Count; } }

        public ReadOnlyCollection<IOldProcessor> Processors
        {
            get
            {
                return new ReadOnlyCollection<IOldProcessor>(_processorsLazy.Value);
            }
        }


        public ProcessorContainer(IOptions<ProcessorContainerOptions> options, IServiceProvider services, ILogger<ProcessorContainer> logger, ITelemetryLogger telemetry)
        {
            _services = services;
            _logger = logger;
            _Options = options.Value;
            // Initialize the lazy processor list
            _processorsLazy = new Lazy<List<IOldProcessor>>(() => LoadProcessorsfromOptions(_Options));
        }

        private List<IOldProcessor> LoadProcessorsfromOptions(ProcessorContainerOptions options)
        {
            var processors = new List<IOldProcessor>();
            if (options.Processors != null)
            {
                var enabledProcessors = options.Processors.Where(x => x.Enabled).ToList();
                _logger.LogInformation("ProcessorContainer: Of {ProcessorCount} configured Processors only {EnabledProcessorCount} are enabled", options.Processors.Count, enabledProcessors.Count);
                var allTypes = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOldProcessor>().ToList();

                foreach (IProcessorConfig processorConfig in enabledProcessors)
                {
                    if (processorConfig.IsProcessorCompatible(enabledProcessors))
                    {
                        _logger.LogInformation("ProcessorContainer: Adding Processor {ProcessorName}", processorConfig.ConfigurationOptionFor);


                        Type type = allTypes
                              .FirstOrDefault(t => t.Name.Equals(processorConfig.ConfigurationOptionFor));

                        if (type == null)
                        {
                            _logger.LogError("Type " + processorConfig.ConfigurationOptionFor + " not found.", processorConfig.ConfigurationOptionFor);
                            throw new Exception("Type " + processorConfig.ConfigurationOptionFor + " not found.");
                        }
                        IOldProcessor pc = (IOldProcessor)ActivatorUtilities.CreateInstance(_services, type);
                        processors.Add(pc);
                    }
                    else
                    {
                        var message = "ProcessorContainer: Cannot add Processor {ProcessorName}. Processor is not compatible with other enabled processors in configuration.";
                        _logger.LogError(message, processorConfig.ConfigurationOptionFor);
                        throw new InvalidOperationException(string.Format(message, processorConfig.ConfigurationOptionFor, "ProcessorContainer"));
                    }
                }
            }
            return processors;
        }
    }
}
