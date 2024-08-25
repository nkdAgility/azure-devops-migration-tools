using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
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

        private readonly Lazy<List<IProcessor>> _processorsLazy;

        public int Count { get { return _processorsLazy.Value.Count; } }

        public ReadOnlyCollection<IProcessor> Processors
        {
            get
            {
                return new ReadOnlyCollection<IProcessor>(_processorsLazy.Value);
            }
        }


        public ProcessorContainer(IOptions<ProcessorContainerOptions> options, IServiceProvider services, ILogger<ProcessorContainer> logger, ITelemetryLogger telemetry)
        {
            _services = services;
            _logger = logger;
            _Options = options.Value;
            // Initialize the lazy processor list
            _processorsLazy = new Lazy<List<IProcessor>>(() => LoadProcessorsfromOptions(_Options));
        }

        private List<IProcessor> LoadProcessorsfromOptions(ProcessorContainerOptions options)
        {
            var processors = new List<IProcessor>();
            if (options.Processors != null)
            {
                var enabledProcessors = options.Processors.Where(x => x.Enabled).ToList();
                _logger.LogInformation("ProcessorContainer: Of {ProcessorCount} configured Processors only {EnabledProcessorCount} are enabled", options.Processors.Count, enabledProcessors.Count);
                var allTypes = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IProcessor>().ToList();

                foreach (IProcessorOptions processorOption in enabledProcessors)
                {
                    if (processorOption.IsProcessorCompatible(enabledProcessors))
                    {
                        _logger.LogInformation("ProcessorContainer: Adding Processor {ProcessorName}", processorOption.ConfigurationOptionFor);


                        Type type = allTypes
                              .FirstOrDefault(t => t.Name.Equals(processorOption.ConfigurationOptionFor));

                        if (type == null)
                        {
                            _logger.LogError("Type " + processorOption.ConfigurationOptionFor + " not found.", processorOption.ConfigurationOptionFor);
                            throw new Exception("Type " + processorOption.ConfigurationOptionFor + " not found.");
                        }

                        var constructors = type.GetConstructors();
                        foreach (var constructor in constructors)
                        {
                            var parameters = constructor.GetParameters();
                            _logger.LogInformation("Constructor found: {Constructor}", string.Join(", ", parameters.Select(p => p.ParameterType.Name)));
                        }

                        _logger.LogInformation("Attempting to pass parameters: {Parameters}", string.Join(", ", new object[] { Microsoft.Extensions.Options.Options.Create(processorOption) }.Select(p => p.GetType().Name)));


                        //var optionsWrapperType = typeof(IOptions<>).MakeGenericType(processorOption.GetType());
                        //var optionsWrapper = Activator.CreateInstance(optionsWrapperType, processorOption);

                        var optionsWrapper = typeof(Microsoft.Extensions.Options.Options).GetMethod("Create")
                        .MakeGenericMethod(processorOption.GetType())
                        .Invoke(null, new object[] { processorOption });

                        IProcessor pc = (IProcessor)ActivatorUtilities.CreateInstance(_services, type, optionsWrapper);
                        processors.Add(pc);
                    }
                    else
                    {
                        var message = "ProcessorContainer: Cannot add Processor {ProcessorName}. Processor is not compatible with other enabled processors in configuration.";
                        _logger.LogError(message, processorOption.ConfigurationOptionFor);
                        throw new InvalidOperationException(string.Format(message, processorOption.ConfigurationOptionFor, "ProcessorContainer"));
                    }
                }
            }
            return processors;
        }
    }
}
