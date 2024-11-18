using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace MigrationTools.Processors
{
    internal static class TfsTeamSettingsCore
    {
        internal static void MigrateCapacities(
            WorkHttpClient sourceHttpClient,
            Guid sourceProjectId,
            TeamFoundationTeam sourceTeam,
            WorkHttpClient targetHttpClient,
            Guid targetProjectId,
            TeamFoundationTeam targetTeam,
            Dictionary<string, string> iterationMap,
            Lazy<List<TeamFoundationIdentity>> identityCache,
            ITelemetryLogger telemetry,
            ILogger log,
            LogLevel exceptionLogLevel)
        {
            log.LogInformation("Migrating team capacities..");

            try
            {
                var sourceTeamContext = new TeamContext(sourceProjectId, sourceTeam.Identity.TeamFoundationId);
                var sourceIterations = sourceHttpClient.GetTeamIterationsAsync(sourceTeamContext).ConfigureAwait(false).GetAwaiter().GetResult();

                var targetTeamContext = new TeamContext(targetProjectId, targetTeam.Identity.TeamFoundationId);
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
                            var targetTeamFoundatationIdentity = identityCache.Value.FirstOrDefault(i => i.DisplayName == sourceDisplayName);
                            if (targetTeamFoundatationIdentity == null)
                            {
                                if (sourceDisplayName.Contains(", "))
                                {
                                    // Match:
                                    //   "Doe, John" to "John Doe"
                                    var splitName = sourceDisplayName.Split(',');
                                    sourceDisplayName = $"{splitName[1].Trim()} {splitName[0].Trim()}";
                                    targetTeamFoundatationIdentity = identityCache.Value.FirstOrDefault(i => i.DisplayName == sourceDisplayName);
                                }
                                else
                                {
                                    if (sourceDisplayName.Contains(' '))
                                    {
                                        // Match:
                                        //   "John Doe" to "Doe, John"
                                        var splitName = sourceDisplayName.Split(' ');
                                        sourceDisplayName = $"{splitName[1].Trim()}, {splitName[0].Trim()}";
                                        targetTeamFoundatationIdentity = identityCache.Value.FirstOrDefault(i => i.DisplayName == sourceDisplayName);
                                    }
                                }

                                // last attempt to match on unique name
                                // Match: "John Michael Bolden" to Bolden, "John Michael" on "john.m.bolden@example.com" unique name
                                if (targetTeamFoundatationIdentity == null)
                                {
                                    var sourceUniqueName = sourceCapacity.TeamMember.UniqueName;
                                    targetTeamFoundatationIdentity = identityCache.Value.FirstOrDefault(i => i.UniqueName == sourceUniqueName);
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
                                log.LogWarning("[SKIP] Team Member {member} was not found on target when replacing capacities on iteration {iteration}.", sourceCapacity.TeamMember.DisplayName, targetIteration.Path);
                            }
                        }

                        if (targetCapacities.Count > 0)
                        {
                            targetHttpClient.ReplaceCapacitiesWithIdentityRefAsync(targetCapacities, targetTeamContext, targetIteration.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                            log.LogDebug("Team {team} capacities for iteration {iteration} migrated.", targetTeam.Name, targetIteration.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        telemetry.TrackException(ex, null);
                        log.Log(exceptionLogLevel, ex, "[SKIP] Problem migrating team capacities for iteration {iteration}.", sourceIteration.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                telemetry.TrackException(ex, null);
                log.Log(exceptionLogLevel, ex, "[SKIP] Problem migrating team capacities.");
            }

            log.LogInformation("Team capacities migration done..");
        }
    }
}
