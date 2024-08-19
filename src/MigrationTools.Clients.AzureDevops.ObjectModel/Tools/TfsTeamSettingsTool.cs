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

        public IMigrationEngine Engine { get; }

        private readonly Lazy<List<TeamFoundationIdentity>> _targetTeamFoundationIdentitiesLazyCache;

        public TfsTeamService SourceTeamService { get; }
        public TeamSettingsConfigurationService SourceTeamSettings { get; }
        public TfsTeamService TargetTeamService { get; }
        public TeamSettingsConfigurationService TargetTeamSettings { get; }

        public TfsTeamSettingsTool(IOptions<TfsTeamSettingsToolOptions> options, IServiceProvider services, ILogger<TfsTeamSettingsTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
            _targetTeamFoundationIdentitiesLazyCache = new Lazy<List<TeamFoundationIdentity>>(() =>
            {
                try
                {
                    var identityService = Engine.Target.GetService<IIdentityManagementService>();
                    var tfi = identityService.ReadIdentity(IdentitySearchFactor.General, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);
                    return identityService.ReadIdentities(tfi.Members, MembershipQuery.None, ReadIdentityOptions.None).ToList();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "{LogTypeName}: Unable load list of identities from target collection.", LogTypeName);
                    Telemetry.TrackException(ex, null, null);
                    return new List<TeamFoundationIdentity>();
                }
            });
            SourceTeamService = Engine.Source.GetService<TfsTeamService>();
            SourceTeamSettings = Engine.Source.GetService<TeamSettingsConfigurationService>();
            TargetTeamService = Engine.Target.GetService<TfsTeamService>();
            TargetTeamSettings = Engine.Target.GetService<TeamSettingsConfigurationService>();
        }


        public void ProcessorExecutionBegin(IProcessor processor) // Could be a IProcessorEnricher
        {
            if (Options.Enabled)
            {
                Log.LogInformation("----------------------------------------------");
                Log.LogInformation("Migrating all Teams before the Processor run.");
                MigrateTeamSettings();

            }
        }

        private void MigrateTeamSettings()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            List<TeamFoundationTeam> sourceTeams = SourceTeamService.QueryTeams(Engine.Source.Config.AsTeamProjectConfig().Project).ToList();
            Log.LogInformation("TfsTeamSettingsProcessor::InternalExecute: Found {0} teams in Source?", sourceTeams.Count);
            //////////////////////////////////////////////////
            List<TeamFoundationTeam> targetTeams = TargetTeamService.QueryTeams(Engine.Target.Config.AsTeamProjectConfig().Project).ToList();
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
                    TeamFoundationTeam newTeam = foundTargetTeam ?? TargetTeamService.CreateTeam(Engine.Target.WorkItems.Project.Url, sourceTeam.Name, sourceTeam.Description, null);
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

        private void MigrateCapacities(TeamFoundationTeam sourceTeam, TeamFoundationTeam targetTeam, Dictionary<string, string> iterationMap)
        {
            if (!Options.MigrateTeamCapacities) return;

            Log.LogInformation("Migrating team capacities..");

            WorkHttpClient sourceHttpClient = Engine.Source.GetClient<WorkHttpClient>();
            WorkHttpClient targetHttpClient = Engine.Target.GetClient<WorkHttpClient>();

            try
            {

                var sourceTeamContext = new TeamContext(Engine.Source.WorkItems.Project.Guid, sourceTeam.Identity.TeamFoundationId);
                var sourceIterations = sourceHttpClient.GetTeamIterationsAsync(sourceTeamContext).ConfigureAwait(false).GetAwaiter().GetResult();

                var targetTeamContext = new TeamContext(Engine.Target.WorkItems.Project.Guid, targetTeam.Identity.TeamFoundationId);
                var targetIterations = targetHttpClient.GetTeamIterationsAsync(targetTeamContext).ConfigureAwait(false).GetAwaiter().GetResult();

                foreach (var sourceIteration in sourceIterations)
                {
                    try
                    {
                        var targetIterationPath = iterationMap[sourceIteration.Path];
                        var targetIteration = targetIterations.FirstOrDefault(i => i.Path == targetIterationPath);
                        if (targetIteration == null) continue;

                        var targetCapacities = new List<TeamMemberCapacityIdentityRef>();
                        var sourceCapacities = sourceHttpClient.GetCapacitiesWithIdentityRefAsync(sourceTeamContext, sourceIteration.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                        foreach (var sourceCapacity in sourceCapacities)
                        {
                            var sourceDisplayName = sourceCapacity.TeamMember.DisplayName;
                            var index = sourceDisplayName.IndexOf("<");
                            if (index > 0)
                            {
                                sourceDisplayName = sourceDisplayName.Substring(0, index).Trim();
                            }

                            // Match:
                            //   "Doe, John" to "Doe, John"
                            //   "John Doe" to "John Doe"
                            var targetTeamFoundatationIdentity = _targetTeamFoundationIdentitiesLazyCache.Value.FirstOrDefault(i => i.DisplayName == sourceDisplayName);
                            if (targetTeamFoundatationIdentity == null)
                            {
                                if (sourceDisplayName.Contains(", "))
                                {
                                    // Match:
                                    //   "Doe, John" to "John Doe"
                                    var splitName = sourceDisplayName.Split(',');
                                    sourceDisplayName = $"{splitName[1].Trim()} {splitName[0].Trim()}";
                                    targetTeamFoundatationIdentity = _targetTeamFoundationIdentitiesLazyCache.Value.FirstOrDefault(i => i.DisplayName == sourceDisplayName);
                                }
                                else
                                {
                                    if (sourceDisplayName.Contains(' '))
                                    {
                                        // Match:
                                        //   "John Doe" to "Doe, John"
                                        var splitName = sourceDisplayName.Split(' ');
                                        sourceDisplayName = $"{splitName[1].Trim()}, {splitName[0].Trim()}";
                                        targetTeamFoundatationIdentity = _targetTeamFoundationIdentitiesLazyCache.Value.FirstOrDefault(i => i.DisplayName == sourceDisplayName);
                                    }
                                }

                                // last attempt to match on unique name
                                // Match: "John Michael Bolden" to Bolden, "John Michael" on "john.m.bolden@example.com" unique name
                                if (targetTeamFoundatationIdentity == null)
                                {
                                    var sourceUniqueName = sourceCapacity.TeamMember.UniqueName;
                                    targetTeamFoundatationIdentity = _targetTeamFoundationIdentitiesLazyCache.Value.FirstOrDefault(i => i.UniqueName == sourceUniqueName);
                                }
                            }

                            if (targetTeamFoundatationIdentity != null)
                            {
                                targetCapacities.Add(new TeamMemberCapacityIdentityRef
                                {
                                    Activities = sourceCapacity.Activities,
                                    DaysOff = sourceCapacity.DaysOff,
                                    TeamMember = new IdentityRef
                                    {
                                        Id = targetTeamFoundatationIdentity.TeamFoundationId.ToString()
                                    }
                                });
                            }
                            else
                            {
                                Log.LogWarning("[SKIP] Team Member {member} was not found on target when replacing capacities on iteration {iteration}.", sourceCapacity.TeamMember.DisplayName, targetIteration.Path);
                            }
                        }

                        if (targetCapacities.Count > 0)
                        {
                            targetHttpClient.ReplaceCapacitiesWithIdentityRefAsync(targetCapacities, targetTeamContext, targetIteration.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                            Log.LogDebug("Team {team} capacities for iteration {iteration} migrated.", targetTeam.Name, targetIteration.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, null, null);
                        Log.LogError(ex, "[SKIP] Problem migrating team capacities for iteration {iteration}.", sourceIteration.Path);
                    }

                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, null, null);
                Log.LogError(ex, "[SKIP] Problem migrating team capacities.");
            }

            Log.LogInformation("Team capacities migration done..");
        }

    }
}
