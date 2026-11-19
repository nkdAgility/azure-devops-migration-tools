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
            Failed, PartMigrated, FieldNotMapped, FieldMapFailed, UserNotMapped, PathNotMapped, TypeChanged,
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
            [PathNotMapped] = "follow-up-area-iteration-paths.jsonl",
            [Skipped] = "follow-up-skipped.jsonl",
        };
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
        private readonly HashSet<string> _seen = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, WorkItemReviewEntry> _latest = new Dictionary<string, WorkItemReviewEntry>(StringComparer.Ordinal);
        private readonly Dictionary<string, Dictionary<string, WorkItemReviewEntry>> _openFromEarlierRuns = new Dictionary<string, Dictionary<string, WorkItemReviewEntry>>(StringComparer.Ordinal);
        private readonly AsyncLocal<ItemScope> _current = new AsyncLocal<ItemScope>();
        private bool _earlierRunsLoaded;

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

        /// <summary>
        /// Reads the review logs of earlier runs in sibling folders of this run's log folder, so that an item
        /// that needed review before and now migrates cleanly gets a <c>resolved</c> entry.
        /// </summary>
        public void LoadEarlierRuns()
        {
            lock (_gate)
            {
                if (_earlierRunsLoaded)
                {
                    return;
                }
                _earlierRunsLoaded = true;
                var root = Directory.GetParent(Path.GetFullPath(_folder))?.FullName;
                if (root == null || !Directory.Exists(root))
                {
                    return;
                }
                var ownName = Path.GetFileName(Path.GetFullPath(_folder).TrimEnd(Path.DirectorySeparatorChar));
                var earlier = Directory.GetDirectories(root)
                    .Where(d => string.CompareOrdinal(Path.GetFileName(d), ownName) < 0)
                    .OrderBy(d => Path.GetFileName(d), StringComparer.Ordinal)
                    .Select(d => Path.Combine(d, FileName))
                    .Where(File.Exists);
                foreach (var file in earlier)
                {
                    foreach (var entry in ReadEntries(file))
                    {
                        if (string.IsNullOrEmpty(entry.SourceId))
                        {
                            continue;
                        }
                        _latest[entry.SourceId] = entry;
                        if (entry.Category == WorkItemReviewCategory.Resolved)
                        {
                            _openFromEarlierRuns.Remove(entry.SourceId);
                            continue;
                        }
                        if (!_openFromEarlierRuns.TryGetValue(entry.SourceId, out var open))
                        {
                            open = new Dictionary<string, WorkItemReviewEntry>(StringComparer.Ordinal);
                            _openFromEarlierRuns[entry.SourceId] = open;
                        }
                        open[entry.Category] = entry;
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

        private readonly HashSet<string> _comingLater = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// Sets the source ids this run will migrate. A link to one of them that has not been reached yet is added
        /// from that item's side when it is migrated, so it is not a problem to review.
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

        /// <summary>
        /// True when the current item already has an entry in this category in this run.
        /// </summary>
        public bool CurrentItemHas(string category)
        {
            var scope = _current.Value;
            return scope != null && scope.Categories.Contains(category);
        }

        /// <summary>
        /// Records that revisions of the current item were replayed in this run, so problems that are only found
        /// while replaying a revision were checked again.
        /// </summary>
        public void MarkRevisionsReplayed()
        {
            var scope = _current.Value;
            if (scope != null)
            {
                scope.RevisionsReplayed = true;
            }
        }

        /// <summary>
        /// Marks the current item as migrated without failing. If nothing was recorded for it in this run and an
        /// earlier run left it needing review, a <c>resolved</c> entry is written. When no revision was replayed,
        /// an earlier problem that is only found while replaying (<see cref="WorkItemReviewCategory.FoundOnlyWhileReplaying"/>)
        /// was not checked again, so the item stays open.
        /// </summary>
        public void CompleteCurrentItem()
        {
            var scope = _current.Value;
            if (scope == null || string.IsNullOrEmpty(scope.SourceId) || scope.Categories.Count > 0)
            {
                return;
            }
            List<string> openCategories;
            lock (_gate)
            {
                if (!_openFromEarlierRuns.TryGetValue(scope.SourceId, out var open))
                {
                    return;
                }
                openCategories = open.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
            }
            if (!scope.RevisionsReplayed && openCategories.Any(WorkItemReviewCategory.FoundOnlyWhileReplaying.Contains))
            {
                return;
            }
            Record(WorkItemReviewCategory.Resolved,
                $"Migrated without the problems an earlier run reported: {string.Join(", ", openCategories)}.",
                "None.");
        }

        /// <summary>
        /// Appends an entry and flushes it to disk. Ids not given come from the current item. The same
        /// category, item and reason is written once per run.
        /// </summary>
        public void Record(string category, string reason, string action, string sourceId = null, string targetId = null, string workItemType = null, int? revision = null)
        {
            var scope = _current.Value;
            var entry = new WorkItemReviewEntry
            {
                Time = DateTimeOffset.Now,
                SourceId = sourceId ?? scope?.SourceId,
                TargetId = targetId ?? scope?.TargetId,
                WorkItemType = workItemType ?? scope?.WorkItemType,
                Revision = revision ?? scope?.Revision,
                Category = category,
                Reason = reason,
                Action = action,
            };
            if (scope != null && (sourceId == null || sourceId == scope.SourceId) && category != WorkItemReviewCategory.Resolved)
            {
                scope.Categories.Add(category);
            }
            Write(entry);
        }

        private void Write(WorkItemReviewEntry entry)
        {
            var key = entry.Category + "|" + entry.SourceId + "|" + entry.Reason;
            var line = JsonConvert.SerializeObject(entry, JsonSettings) + "\n";
            var bytes = Encoding.UTF8.GetBytes(line);
            lock (_gate)
            {
                if (!_seen.Add(key))
                {
                    return;
                }
                try
                {
                    Append(FileName, bytes);
                    if (WorkItemReviewCategory.FollowUpFiles.TryGetValue(entry.Category, out var followUp))
                    {
                        Append(followUp, bytes);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Could not write to the work item review log {ReviewLogPath}", FilePath);
                    return;
                }
                _categoryCounts.TryGetValue(entry.Category, out var count);
                _categoryCounts[entry.Category] = count + 1;
                if (!string.IsNullOrEmpty(entry.SourceId))
                {
                    _latest[entry.SourceId] = entry;
                    if (entry.Category == WorkItemReviewCategory.Resolved)
                    {
                        _openFromEarlierRuns.Remove(entry.SourceId);
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
        /// <see cref="LoadEarlierRuns"/>. Not needed for the review log to be correct.
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
                Directory.CreateDirectory(_folder);
                var lines = _latest.Values
                    .OrderBy(e => int.TryParse(e.SourceId, out var id) ? id : int.MaxValue)
                    .ThenBy(e => e.SourceId, StringComparer.Ordinal)
                    .Select(e => JsonConvert.SerializeObject(e, JsonSettings));
                File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
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
            public bool RevisionsReplayed { get; set; }
            public HashSet<string> Categories { get; } = new HashSet<string>(StringComparer.Ordinal);

            public void Dispose()
            {
                _owner._current.Value = _parent;
            }
        }
    }
}
