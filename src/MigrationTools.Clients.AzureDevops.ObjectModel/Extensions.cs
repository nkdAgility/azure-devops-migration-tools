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
using MigrationTools.Engine.Containers;

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

        public static void SaveWorkItem(this IProcessor context, WorkItem workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            workItem.Fields["System.ChangedBy"].Value = "Migration";
            workItem.Save();
        }

        public static void SaveWorkItem(this IProcessor context, WorkItemData workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            var wi = (WorkItem)workItem.InternalWorkItem;
            wi.Fields["System.ChangedBy"].Value = "Migration";
            wi.Save();
        }

        public static Dictionary<string, object> AsDictionary(this FieldCollection col)
        {
            var dict = new Dictionary<string, object>();
            for (var ix = 0; ix < col.Count; ix++)
            {
                dict.Add(col[ix].Name, col[ix].Value);
            }
            return dict;
        }

        public static WorkItemData ToWorkItemData(this WorkItem workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var internalWorkItem = new WorkItemData();
            internalWorkItem.Id = workItem.Id.ToString();
            internalWorkItem.Type = workItem.Type.Name;
            internalWorkItem.Title = workItem.Title;
            internalWorkItem.Rev = workItem.Rev;
            internalWorkItem.RevisedDate = workItem.RevisedDate;
            internalWorkItem.Revision = workItem.Revision;
            internalWorkItem.ProjectName = workItem?.Project?.Name;
            internalWorkItem.Fields = workItem.Fields.AsDictionary();
            internalWorkItem.InternalWorkItem = workItem;
            return internalWorkItem;
        }

        public static ProjectData ToProjectData(this Project project)
        {
            var internalproject = new ProjectData();
            internalproject.Id = project.Id.ToString();
            internalproject.Name = project.Name;
            internalproject.InternalProject = project;
            return internalproject;
        }

        public static Project ToProject(this ProjectData projectdata)
        {
            if (!(projectdata.InternalProject is Project))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(nameof(Project))}");
            }
            return (Project)projectdata.InternalProject;
        }

        public static WorkItem ToWorkItem(this WorkItemData  workItemData)
        {
            if (!(workItemData.InternalWorkItem is WorkItem))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(nameof(WorkItem))}");
            }
            return (WorkItem)workItemData.InternalWorkItem;
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
