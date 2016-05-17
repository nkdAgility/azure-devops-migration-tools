using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TfsWitMigrator.Core
{
    public class WorkItemPostProcessingContext : MigrationContextBase
    {

        private IList<string> _workItemTypes;
        private IList<int> _workItemIDs;
        private string _queryBit;
        
        public WorkItemPostProcessingContext(MigrationEngine me) : base(me)
        {
            
        }

        public override string Name
        {
            get
            {
                return "WorkItemPostProcessingContext";
            }
        }
        public WorkItemPostProcessingContext(MigrationEngine me, IList<string> wiTypes) : this(me)
        {
            _workItemTypes = wiTypes;
        }

        public WorkItemPostProcessingContext(MigrationEngine me, IList<int> wiIDs) : this(me )
        {
            _workItemIDs = wiIDs;
        }

        public WorkItemPostProcessingContext(MigrationEngine me, string queryBit) : this (me)
        {
            _queryBit = queryBit;
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Name);

            //Builds the constraint part of the query
            string constraints = CreateConstraints();
            
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id] ", constraints); 
            
            WorkItemCollection sourceWIS = tfsqc.Execute();
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count));
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project destProject = targetStore.GetProject();
            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name));
           
           
            int current = sourceWIS.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                Stopwatch witstopwatch = new Stopwatch();
                witstopwatch.Start();
                WorkItem targetFound;
                targetFound = targetStore.FindReflectedWorkItem(sourceWI, me.ReflectedWorkItemIdFieldName);
                Trace.WriteLine(string.Format("{0} - Updating: {1}-{2}", current, sourceWI.Id, sourceWI.Type.Name));
                if (targetFound == null)
                {
                    Trace.WriteLine(string.Format("{0} - WARNING: does not exist {1}-{2}", current, sourceWI.Id, sourceWI.Type.Name));
                }
                else
                {
                    Console.WriteLine("...Exists");
                    targetFound.Open();
                    me.ApplyFieldMappings(sourceWI, targetFound);
                    if (targetFound.IsDirty)
                    {
                        try
                    {
                        targetFound.Save();
                        Trace.WriteLine(string.Format("          Updated"));
                        }
                        catch (ValidationException ve)
                        {

                            Trace.WriteLine(string.Format("          [FAILED] {0}", ve.ToString()));
                        }

                    }
                    else
                    {
                        Trace.WriteLine(string.Format("          No changes"));
                    }
                    
                }
                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

        //TODO: Can be removed
        //private string CreateTypeOrIdContraints()
        //{
        //    string idContraints = string.Empty;
        //    if (_workItemIDs != null && _workItemIDs.Count > 0)
        //    {
        //        if (_workItemIDs.Count == 1)
        //        {
        //            idContraints = string.Format(" AND [System.Id] = {0} ", _workItemIDs[0]);
        //        }
        //        else
        //        {
        //            idContraints = string.Format(" AND [System.Id] IN ({0}) ", string.Join(",", _workItemIDs));
        //        }
        //        return idContraints;
        //    }

        //    string typeConstraints = string.Empty;
        //    if (_workItemTypes != null && _workItemTypes.Count > 0)
        //    {
        //        if (_workItemTypes.Count == 1)
        //        {
        //            typeConstraints = string.Format(" AND [System.WorkItemType] = '{0}' ", _workItemTypes[0]);
        //        }
        //        else
        //        {
        //            typeConstraints = string.Format(" AND [System.WorkItemType] IN ('{0}') ", string.Join("','", _workItemTypes));
        //        }
        //        return typeConstraints;
        //    }

        //    return string.Empty;            
        //}

        private string CreateConstraints()
        {
            if (_workItemIDs != null && _workItemIDs.Count > 0)
            {
                string idContraints;
                if (_workItemIDs.Count == 1)
                {
                    idContraints = string.Format (" AND [System.Id] = {0} ", _workItemIDs [0]);
                }
                else
                {
                    idContraints = string.Format (" AND [System.Id] IN ({0}) ", string.Join (",", _workItemIDs));
                }
                return idContraints;
            }

            
            if (_workItemTypes != null && _workItemTypes.Count > 0)
            {
                string typeConstraints;
                if (_workItemTypes.Count == 1)
                {
                    typeConstraints = string.Format (" AND [System.WorkItemType] = '{0}' ", _workItemTypes [0]);
                }
                else
                {
                    typeConstraints = string.Format (" AND [System.WorkItemType] IN ('{0}') ", string.Join ("','", _workItemTypes));
                }
                return typeConstraints;
            }

            
            if (!String.IsNullOrEmpty (_queryBit))
            {
                var queryConstraints = _queryBit;
                return queryConstraints;
            }
            return string.Empty;      
        }
    }
}