//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.TraceListener;
//using Microsoft.TeamFoundation.WorkItemTracking.Client;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VSTS.DataBulkEditor.Engine.ComponentContext;

//namespace VSTS.DataBulkEditor.Engine
//{
//   public class ProcessingEngine
//    {
//        List<ITfsProcessingContext> processors = new List<ITfsProcessingContext>();
//        List<Action<WorkItem, WorkItem>> processorActions = new List<Action<WorkItem, WorkItem>>();
//        Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();
//        ITeamProjectContext target;

//        public ITeamProjectContext Target
//        {
//            get
//            {
//                return target;
//            }
//        }

//        public ProcessingStatus Run()
//        {
//            Telemetry.Current.TrackEvent("EngineStart",
//                new Dictionary<string, string> {
//                    { "Engine", "Processing" }
//                },
//                new Dictionary<string, double> {
//                    { "Processors", processors.Count },
//                    { "Actions",  processorActions.Count},
//                    { "Mappings", fieldMapps.Count }
//                });
//            Stopwatch engineTimer = new Stopwatch();
//            engineTimer.Start();
//            ProcessingStatus ps = ProcessingStatus.Complete;
//            foreach (ITfsProcessingContext process in processors)
//            {
//                Stopwatch processorTimer = new Stopwatch();
//                processorTimer.Start();
//                process.Execute();
//                processorTimer.Stop();
//                Telemetry.Current.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });
//                if (process.Status == ProcessingStatus.Failed)
//                {
//                    ps = ProcessingStatus.Failed;
//                    Trace.WriteLine("The Processor {0} entered the failed state...stopping run", process.Name);
//                    break;
//                }
//            }
//            engineTimer.Stop();
//            Telemetry.Current.TrackEvent("EngineComplete",
//                new Dictionary<string, string> {
//                    { "Engine", "Processing" }
//                },
//                new Dictionary<string, double> {
//                    { "EngineTime", engineTimer.ElapsedMilliseconds }
//                });
//            return ps;
//        }

//        public void AddProcessor<TProcessor>()
//        {
//            ITfsProcessingContext pc = (ITfsProcessingContext)Activator.CreateInstance(typeof(TProcessor), new object[] { this });
//            AddProcessor(pc);
//        }

//        public void AddProcessor(ITfsProcessingContext processor)
//        {
//            processors.Add(processor);
//        }

//        public void SetTarget(ITeamProjectContext teamProjectContext)
//        {
//            target = teamProjectContext;
//        }

//        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
//        {
//            if (!fieldMapps.ContainsKey(workItemTypeName))
//            {
//                fieldMapps.Add(workItemTypeName, new List<IFieldMap>());
//            }
//            fieldMapps[workItemTypeName].Add(fieldToTagFieldMap);
//        }


//        internal void ApplyFieldMappings(WorkItem source, WorkItem target)
//        { 
//            if (fieldMapps.ContainsKey("*"))
//            {
//                ProcessFieldMapList(source, target, fieldMapps["*"]);
//            }
//            if (fieldMapps.ContainsKey(source.Type.Name))
//            {
//                ProcessFieldMapList(source, target, fieldMapps[source.Type.Name]);
//            }
//        }

//        private  void ProcessFieldMapList(WorkItem source, WorkItem target, List<IFieldMap> list)
//        {
//            foreach (IFieldMap map in list)
//            {
//                map.Execute(source, target);
//            }
//        }


//    }
//}
