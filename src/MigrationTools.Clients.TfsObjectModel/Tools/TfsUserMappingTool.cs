using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// The TfsUserMappingTool is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
    /// </summary>
    public class TfsUserMappingTool : Tool<TfsUserMappingToolOptions>
    {
        new public TfsUserMappingToolOptions Options => (TfsUserMappingToolOptions)base.Options;

        public TfsUserMappingTool(IOptions<TfsUserMappingToolOptions> options, IServiceProvider services, ILogger<TfsUserMappingTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {
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
                        if (identityFieldsToCheck.Contains(fItem.ReferenceName, new CaseInsensativeStringComparer()))
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

        public void MapUserIdentityField(Field field)
        {
            if (Options.Enabled && Options.IdentityFieldsToCheck.Contains(field.ReferenceName))
            {
                Log.LogDebug($"TfsUserMappingTool::MapUserIdentityField [ReferenceName|{field.ReferenceName}]");
                var mapps = GetMappingFileData();
                if (mapps != null && mapps.ContainsKey(field.Value.ToString()))
                {
                    var original = field.Value;
                    field.Value = mapps[field.Value.ToString()];
                    Log.LogDebug($"TfsUserMappingTool::MapUserIdentityField::Map:[original|{original}][new|{field.Value}]");
                }
            }
        }

        private Dictionary<string, string> _UserMappings = null;

        private Dictionary<string, string> GetMappingFileData()
        {
            if (!System.IO.File.Exists(Options.UserMappingFile))
            {
                Log.LogError("TfsUserMappingTool::GetMappingFileData:: The UserMappingFile '{UserMappingFile}' cant be found! Provide a valid file or disable TfsUserMappingTool!", Options.UserMappingFile);
                _UserMappings = new Dictionary<string, string>();
            }
            if (_UserMappings == null)
            {
                var fileData = System.IO.File.ReadAllText(Options.UserMappingFile);
                try
                {
                    var fileMaps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IdentityMapData>>(fileData);
                    _UserMappings = fileMaps.ToDictionary(x => x.Source.FriendlyName, x => x.Target?.FriendlyName);
                }
                catch (Exception)
                {
                    _UserMappings = new Dictionary<string, string>();
                    Log.LogError($"TfsUserMappingTool::GetMappingFileData [UserMappingFile|{Options.UserMappingFile}] <-- invalid - No mapping are applied!");
                }
            }
            return _UserMappings;
        }

        private List<IdentityItemData> GetUsersListFromServer(IGroupSecurityService gss)
        {
            Identity allIdentities = gss.ReadIdentity(SearchFactor.AccountName, "Project Collection Valid Users", QueryMembership.Expanded);
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
                        foundUsers.Add(new IdentityItemData()
                        {
                            FriendlyName = identity.DisplayName,
                            AccountName = identity.AccountName
                        });
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
            Log.LogInformation("TfsUserMappingTool::GetUsersListFromServer {count} user identities are applicable for mapping", foundUsers.Count);
            return foundUsers;
        }

        public List<IdentityMapData> GetUsersInSourceMappedToTarget(TfsProcessor processor)
        {
            Log.LogDebug("TfsUserMappingTool::GetUsersInSourceMappedToTarget");
            if (Options.Enabled)
            {
                Log.LogInformation($"TfsUserMappingTool::GetUsersInSourceMappedToTarget Loading identities from source server");
                var sourceUsers = GetUsersListFromServer(processor.Source.GetService<IGroupSecurityService>());
                Log.LogInformation($"TfsUserMappingTool::GetUsersInSourceMappedToTarget Loading identities from target server");
                var targetUsers = GetUsersListFromServer(processor.Target.GetService<IGroupSecurityService>());
                return sourceUsers.Select(sUser => new IdentityMapData { Source = sUser, Target = targetUsers.SingleOrDefault(tUser => tUser.FriendlyName == sUser.FriendlyName) }).ToList();
            }
            else
            {
                Log.LogWarning("TfsUserMappingTool is disabled in settings. You may have users in the source that are not mapped to the target. ");
                return null;
            }
        }

        public List<IdentityMapData> GetUsersInSourceMappedToTargetForWorkItems(TfsProcessor processor, List<WorkItemData> sourceWorkItems)
        {
            if (Options.Enabled)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                HashSet<string> workItemUsers = GetUsersFromWorkItems(sourceWorkItems, Options.IdentityFieldsToCheck);
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTargetForWorkItems [workItemUsers|{workItemUsers.Count}]");
                List<IdentityMapData> mappedUsers = GetUsersInSourceMappedToTarget(processor);
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTargetForWorkItems [mappedUsers|{mappedUsers.Count}]");
                return mappedUsers.Where(x => workItemUsers.Contains(x.Source.FriendlyName)).ToList();
            }
            else
            {
                Log.LogWarning("TfsUserMappingTool is disabled in settings. You may have users in the source that are not mapped to the target. ");
                return null;
            }
        }
    }

    public class CaseInsensativeStringComparer : IEqualityComparer<string>
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
}
