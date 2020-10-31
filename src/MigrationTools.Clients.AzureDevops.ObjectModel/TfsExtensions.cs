using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools
{
    public static class TfsExtensions
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
            var workItem = (WorkItem)context.internalObject;
            var fails = workItem.Validate();
            if (fails.Count > 0)
            {
                Log.Warning("Work Item is not ready to save as it has some invalid fields. This may not result in an error. Enable LogLevel as 'Debug' in the ocnfig to see more.");
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
                foreach (Field f in fails)
                {
                    Log.Debug("   Invalid Field: [Id:{CurrentRevisionWorkItemId}/{CurrentRevisionWorkItemRev}][Type:{CurrentRevisionWorkItemTypeName}][IsDirty:{IsDirty}][ReferenceName:{FieldReferenceName}][Value: {FieldValue}]",
                       new Dictionary<string, object>() {
                               {"CurrentRevisionWorkItemId", workItem.Id },
                               {"CurrentRevisionWorkItemRev", workItem.Rev },
                               {"CurrentRevisionWorkItemTypeName",  workItem.Type},
                               {"IsDirty", f.IsDirty },
                               {"FieldReferenceName", f.ReferenceName },
                               {"FieldValue", f.Value }
                       });
                    Log.Verbose("{   Field Object: {field}", f);
                }
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
            }
            workItem.Fields["System.ChangedBy"].Value = "Migration";
            workItem.Save();
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
            context.Revisions = (from Revision x in workItem.Revisions
                                 select new RevisionItem()
                                 {
                                     Index = x.Index,
                                     Number = Convert.ToInt32(x.Fields["System.Rev"].Value),
                                     ChangedDate = Convert.ToDateTime(x.Fields["System.ChangedDate"].Value)
                                 }).ToList();
        }

        public static WorkItemData AsWorkItemData(this WorkItem context)
        {
            var internalWorkItem = new WorkItemData
            {
                internalObject = context
            };
            internalWorkItem.RefreshWorkItem();
            return internalWorkItem;
        }

        public static TfsTeamProjectConfig AsTeamProjectConfig(this IMigrationClientConfig context)
        {
            return (TfsTeamProjectConfig)context;
        }

        public static WorkItem ToWorkItem(this WorkItemData workItemData)
        {
            if (!(workItemData.internalObject is WorkItem))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(nameof(WorkItem))}");
            }
            return (WorkItem)workItemData.internalObject;
        }

        public static List<WorkItemData> ToWorkItemDataList(this IList<WorkItem> collection)
        {
            List<WorkItemData> list = new List<WorkItemData>();
            foreach (WorkItem wi in collection)
            {
                list.Add(wi.AsWorkItemData());
            }
            return list;
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
            var internalObject = new ProjectData
            {
                Id = project.Id.ToString(),
                Name = project.Name,
                internalObject = project
            };
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