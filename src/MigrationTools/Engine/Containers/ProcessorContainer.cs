using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MigrationTools.Engine.Containers
{
    public class ProcessorContainer : EngineContainer<ReadOnlyCollection<IProcessor>>
    {

        private List<IProcessor> _Processors = new List<IProcessor>();
        public override ReadOnlyCollection<IProcessor> Items { get {
                EnsureConfigured();
                return _Processors.AsReadOnly();
            } }
        public int Count { get { EnsureConfigured(); return _Processors.Count; } }

        public ProcessorContainer(IServiceProvider services, EngineConfiguration config) : base(services, config)
        {
        }

        protected override void Configure()
        {
            if (Config.Processors != null)
            {
                var enabledProcessors = Config.Processors.Where(x => x.Enabled).ToList();
                Log.Information("ProcessorContainer: Of {ProcessorCount} configured Processors only {EnabledProcessorCount} are enabled", Config.Processors.Count, enabledProcessors.Count);
                foreach (IProcessorConfig processorConfig in enabledProcessors)
                {
                    if (processorConfig.IsProcessorCompatible(enabledProcessors))
                    {
                        Log.Information("ProcessorContainer: Adding Processor {ProcessorName}", processorConfig.Processor);
                        string typePattern = $"VstsSyncMigrator.Engine.{processorConfig.Processor}";

                        Type type = AppDomain.CurrentDomain.GetAssemblies()
                              .Where(a => !a.IsDynamic)
                              .SelectMany(a => a.GetTypes())
                              .FirstOrDefault(t => t.Name.Equals(processorConfig.Processor) || t.FullName.Equals(typePattern));

                        if (type == null)
                        {
                            Log.Error("Type " + typePattern + " not found.", typePattern);
                            throw new Exception("Type " + typePattern + " not found.");
                        }

                        IProcessor pc = (IProcessor)Services.GetRequiredService(type);
                        pc.Configure(processorConfig);
                         _Processors.Add(pc);
                    }
                    else
                    {
                        var message = "ProcessorContainer: Cannot add Processor {ProcessorName}. Processor is not compatible with other enabled processors in configuration.";
                        Log.Error(message, processorConfig.Processor);
                        throw new InvalidOperationException(string.Format(message, processorConfig.Processor, "ProcessorContainer"));
                    }
                }
            }
        }

    }
}
