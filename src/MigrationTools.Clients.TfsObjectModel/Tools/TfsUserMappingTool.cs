using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Exceptions;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            IdentityMappings = new Lazy<List<IdentityMapData>>(GetIdentityMapDataFromFile);
            DetectedFormatIsIdentityMapData = new Lazy<bool>(DetectFormatFromFile);
        }

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

        /// <summary>
        /// Serializes a list of identity mappings to a JSON file for use in user mapping operations.
        /// </summary>
        /// <param name="fileName">The file path where the user identity mapping will be saved</param>
        /// <param name="identityMappings">The list of identity mappings to serialize</param>
        /// <param name="logger">Logger for the operation</param>
        public static void SerializeIdentityMapData(string fileName, List<IdentityMapData> identityMappings, ILogger logger)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(identityMappings, Formatting.Indented));
            logger.LogInformation("Identity mappings written to: {fileName}", fileName);
        }

        /// <summary>
        /// Deserializes a list of user identity mappings from a JSON array. Each entry's Source and Target can be either an
        /// identity object or a plain string, which is treated as the display name.
        /// </summary>
        /// <param name="fileName">The file path where the user identity mappings are stored</param>
        /// <param name="logger">Logger for the operation</param>
        /// <returns>A list of IdentityMapData containing source and target user mappings, or an empty list if the file doesn't exist</returns>
        public static List<IdentityMapData> DeserializeIdentityMapData(string fileName, ILogger logger)
        {
            JArray entries;
            try
            {
                entries = JArray.Parse(File.ReadAllText(fileName));
            }
            catch (Exception ex)
            {
                logger.LogError("TfsUserMappingTool::DeserializeIdentityMapData User identity mapping could not be deserialized from file '{fileName}'. Error: {error}", fileName, ex.Message);
                return [];
            }

            List<IdentityMapData> result = [];
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] is not JObject item)
                {
                    continue;
                }
                try
                {
                    result.Add(new IdentityMapData
                    {
                        Source = ToIdentityItemData(item.GetValue("Source", StringComparison.OrdinalIgnoreCase)),
                        Target = ToIdentityItemData(item.GetValue("Target", StringComparison.OrdinalIgnoreCase))
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning("TfsUserMappingTool::DeserializeIdentityMapData Skipping invalid entry {index} in '{fileName}'. Error: {error}", i, fileName, ex.Message);
                }
            }
            return result;
        }

        private static IdentityItemData ToIdentityItemData(JToken token)
        {
            switch (token?.Type)
            {
                case JTokenType.String:
                    return new IdentityItemData { DisplayName = token.Value<string>() };
                case JTokenType.Object:
                    var identity = token.ToObject<IdentityItemData>();
                    // Older guidance and exports use FriendlyName for the display name.
                    if (string.IsNullOrEmpty(identity.DisplayName))
                    {
                        identity.DisplayName = ((JObject)token).GetValue("FriendlyName", StringComparison.OrdinalIgnoreCase)?.Value<string>();
                    }
                    return identity;
                default:
                    return null;
            }
        }

        private static string GetTargetName(IdentityItemData target) =>
            !string.IsNullOrEmpty(target?.DisplayName) ? target.DisplayName
            : !string.IsNullOrEmpty(target?.AccountName) ? target.AccountName
            : null;

        /// <summary>
        /// Detects the format of a user mapping file (dictionary or IdentityMapData list).
        /// </summary>
        /// <param name="fileName">The file path to analyze</param>
        /// <param name="logger">Logger for the operation</param>
        /// <returns>True if the file contains IdentityMapData format (list with Source/Target), false if dictionary format</returns>
        public static bool IsIdentityMapDataFormat(string fileName, ILogger logger)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return false;
                }

                string fileData = File.ReadAllText(fileName);
                return JToken.Parse(fileData).Type == JTokenType.Array;
            }
            catch (Exception ex)
            {
                logger.LogWarning("TfsUserMappingTool::IsIdentityMapDataFormat Error detecting file format for '{fileName}': {error}. Defaulting to dictionary format.", fileName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Collects the distinct (case-insensitive) user values found in the given identity fields across all revisions.
        /// </summary>
        public static HashSet<string> GetUsersFromWorkItems(List<WorkItemData> workitems, IEnumerable<string> identityFieldsToCheck)
        {
            HashSet<string> foundUsers = new(StringComparer.CurrentCultureIgnoreCase);
            foreach (var wItem in workitems)
            {
                foreach (var rItem in wItem.Revisions.Values)
                {
                    foreach (var fItem in rItem.Fields.Values)
                    {
                        if (identityFieldsToCheck.Contains(fItem.ReferenceName, StringComparer.OrdinalIgnoreCase))
                        {
                            var user = fItem.Value?.ToString();
                            if (!string.IsNullOrEmpty(user))
                            {
                                foundUsers.Add(user);
                            }
                        }
                    }
                }
            }
            return foundUsers;
        }

        /// <summary>
        /// Maps a user identity field value using the configured user mappings if the field is configured for mapping.
        /// The file format is automatically detected based on the file content when UseIdentityMapDataFormat is enabled.
        /// </summary>
        /// <param name="field">The work item field containing a user identity to be mapped</param>
        public void MapUserIdentityField(Field field)
        {
            if (TryMapUserIdentityValue(field.ReferenceName, field.Value, out var mapped))
            {
                field.Value = mapped;
            }
        }

        /// <summary>
        /// Maps a user identity value and reports whether it changed. Only a changed value should be written back:
        /// rewriting an unchanged value can fail on fields the target rules make read-only (for example
        /// TF26194 on Activated Date when a revision reactivates a work item).
        /// </summary>
        /// <param name="referenceName">The field reference name</param>
        /// <param name="value">The current field value</param>
        /// <param name="mapped">The mapped value, or <paramref name="value"/> when no mapping applies</param>
        /// <returns><see langword="true"/> when the mapped value differs from <paramref name="value"/></returns>
        public bool TryMapUserIdentityValue(string referenceName, object value, out object mapped)
        {
            mapped = MapUserIdentityValue(referenceName, value);
            // Compare by value: Field.Value boxes value types (dates, numbers) afresh on every read.
            return !Equals(mapped, value);
        }

        /// <summary>
        /// Returns the mapped value for a user identity field without modifying any work item. If the field is not
        /// configured for mapping, the value is null or empty, or no mapping exists, the original value is returned.
        /// </summary>
        /// <param name="referenceName">The field reference name</param>
        /// <param name="value">The (source) field value</param>
        /// <returns>The mapped target value, or the original value when no mapping applies</returns>
        public object MapUserIdentityValue(string referenceName, object value)
        {
            if (!Options.Enabled || referenceName == null
                || !IdentityFieldsToCheck.Contains(referenceName, StringComparer.OrdinalIgnoreCase))
            {
                return value;
            }
            var sourceValue = value?.ToString();
            if (string.IsNullOrEmpty(sourceValue))
            {
                return value;
            }

            Log.LogDebug($"TfsUserMappingTool::MapUserIdentityValue [ReferenceName|{referenceName}]");
            string target = DetectedFormatIsIdentityMapData.Value
                ? FindIdentityMapTarget(IdentityMappings.Value, sourceValue)
                : TfsUserIdentityReference.FindMappedTarget(UserMappings.Value, sourceValue);
            if (target == null)
            {
                if (!DetectedFormatIsIdentityMapData.Value && TfsUserIdentityReference.IsAmbiguous(UserMappings.Value, sourceValue))
                {
                    Log.LogWarning("TfsUserMappingTool::MapUserIdentityValue '{sourceValue}' matches more than one 'Display Name <upn>' entry in the mapping file and was not mapped.", sourceValue);
                }
                return value;
            }
            Log.LogDebug($"TfsUserMappingTool::MapUserIdentityValue::Map:[original|{sourceValue}][new|{target}]");
            return target;
        }

        /// <summary>
        /// Finds the target identity for a source display name or account name, ignoring case.
        /// </summary>
        /// <param name="identityMapData">The list of identity map data to search</param>
        /// <param name="sourceValue">The source display name or account name</param>
        /// <returns>The target display name (or account name when no display name exists), or null if no mapping is found</returns>
        public static string FindIdentityMapTarget(IEnumerable<IdentityMapData> identityMapData, string sourceValue)
        {
            if (identityMapData == null || string.IsNullOrEmpty(sourceValue))
            {
                return null;
            }

            return identityMapData
                .Where(x => x?.Source != null &&
                    (string.Equals(x.Source.DisplayName, sourceValue, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(x.Source.AccountName, sourceValue, StringComparison.OrdinalIgnoreCase)))
                .Select(x => GetTargetName(x.Target))
                .FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Converts detailed identity mappings to the simple dictionary used by existing consumers such as
        /// team capacity migration. Both the source display name and account name are added as keys.
        /// </summary>
        /// <param name="identityMapData">The list of identity map data to convert</param>
        /// <returns>A case-insensitive dictionary of source names to target names</returns>
        public static Dictionary<string, string> ToUserMapDictionary(IEnumerable<IdentityMapData> identityMapData)
        {
            Dictionary<string, string> result = new(StringComparer.CurrentCultureIgnoreCase);
            foreach (var mapping in identityMapData ?? [])
            {
                var target = GetTargetName(mapping?.Target);
                if (mapping?.Source == null || target == null)
                {
                    continue;
                }
                foreach (var key in new[] { mapping.Source.DisplayName, mapping.Source.AccountName })
                {
                    if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key))
                    {
                        result[key] = target;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The identity fields to map: the configured IdentityFieldsToCheck, or
        /// <see cref="TfsUserMappingToolOptions.DefaultIdentityFieldsToCheck"/> when none are configured. The defaults live in
        /// code rather than appsettings.json because configuration providers merge arrays by index, so a shorter configured
        /// list would otherwise keep the remaining default entries.
        /// </summary>
        public IReadOnlyList<string> IdentityFieldsToCheck =>
            Options.IdentityFieldsToCheck is { Count: > 0 } configured ? configured : TfsUserMappingToolOptions.DefaultIdentityFieldsToCheck;

        /// <summary>
        /// Checks that each target value in the user mapping resolves to exactly one identity in <paramref name="targetUsers"/>.
        /// A plain value matches a display name, account name, email address or <c>domain\account</c>; a
        /// <c>Display Name &lt;unique&gt;</c> value matches on the unique part alone.
        /// </summary>
        /// <returns>One entry for each mapping whose target does not resolve or is ambiguous.</returns>
        public static List<UnresolvedUserMapping> FindUnresolvedMappingTargets(IDictionary<string, string> userMap, IEnumerable<IdentityItemData> targetUsers)
        {
            var users = (targetUsers ?? []).Where(u => u != null).ToList();
            List<UnresolvedUserMapping> result = [];
            foreach (var mapping in userMap ?? new Dictionary<string, string>())
            {
                var target = mapping.Value?.Trim();
                if (string.IsNullOrEmpty(target))
                {
                    continue;
                }
                List<IdentityItemData> candidates;
                if (TfsUserIdentityReference.TryParse(target, out var displayName, out var unique))
                {
                    candidates = users.Where(u => MatchesUnique(u, unique)).ToList();
                    if (candidates.Count == 0)
                    {
                        // Show users with the same display name to help correct the unique part.
                        var sameName = users.Where(u => string.Equals(u.DisplayName, displayName, StringComparison.OrdinalIgnoreCase)).ToList();
                        result.Add(new UnresolvedUserMapping(mapping.Key, mapping.Value, false, sameName));
                        continue;
                    }
                }
                else
                {
                    candidates = users.Where(u => string.Equals(u.DisplayName, target, StringComparison.OrdinalIgnoreCase) || MatchesUnique(u, target)).ToList();
                }
                if (candidates.Count != 1)
                {
                    result.Add(new UnresolvedUserMapping(mapping.Key, mapping.Value, candidates.Count > 1, candidates));
                }
            }
            return result;
        }

        private static bool MatchesUnique(IdentityItemData user, string unique) =>
            (!string.IsNullOrEmpty(user.AccountName) && string.Equals(user.AccountName, unique, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(user.MailAddress) && string.Equals(user.MailAddress, unique, StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(user.AccountName) && !string.IsNullOrEmpty(user.Domain)
                && string.Equals($"{user.Domain}\\{user.AccountName}", unique, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// The unambiguous <c>Display Name &lt;domain\account&gt;</c> form for an identity, or the
        /// <see cref="TfsUserIdentityReference.Format"/> form when the domain is unknown.
        /// </summary>
        public static string FormatUnambiguous(IdentityItemData user) =>
            !string.IsNullOrEmpty(user?.Domain) && !string.IsNullOrEmpty(user.AccountName)
                ? $"{user.DisplayName} <{user.Domain}\\{user.AccountName}>"
                : TfsUserIdentityReference.Format(user);

        /// <summary>
        /// Startup validation: resolves every target value in the UserMappingFile against the target identities and reports each
        /// one that does not resolve or is ambiguous. Azure DevOps would otherwise save such a value as plain text with no identity.
        /// </summary>
        /// <param name="processor">The processor whose target is checked.</param>
        /// <param name="targetUsers">The target identities, when already loaded; otherwise they are read from the target.</param>
        /// <exception cref="MigrationRunStoppedException">Thrown when a target does not resolve and StopIfMappingTargetsDoNotResolve is true.</exception>
        public void ValidateMappingTargets(TfsProcessor processor, List<IdentityItemData> targetUsers = null)
        {
            if (!Options.Enabled)
            {
                return;
            }
            if (string.IsNullOrEmpty(Options.UserMappingFile) || !File.Exists(Options.UserMappingFile))
            {
                Log.LogWarning("TfsUserMappingTool::ValidateMappingTargets The UserMappingFile '{UserMappingFile}' cant be found, so no users will be mapped.", Options.UserMappingFile);
                return;
            }
            if (UserMappings.Value.Count == 0)
            {
                return;
            }
            Log.LogInformation("Validating::Check that every target in the user mapping file resolves to one identity in the target");
            if (targetUsers == null || targetUsers.Count == 0)
            {
                targetUsers = GetUsersListFromServer(processor.Target.GetService<IGroupSecurityService>());
            }
            ReportUnresolvedMappingTargets(FindUnresolvedMappingTargets(UserMappings.Value, targetUsers));
        }

        internal void ReportUnresolvedMappingTargets(List<UnresolvedUserMapping> unresolved)
        {
            if (unresolved.Count == 0)
            {
                Log.LogInformation("Validating::All user mapping targets resolve to an identity in the target.");
                return;
            }
            foreach (var item in unresolved)
            {
                var candidates = item.Candidates.Count == 0 ? "none" : string.Join("; ", item.Candidates.Select(FormatUnambiguous));
                Log.LogWarning("TfsUserMappingTool: the mapping '{source}' -> '{target}' {problem} in the target, so it would be saved as plain text. Candidate identities: {candidates}",
                    item.Source, item.Target, item.IsAmbiguous ? "matches more than one identity" : "does not match any identity", candidates);
            }
            if (Options.StopIfMappingTargetsDoNotResolve)
            {
                Log.LogError("Validating Failed! {count} user mapping target(s) do not resolve to exactly one identity in the target. Correct the UserMappingFile, "
                    + "using the unambiguous 'Display Name <domain\\account>' form shown in the candidates, or set StopIfMappingTargetsDoNotResolve to false to continue.", unresolved.Count);
                throw new MigrationRunStoppedException($"{unresolved.Count} user mapping target(s) do not resolve to an identity in the target.");
            }
            Log.LogWarning("Validating Failed! {count} user mapping target(s) do not resolve to exactly one identity in the target and will be saved as plain text. "
                + "StopIfMappingTargetsDoNotResolve is false, so the migration continues.", unresolved.Count);
        }

        public Lazy<Dictionary<string, string>> UserMappings { get; }

        public Lazy<List<IdentityMapData>> IdentityMappings { get; }

        public Lazy<bool> DetectedFormatIsIdentityMapData { get; }

        private Dictionary<string, string> GetMappingFileData()
        {
            if (!File.Exists(Options.UserMappingFile))
            {
                Log.LogError("TfsUserMappingTool::GetMappingFileData:: The UserMappingFile '{UserMappingFile}' cant be found! Provide a valid file or disable TfsUserMappingTool!", Options.UserMappingFile);
                return [];
            }
            if (DetectedFormatIsIdentityMapData.Value)
            {
                return ToUserMapDictionary(IdentityMappings.Value);
            }
            return DeserializeUserMap(Options.UserMappingFile, Log);
        }

        private List<IdentityMapData> GetIdentityMapDataFromFile()
        {
            if (!Options.UseIdentityMapDataFormat)
            {
                Log.LogDebug("TfsUserMappingTool::GetIdentityMapDataFromFile:: UseIdentityMapDataFormat is disabled, skipping IdentityMapData loading");
                return [];
            }

            if (!File.Exists(Options.UserMappingFile))
            {
                Log.LogError("TfsUserMappingTool::GetIdentityMapDataFromFile:: The UserMappingFile '{UserMappingFile}' cant be found!", Options.UserMappingFile);
                return [];
            }

            return DeserializeIdentityMapData(Options.UserMappingFile, Log);
        }

        private bool DetectFormatFromFile()
        {
            if (!Options.UseIdentityMapDataFormat)
            {
                Log.LogDebug("TfsUserMappingTool::DetectFormatFromFile:: UseIdentityMapDataFormat is disabled, format detection skipped");
                return false;
            }

            bool isIdentityMapDataFormat = IsIdentityMapDataFormat(Options.UserMappingFile, Log);
            Log.LogInformation("TfsUserMappingTool::DetectFormatFromFile:: Detected format is {Format}", isIdentityMapDataFormat ? "IdentityMapData" : "Dictionary");
            return isIdentityMapDataFormat;
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

        /// <summary>
        /// Matches each source user to a target user, by unique email address first (when enabled) and then by display name.
        /// Email matching tolerates a domain change through <paramref name="emailDomainMappings"/> and <paramref name="matchByEmailLocalPart"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when several target users share the source user's display name.</exception>
        public static List<IdentityMapData> MatchSourceUsersToTarget(List<IdentityItemData> sourceUsers, List<IdentityItemData> targetUsers, bool matchUsersByEmail, ILogger log,
            IDictionary<string, string> emailDomainMappings = null, bool matchByEmailLocalPart = false)
        {
            List<IdentityMapData> identityMap = [];
            foreach (var sourceUser in sourceUsers)
            {
                IdentityItemData targetUser = null;
                if (matchUsersByEmail && !string.IsNullOrEmpty(sourceUser.MailAddress))
                {
                    targetUser = TfsUserEmailMatcher.FindTarget(sourceUser, targetUsers, emailDomainMappings, matchByEmailLocalPart);
                }
                try
                {
                    targetUser ??= targetUsers.SingleOrDefault(x => x.DisplayName == sourceUser.DisplayName);
                }
                catch (InvalidOperationException)
                {
                    log.LogError("TfsUserMappingTool::GetUsersInSourceMappedToTarget:: Multiple target users found with the same display name '{displayName}'. "
                        + "Consider enabling MatchUsersByEmail option to avoid this issue. ",
                        sourceUser.DisplayName, sourceUser.AccountName);
                    var matchingUsers = targetUsers.Where(x => x.DisplayName == sourceUser.DisplayName).ToList();
                    log.LogWarning("TfsUserMappingTool::GetUsersInSourceMappedToTarget:: The list of matching users is {matchingUsers}",
                        string.Join(", ", matchingUsers.Select(x => $"{x.DisplayName} [{x.MailAddress}]")));
                    throw;
                }

                identityMap.Add(new IdentityMapData { Source = sourceUser, Target = targetUser });
            }
            return identityMap;
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

                List<IdentityMapData> identityMap = MatchSourceUsersToTarget(sourceUsers, targetUsers, Options.MatchUsersByEmail, Log,
                    Options.EmailDomainMappings, Options.MatchUsersByEmailLocalPart);
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
                HashSet<string> workItemUsers = GetUsersFromWorkItems(sourceWorkItems, IdentityFieldsToCheck);
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTargetForWorkItems [workItemUsers|{workItemUsers.Count}]");
                IdentityMapResult mappedUsers = GetUsersInSourceMappedToTarget(processor);
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTargetForWorkItems [mappedUsers|{mappedUsers.IdentityMap.Count}]");
                mappedUsers.IdentityMap = mappedUsers.IdentityMap.Where(x => workItemUsers.Contains(x.Source.DisplayName)
                    || (!string.IsNullOrEmpty(x.Source.AccountName) && workItemUsers.Contains(x.Source.AccountName))
                    || workItemUsers.Contains(TfsUserIdentityReference.Format(x.Source))).ToList();
                return mappedUsers;
            }
            else
            {
                Log.LogWarning("TfsUserMappingTool is disabled in settings. You may have users in the source that are not mapped to the target. ");
                return null;
            }
        }
    }

    /// <summary>
    /// A user mapping whose target does not resolve to exactly one identity in the target.
    /// </summary>
    public class UnresolvedUserMapping(string source, string target, bool isAmbiguous, IReadOnlyList<IdentityItemData> candidates)
    {
        /// <summary>The source name in the mapping file.</summary>
        public string Source { get; } = source;

        /// <summary>The mapped target value.</summary>
        public string Target { get; } = target;

        /// <summary>True when the target matches more than one identity; false when it matches none.</summary>
        public bool IsAmbiguous { get; } = isAmbiguous;

        /// <summary>The matching identities, or for an unmatched <c>Display Name &lt;unique&gt;</c> value those with the same display name.</summary>
        public IReadOnlyList<IdentityItemData> Candidates { get; } = candidates ?? [];
    }

    [Mapper]
    internal partial class TfsUserMappingToolMapper
    {
#pragma warning disable RMG020 // Source member is not mapped to any target member
        public partial IdentityItemData IdentityToIdentityItemData(Identity identity);
#pragma warning restore RMG020 // Source member is not mapped to any target member
    }
}
