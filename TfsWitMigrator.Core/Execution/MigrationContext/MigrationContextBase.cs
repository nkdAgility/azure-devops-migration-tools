using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using VSTS.DataBulkEditor.Engine.Configuration.Processing;

namespace VSTS.DataBulkEditor.Engine
{
    public abstract class MigrationContextBase : ITfsProcessingContext
    {
        internal MigrationEngine me;
        ProcessingStatus status = ProcessingStatus.None;


        public MigrationContextBase(MigrationEngine me, ITfsProcessingConfig config)
        {
            this.me = me;
        }

        public abstract string Name { get; }

        public ProcessingStatus Status
        {
            get
            {
                return status;
            }
        }

        public void Execute()
        {
            Telemetry.Current.TrackEvent("MigrationContextExecute",
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", me.Target.Name},
                          { "Target Collection", me.Target.Collection.Name },
                          { "Source Project", me.Source.Name},
                          { "Source Collection", me.Source.Collection.Name }
                      });
            Trace.TraceInformation(string.Format(" Migration Context Start {0} ", Name));
            Stopwatch executeTimer = new Stopwatch();
            executeTimer.Start();
            //////////////////////////////////////////////////
            try
            {
                status = ProcessingStatus.Running;
                InternalExecute();
                status = ProcessingStatus.Complete;
                executeTimer.Stop();
                Telemetry.Current.TrackEvent("MigrationContextComplete",
                    new Dictionary<string, string> {
                        { "Name", Name},
                        { "Target Project", me.Target.Name},
                        { "Target Collection", me.Target.Collection.Name },
                        { "Source Project", me.Source.Name},
                        { "Source Collection", me.Source.Collection.Name },
                        { "Status", Status.ToString() }
                    },
                    new Dictionary<string, double> {
                        { "MigrationContextTime", executeTimer.ElapsedMilliseconds }
                    });
                Trace.TraceInformation(string.Format(" Migration Context Complete {0} ", Name));
            }
            catch (Exception ex)
            {
                status = ProcessingStatus.Failed;
                executeTimer.Stop();
                Telemetry.Current.TrackException(ex,
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", me.Target.Name},
                          { "Target Collection", me.Target.Collection.Name },
                          { "Source Project", me.Source.Name},
                          { "Source Collection", me.Source.Collection.Name },
                          { "Status", Status.ToString() }
                      },
                      new Dictionary<string, double> {
                            { "MigrationContextTime", executeTimer.ElapsedMilliseconds }
                      });
                Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.Message));
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
            Regex r = new Regex(me.Source.Name, RegexOptions.IgnoreCase);
            //// Output = [targetTeamProject]\[sourceTeamProject]\[AreaPath]
            return r.Replace(input, me.Target.Name, 1);

        }
    }



}