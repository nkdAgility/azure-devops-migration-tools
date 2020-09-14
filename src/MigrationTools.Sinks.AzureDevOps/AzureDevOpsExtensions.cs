using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Sinks;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Sinks.AzureDevOps
{
   public static class AzureDevOpsExtensions
    {
            public static IServiceCollection AzureDevOpsWorkerServices(this IServiceCollection collection, EngineConfiguration config)
            {
                if (collection == null) throw new ArgumentNullException(nameof(collection));
                if (config == null) throw new ArgumentNullException(nameof(config));

                return collection.AddTransient<IWorkItemSink, AzureDevOpsWorkItemSink>();
            }
 
    }
}
