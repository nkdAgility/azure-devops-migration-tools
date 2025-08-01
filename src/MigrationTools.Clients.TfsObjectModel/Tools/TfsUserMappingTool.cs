﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json;
using Riok.Mapperly.Abstractions;

namespace MigrationTools.Tools
{
    /// <summary>
    /// The TfsUserMappingTool is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
    /// </summary>
    public class TfsUserMappingTool : Tool<TfsUserMappingToolOptions>
    {
        new public TfsUserMappingToolOptions Options => (TfsUserMappingToolOptions)base.Options;

        public TfsUserMappingTool(
            IOptions<TfsUserMappingToolOptions> options,
            IServiceProvider services,
            ILogger<TfsUserMappingTool> logger,
            ITelemetryLogger telemetryLogger)
            : base(options, services, logger, telemetryLogger)
        {
            UserMappings = new Lazy<Dictionary<string, string>>(GetMappingFileData);
        }

        private readonly CaseInsensitiveStringComparer _workItemNameComparer = new();
        private readonly TfsUserMappingToolMapper _mapper = new();

        /// <summary>
        /// Serializes a user mapping dictionary to a JSON file for use in user mapping operations.
        /// </summary>
        /// <param name="fileName">The file path where the user mapping will be saved</param>
        /// <param name="userMap">The dictionary of user mappings to serialize</param>
        /// <param name="logger">Logger for the operation</param>
        public static void SerializeUserMap(string fileName, Dictionary<string, string> userMap, ILogger logger)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(userMap, Formatting.Indented));
            logger.LogInformation("User mappings writen to: {fileName}", fileName);
        }

        /// <summary>
        /// Deserializes a user mapping dictionary from a JSON file for use in user mapping operations.
        /// </summary>
        /// <param name="fileName">The file path where the user mapping is stored</param>
        /// <param name="logger">Logger for the operation</param>
        /// <returns>A dictionary containing the user mappings, or an empty dictionary if the file doesn't exist</returns>
        public static Dictionary<string, string> DeserializeUserMap(string fileName, ILogger logger)
        {
            try
            {
                string fileData = File.ReadAllText(fileName);
                var mapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileData);
                return new Dictionary<string, string>(mapping, StringComparer.CurrentCultureIgnoreCase);
            }
            catch (Exception)
            {
                logger.LogError($"TfsUserMappingTool::DeserializeUserMap User mapping could not be deserialized from file '{fileName}'.", fileName);
            }
            return [];
        }

        private HashSet<string> GetUsersFromWorkItems(List<WorkItemData> workitems, List<string> identityFieldsToCheck)
        {
            HashSet<string> foundUsers = new(StringComparer.CurrentCultureIgnoreCase);
            foreach (var wItem in workitems)
            {
                foreach (var rItem in wItem.Revisions.Values)
                {
                    foreach (var fItem in rItem.Fields.Values)
                    {
                        if (identityFieldsToCheck.Contains(fItem.ReferenceName, _workItemNameComparer))
                        {
                            if (!foundUsers.Contains(fItem.Value) && !string.IsNullOrEmpty((string)fItem.Value))
                            {
                                foundUsers.Add(fItem.Value.ToString());
                            }
                        }
                    }
                }
            }
            return foundUsers;
        }

        /// <summary>
        /// Maps a user identity field value using the configured user mappings if the field is configured for mapping.
        /// </summary>
        /// <param name="field">The work item field containing a user identity to be mapped</param>
        public void MapUserIdentityField(Field field)
        {
            if (Options.Enabled && Options.IdentityFieldsToCheck.Contains(field.ReferenceName))
            {
                Log.LogDebug($"TfsUserMappingTool::MapUserIdentityField [ReferenceName|{field.ReferenceName}]");
                if (UserMappings.Value.ContainsKey(field.Value.ToString()))
                {
                    var original = field.Value;
                    field.Value = UserMappings.Value[field.Value.ToString()];
                    Log.LogDebug($"TfsUserMappingTool::MapUserIdentityField::Map:[original|{original}][new|{field.Value}]");
                }
            }
        }

        public Lazy<Dictionary<string, string>> UserMappings { get; }

        private Dictionary<string, string> GetMappingFileData()
        {
            if (!File.Exists(Options.UserMappingFile))
            {
                Log.LogError("TfsUserMappingTool::GetMappingFileData:: The UserMappingFile '{UserMappingFile}' cant be found! Provide a valid file or disable TfsUserMappingTool!", Options.UserMappingFile);
                return [];
            }
            return DeserializeUserMap(Options.UserMappingFile, Log);
        }

        private List<IdentityItemData> GetUsersListFromServer(IGroupSecurityService gss)
        {
            Identity allIdentities = gss.ReadIdentity(SearchFactor.AccountName, Options.ProjectCollectionValidUsersGroupName, QueryMembership.Expanded);
            Log.LogInformation("TfsUserMappingTool::GetUsersListFromServer Found {count} identities (users and groups) in server.", allIdentities.Members.Length);

            List<IdentityItemData> foundUsers = new List<IdentityItemData>();
            foreach (string sid in allIdentities.Members)
            {
                Log.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Atempting to load user", sid);
                try
                {
                    Identity identity = gss.ReadIdentity(SearchFactor.Sid, sid, QueryMembership.Expanded);
                    if (identity is null)
                    {
                        Log.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] ReadIdentity returned null", sid);
                    }
                    else if ((identity.Type == IdentityType.WindowsUser) || (identity.Type == IdentityType.UnknownIdentityType))
                    {
                        // UnknownIdentityType is set for users in Azure Entra ID.
                        foundUsers.Add(_mapper.IdentityToIdentityItemData(identity));
                    }
                    else
                    {
                        Log.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Not applicable identity type {identityType}", sid, identity.Type);
                    }
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, null);
                    Log.LogWarning("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Failed With {Exception}", sid, ex.Message);
                }
            }
            foundUsers.Sort((x, y) => x.AccountName.CompareTo(y.AccountName));
            Log.LogInformation("TfsUserMappingTool::GetUsersListFromServer {count} user identities are applicable for mapping", foundUsers.Count);
            return foundUsers;
        }

        public IdentityMapResult GetUsersInSourceMappedToTarget(TfsProcessor processor)
        {
            Log.LogDebug("TfsUserMappingTool::GetUsersInSourceMappedToTarget");
            if (Options.Enabled)
            {
                Log.LogInformation($"TfsUserMappingTool::GetUsersInSourceMappedToTarget Loading identities from source server");
                var sourceUsers = GetUsersListFromServer(processor.Source.GetService<IGroupSecurityService>());
                Log.LogInformation($"TfsUserMappingTool::GetUsersInSourceMappedToTarget Loading identities from target server");
                var targetUsers = GetUsersListFromServer(processor.Target.GetService<IGroupSecurityService>());

                if (Options.MatchUsersByEmail)
                {
                    Log.LogInformation("TfsUserMappingTool::GetUsersInSourceMappedToTarget "
                        + "Matching users between source and target by email is enabled. In no match by email is found, "
                        + "matching by display name will be used.");
                }

                List<IdentityMapData> identityMap = [];
                foreach (var sourceUser in sourceUsers)
                {
                    IdentityItemData targetUser = null;
                    if (Options.MatchUsersByEmail && !string.IsNullOrEmpty(sourceUser.MailAddress))
                    {
                        var candidates = targetUsers
                            .Where(tu => tu.MailAddress.Equals(sourceUser.MailAddress, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        if (candidates.Count == 1)
                        {
                            // If there are more than one user with the same email address, we can't be sure which one is
                            // the correct one, so mapping will match either by display name, or will be skipped and
                            // exported for manual mapping.
                            targetUser = candidates[0];
                        }
                    }
                    targetUser ??= targetUsers.SingleOrDefault(x => x.DisplayName == sourceUser.DisplayName);
                    identityMap.Add(new IdentityMapData { Source = sourceUser, Target = targetUser });
                }
                return new()
                {
                    IdentityMap = identityMap,
                    SourceUsers = sourceUsers,
                    TargetUsers = targetUsers
                };
            }
            else
            {
                Log.LogWarning("TfsUserMappingTool is disabled in settings. You may have users in the source that are not mapped to the target. ");
                return new();
            }
        }

        public IdentityMapResult GetUsersInSourceMappedToTargetForWorkItems(TfsProcessor processor, List<WorkItemData> sourceWorkItems)
        {
            if (Options.Enabled)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                HashSet<string> workItemUsers = GetUsersFromWorkItems(sourceWorkItems, Options.IdentityFieldsToCheck);
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTargetForWorkItems [workItemUsers|{workItemUsers.Count}]");
                IdentityMapResult mappedUsers = GetUsersInSourceMappedToTarget(processor);
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTargetForWorkItems [mappedUsers|{mappedUsers.IdentityMap.Count}]");
                mappedUsers.IdentityMap = mappedUsers.IdentityMap.Where(x => workItemUsers.Contains(x.Source.DisplayName)).ToList();
                return mappedUsers;
            }
            else
            {
                Log.LogWarning("TfsUserMappingTool is disabled in settings. You may have users in the source that are not mapped to the target. ");
                return null;
            }
        }
    }

    internal class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x?.IndexOf(y, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    [Mapper]
    internal partial class TfsUserMappingToolMapper
    {
#pragma warning disable RMG020 // Source member is not mapped to any target member
        public partial IdentityItemData IdentityToIdentityItemData(Identity identity);
#pragma warning restore RMG020 // Source member is not mapped to any target member
    }
}
