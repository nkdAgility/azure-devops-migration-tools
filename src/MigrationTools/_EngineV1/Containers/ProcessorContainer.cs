using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools._EngineV1.Containers
{
    public class ProcessorContainer : EngineContainer<ReadOnlyCollection<IProcessor>>
    {
        private List<IProcessor> _Processors = new List<IProcessor>();
        private readonly ILogger<ProcessorContainer> _logger;

        public override ReadOnlyCollection<IProcessor> Items
        {
            get
            {
                EnsureConfigured();
                return _Processors.AsReadOnly();
            }
        }

        public int Count { get { EnsureConfigured(); return _Processors.Count; } }

        public ProcessorContainer(IServiceProvider services, IOptions<EngineConfiguration> config, ILogger<ProcessorContainer> logger) : base(services, config)
        {
            _logger = logger;
        }

        protected override void Configure()
        {
            if (Config.Processors != null)
            {
                var enabledProcessors = Config.Processors.Where(x => x.Enabled).ToList();
                _logger.LogInformation("ProcessorContainer: Of {ProcessorCount} configured Processors only {EnabledProcessorCount} are enabled", Config.Processors.Count, enabledProcessors.Count);
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

                        IProcessor pc = (IProcessor)Services.GetRequiredService(type);
                        pc.Configure(processorConfig);
                        _Processors.Add(pc);
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