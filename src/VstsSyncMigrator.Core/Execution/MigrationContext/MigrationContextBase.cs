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
            Telemetry.Current.TrackEvent("MigrationContextExecute",
                new Dictionary<string, string>
                {
                    {"Name", Name},
                    {"Target Project", me.Target.Name},
                    {"Target Collection", me.Target.Collection.Name},
                    {"Source Project", me.Source.Name},
                    {"Source Collection", me.Source.Collection.Name}
                });
            Trace.TraceInformation(" Migration Context Start {0} ", Name);
            var executeTimer = new Stopwatch();
            executeTimer.Start();
            //////////////////////////////////////////////////
            try
            {
                Status = ProcessingStatus.Running;
                InternalExecute();
                Status = ProcessingStatus.Complete;
                executeTimer.Stop();
                Telemetry.Current.TrackEvent("MigrationContextComplete",
                    new Dictionary<string, string>
                    {
                        {"Name", Name},
                        {"Target Project", me.Target.Name},
                        {"Target Collection", me.Target.Collection.Name},
                        {"Source Project", me.Source.Name},
                        {"Source Collection", me.Source.Collection.Name},
                        {"Status", Status.ToString()}
                    },
                    new Dictionary<string, double>
                    {
                        {"MigrationContextTime", executeTimer.ElapsedMilliseconds}
                    });
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
                        {"Target Project", me.Target.Name},
                        {"Target Collection", me.Target.Collection.Name},
                        {"Source Project", me.Source.Name},
                        {"Source Collection", me.Source.Collection.Name},
                        {"Status", Status.ToString()}
                    },
                    new Dictionary<string, double>
                    {
                        {"MigrationContextTime", executeTimer.ElapsedMilliseconds}
                    });
                Trace.TraceWarning("  [EXCEPTION] {0}", ex.Message);
                throw ex;
            }
        }

        internal abstract void InternalExecute();

        internal string NodeStructreSourceToTarget(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            return string.Format("{0}\\{1}", me.Target.Name, input);


            //Regex r = new Regex(source.Name, RegexOptions.IgnoreCase);


            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            //return r.Replace(input, target.Name, 1);
        }

        internal string ReplaceFirstInstanceOf(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            var r = new Regex(me.Source.Name, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            return r.Replace(input, me.Target.Name, 1);
        }
    }
}