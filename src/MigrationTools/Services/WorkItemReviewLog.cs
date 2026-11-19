using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace MigrationTools.Services
{
    /// <summary>
    /// The categories written to the review log.
    /// </summary>
    public static class WorkItemReviewCategory
    {
        public const string Failed = "failed";
        public const string PartMigrated = "part-migrated";
        public const string RerunMismatch = "rerun-mismatch";
        public const string LinkSkipped = "link-skipped";
        public const string LinkLeftOnSource = "link-left-on-source";
        public const string FieldNotMapped = "field-not-mapped";
        public const string FieldMapFailed = "field-map-failed";
        public const string FieldTruncated = "field-truncated";
        public const string UserNotMapped = "user-not-mapped";
        public const string AttachmentNotMigrated = "attachment-not-migrated";
        public const string PathNotMapped = "path-not-mapped";
        public const string TypeChanged = "type-changed";
        public const string Skipped = "skipped";
        public const string Resolved = "resolved";

        /// <summary>
        /// Categories that are only found while a revision is replayed. A re-run that replays no revision of an
        /// item does not check them again, so it cannot resolve them.
        /// </summary>
        public static readonly ISet<string> FoundOnlyWhileReplaying = new HashSet<string>(StringComparer.Ordinal)
        {
            Failed, PartMigrated, FieldNotMapped, FieldMapFailed, FieldTruncated, UserNotMapped, PathNotMapped, TypeChanged,
        };

        /// <summary>
        /// The follow-up file each category is also written to; a category not listed has no follow-up file.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> FollowUpFiles = new Dictionary<string, string>
        {
            [LinkSkipped] = "follow-up-links.jsonl",
            [LinkLeftOnSource] = "follow-up-links.jsonl",
            [UserNotMapped] = "follow-up-users-not-mapped.jsonl",
            [AttachmentNotMigrated] = "follow-up-attachments.jsonl",
            [FieldNotMapped] = "follow-up-fields.jsonl",
            [FieldMapFailed] = "follow-up-fields.jsonl",
            [FieldTruncated] = "follow-up-fields.jsonl",
            [PathNotMapped] = "follow-up-area-iteration-paths.jsonl",
            [Skipped] = "follow-up-skipped.jsonl",
        };
    }

    /// <summary>
    /// Implemented by components created outside the tool container (field maps) that write to the review log;
    /// the creator sets <see cref="ReviewLog"/>.
    /// </summary>
    public interface IRecordsWorkItemReview
    {
        WorkItemReviewLog ReviewLog { set; }
    }

    /// <summary>
    /// One row of the review log.
    /// </summary>
    public sealed class WorkItemReviewEntry
    {
        [JsonProperty("time")] public DateTimeOffset Time { get; set; }
        [JsonProperty("sourceId")] public string SourceId { get; set; }
        [JsonProperty("targetId")] public string TargetId { get; set; }
        [JsonProperty("workItemType")] public string WorkItemType { get; set; }
        [JsonProperty("revision")] public int? Revision { get; set; }
        [JsonProperty("category")] public string Category { get; set; }
        [JsonProperty("reason")] public string Reason { get; set; }
        [JsonProperty("action")] public string Action { get; set; }
        [JsonProperty("migration")] public string Migration { get; set; }
    }

    /// <summary>
    /// Writes <c>work-items-to-review.jsonl</c> (and one follow-up file per concern) in the run's log folder.
    /// Each entry is appended and flushed to disk as the problem is found, so the files are complete up to the
    /// last problem even if the process is killed. Files are created on the first entry.
    /// </summary>
    public class WorkItemReviewLog : IDisposable
    {
        public const string FileName = "work-items-to-review.jsonl";
        public const string LatestFileName = "work-items-to-review-latest.jsonl";

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.None,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffzzz",
        };

        private readonly object _gate = new object();
        private readonly string _folder;
        private readonly ILogger _logger;
        private readonly Dictionary<string, FileStream> _streams = new Dictionary<string, FileStream>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _rowCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _categoryCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _written = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, WorkItemReviewEntry> _latest = new Dictionary<string, WorkItemReviewEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<WorkItemReviewEntry>> _openFromEarlierRuns = new Dictionary<string, List<WorkItemReviewEntry>>(StringComparer.Ordinal);
        private readonly HashSet<string> _loadedMigrations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _comingLater = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<WorkItemReviewEntry>> _waitingOnItem = new Dictionary<string, List<WorkItemReviewEntry>>(StringComparer.Ordinal);
        private readonly AsyncLocal<ItemScope> _current = new AsyncLocal<ItemScope>();
        private string _migration;

        public WorkItemReviewLog(string folder, ILogger<WorkItemReviewLog> logger = null)
        {
            _folder = folder ?? throw new ArgumentNullException(nameof(folder));
            _logger = (ILogger)logger ?? NullLogger.Instance;
        }

        public string FilePath => Path.Combine(_folder, FileName);

        public string Folder => _folder;

        public IReadOnlyDictionary<string, int> CategoryCounts
        {
            get { lock (_gate) { return new Dictionary<string, int>(_categoryCounts); } }
        }

        public IReadOnlyDictionary<string, int> FileRowCounts
        {
            get { lock (_gate) { return new Dictionary<string, int>(_rowCounts); } }
        }

        // Work item ids are only unique within one migration, so all per-item state is keyed by both.
        private static string ItemKey(string migration, string sourceId) => (migration ?? string.Empty) + "|" + sourceId;

        /// <summary>
        /// Starts recording for one migration (source and target endpoints) and reads the review logs of earlier
        /// runs in sibling folders of this run's log folder, so that an item that needed review before and now
        /// migrates cleanly gets a <c>resolved</c> entry. Only entries written for the same
        /// <paramref name="migration"/> are read. Every entry recorded after this call carries it. Each processor
        /// in a run calls it for its own endpoints.
        /// </summary>
        public void LoadEarlierRuns(string migration)
        {
            lock (_gate)
            {
                _migration = migration;
                if (!_loadedMigrations.Add(migration ?? string.Empty))
                {
                    return;
                }
                string root;
                string ownName;
                try
                {
                    root = Directory.GetParent(Path.GetFullPath(_folder))?.FullName;
                    ownName = Path.GetFileName(Path.GetFullPath(_folder).TrimEnd(Path.DirectorySeparatorChar));
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
                {
                    _logger.LogWarning(ex, "Could not find earlier work item review logs; no resolved entries will be written in this run.");
                    return;
                }
                if (root == null || !Directory.Exists(root))
                {
                    return;
                }
                IEnumerable<string> earlier;
                try
                {
                    earlier = Directory.GetDirectories(root)
                        .Where(d => string.CompareOrdinal(Path.GetFileName(d), ownName) < 0)
                        .OrderBy(d => Path.GetFileName(d), StringComparer.Ordinal)
                        .Select(d => Path.Combine(d, FileName))
                        .Where(File.Exists)
                        .ToList();
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Could not list earlier work item review logs in {LogsFolder}; no resolved entries will be written in this run.", root);
                    return;
                }
                foreach (var file in earlier)
                {
                    List<WorkItemReviewEntry> entries;
                    try
                    {
                        entries = ReadEntries(file).ToList();
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        _logger.LogWarning(ex, "Could not read the earlier work item review log {ReviewLogPath}; items it lists are not resolved in this run.", file);
                        continue;
                    }
                    foreach (var entry in entries)
                    {
                        if (string.IsNullOrEmpty(entry.SourceId) || !string.Equals(entry.Migration, migration, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        var key = ItemKey(migration, entry.SourceId);
                        _latest[key] = entry;
                        if (entry.Category == WorkItemReviewCategory.Resolved)
                        {
                            _openFromEarlierRuns.Remove(key);
                            continue;
                        }
                        if (!_openFromEarlierRuns.TryGetValue(key, out var open))
                        {
                            open = new List<WorkItemReviewEntry>();
                            _openFromEarlierRuns[key] = open;
                        }
                        open.Add(entry);
                    }
                }
            }
        }

        /// <summary>
        /// Reads a review log, skipping a last line cut short by a killed process.
        /// </summary>
        public static IEnumerable<WorkItemReviewEntry> ReadEntries(string file)
        {
            string[] lines;
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                lines = reader.ReadToEnd().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            foreach (var line in lines)
            {
                WorkItemReviewEntry entry = null;
                try
                {
                    entry = JsonConvert.DeserializeObject<WorkItemReviewEntry>(line.TrimEnd('\r'), JsonSettings);
                }
                catch (JsonException)
                {
                }
                if (entry != null)
                {
                    yield return entry;
                }
            }
        }

        /// <summary>
        /// Sets the source ids this run will migrate. A link to one of them that has not been reached yet is added
        /// from that item's side when it is migrated; see <see cref="RecordUnlessMigratedLater"/>.
        /// </summary>
        public void SetItemsToMigrate(IEnumerable<string> sourceIds)
        {
            lock (_gate)
            {
                _comingLater.Clear();
                foreach (var id in sourceIds ?? Enumerable.Empty<string>())
                {
                    _comingLater.Add(id);
                }
            }
        }

        /// <summary>
        /// True when the source item is migrated later in this run.
        /// </summary>
        public bool IsMigratedLater(string sourceId)
        {
            lock (_gate)
            {
                return sourceId != null && _comingLater.Contains(sourceId);
            }
        }

        /// <summary>
        /// Records a problem that goes away if <paramref name="relatedSourceId"/> migrates later in this run (a link
        /// to it is then added from its side). While that item is still to come, the entry is held; it is written
        /// if that item fails (<see cref="ItemNotMigrated"/>) or is never reached (<see cref="FlushHeldEntries"/>),
        /// and dropped if it migrates.
        /// </summary>
        public void RecordUnlessMigratedLater(string relatedSourceId, string category, string reason, string action, string sourceId = null, string targetId = null)
        {
            var entry = NewEntry(category, reason, action, sourceId, targetId, null, null);
            lock (_gate)
            {
                if (relatedSourceId != null && _comingLater.Contains(relatedSourceId))
                {
                    if (!_waitingOnItem.TryGetValue(relatedSourceId, out var held))
                    {
                        held = new List<WorkItemReviewEntry>();
                        _waitingOnItem[relatedSourceId] = held;
                    }
                    held.Add(entry);
                    return;
                }
            }
            AddToScope(entry, sourceId);
            Write(entry);
        }

        /// <summary>
        /// Writes the entries held for links to <paramref name="sourceId"/>, because it was not migrated.
        /// </summary>
        public void ItemNotMigrated(string sourceId)
        {
            List<WorkItemReviewEntry> held;
            lock (_gate)
            {
                if (sourceId == null || !_waitingOnItem.TryGetValue(sourceId, out held))
                {
                    return;
                }
                _waitingOnItem.Remove(sourceId);
            }
            foreach (var entry in held)
            {
                entry.Time = DateTimeOffset.Now;
                entry.Reason += $" Source work item {sourceId} was not migrated in this run.";
                Write(entry);
            }
        }

        /// <summary>
        /// Writes every entry still held at the end of the run: the items they wait for were never reached.
        /// </summary>
        public void FlushHeldEntries()
        {
            List<string> ids;
            lock (_gate)
            {
                ids = _waitingOnItem.Keys.ToList();
            }
            foreach (var id in ids)
            {
                ItemNotMigrated(id);
            }
        }

        /// <summary>
        /// Starts the work on one source work item. Entries recorded inside the scope without explicit ids
        /// take them from it.
        /// </summary>
        public IDisposable BeginItem(string sourceId, string workItemType, int? revision = null)
        {
            lock (_gate)
            {
                _comingLater.Remove(sourceId);
            }
            var scope = new ItemScope(this, _current.Value) { SourceId = sourceId, WorkItemType = workItemType, Revision = revision };
            _current.Value = scope;
            return scope;
        }

        /// <summary>
        /// Sets the target id (once known) and revision being replayed for the current item.
        /// </summary>
        public void SetCurrentItem(string targetId = null, int? revision = null)
        {
            var scope = _current.Value;
            if (scope == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(targetId))
            {
                scope.TargetId = targetId;
            }
            if (revision.HasValue)
            {
                scope.Revision = revision;
            }
        }

        public string CurrentTargetId => _current.Value?.TargetId;

        public string CurrentSourceId => _current.Value?.SourceId;

        /// <summary>
        /// True when the current item already has an entry in this category in this run.
        /// </summary>
        public bool CurrentItemHas(string category)
        {
            var scope = _current.Value;
            return scope != null && scope.Categories.Contains(category);
        }

        /// <summary>
        /// Records that a source revision of the current item was replayed in this run, so problems found while
        /// replaying that revision were checked again.
        /// </summary>
        public void MarkRevisionReplayed(int revision)
        {
            _current.Value?.ReplayedRevisions.Add(revision);
        }

        /// <summary>
        /// Records that the check behind <paramref name="category"/> ran for the current item in this run (for
        /// example the attachment tool processed its attachments). A problem in a category that was not checked
        /// again, because the tool was disabled or skipped, is not resolved.
        /// </summary>
        public void MarkChecked(string category)
        {
            _current.Value?.Checked.Add(category);
        }

        /// <summary>
        /// Marks the current item as migrated without failing. If nothing was recorded for it in this run and an
        /// earlier run left it needing review, a <c>resolved</c> entry is written, but only when every earlier
        /// problem was checked again: a problem found while replaying a revision
        /// (<see cref="WorkItemReviewCategory.FoundOnlyWhileReplaying"/>) needs the revision it was found on to be
        /// replayed in this run, and any other problem needs its check to have run (<see cref="MarkChecked"/>).
        /// A missing area or iteration path found by validation has no revision; reaching the item means
        /// validation passed.
        /// </summary>
        public void CompleteCurrentItem()
        {
            var scope = _current.Value;
            if (scope == null || string.IsNullOrEmpty(scope.SourceId))
            {
                return;
            }
            lock (_gate)
            {
                _waitingOnItem.Remove(scope.SourceId);
            }
            if (scope.Categories.Count > 0)
            {
                return;
            }
            List<WorkItemReviewEntry> open;
            lock (_gate)
            {
                if (!_openFromEarlierRuns.TryGetValue(ItemKey(_migration, scope.SourceId), out var entries))
                {
                    return;
                }
                open = entries.ToList();
            }
            if (!open.All(e => WasCheckedAgain(e, scope)))
            {
                return;
            }
            var openCategories = open.Select(e => e.Category).Distinct().OrderBy(k => k, StringComparer.Ordinal);
            Record(WorkItemReviewCategory.Resolved,
                $"Migrated without the problems an earlier run reported: {string.Join(", ", openCategories)}.",
                "None.");
        }

        private static bool WasCheckedAgain(WorkItemReviewEntry entry, ItemScope scope)
        {
            if (entry.Category == WorkItemReviewCategory.PathNotMapped && !entry.Revision.HasValue)
            {
                return true;
            }
            if (WorkItemReviewCategory.FoundOnlyWhileReplaying.Contains(entry.Category))
            {
                return entry.Revision.HasValue && scope.ReplayedRevisions.Contains(entry.Revision.Value);
            }
            return scope.Checked.Contains(entry.Category);
        }

        /// <summary>
        /// Appends an entry and flushes it to disk. Ids not given come from the current item. The same
        /// category, item and reason is written once per run.
        /// </summary>
        public void Record(string category, string reason, string action, string sourceId = null, string targetId = null, string workItemType = null, int? revision = null)
        {
            var entry = NewEntry(category, reason, action, sourceId, targetId, workItemType, revision);
            AddToScope(entry, sourceId);
            Write(entry);
        }

        private WorkItemReviewEntry NewEntry(string category, string reason, string action, string sourceId, string targetId, string workItemType, int? revision)
        {
            var scope = _current.Value;
            return new WorkItemReviewEntry
            {
                Time = DateTimeOffset.Now,
                SourceId = sourceId ?? scope?.SourceId,
                TargetId = targetId ?? scope?.TargetId,
                WorkItemType = workItemType ?? scope?.WorkItemType,
                Revision = revision ?? scope?.Revision,
                Category = category,
                Reason = reason,
                Action = action,
                Migration = _migration,
            };
        }

        private void AddToScope(WorkItemReviewEntry entry, string explicitSourceId)
        {
            var scope = _current.Value;
            if (scope != null && (explicitSourceId == null || explicitSourceId == scope.SourceId) && entry.Category != WorkItemReviewCategory.Resolved)
            {
                scope.Categories.Add(entry.Category);
            }
        }

        // Each output file is tracked on its own, so a write that failed is tried again the next time the same
        // problem is recorded, without writing a row twice to a file that already has it.
        private void Write(WorkItemReviewEntry entry)
        {
            var key = entry.Migration + "|" + entry.Category + "|" + entry.SourceId + "|" + entry.Reason;
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry, JsonSettings) + "\n");
            lock (_gate)
            {
                var files = new List<string> { FileName };
                if (WorkItemReviewCategory.FollowUpFiles.TryGetValue(entry.Category, out var followUp))
                {
                    files.Add(followUp);
                }
                foreach (var file in files)
                {
                    var fileKey = file + "|" + key;
                    if (_written.Contains(fileKey))
                    {
                        continue;
                    }
                    try
                    {
                        Append(file, bytes);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        _logger.LogWarning(ex, "Could not write to the work item review file {ReviewFilePath}", Path.Combine(_folder, file));
                        continue;
                    }
                    _written.Add(fileKey);
                    if (file == FileName)
                    {
                        _categoryCounts.TryGetValue(entry.Category, out var count);
                        _categoryCounts[entry.Category] = count + 1;
                        if (!string.IsNullOrEmpty(entry.SourceId))
                        {
                            var itemKey = ItemKey(entry.Migration, entry.SourceId);
                            _latest[itemKey] = entry;
                            if (entry.Category == WorkItemReviewCategory.Resolved)
                            {
                                _openFromEarlierRuns.Remove(itemKey);
                            }
                        }
                    }
                }
            }
        }

        private void Append(string fileName, byte[] bytes)
        {
            if (!_streams.TryGetValue(fileName, out var stream))
            {
                Directory.CreateDirectory(_folder);
                stream = new FileStream(Path.Combine(_folder, fileName), FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                _streams[fileName] = stream;
            }
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
            _rowCounts.TryGetValue(fileName, out var rows);
            _rowCounts[fileName] = rows + 1;
        }

        /// <summary>
        /// Writes the combined view: the latest entry per item across this run and the earlier runs read by
        /// <see cref="LoadEarlierRuns"/>. Not needed for the review log to be correct, so a failure to write it
        /// is logged and does not change the run's result.
        /// </summary>
        public string WriteLatestView()
        {
            lock (_gate)
            {
                if (_rowCounts.Count == 0 && _latest.Count == 0)
                {
                    return null;
                }
                var path = Path.Combine(_folder, LatestFileName);
                try
                {
                    Directory.CreateDirectory(_folder);
                    var lines = _latest.Values
                        .OrderBy(e => e.Migration, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(e => int.TryParse(e.SourceId, out var id) ? id : int.MaxValue)
                        .ThenBy(e => e.SourceId, StringComparer.Ordinal)
                        .Select(e => JsonConvert.SerializeObject(e, JsonSettings));
                    File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Could not write the combined work item review view {ReviewLatestPath}; the per-run review log is complete without it.", path);
                    return null;
                }
                return path;
            }
        }

        /// <summary>
        /// Writes the end-of-run summary: counts by category and each file with its row count.
        /// </summary>
        public void LogSummary(ILogger logger)
        {
            Dictionary<string, int> categories;
            Dictionary<string, int> files;
            lock (_gate)
            {
                categories = new Dictionary<string, int>(_categoryCounts);
                files = new Dictionary<string, int>(_rowCounts);
            }
            if (categories.Count == 0)
            {
                logger.LogInformation("Work item review log: no work items need review. ({ReviewLogPath} was not created.)", FilePath);
                return;
            }
            var needReview = categories.Where(c => c.Key != WorkItemReviewCategory.Resolved).Sum(c => c.Value);
            var level = needReview > 0 ? LogLevel.Warning : LogLevel.Information;
            logger.Log(level, "Work item review log: {EntryCount} review entries written to {ReviewLogPath}", needReview, FilePath);
            foreach (var category in categories.OrderBy(c => c.Key, StringComparer.Ordinal))
            {
                logger.Log(level, "  {ReviewCategory}: {ReviewCategoryCount}", category.Key, category.Value);
            }
            foreach (var file in files.Where(f => f.Key != FileName).OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                logger.Log(level, "  Follow-up file {FollowUpFile}: {FollowUpRows} row(s)", Path.Combine(_folder, file.Key), file.Value);
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                foreach (var stream in _streams.Values)
                {
                    stream.Dispose();
                }
                _streams.Clear();
            }
        }

        private sealed class ItemScope : IDisposable
        {
            private readonly WorkItemReviewLog _owner;
            private readonly ItemScope _parent;

            public ItemScope(WorkItemReviewLog owner, ItemScope parent)
            {
                _owner = owner;
                _parent = parent;
            }

            public string SourceId { get; set; }
            public string TargetId { get; set; }
            public string WorkItemType { get; set; }
            public int? Revision { get; set; }
            public HashSet<int> ReplayedRevisions { get; } = new HashSet<int>();
            public HashSet<string> Checked { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> Categories { get; } = new HashSet<string>(StringComparer.Ordinal);

            public void Dispose()
            {
                _owner._current.Value = _parent;
            }
        }
    }
}
