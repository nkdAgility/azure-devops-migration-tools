using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MigrationTools.Engine;
using MigrationTools.Configuration;
using MigrationTools;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.ApplicationInsights.DataContracts;
using MigrationTools.Engine.Containers;
using MigrationTools;

namespace MigrationTools.Engine.Processors
{
    public abstract class MigrationProcessorBase : IProcessor
    {
        internal IMigrationEngine _me;
        internal IServiceProvider _services;

        public IMigrationEngine Engine { get { return _me; } }
        public IServiceProvider Services { get { return _services; } }

        protected MigrationProcessorBase(IMigrationEngine me, IServiceProvider services, ITelemetryLogger telemetry)
        {
            _me = me;
            _services = services;
            Telemetry = telemetry;
        }

        public abstract void Configure(IProcessorConfig config);

        public abstract string Name { get; }

        public ProcessingStatus Status { get; private set; } = ProcessingStatus.None;
        public ITelemetryLogger Telemetry { get; }

        public void Execute()
        {
            Telemetry.TrackEvent(this.Name);
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

                Telemetry.TrackException(ex,
                    new Dictionary<string, string>
                    {
                        {"Name", Name},
                        {"Target Project", Engine.Target.Config.Project},
                        {"Target Collection", Engine.Target.Config.Collection.ToString()},
                        {"Source Project", Engine.Source.Config.Project},
                        {"Source Collection", Engine.Source.Config.Collection.ToString()},
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
                Telemetry.TrackRequest( this.Name, start, executeTimer.Elapsed, Status.ToString(), (Status == ProcessingStatus.Complete));
            }

        }

        protected abstract void InternalExecute();

        protected string NodeStructreSourceToTarget(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            return string.Format("{0}\\{1}", Engine.Target.Config.Project, input);
            //Regex r = new Regex(source.Name, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            //return r.Replace(input, target.Name, 1);
        }

        protected string ReplaceFirstInstanceOf(string input)
        {
            //input = [sourceTeamProject]\[AreaPath]
            var r = new Regex(Engine.Source.Config.Project, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            return r.Replace(input, Engine.Target.Config.Project, 1);
        }

        protected static void AddParameter(string name, IDictionary<string, string> store, string value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }

        protected static void AddMetric(string name, IDictionary<string, double> store, double value)
        {
            if (!store.ContainsKey(name)) store.Add(name, value);
        }
    }
}