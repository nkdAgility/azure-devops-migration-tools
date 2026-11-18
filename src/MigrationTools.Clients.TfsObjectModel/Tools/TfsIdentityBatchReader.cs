using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;

namespace MigrationTools.Tools
{
    /// <summary>
    /// The outcome of reading one identity on its own after the batched read rejected the batch it was in.
    /// </summary>
    public enum FallbackIdentityKind
    {
        User,
        NotAUser,
        Unreadable
    }

    /// <summary>
    /// Reads identities in batches and returns the users among them. A batch that the server cannot return (for example one holding a
    /// group of ApplicationGroupSpecialType 5, which an older service fails to serialise) is split in half until the failing
    /// identities are isolated; each of those is read on its own through the fallback, so no user is lost to a bad neighbour.
    /// </summary>
    public class TfsIdentityBatchReader<TKey>
    {
        public const int DefaultBatchSize = 100;

        private readonly Func<TKey[], IEnumerable<IdentityItemData>> _readUsers;
        private readonly Func<TKey, (FallbackIdentityKind Kind, IdentityItemData User)> _readFallback;
        private readonly Func<TKey, string> _describe;
        private readonly ILogger _log;
        private readonly int _batchSize;

        /// <param name="readUsers">Reads a batch and returns the users in it; throws when the server cannot return the batch.</param>
        /// <param name="readFallback">Reads one identity that failed in a batch.</param>
        /// <param name="describe">Names an identity in the log.</param>
        public TfsIdentityBatchReader(
            Func<TKey[], IEnumerable<IdentityItemData>> readUsers,
            Func<TKey, (FallbackIdentityKind Kind, IdentityItemData User)> readFallback,
            Func<TKey, string> describe,
            ILogger log,
            int batchSize = DefaultBatchSize)
        {
            _readUsers = readUsers ?? throw new ArgumentNullException(nameof(readUsers));
            _readFallback = readFallback ?? throw new ArgumentNullException(nameof(readFallback));
            _describe = describe ?? (k => k?.ToString());
            _log = log;
            _batchSize = Math.Max(1, batchSize);
        }

        /// <summary>Identities that failed in a batch and turned out not to be users, so nothing is lost.</summary>
        public List<string> SkippedNonUsers { get; } = [];

        /// <summary>Identities that failed in a batch and could not be read on their own either.</summary>
        public List<string> Unreadable { get; } = [];

        /// <summary>The number of server calls made, batched and fallback.</summary>
        public int Calls { get; private set; }

        public List<IdentityItemData> ReadUsers(IReadOnlyList<TKey> keys)
        {
            var users = new List<IdentityItemData>();
            var timer = Stopwatch.StartNew();
            int done = 0;
            for (int start = 0; start < keys.Count; start += _batchSize)
            {
                var batch = keys.Skip(start).Take(_batchSize).ToArray();
                ReadBatch(batch, users);
                done += batch.Length;
                _log?.LogInformation("TfsUserMappingTool::GetUsersListFromServer Read {done}/{total} identities, {users} users so far ({elapsed:0.0}s)",
                    done, keys.Count, users.Count, timer.Elapsed.TotalSeconds);
            }
            return users;
        }

        private void ReadBatch(TKey[] batch, List<IdentityItemData> users)
        {
            List<IdentityItemData> read;
            try
            {
                Calls++;
                read = (_readUsers(batch) ?? []).Where(u => u != null).ToList();
            }
            catch (Exception ex)
            {
                if (batch.Length == 1)
                {
                    _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[{identity}] Batched read failed with {error}; reading it on its own", _describe(batch[0]), ex.Message);
                    ReadFallback(batch[0], users);
                    return;
                }
                _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer A batch of {count} identities failed with {error}; splitting it", batch.Length, ex.Message);
                int half = batch.Length / 2;
                ReadBatch(batch.Take(half).ToArray(), users);
                ReadBatch(batch.Skip(half).ToArray(), users);
                return;
            }
            users.AddRange(read);
        }

        private void ReadFallback(TKey key, List<IdentityItemData> users)
        {
            (FallbackIdentityKind Kind, IdentityItemData User) result;
            try
            {
                Calls++;
                result = _readFallback(key);
            }
            catch (Exception ex)
            {
                _log?.LogDebug("TfsUserMappingTool::GetUsersListFromServer::[{identity}] Fallback read failed with {error}", _describe(key), ex.Message);
                result = (FallbackIdentityKind.Unreadable, null);
            }
            switch (result.Kind)
            {
                case FallbackIdentityKind.User when result.User != null:
                    users.Add(result.User);
                    break;
                case FallbackIdentityKind.NotAUser:
                    SkippedNonUsers.Add(_describe(key));
                    break;
                default:
                    Unreadable.Add(_describe(key));
                    break;
            }
        }
    }
}
