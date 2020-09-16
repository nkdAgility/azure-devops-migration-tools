using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MigrationTools.Core.Engine.Containers
{
    public class ProcessorContainer : EngineContainer<ReadOnlyCollection<ITfsProcessingContext>>
    {
        List<ITfsProcessingContext> _Processors = new List<ITfsProcessingContext>();
        public override ReadOnlyCollection<ITfsProcessingContext> Items { get { return _Processors.AsReadOnly(); } }
        public int Count { get { return _Processors.Count; } }

        public ProcessorContainer(IHost host, EngineConfiguration config) : base(host, config)
        {
        }

        protected override void Configure()
        {
            if (Config.Processors != null)
            {
                var enabledProcessors = Config.Processors.Where(x => x.Enabled).ToList();
                foreach (ITfsProcessingConfig processorConfig in enabledProcessors)
                {
                    if (processorConfig.IsProcessorCompatible(enabledProcessors))
                    {
                        Log.Information("Adding Processor {ProcessorName}", processorConfig.Processor);
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

                        ITfsProcessingContext pc = (ITfsProcessingContext)Host.Services.GetService(type);
                        pc.Configure(processorConfig);
                        // ITfsProcessingContext pc = (ITfsProcessingContext)Activator.CreateInstance(type, Host, processorConfig);
                         _Processors.Add(pc);
                    }
                    else
                    {
                        var message = "{Context}: Cannot add Processor {ProcessorName}. Processor is not compatible with other enabled processors in configuration.";
                        Log.Error(message, processorConfig.Processor, "MigrationEngine");
                        throw new InvalidOperationException(string.Format(message, processorConfig.Processor, "MigrationEngine"));
                    }
                }
            }
        }

    }
}
