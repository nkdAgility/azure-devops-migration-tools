using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSTS.DataBulkEditor.Engine.ComponentContext;

namespace VSTS.DataBulkEditor.Engine
{
   public class ProcessingEngine
    {
        string _applicationInsightsKey = "6a59fbae-e6a1-4df4-9316-37b4db0fadde";

        List<ITfsProcessingContext> processors = new List<ITfsProcessingContext>();
        List<Action<WorkItem, WorkItem>> processorActions = new List<Action<WorkItem, WorkItem>>();
        Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();
        ITeamProjectContext target;

        public ITeamProjectContext Target
        {
            get
            {
                return target;
            }
        }

        public ProcessingStatus Run()
        {
            InitiliseTelemetry();
             var measurements = new Dictionary<string, double>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TelemetryClient tc = new TelemetryClient();
            tc.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            tc.Context.Session.Id = Guid.NewGuid().ToString();
            measurements.Add("Processors", processors.Count);
            measurements.Add("Actions", processorActions.Count);
            measurements.Add("Mappings", fieldMapps.Count);
            tc.TrackEvent("MigrationEngine:Run", null, measurements);
            ProcessingStatus ps = ProcessingStatus.Complete;
            foreach (ITfsProcessingContext process in processors)
            {
                process.Execute();
                if (process.Status == ProcessingStatus.Failed)
                {
                    ps = ProcessingStatus.Failed;
                    Trace.WriteLine("The Processor {0} entered the failed state...stopping run", process.Name);
                    break;
                }
            }
            stopwatch.Stop();
            tc.TrackMetric("RunTime", stopwatch.ElapsedMilliseconds);
            return ps;
        }

        public void InitiliseTelemetry()
        {
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = _applicationInsightsKey;

        }

        public void AddProcessor<TProcessor>()
        {
            ITfsProcessingContext pc = (ITfsProcessingContext)Activator.CreateInstance(typeof(TProcessor), new object[] { this });
            AddProcessor(pc);
        }

        public void AddProcessor(ITfsProcessingContext processor)
        {
            processors.Add(processor);
        }

        public void SetTarget(ITeamProjectContext teamProjectContext)
        {
            target = teamProjectContext;
        }

        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
        {
            if (!fieldMapps.ContainsKey(workItemTypeName))
            {
                fieldMapps.Add(workItemTypeName, new List<IFieldMap>());
            }
            fieldMapps[workItemTypeName].Add(fieldToTagFieldMap);
        }


        internal void ApplyFieldMappings(WorkItem source, WorkItem target)
        { 
            if (fieldMapps.ContainsKey("*"))
            {
                ProcessFieldMapList(source, target, fieldMapps["*"]);
            }
            if (fieldMapps.ContainsKey(source.Type.Name))
            {
                ProcessFieldMapList(source, target, fieldMapps[source.Type.Name]);
            }
        }

        private  void ProcessFieldMapList(WorkItem source, WorkItem target, List<IFieldMap> list)
        {
            foreach (IFieldMap map in list)
            {
                map.Execute(source, target);
            }
        }


    }
}
