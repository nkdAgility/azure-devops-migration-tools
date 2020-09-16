using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Configuration;
using MigrationTools;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace VstsSyncMigrator.Engine
{
    public abstract class MigrationContextBase : ITfsProcessingContext
    {
        protected MigrationEngine me;
        protected IHost _host;

        protected MigrationContextBase(IHost host)
        {
            _host = host;
           
        }

        public abstract void Configure(ITfsProcessingConfig config);

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;

        public void Execute()
        {
            this.me = _host.Services.GetService<MigrationEngine>();
            Telemetry.Current.TrackPageView(this.Name);
            Log.Information("Migration Context Start: {MigrationContextname} ", Name);
            DateTime start = DateTime.Now;
            var executeTimer = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            try
            {
                Status = ProcessingStatus.Running;
                InternalExecute();
                Status = ProcessingStatus.Complete;
                executeTimer.Stop();

                Log.Information(" Migration Context Complete {MigrationContextname} ", Name);
            }
            catch (Exception ex)
            {
                Status = ProcessingStatus.Failed;
                executeTimer.Stop();
                
                Telemetry.Current.TrackException(ex,
                    new Dictionary<string, string>
                    {
                        {"Name", Name},
                        {"Target Project", me.Target.Config.Project},
                        {"Target Collection", me.Target.Collection.Name},
                        {"Source Project", me.Source.Config.Project},
                        {"Source Collection", me.Source.Collection.Name},
                        {"Status", Status.ToString()}
                    },
                    new Dictionary<string, double>
                    {
                        {"MigrationContextTime", executeTimer.ElapsedMilliseconds}
                    });
                Log.Fatal(ex, "Error while running {MigrationContextname}", Name);
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
            return string.Format("{0}\\{1}", me.Target.Config.Project, input);
            //Regex r = new Regex(source.Name, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            //return r.Replace(input, target.Name, 1);
        }

        internal string ReplaceFirstInstanceOf(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            var r = new Regex(me.Source.Config.Project, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            return r.Replace(input, me.Target.Config.Project, 1);
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