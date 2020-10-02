using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Clients;
using MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.Engine.Containers;
using Serilog;

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

        //public static void SaveWorkItem(this IProcessor context, WorkItem workItem)
        //{
        //    if (workItem == null) throw new ArgumentNullException(nameof(workItem));
        //    workItem.Fields["System.ChangedBy"].Value = "Migration";
        //    workItem.Save();
        //}

        public static void SaveToAzureDevOps(this WorkItemData context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var wi = (WorkItem)context.internalObject;
            wi.Fields["System.ChangedBy"].Value = "Migration";
            wi.Save();
            context.RefreshWorkItem();
        }
        public static void RefreshWorkItem(this WorkItemData context)
        {
            var workItem = (WorkItem)context.internalObject;
            context.Id = workItem.Id.ToString();
            context.Type = workItem.Type.Name;
            context.Title = workItem.Title;
            context.Rev = workItem.Rev;
            context.RevisedDate = workItem.RevisedDate;
            context.Revision = workItem.Revision;
            context.ProjectName = workItem?.Project?.Name;
            context.Fields = workItem.Fields.AsDictionary();
        }

        public static WorkItemData AsWorkItemData(this WorkItem context)
        {
            if (context.AreaPath == "migrationTarget1")
            {
                //stop here
                Log.Information(context.AreaPath);
            }
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var internalWorkItem = new WorkItemData();
            internalWorkItem.internalObject = context;
            internalWorkItem.RefreshWorkItem();
            return internalWorkItem;
        }

        public static WorkItem ToWorkItem(this WorkItemData workItemData)
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
                list.Add(wi.AsWorkItemData());
            }
            return list;
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

     
    }
}
