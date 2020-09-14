using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Sinks;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Sinks.TfsObjectModel
{
   public static class TfsObjectModelExtensions
    {
            //public static IServiceCollection TfsObjectModelWorkerServices(this IServiceCollection collection, EngineConfiguration config)
            //{
            //    if (collection == null) throw new ArgumentNullException(nameof(collection));
            //    if (config == null) throw new ArgumentNullException(nameof(config));

            //   // return collection.AddTransient<IWorkItemSink, AzureDevOpsWorkItemSink>();
            //}

        public static WorkItemData ToWorkItemData(this WorkItem workItem)
        {
            var fakeWorkItem = new WorkItemData();
            fakeWorkItem.id = workItem.Id.ToString();
            fakeWorkItem.title = workItem.Title;
            fakeWorkItem.Type = workItem.Type.Name;
            return fakeWorkItem;
        }

    }
}
