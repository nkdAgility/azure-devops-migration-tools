﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using Newtonsoft.Json;
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

        public static Dictionary<string, object> AsDictionary(this FieldCollection col)
        {
            var dict = new Dictionary<string, object>();
            for (var ix = 0; ix < col.Count; ix++)
            {
                dict.Add(col[ix].ReferenceName, col[ix].Value);
            }
            return dict;
        }

        public static TfsTeamProjectConfig AsTeamProjectConfig(this IMigrationClientConfig context)
        {
            return (TfsTeamProjectConfig)context;
        }

        public static WorkItemData AsWorkItemData(this WorkItem context, Dictionary<string, FieldItem> fieldsOfRevision = null)
        {
            var internalWorkItem = new WorkItemData
            {
                internalObject = context
            };

            internalWorkItem.RefreshWorkItem(fieldsOfRevision);
            return internalWorkItem;
        }

        public static WorkItemData GetRevision(this WorkItemData context, int rev)
        {
            var originalWi = (WorkItem)context.internalObject;
            var wid = new WorkItemData
            {
                // internalObject = context.internalObject
                // TODO: Had to revert to calling revision load again untill WorkItemMigrationContext.PopulateWorkItem can be updated to pull from WorkItemData
                internalObject = originalWi.Store.GetWorkItem(originalWi.Id, rev)
            };

            wid.RefreshWorkItem(context.Revisions[rev].Fields);

            return wid;
        }

        public static void RefreshWorkItem(this WorkItemData context, Dictionary<string, FieldItem> fieldsOfRevision = null)
        {
            var workItem = (WorkItem)context.internalObject;
            TfsWorkItemConvertor tfswic = new TfsWorkItemConvertor();
            tfswic.MapWorkItemtoWorkItemData(context, workItem, fieldsOfRevision);
        }

        public static void SaveToAzureDevOps(this WorkItemData context)
        {
            Log.Debug("TfsExtensions::SaveToAzureDevOps");

            if (context == null) throw new ArgumentNullException(nameof(context));
            var workItem = (WorkItem)context.internalObject;
            Log.Debug("TfsExtensions::SaveToAzureDevOps: ChangedBy: {ChangedBy}, AuthorisedBy: {AuthorizedIdentity}", workItem.ChangedBy, workItem.Store.TeamProjectCollection.AuthorizedIdentity.DisplayName);
            var fails = workItem.Validate();
            if (fails.Count > 0)
            {
                Log.Warning("Work Item is not ready to save as it has some invalid fields. This may not result in an error. Enable LogLevel as 'Debug' in the config to see more.");
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
                foreach (Field f in fails)
                {
                    Log.Debug("Invalid Field Object:\r\n{Field}", f.ToJson());
                }
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
                Log.Debug("--------------------------------------------------------------------------------------------------------------------");
            }
            Log.Verbose("TfsExtensions::SaveToAzureDevOps::Save()");
            workItem.Save();
            context.RefreshWorkItem();
        }

        public static string ToJson(this Field f)
        {
            Log.Debug("TfsExtensions::ToJson");
            dynamic expando = new ExpandoObject();
            expando.WorkItemId = f.WorkItem.Id;
            expando.CurrentRevisionWorkItemRev = f.WorkItem.Rev;
            expando.CurrentRevisionWorkItemTypeName = f.WorkItem.Type.Name;
            expando.Name = f.Name;
            expando.ReferenceName = f.ReferenceName;
            expando.Value = f.Value;
            expando.OriginalValue = f.OriginalValue;
            expando.ValueWithServerDefault = f.ValueWithServerDefault;

            expando.Status = f.Status;
            expando.IsRequired = f.IsRequired;
            expando.IsEditable = f.IsEditable;
            expando.IsDirty = f.IsDirty;
            expando.IsComputed = f.IsComputed;
            expando.IsChangedByUser = f.IsChangedByUser;
            expando.IsChangedInRevision = f.IsChangedInRevision;
            expando.HasPatternMatch = f.HasPatternMatch;

            expando.IsLimitedToAllowedValues = f.IsLimitedToAllowedValues;
            expando.HasAllowedValuesList = f.HasAllowedValuesList;
            expando.AllowedValues = f.AllowedValues;
            expando.IdentityFieldAllowedValues = f.IdentityFieldAllowedValues;
            expando.ProhibitedValues = f.ProhibitedValues;

            return JsonConvert.SerializeObject(expando, Formatting.Indented);
        }

        public static Project ToProject(this ProjectData projectdata)
        {
            if (!(projectdata.internalObject is Project))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(nameof(Project))}");
            }
            return (Project)projectdata.internalObject;
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
    }
}