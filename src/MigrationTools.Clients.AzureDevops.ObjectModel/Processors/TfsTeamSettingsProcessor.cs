using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Native TFS Processor, does not work with any other Endpoints.
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Teams</processingtarget>
    public class TfsTeamSettingsProcessor : Processor
    {
        private const string LogTypeName = nameof(TfsTeamSettingsProcessor);
        private readonly Lazy<List<TeamFoundationIdentity>> _targetTeamFoundationIdentitiesLazyCache;

        private TfsTeamSettingsProcessorOptions _Options;

        public TfsTeamSettingsProcessor(ProcessorEnricherContainer processorEnrichers,
                                        IEndpointFactory endpointFactory,
                                        IServiceProvider services,
                                        ITelemetryLogger telemetry,
                                        ILogger<Processor> logger)
            : base(processorEnrichers, endpointFactory, services, telemetry, logger)
        {
            _targetTeamFoundationIdentitiesLazyCache = new Lazy<List<TeamFoundationIdentity>>(() =>
            {
                try
                {
                    TfsTeamService teamService = Target.TfsTeamService;

                    var identityService = Target.TfsCollection.GetService<IIdentityManagementService>();
                    var tfi = identityService.ReadIdentity(IdentitySearchFactor.General, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);
                    return identityService.ReadIdentities(tfi.Members, MembershipQuery.None, ReadIdentityOptions.None).ToList();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "{LogTypeName}: Unable load list of identities from target collection.", LogTypeName);
                    return new List<TeamFoundationIdentity>();
                }
            });
        }

        public new TfsTeamSettingsEndpoint Source => (TfsTeamSettingsEndpoint)base.Source;

        public new TfsTeamSettingsEndpoint Target => (TfsTeamSettingsEndpoint)base.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsTeamSettingsProcessor::Configure");
            _Options = (TfsTeamSettingsProcessorOptions)options;
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            MigrateTeamSettings();
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }



        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (Source is not TfsTeamSettingsEndpoint)
            {
                throw new Exception("The Source endpoint configured must be of type TfsTeamSettingsEndpoint");
            }
            if (Target is not TfsTeamSettingsEndpoint)
            {
                throw new Exception("The Target endpoint configured must be of type TfsTeamSettingsEndpoint");
            }
        }
        private void MigrateTeamSettings()
        {
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

            /////////
            if (_Options.Teams != null)
            {
                sourceTeams = sourceTeams.Where(t => _Options.Teams.Contains(t.Name)).ToList();
            }

            var sourceHttpClient = Source.WorkHttpClient;
            var targetHttpClient = Target.WorkHttpClient;

            // Create teams
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
                        // Duplicate settings
                        Log.LogDebug("-> Processing team '{0}' settings:", sourceTeam.Name);
                        var sourceConfigurations = Source.TfsTeamSettingsService.GetTeamConfigurations(new List<Guid> { sourceTeam.Identity.TeamFoundationId });
                        var targetConfigurations = Target.TfsTeamSettingsService.GetTeamConfigurations(new List<Guid> { newTeam.Identity.TeamFoundationId });

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
                                if (_Options.PrefixProjectToNodes)
                                {
                                    targetConfig.TeamSettings.BacklogIterationPath =
                                        string.Format("{0}\\{1}", Target.Project, sourceConfig.TeamSettings.BacklogIterationPath);
                                    targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths
                                        .Select(path =>
                                        {
                                            var result = string.Format("{0}\\{1}", Target.Project, path);
                                            iterationMap[path] = result;
                                            return result;
                                        })
                                        .ToArray();
                                    targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues
                                        .Select(field => new Microsoft.TeamFoundation.ProcessConfiguration.Client.TeamFieldValue
                                        {
                                            IncludeChildren = field.IncludeChildren,
                                            Value = string.Format("{0}\\{1}", Target.Project, field.Value)
                                        })
                                        .ToArray();
                                }
                                else
                                {
                                    targetConfig.TeamSettings.BacklogIterationPath = SwitchProjectName(sourceConfig.TeamSettings.BacklogIterationPath, Source.Project, Target.Project);
                                    Log.LogDebug("targetConfig.TeamSettings.BacklogIterationPath={BacklogIterationPath}", targetConfig.TeamSettings.BacklogIterationPath);
                                    targetConfig.TeamSettings.IterationPaths = sourceConfig.TeamSettings.IterationPaths.Select(ip =>
                                    {
                                        var result = SwitchProjectName(ip, Source.Project, Target.Project);
                                        iterationMap[ip] = result;
                                        return result;
                                    }).ToArray();
                                    Log.LogDebug("targetConfig.TeamSettings.IterationPaths={@IterationPaths}", targetConfig.TeamSettings.IterationPaths);
                                    targetConfig.TeamSettings.TeamFieldValues = sourceConfig.TeamSettings.TeamFieldValues;
                                    foreach (var item in targetConfig.TeamSettings.TeamFieldValues)
                                    {
                                        item.Value = SwitchProjectName(item.Value, Source.Project, Target.Project);
                                    }
                                    Log.LogDebug("targetConfig.TeamSettings.TeamFieldValues={@TeamFieldValues}", targetConfig.TeamSettings.TeamFieldValues);
                                }

                                Target.TfsTeamSettingsService.SetTeamSettings(targetConfig.TeamId, targetConfig.TeamSettings);

                                Log.LogDebug("-> Team '{0}' settings... applied", targetConfig.TeamName);

                                MigrateCapacities(sourceHttpClient, targetHttpClient, sourceTeam, newTeam, iterationMap);
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

        internal static string SwitchProjectName(string expressionString, string sourceProjectName, string targetProjectName)
        {
            if (expressionString == sourceProjectName)
            {
                return targetProjectName;
            }

            var slashIndex = expressionString.IndexOf('\\');
            if (slashIndex > 0)
            {
                var subValue = expressionString.Substring(0, slashIndex);
                if (subValue == sourceProjectName)
                {
                    return targetProjectName + expressionString.Substring(slashIndex);
                }
            }

            return string.Empty;
        }

        private void MigrateCapacities(WorkHttpClient sourceHttpClient, WorkHttpClient targetHttpClient, TeamFoundationTeam sourceTeam, TeamFoundationTeam targetTeam, Dictionary<string, string> iterationMap)
        {
            if (!_Options.MigrateTeamCapacities) return;

            Log.LogInformation("Migrating team capacities..");
            try
            {
                var sourceTeamContext = new TeamContext(Source.TfsProject.Guid, sourceTeam.Identity.TeamFoundationId);
                var sourceIterations = sourceHttpClient.GetTeamIterationsAsync(sourceTeamContext).ConfigureAwait(false).GetAwaiter().GetResult();

                var targetTeamContext = new TeamContext(Target.TfsProject.Guid, targetTeam.Identity.TeamFoundationId);
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
                        Log.LogError(ex, "[SKIP] Problem migrating team capacities for iteration {iteration}.", sourceIteration.Path);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "[SKIP] Problem migrating team capacities.");
            }

            Log.LogInformation("Team capacities migration done..");
        }
    }
}
