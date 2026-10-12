using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;
using Rev = System.Collections.Generic.Dictionary<string, object>;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class HistoryOnlyFieldScannerTests
    {
        private static ISet<string> Fields(params string[] names)
            => new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, ISet<string>> SourceTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Product Backlog Item"] = Fields("System.Title", "System.TeamProject", "System.WorkItemType", "Custom.SrcOnly"),
            ["Bug"] = Fields("System.Title", "System.TeamProject", "System.WorkItemType"),
        };

        private static readonly Dictionary<string, ISet<string>> TargetTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Product Backlog Item"] = Fields("System.Title", "System.TeamProject", "System.WorkItemType", "Custom.PbiOnly"),
            ["Bug"] = Fields("System.Title", "System.TeamProject", "System.WorkItemType", "Custom.BugOnly"),
        };

        private static HistoryOnlyFieldScanner Create(
            Dictionary<string, string> mappings = null,
            Func<WorkItemData, RevisionItem, IEnumerable<FieldItem>> getRevisionFields = null,
            Func<string, string, bool> isExcluded = null,
            Func<string, ISet<string>> getSourceTypeFields = null)
            => new HistoryOnlyFieldScanner(
                mappings ?? new Dictionary<string, string>(),
                TargetTypes.ContainsKey,
                getSourceTypeFields ?? (type => SourceTypes.TryGetValue(type, out var f) ? f : null),
                type => TargetTypes.TryGetValue(type, out var f) ? f : null,
                getRevisionFields,
                isExcluded);

        // Revisions are saved under the "old" project unless they set System.TeamProject; the item is now in "src".
        private static WorkItemData WorkItem(string id, string type, params Rev[] revisions)
        {
            var revs = new SortedDictionary<int, RevisionItem>();
            for (int i = 0; i < revisions.Length; i++)
            {
                var values = new Rev(revisions[i]);
                if (!values.ContainsKey("System.TeamProject"))
                {
                    values["System.TeamProject"] = "old";
                }
                revs[i + 1] = new RevisionItem
                {
                    Number = i + 1,
                    Type = values.TryGetValue("System.WorkItemType", out var revType) ? (string)revType : type,
                    Fields = values.ToDictionary(kv => kv.Key, kv => new FieldItem { ReferenceName = kv.Key, Value = kv.Value }),
                };
            }
            return new WorkItemData { Id = id, Type = type, ProjectName = "src", Revisions = revs };
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_ReportsHistoryOnlyField()
        {
            var item = WorkItem("7173", "Product Backlog Item",
                new Rev { ["System.Title"] = "a", ["Custom.HistOnly"] = "old value" },
                new Rev { ["System.Title"] = "b", ["Custom.HistOnly"] = "old value" },
                new Rev { ["System.Title"] = "c", ["System.TeamProject"] = "src" });

            var result = Create().Scan(new[] { item });

            Assert.AreEqual(1, result.Count);
            var line = result.GetSummary()[0];
            StringAssert.StartsWith(line, "Custom.HistOnly");
            StringAssert.Contains(line, "2 revision(s) across 1 work item(s) [7173]");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_LoadsOnlyMovedOrRetypedRevisions()
        {
            var item = WorkItem("1", "Product Backlog Item",
                new Rev { ["Custom.HistOnly"] = "moved" },
                new Rev { ["System.TeamProject"] = "src", ["System.WorkItemType"] = "Bug", ["Custom.HistOnly"] = "retyped" },
                new Rev { ["System.TeamProject"] = "src", ["Custom.HistOnly"] = "current type" });
            var loaded = new List<int>();
            var scanner = Create(getRevisionFields: (wi, rev) => { loaded.Add(rev.Number); return rev.Fields.Values; });

            scanner.Scan(new[] { item });

            CollectionAssert.AreEqual(new[] { 1, 2 }, loaded);
            Assert.AreEqual(2, scanner.RevisionsLoaded);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_IgnoresFieldOnCurrentSourceType()
        {
            // Reported by the type validator and can be rescued by a field map, so not a history-only field.
            var item = WorkItem("1", "Product Backlog Item", new Rev { ["Custom.SrcOnly"] = "x" });

            Assert.AreEqual(0, Create().Scan(new[] { item }).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_IgnoresEmptyValues()
        {
            var item = WorkItem("1", "Product Backlog Item",
                new Rev { ["Custom.HistOnly"] = null },
                new Rev { ["Custom.HistOnly"] = string.Empty });

            Assert.AreEqual(0, Create().Scan(new[] { item }).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_IgnoresFieldOnTargetType()
        {
            var item = WorkItem("1", "Product Backlog Item", new Rev { ["Custom.PbiOnly"] = "x" });

            Assert.AreEqual(0, Create().Scan(new[] { item }).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_UsesWorkItemTypeMapping()
        {
            var scanner = Create(
                mappings: new Dictionary<string, string> { ["User Story"] = "Product Backlog Item" },
                getSourceTypeFields: type => Fields("System.Title", "System.TeamProject"));
            var item = WorkItem("5", "User Story", new Rev { ["Custom.HistOnly"] = "v" });

            var line = scanner.Scan(new[] { item }).GetSummary().Single();

            StringAssert.Contains(line, "source type 'User Story' -> target type 'Product Backlog Item'");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_ComparesEachRevisionWithItsReplayTargetType()
        {
            // Bug -> PBI: replay writes the Bug revision to a target Bug, so BugOnly is kept and PbiOnly is dropped.
            var item = WorkItem("9", "Product Backlog Item",
                new Rev { ["System.TeamProject"] = "src", ["System.WorkItemType"] = "Bug", ["Custom.BugOnly"] = "kept", ["Custom.PbiOnly"] = "dropped" },
                new Rev { ["System.TeamProject"] = "src" });

            var line = Create().Scan(new[] { item }).GetSummary().Single();

            StringAssert.StartsWith(line, "Custom.PbiOnly (source type 'Bug' -> target type 'Bug')");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_KeepsCurrentTargetTypeWhenRevisionTypeIsMissingInTarget()
        {
            // As in replay: a revision type that does not exist in the target keeps the current target type.
            var source = new Dictionary<string, ISet<string>>(SourceTypes, StringComparer.OrdinalIgnoreCase) { ["Issue"] = Fields() };
            var item = WorkItem("10", "Product Backlog Item",
                new Rev { ["System.TeamProject"] = "src" },
                new Rev { ["System.TeamProject"] = "src", ["System.WorkItemType"] = "Issue", ["Custom.PbiOnly"] = "kept", ["Custom.HistOnly"] = "dropped" });

            var line = Create(getSourceTypeFields: t => source.TryGetValue(t, out var f) ? f : null)
                .Scan(new[] { item }).GetSummary().Single();

            StringAssert.StartsWith(line, "Custom.HistOnly (source type 'Issue' -> target type 'Product Backlog Item')");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_SkipsTypesNotValidatedOrMissingInTarget()
        {
            var notValidated = WorkItem("1", "Shared Steps", new Rev { ["Custom.HistOnly"] = "v" });
            var noTarget = Create(mappings: new Dictionary<string, string> { ["Product Backlog Item"] = "Missing" })
                .Scan(new[] { WorkItem("2", "Product Backlog Item", new Rev { ["Custom.HistOnly"] = "v" }) });

            Assert.AreEqual(0, Create().Scan(new[] { notValidated }).Count);
            Assert.AreEqual(0, noTarget.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_ExcludedFieldsAreReportedSeparately()
        {
            var scanner = Create(isExcluded: (type, field) => field == "Custom.HistOnly");
            var item = WorkItem("1", "Product Backlog Item", new Rev { ["Custom.HistOnly"] = "v" });

            Assert.AreEqual(0, scanner.Scan(new[] { item }).Count);
            Assert.AreEqual(1, scanner.Excluded.Count);
        }
    }
}
