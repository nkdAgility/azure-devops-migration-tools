using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Engine.Enrichers;
using MigrationTools.Core.Sinks;
using MigrationTools.Sinks.TfsObjectModel.Enrichers;
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
            var internalWorkItem = new WorkItemData();
            internalWorkItem.id = workItem.Id.ToString();
            internalWorkItem.title = workItem.Title;
            internalWorkItem.Type = workItem.Type.Name;
            internalWorkItem.InternalWorkItem = workItem;
            return internalWorkItem;
        }

        public static WorkItem ToWorkItem(this WorkItemData  workItemData)
        {
            if (!(workItemData.InternalWorkItem is WorkItem))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(typeof (WorkItem)).FullName}");
            }
            return (WorkItem)workItemData.InternalWorkItem;
        }

        public static void SaveMigratedWorkItem(this IAttachmentMigrationEnricher context, WorkItemData workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            workItem.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
            workItem.ToWorkItem().Save();
        }
        public static void SaveMigratedWorkItem(this IEmbededImagesRepairEnricher context, WorkItemData workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            workItem.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
            workItem.ToWorkItem().Save();
        }
    }
}
