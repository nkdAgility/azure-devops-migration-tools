using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class TeamMigrationContext : MigrationContextBase
    {

        TeamMigrationConfig _config;
        MigrationEngine _me;

        public override string Name
        {
            get
            {
                return "TeamMigrationContext";
            }
        }

        public TeamMigrationContext(MigrationEngine me, TeamMigrationConfig config) : base(me, config)
        {
            _me = me;
            _config = config;
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.BypassRules);
            TfsTeamService sourceTS = me.Source.Collection.GetService<TfsTeamService>();
            List<TeamFoundationTeam> sourceTL = sourceTS.QueryTeams(me.Source.Config.Project).ToList();
            Trace.WriteLine(string.Format("Found {0} teams in Source?", sourceTL.Count));
            var sourceTSCS = me.Source.Collection.GetService<TeamSettingsConfigurationService>();
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project targetProject = targetStore.GetProject();
            Trace.WriteLine(string.Format("Found target project as {0}", targetProject.Name));
            TfsTeamService targetTS = me.Target.Collection.GetService<TfsTeamService>();
            List<TeamFoundationTeam> targetTL = targetTS.QueryTeams(me.Target.Config.Project).ToList();
            Trace.WriteLine(string.Format("Found {0} teams in Target?", targetTL.Count));
            var targetTSCS = me.Target.Collection.GetService<TeamSettingsConfigurationService>();
            //////////////////////////////////////////////////
            int current = sourceTL.Count;
            int count = 0;
            long elapsedms = 0;

            /// Create teams
            /// 
            foreach (TeamFoundationTeam sourceTeam in sourceTL)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
                var foundTargetTeam = (from x in targetTL where x.Name == sourceTeam.Name select x).SingleOrDefault();
                if (foundTargetTeam == null)
                {
                    Trace.WriteLine(string.Format("Processing team '{0}':", sourceTeam.Name));
                    TeamFoundationTeam newTeam = targetTS.CreateTeam(targetProject.Uri.ToString(), sourceTeam.Name, sourceTeam.Description, null);
                    Trace.WriteLine(string.Format("-> Team '{0}' created", sourceTeam.Name));

                    if (_config.EnableTeamSettingsMigration)
                    {
                        /// Duplicate settings
                        Trace.WriteLine(string.Format("-> Processing team '{0}' settings:", sourceTeam.Name));
                        var sourceConfigurations = sourceTSCS.GetTeamConfigurations(new List<Guid> { sourceTeam.Identity.TeamFoundationId });
                        var targetConfigurations = targetTSCS.GetTeamConfigurations(new List<Guid> { newTeam.Identity.TeamFoundationId });

                        foreach (var sourceConfig in sourceConfigurations)
                        {
                            var targetConfig = targetConfigurations.FirstOrDefault(t => t.TeamName == sourceConfig.TeamName);
                            if (targetConfig == null)
                            {
                                Trace.WriteLine(string.Format("-> Settings for team '{0}'.. not found", sourceTeam.Name));
                                continue;
                            }

                            Trace.WriteLine(string.Format("-> Settings found for team '{0}'..", sourceTeam.Name));
                            if (_config.PrefixProjectToNodes)
                            {
                                targetConfig.TeamSettings.BacklogIterationPath = 
                                    string.Format("{0}\\{1}", me.Target.Config.Project, sourceConfig.TeamSettings.BacklogIterationPath);
                                targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths
                                    .Select(path => string.Format("{0}\\{1}", me.Target.Config.Project, path))
                                    .ToArray();
                                targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues
                                    .Select(field => new TeamFieldValue
                                    {
                                        IncludeChildren = field.IncludeChildren,
                                        Value = string.Format("{0}\\{1}", me.Target.Config.Project, field.Value)
                                    })
                                    .ToArray();
                            }
                            else
                            {
                                targetConfig.TeamSettings.BacklogIterationPath = sourceConfig.TeamSettings.BacklogIterationPath;
                                targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths;
                                targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues;
                            }

                            targetTSCS.SetTeamSettings(targetConfig.TeamId, targetConfig.TeamSettings);
                            Trace.WriteLine(string.Format("-> Team '{0}' settings... applied", targetConfig.TeamName));
                        }
                    }
                }
                else
                {
                    Trace.WriteLine(string.Format("Team '{0}' found.. skipping", sourceTeam.Name));
                }

                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine("");
                //Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));
            }
            // Set Team Settings
            //foreach (TeamFoundationTeam sourceTeam in sourceTL)
            //{
            //    Stopwatch witstopwatch = new Stopwatch();
            //    witstopwatch.Start();
            //    var foundTargetTeam = (from x in targetTL where x.Name == sourceTeam.Name select x).SingleOrDefault();
            //    if (foundTargetTeam == null)
            //    {
            //        Trace.WriteLine(string.Format("Processing team {0}", sourceTeam.Name));
            //        var sourceTCfU = sourceTSCS.GetTeamConfigurations((new[] { sourceTeam.Identity.TeamFoundationId })).SingleOrDefault();
            //        TeamSettings newTeamSettings = CreateTargetTeamSettings(sourceTCfU);
            //        TeamFoundationTeam newTeam = targetTS.CreateTeam(targetProject.Uri.ToString(), sourceTeam.Name, sourceTeam.Description, null);
            //        targetTSCS.SetTeamSettings(newTeam.Identity.TeamFoundationId, newTeamSettings);
            //    }
            //    else
            //    {
            //        Trace.WriteLine(string.Format("Team found.. skipping"));
            //    }

            //    witstopwatch.Stop();
            //    elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
            //    current--;
            //    count++;
            //    TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
            //    TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
            //    Trace.WriteLine("");
            //    //Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));

            //}
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }


        private TeamSettings CreateTargetTeamSettings(TeamConfiguration sourceTCfU)
        {
            ///////////////////////////////////////////////////
            TeamSettings newTeamSettings = sourceTCfU.TeamSettings;
            newTeamSettings.BacklogIterationPath = newTeamSettings.BacklogIterationPath.Replace(me.Source.Config.Project, me.Target.Config.Project);
            List<string> newIterationPaths = new List<string>();
            foreach (var ip in newTeamSettings.IterationPaths)
            {
                newIterationPaths.Add(ip.Replace(me.Source.Config.Project, me.Target.Config.Project));
            }
            newTeamSettings.IterationPaths = newIterationPaths.ToArray();

            ///////////////////////////////////////////////////
            return newTeamSettings;
        }
    }
}