using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.Tools;

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
            bool useUserMapping,
            ITelemetryLogger telemetry,
            ILogger log,
            LogLevel exceptionLogLevel,
            IServiceProvider services)
        {
            log.LogInformation("Migrating team capacities..");

            try
            {
                var sourceTeamContext = new TeamContext(sourceProjectId, sourceTeam.Identity.TeamFoundationId);
                var sourceIterations = sourceHttpClient.GetTeamIterationsAsync(sourceTeamContext)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                var targetTeamContext = new TeamContext(targetProjectId, targetTeam.Identity.TeamFoundationId);
                var targetIterations = targetHttpClient.GetTeamIterationsAsync(targetTeamContext)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                foreach (var sourceIteration in sourceIterations)
                {
                    try
                    {
                        var targetIterationPath = iterationMap[sourceIteration.Path];
                        var targetIteration = targetIterations.FirstOrDefault(i => i.Path == targetIterationPath);
                        if (targetIteration == null) continue;

                        List<TeamMemberCapacityIdentityRef> sourceCapacities = GetSourceCapacities(
                            sourceHttpClient, sourceTeamContext, sourceIteration);
                        List<TeamMemberCapacityIdentityRef> targetCapacities = GetTargetCapacities(
                            sourceCapacities, targetIteration.Path, useUserMapping, identityCache, log, services);

                        if (targetCapacities.Count > 0)
                        {
                            ReplaceTargetCapacities(targetHttpClient, targetTeamContext, targetTeam, targetIteration, targetCapacities, log);
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

        private static List<TeamMemberCapacityIdentityRef> GetSourceCapacities(
            WorkHttpClient sourceHttpClient,
            TeamContext sourceTeamContext,
            TeamSettingsIteration sourceIteration)
        {
            return sourceHttpClient.GetCapacitiesWithIdentityRefAsync(sourceTeamContext, sourceIteration.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static List<TeamMemberCapacityIdentityRef> GetTargetCapacities(
            List<TeamMemberCapacityIdentityRef> sourceCapacities,
            string targetIteration,
            bool useUserMapping,
            Lazy<List<TeamFoundationIdentity>> identityCache,
            ILogger log,
            IServiceProvider services)
        {
            List<TeamMemberCapacityIdentityRef> targetCapacities = [];
            Dictionary<string, string> userMapping = null;
            if (useUserMapping)
            {
                TfsCommonTools commonTools = services.GetRequiredService<TfsCommonTools>();
                userMapping = commonTools.UserMapping.UserMappings.Value;
            }

            foreach (var sourceCapacity in sourceCapacities)
            {
                if (TryMatchIdentity(sourceCapacity, identityCache, userMapping, out TeamFoundationIdentity targetIdentity))
                {
                    targetCapacities.Add(new TeamMemberCapacityIdentityRef
                    {
                        Activities = sourceCapacity.Activities,
                        DaysOff = sourceCapacity.DaysOff,
                        TeamMember = new IdentityRef
                        {
                            Id = targetIdentity.TeamFoundationId.ToString()
                        }
                    });
                }
                else
                {
                    log.LogWarning("[SKIP] Team Member {member} was not found on target when replacing capacities "
                        + "on iteration {iteration}.", sourceCapacity.TeamMember.DisplayName, targetIteration);
                }
            }

            return targetCapacities;
        }

        private static void ReplaceTargetCapacities(
            WorkHttpClient targetHttpClient,
            TeamContext targetTeamContext,
            TeamFoundationTeam targetTeam,
            TeamSettingsIteration targetIteration,
            List<TeamMemberCapacityIdentityRef> targetCapacities,
            ILogger log)
        {
            targetHttpClient.ReplaceCapacitiesWithIdentityRefAsync(targetCapacities, targetTeamContext, targetIteration.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            log.LogDebug("Team {team} capacities for iteration {iteration} migrated.", targetTeam.Name, targetIteration.Path);
        }

        private static bool TryMatchIdentity(
            TeamMemberCapacityIdentityRef sourceCapacity,
            Lazy<List<TeamFoundationIdentity>> identityCache,
            Dictionary<string, string> userMapping,
            out TeamFoundationIdentity targetIdentity)
        {
            var sourceName = sourceCapacity.TeamMember.DisplayName;
            var index = sourceName.IndexOf("<");
            if (index > 0)
            {
                sourceName = sourceName.Substring(0, index).Trim();
            }

            targetIdentity = MatchIdentity(sourceName, identityCache);
            if ((targetIdentity is null) && (userMapping is not null))
            {
                if (userMapping.TryGetValue(sourceName, out var mappedName) && !string.IsNullOrEmpty(mappedName))
                {
                    targetIdentity = MatchIdentity(mappedName, identityCache);
                }
            }

            // last attempt to match on unique name
            // Match: "John Michael Bolden" to Bolden, "John Michael" on "john.m.bolden@example.com" unique name
            if (targetIdentity is null)
            {
                var sourceUniqueName = sourceCapacity.TeamMember.UniqueName;
                targetIdentity = identityCache.Value.FirstOrDefault(i => i.UniqueName == sourceUniqueName);
            }

            return targetIdentity is not null;
        }

        private static TeamFoundationIdentity MatchIdentity(string sourceName, Lazy<List<TeamFoundationIdentity>> identityCache)
        {
            // Match:
            //   "Doe, John" to "Doe, John"
            //   "John Doe" to "John Doe"
            TeamFoundationIdentity targetIdentity = identityCache.Value.FirstOrDefault(i => i.DisplayName == sourceName);
            if (targetIdentity == null)
            {
                if (sourceName.Contains(", "))
                {
                    // Match:
                    //   "Doe, John" to "John Doe"
                    var splitName = sourceName.Split(',');
                    sourceName = $"{splitName[1].Trim()} {splitName[0].Trim()}";
                    targetIdentity = identityCache.Value.FirstOrDefault(i => i.DisplayName == sourceName);
                }
                else
                {
                    if (sourceName.Contains(' '))
                    {
                        // Match:
                        //   "John Doe" to "Doe, John"
                        var splitName = sourceName.Split(' ');
                        sourceName = $"{splitName[1].Trim()}, {splitName[0].Trim()}";
                        targetIdentity = identityCache.Value.FirstOrDefault(i => i.DisplayName == sourceName);
                    }
                }
            }

            return targetIdentity;
        }
    }
}

