using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Server;
using MigrationTools.DataContracts;

namespace MigrationTools.Tools
{
    /// <summary>
    /// The outcome of reading one identity through the fallback API after the batched read rejected it.
    /// </summary>
    public enum FallbackIdentityKind
    {
        User,
        Group,
        Unreadable
    }

    /// <summary>
    /// Reads the members of Project Collection Valid Users in batches. A batch that the server cannot return (for example a group
    /// of ApplicationGroupSpecialType 5, which the legacy group security service fails to serialise) is split in half until the
    /// failing identities are isolated; each of those is read through the fallback reader, so no user is lost to a bad neighbour.
    /// </summary>
    public class TfsIdentityBatchReader
    {
        public const int DefaultBatchSize = 100;

        private readonly Func<string[], Identity[]> _readBatch;
        private readonly Func<string, (FallbackIdentityKind Kind, IdentityItemData User)> _readFallback;
        private readonly Func<Identity, IdentityItemData> _toItem;
        private readonly ILogger _log;
        private readonly int _batchSize;

        public TfsIdentityBatchReader(
            Func<string[], Identity[]> readBatch,
            Func<string, (FallbackIdentityKind Kind, IdentityItemData User)> readFallback,
            Func<Identity, IdentityItemData> toItem,
            ILogger log,
            int batchSize = DefaultBatchSize)
        {
            _readBatch = readBatch ?? throw new ArgumentNullException(nameof(readBatch));
            _readFallback = readFallback ?? throw new ArgumentNullException(nameof(readFallback));
            _toItem = toItem ?? throw new ArgumentNullException(nameof(toItem));
            _log = log;
            _batchSize = Math.Max(1, batchSize);
        }

        /// <summary>The SIDs of groups that only the fallback could read. They are not users, so nothing is lost.</summary>
        public List<string> FallbackGroups { get; } = [];

        /// <summary>The SIDs that neither the batched read nor the fallback could read.</summary>
        public List<string> Unreadable { get; } = [];

        /// <summary>The number of server calls made, batched and fallback.</summary>
        public int Calls { get; private set; }

        public List<IdentityItemData> ReadUsers(IReadOnlyList<string> sids)
        {
            var users = new List<IdentityItemData>();
            var timer = Stopwatch.StartNew();
            int done = 0;
            for (int start = 0; start < sids.Count; start += _batchSize)
            {
                var batch = sids.Skip(start).Take(_batchSize).ToArray();
                ReadBatch(batch, users);
                done += batch.Length;
                _log?.LogInformation("TfsUserMappingTool::GetUsersListFromServer Read {done}/{total} identities, {users} users so far ({elapsed:0.0}s)",
                    done, sids.Count, users.Count, timer.Elapsed.TotalSeconds);
            }
            return users;
        }

        private void ReadBatch(string[] batch, List<IdentityItemData> users)
        {
            Identity[] identities;
            try
            {
                Calls++;
                identities = _readBatch(batch);
            }
            catch (Exception ex)
            {
                if (batch.Length == 1)
                {
                    _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Batched read failed with {error}; trying the identity management service", batch[0], ex.Message);
                    ReadFallback(batch[0], users);
                    return;
                }
                _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer A batch of {count} identities failed with {error}; splitting it", batch.Length, ex.Message);
                int half = batch.Length / 2;
                ReadBatch(batch.Take(half).ToArray(), users);
                ReadBatch(batch.Skip(half).ToArray(), users);
                return;
            }

            foreach (var identity in identities ?? [])
            {
                if (identity is null)
                {
                    continue;
                }
                // UnknownIdentityType is set for users in Azure Entra ID.
                if (identity.Type == IdentityType.WindowsUser || identity.Type == IdentityType.UnknownIdentityType)
                {
                    users.Add(_toItem(identity));
                }
                else
                {
                    _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Not applicable identity type {identityType}", identity.Sid, identity.Type);
                }
            }
        }

        private void ReadFallback(string sid, List<IdentityItemData> users)
        {
            (FallbackIdentityKind Kind, IdentityItemData User) result;
            try
            {
                Calls++;
                result = _readFallback(sid);
            }
            catch (Exception ex)
            {
                _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[user:{user}] Fallback read failed with {error}", sid, ex.Message);
                result = (FallbackIdentityKind.Unreadable, null);
            }
            switch (result.Kind)
            {
                case FallbackIdentityKind.User when result.User != null:
                    users.Add(result.User);
                    break;
                case FallbackIdentityKind.Group:
                    FallbackGroups.Add(sid);
                    break;
                default:
                    Unreadable.Add(sid);
                    break;
            }
        }
    }
}
