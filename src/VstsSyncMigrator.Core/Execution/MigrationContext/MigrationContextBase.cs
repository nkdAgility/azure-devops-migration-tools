using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public abstract class MigrationContextBase : ITfsProcessingContext
    {
        internal readonly MigrationEngine me;


        protected MigrationContextBase(MigrationEngine me, ITfsProcessingConfig config)
        {
            this.me = me;
        }

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;

        public void Execute()
        {
            Telemetry.Current.TrackPageView(this.Name);
            Trace.TraceInformation(" Migration Context Start {0} ", Name);
            DateTime start = DateTime.Now;
            var executeTimer = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            try
            {
                Status = ProcessingStatus.Running;
                InternalExecute();
                Status = ProcessingStatus.Complete;
                executeTimer.Stop();

                Trace.TraceInformation(" Migration Context Complete {0} ", Name);
            }
            catch (Exception ex)
            {
                Status = ProcessingStatus.Failed;
                executeTimer.Stop();
                Telemetry.Current.TrackException(ex,
                    new Dictionary<string, string>
                    {
                        {"Name", Name},
                        {"Target Project", me.Target.Config.Name},
                        {"Target Collection", me.Target.Collection.Name},
                        {"Source Project", me.Source.Config.Name},
                        {"Source Collection", me.Source.Collection.Name},
                        {"Status", Status.ToString()}
                    },
                    new Dictionary<string, double>
                    {
                        {"MigrationContextTime", executeTimer.ElapsedMilliseconds}
                    });
                Trace.TraceWarning($"  [EXCEPTION] {ex}");
            }
            finally
            {
                Telemetry.Current.TrackRequest(this.Name, start, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }

        }

        internal abstract void InternalExecute();

        internal string NodeStructreSourceToTarget(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            return string.Format("{0}\\{1}", me.Target.Config.Name, input);


            //Regex r = new Regex(source.Name, RegexOptions.IgnoreCase);


            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            //return r.Replace(input, target.Name, 1);
        }

        internal static void SaveWorkItem(WorkItem workItem)
        {
            workItem.Fields["System.ChangedBy"].Value = "Migration";
            workItem.Save();
        }

        internal string ReplaceFirstInstanceOf(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            var r = new Regex(me.Source.Config.Name, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            return r.Replace(input, me.Target.Config.Name, 1);
        }

        internal static void AddParameter(string name, IDictionary<string, string> store, string value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }

        internal static void AddMetric(string name, IDictionary<string, double> store, double value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }
    }
}