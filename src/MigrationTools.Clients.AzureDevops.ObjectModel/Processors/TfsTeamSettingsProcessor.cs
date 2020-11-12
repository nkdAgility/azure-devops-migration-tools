using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Native TFS Processor, does not work with any other Endpoints.
    /// </summary>
    public class TfsTeamSettingsProcessor : Processor
    {
        private TfsTeamSettingsProcessorOptions _Options;

        public TfsTeamSettingsProcessor(ProcessorEnricherContainer processorEnrichers,
                                        EndpointContainer endpoints,
                                        IServiceProvider services,
                                        ITelemetryLogger telemetry,
                                        ILogger<Processor> logger) : base(processorEnrichers, endpoints, services, telemetry, logger)
        {
        }

        public TfsTeamSettingsEndpoint Source => (TfsTeamSettingsEndpoint)Endpoints.Source;

        public TfsTeamSettingsEndpoint Target => (TfsTeamSettingsEndpoint)Endpoints.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsTeamSettingsProcessor::Configure");
            _Options = (TfsTeamSettingsProcessorOptions)options;
        }

        protected override void InternalExecute()
        {
            if (_Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            List<TeamFoundationTeam> sourceTeams = Source.TfsTeamService.QueryTeams(Source.Project).ToList();
            Log.LogInformation("TfsTeamSettingsProcessor::InternalExecute: Found {0} teams in Source?", sourceTeams.Count);
            //////////////////////////////////////////////////
            List<TeamFoundationTeam> targetTeams = Target.TfsTeamService.QueryTeams(Target.Project).ToList();
            Log.LogDebug("Found {0} teams in Target?", sourceTeams.Count);
            //////////////////////////////////////////////////
            int current = sourceTeams.Count;
            int count = 0;
            long elapsedms = 0;

            /// Create teams
            ///
            foreach (TeamFoundationTeam sourceTeam in sourceTeams)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
                var foundTargetTeam = (from x in targetTeams where x.Name == sourceTeam.Name select x).SingleOrDefault();
                if (foundTargetTeam == null || _Options.UpdateTeamSettings)
                {
                    Log.LogDebug("Processing team '{0}':", sourceTeam.Name);
                    TeamFoundationTeam newTeam = foundTargetTeam ?? Target.TfsTeamService.CreateTeam(Target.TfsProjectUri.ToString(), sourceTeam.Name, sourceTeam.Description, null);
                    Log.LogDebug("-> Team '{0}' created", sourceTeam.Name);

                    if (_Options.MigrateTeamSettings)
                    {
                        /// Duplicate settings
                        Log.LogDebug("-> Processing team '{0}' settings:", sourceTeam.Name);
                        var sourceConfigurations = Source.TfsTeamSettingsService.GetTeamConfigurations(new List<Guid> { sourceTeam.Identity.TeamFoundationId });
                        var targetConfigurations = Target.TfsTeamSettingsService.GetTeamConfigurations(new List<Guid> { newTeam.Identity.TeamFoundationId });

                        foreach (var sourceConfig in sourceConfigurations)
                        {
                            if (sourceConfig.TeamSettings.BacklogIterationPath != null &&
                                sourceConfig.TeamSettings.TeamFieldValues.Length > 0)
                            {
                                var targetConfig = targetConfigurations.FirstOrDefault(t => t.TeamName == sourceConfig.TeamName);
                                if (targetConfig == null)
                                {
                                    Log.LogDebug("-> Settings for team '{sourceTeamName}'.. not found", sourceTeam.Name);
                                    continue;
                                }

                                Log.LogInformation("-> Settings found for team '{sourceTeamName}'..", sourceTeam.Name);
                                if (_Options.PrefixProjectToNodes)
                                {
                                    targetConfig.TeamSettings.BacklogIterationPath =
                                        string.Format("{0}\\{1}", Target.Project, sourceConfig.TeamSettings.BacklogIterationPath);
                                    targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths
                                        .Select(path => string.Format("{0}\\{1}", Target.Project, path))
                                        .ToArray();
                                    targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues
                                        .Select(field => new TeamFieldValue
                                        {
                                            IncludeChildren = field.IncludeChildren,
                                            Value = string.Format("{0}\\{1}", Target.Project, field.Value)
                                        })
                                        .ToArray();
                                }
                                else
                                {
                                    targetConfig.TeamSettings.BacklogIterationPath = sourceConfig.TeamSettings.BacklogIterationPath.Replace(Source.Project, Target.Project);
                                    Log.LogDebug("targetConfig.TeamSettings.BacklogIterationPath={BacklogIterationPath}", targetConfig.TeamSettings.BacklogIterationPath);
                                    targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths.Select(ip => ip.Replace(Source.Project, Target.Project)).ToArray();
                                    Log.LogDebug("targetConfig.TeamSettings.IterationPaths={@IterationPaths}", targetConfig.TeamSettings.IterationPaths);
                                    targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues;
                                    foreach (var item in targetConfig.TeamSettings.TeamFieldValues)
                                    {
                                        item.Value = item.Value.Replace(Source.Project, Target.Project);
                                    }
                                    Log.LogDebug("targetConfig.TeamSettings.TeamFieldValues={@TeamFieldValues}", targetConfig.TeamSettings.TeamFieldValues);
                                }

                                Target.TfsTeamSettingsService.SetTeamSettings(targetConfig.TeamId, targetConfig.TeamSettings);
                                Log.LogDebug("-> Team '{0}' settings... applied", targetConfig.TeamName);
                            }
                            else
                            {
                                Log.LogWarning("-> Settings for team '{sourceTeamName}'.. not configured", sourceTeam.Name);
                            }
                        }
                    }
                }
                else
                {
                    Log.LogDebug("Team '{0}' found.. skipping", sourceTeam.Name);
                }

                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                //Log.LogInformation("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));
            }
            // Set Team Settings
            //foreach (TeamFoundationTeam sourceTeam in sourceTL)
            //{
            //    Stopwatch witstopwatch = new Stopwatch();
            //    witstopwatch.Start();
            //    var foundTargetTeam = (from x in targetTL where x.Name == sourceTeam.Name select x).SingleOrDefault();
            //    if (foundTargetTeam == null)
            //    {
            //        TLog.LogInformation("Processing team {0}", sourceTeam.Name));
            //        var sourceTCfU = sourceTSCS.GetTeamConfigurations((new[] { sourceTeam.Identity.TeamFoundationId })).SingleOrDefault();
            //        TeamSettings newTeamSettings = CreateTargetTeamSettings(sourceTCfU);
            //        TeamFoundationTeam newTeam = targetTS.CreateTeam(targetProject.Uri.ToString(), sourceTeam.Name, sourceTeam.Description, null);
            //        targetTSCS.SetTeamSettings(newTeam.Identity.TeamFoundationId, newTeamSettings);
            //    }
            //    else
            //    {
            //        Log.LogInformation("Team found.. skipping"));
            //    }

            //    witstopwatch.Stop();
            //    elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
            //    current--;
            //    count++;
            //    TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
            //    TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
            //    //Log.LogInformation("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));

            //}
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }
    }
}