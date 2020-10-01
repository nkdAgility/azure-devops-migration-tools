using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using MigrationTools.Engine.Enrichers;
using MigrationTools.Clients;
using MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Services.Common;

namespace MigrationTools.Clients.AzureDevops.ObjectModel
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
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var internalObject = new WorkItemData();
            internalObject.Id = workItem.Id.ToString();
            internalObject.Type = workItem.Type.Name;
            internalObject.Title = workItem.Title;
            internalObject.Rev = workItem.Rev;
            internalObject.RevisedDate = workItem.RevisedDate;
            internalObject.Revision = workItem.Revision;
            internalObject.ProjectName = workItem?.Project?.Name;
            internalObject.internalObject = workItem;
            return internalObject;
        }

        public static ProjectData ToProjectData(this Project project)
        {
            var internalObject = new ProjectData();
            internalObject.Id = project.Id.ToString();
            internalObject.Name = project.Name;
            internalObject.internalObject = project;
            return internalObject;
        }

        public static Project ToProject(this ProjectData projectdata)
        {
            if (!(projectdata.internalObject is Project))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(nameof(Project))}");
            }
            return (Project)projectdata.internalObject;
        }

        public static WorkItem ToWorkItem(this WorkItemData  workItemData)
        {
            if (!(workItemData.internalObject is WorkItem))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(nameof(WorkItem))}");
            }
            return (WorkItem)workItemData.internalObject;
        }

        public static List<WorkItemData> ToWorkItemDataList(this WorkItemCollection collection)
        {
            List<WorkItemData> list = new List<WorkItemData>();
            foreach (WorkItem wi in collection)
            {
                list.Add(wi.ToWorkItemData());
            }
            return list;
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
