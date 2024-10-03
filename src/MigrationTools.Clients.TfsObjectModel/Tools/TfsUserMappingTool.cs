using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Commerce;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
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


        private List<string> GetUsersFromWorkItems(List<WorkItemData> workitems, List<string> identityFieldsToCheck)
        {
            List<string> foundUsers = new List<string>();
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



        private void MapUserIdentityField(FieldItem field)
        {
            if (Options.Enabled && Options.IdentityFieldsToCheck.Contains(field.ReferenceName))
            {
                Log.LogDebug($"TfsUserMappingTool::MapUserIdentityField [ReferenceName|{field.ReferenceName}]");
                var mapps = GetMappingFileData();
                if (mapps != null && mapps.ContainsKey(field.Value.ToString()))
                {
                    field.Value = mapps[field.Value.ToString()];
                }

            }
        }

        public void MapUserIdentityField(TfsProcessor processor, Field field)
        {
            if (Options.Enabled && Options.IdentityFieldsToCheck.Contains(field.ReferenceName))
            {
                Log.LogDebug($"TfsUserMappingTool::MapUserIdentityField [ReferenceName|{field.ReferenceName}]");
                var mapps = GetMappingFileData();
                if (mapps != null && mapps.ContainsKey(field.Value.ToString()))
                {
                    field.Value = mapps[field.Value.ToString()];
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
                    _UserMappings = fileMaps.ToDictionary(x => x.Source.FriendlyName, x => x.target?.FriendlyName);
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
            Identity SIDS = gss.ReadIdentity(SearchFactor.AccountName, "Project Collection Valid Users", QueryMembership.Expanded);
            var people = SIDS.Members.ToList().Where(x => x.Contains("\\")).Select(x => x);

            List<IdentityItemData> foundUsers = new List<IdentityItemData>();
            Log.LogTrace("TfsUserMappingTool::GetUsersListFromServer:foundUsers\\ {@foundUsers}", foundUsers);
            foreach (string user in people)
            {
                Log.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Atempting to load user", user);
                try
                {
                    var bits = user.Split('\\');
                    Identity sids = gss.ReadIdentity(SearchFactor.AccountName, bits[1], QueryMembership.Expanded);
                    if (sids != null)
                    {
                        foundUsers.Add(new IdentityItemData() { FriendlyName = sids.DisplayName, AccountName = sids.AccountName });
                    }
                    else
                    {
                        Log.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] ReadIdentity returned null for {@bits}", user, bits);
                    }

                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, null);
                    Log.LogWarning("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Failed With {Exception}", user, ex.Message);
                }

            }
            return foundUsers;
        }


        public List<IdentityMapData> GetUsersInSourceMappedToTarget(TfsProcessor processor)
        {
            Log.LogDebug("TfsUserMappingTool::GetUsersInSourceMappedToTarget");
            if (Options.Enabled)
            {
                var sourceUsers = GetUsersListFromServer(processor.Source.GetService<IGroupSecurityService>());
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTarget [SourceUsersCount|{sourceUsers.Count}]");
                var targetUsers = GetUsersListFromServer(processor.Target.GetService<IGroupSecurityService>());
                Log.LogDebug($"TfsUserMappingTool::GetUsersInSourceMappedToTarget [targetUsersCount|{targetUsers.Count}]");
                return sourceUsers.Select(sUser => new IdentityMapData { Source = sUser, target = targetUsers.SingleOrDefault(tUser => tUser.FriendlyName == sUser.FriendlyName) }).ToList();
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
                List<string> workItemUsers = GetUsersFromWorkItems(sourceWorkItems, Options.IdentityFieldsToCheck);
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
