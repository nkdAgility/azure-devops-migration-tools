using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Commerce;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// The TfsUserMappingTool is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
    /// </summary>
    public class TfsTeamSettingsTool : Tool<TfsTeamSettingsToolOptions>
    {

        private const string LogTypeName = nameof(TfsTeamSettingsTool);

        private Lazy<List<TeamFoundationIdentity>> _targetTeamFoundationIdentitiesLazyCache;

        private TfsProcessor _processor;

        public TfsTeamService SourceTeamService { get; protected set; }
        public TeamSettingsConfigurationService SourceTeamSettings { get; protected set; }
        public TfsTeamService TargetTeamService { get; protected set; }
        public TeamSettingsConfigurationService TargetTeamSettings { get; protected set; }

        public TfsTeamSettingsTool(IOptions<TfsTeamSettingsToolOptions> options, IServiceProvider services, ILogger<TfsTeamSettingsTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {

        }


        public void ProcessorExecutionBegin(TfsProcessor processor) // Could be a IProcessorEnricher
        {
            _processor = processor;
            if (Options.Enabled)
            {
                Log.LogInformation("----------------------------------------------");
                Log.LogInformation("Migrating all Teams before the Processor run.");
                _targetTeamFoundationIdentitiesLazyCache = new Lazy<List<TeamFoundationIdentity>>(() =>
                {
                    try
                    {
                        var identityService = processor.Target.GetService<IIdentityManagementService>();
                        var tfi = identityService.ReadIdentity(IdentitySearchFactor.General, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);
                        return identityService.ReadIdentities(tfi.Members, MembershipQuery.None, ReadIdentityOptions.None).ToList();
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "{LogTypeName}: Unable load list of identities from target collection.", LogTypeName);
                        Telemetry.TrackException(ex, null);
                        return new List<TeamFoundationIdentity>();
                    }
                });
                SourceTeamService = processor.Source.GetService<TfsTeamService>();
                SourceTeamSettings = processor.Source.GetService<TeamSettingsConfigurationService>();
                TargetTeamService = processor.Target.GetService<TfsTeamService>();
                TargetTeamSettings = processor.Target.GetService<TeamSettingsConfigurationService>();
                MigrateTeamSettings();

            }
        }

        private void MigrateTeamSettings()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            List<TeamFoundationTeam> sourceTeams = SourceTeamService.QueryTeams(_processor.Source.Options.Project).ToList();
            Log.LogInformation("TfsTeamSettingsProcessor::InternalExecute: Found {0} teams in Source?", sourceTeams.Count);
            //////////////////////////////////////////////////
            List<TeamFoundationTeam> targetTeams = TargetTeamService.QueryTeams(_processor.Target.Options.Project).ToList();
            Log.LogDebug("Found {0} teams in Target?", sourceTeams.Count);
            //////////////////////////////////////////////////
            if (!Options.Enabled)
            {
                Log.LogWarning("TfsTeamSettingsProcessor is not enabled"); return;
            }
            TfsNodeStructureTool nodeStructureEnricher = Services.GetService<TfsNodeStructureTool>();

            int current = sourceTeams.Count;
            int count = 0;
            long elapsedms = 0;

            /////////
            if (Options.Teams != null)
            {
                sourceTeams = sourceTeams.Where(t => Options.Teams.Contains(t.Name)).ToList();
            }

            // Create teams
            foreach (TeamFoundationTeam sourceTeam in sourceTeams)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
                var foundTargetTeam = (from x in targetTeams where x.Name == sourceTeam.Name select x).SingleOrDefault();
                if (foundTargetTeam == null || Options.UpdateTeamSettings)
                {
                    Log.LogDebug("Processing team '{0}':", sourceTeam.Name);
                    TeamFoundationTeam newTeam = foundTargetTeam ?? TargetTeamService.CreateTeam(_processor.Target.WorkItems.Project.Url, sourceTeam.Name, sourceTeam.Description, null);
                    Log.LogDebug("-> Team '{0}' created", sourceTeam.Name);

                    if (Options.MigrateTeamSettings)
                    {
                        // Duplicate settings
                        Log.LogDebug("-> Processing team '{0}' settings:", sourceTeam.Name);
                        var sourceConfigurations = SourceTeamSettings.GetTeamConfigurations(new List<Guid> { sourceTeam.Identity.TeamFoundationId });
                        var targetConfigurations = TargetTeamSettings.GetTeamConfigurations(new List<Guid> { newTeam.Identity.TeamFoundationId });

                        foreach (var sourceConfig in sourceConfigurations)
                        {
                            if (sourceConfig.TeamSettings.BacklogIterationPath != null &&
                                sourceConfig.TeamSettings.TeamFieldValues.Length > 0)
                            {
                                var iterationMap = new Dictionary<string, string>();

                                var targetConfig = targetConfigurations.FirstOrDefault(t => t.TeamName == sourceConfig.TeamName);
                                if (targetConfig == null)
                                {
                                    Log.LogDebug("-> Settings for team '{sourceTeamName}'.. not found", sourceTeam.Name);
                                    continue;
                                }

                                Log.LogInformation("-> Settings found for team '{sourceTeamName}'..", sourceTeam.Name);

                                targetConfig.TeamSettings.BacklogIterationPath = nodeStructureEnricher.GetNewNodeName(sourceConfig.TeamSettings.BacklogIterationPath, TfsNodeStructureType.Iteration);
                                Log.LogDebug("targetConfig.TeamSettings.BacklogIterationPath={BacklogIterationPath}", targetConfig.TeamSettings.BacklogIterationPath);
                                targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths.Select(ip =>
                                {
                                    var result = nodeStructureEnricher.GetNewNodeName(ip, TfsNodeStructureType.Iteration);
                                    iterationMap[ip] = result;
                                    return result;
                                }).ToArray();
                                Log.LogDebug("targetConfig.TeamSettings.IterationPaths={@IterationPaths}", targetConfig.TeamSettings.IterationPaths);
                                targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues;
                                foreach (var item in targetConfig.TeamSettings.TeamFieldValues)
                                {
                                    item.Value = nodeStructureEnricher.GetNewNodeName(item.Value, TfsNodeStructureType.Area);
                                }
                                Log.LogDebug("targetConfig.TeamSettings.TeamFieldValues={@TeamFieldValues}", targetConfig.TeamSettings.TeamFieldValues);


                                TargetTeamSettings.SetTeamSettings(targetConfig.TeamId, targetConfig.TeamSettings);

                                Log.LogDebug("-> Team '{0}' settings... applied", targetConfig.TeamName);

                                MigrateCapacities(sourceTeam, newTeam, iterationMap);
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

        private void MigrateCapacities(
            TeamFoundationTeam sourceTeam,
            TeamFoundationTeam targetTeam,
            Dictionary<string, string> iterationMap)
        {
            if (!Options.MigrateTeamCapacities)
            {
                return;
            }

            TfsTeamSettingsCore.MigrateCapacities(
                _processor.Source.GetClient<WorkHttpClient>(), _processor.Source.WorkItems.Project.Guid, sourceTeam,
                _processor.Target.GetClient<WorkHttpClient>(), _processor.Target.WorkItems.Project.Guid, targetTeam,
                iterationMap, _targetTeamFoundationIdentitiesLazyCache,
                Telemetry, Log, exceptionLogLevel: LogLevel.Error);
        }
    }
}
