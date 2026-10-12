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
        };

        private static readonly Dictionary<string, ISet<string>> TargetTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Product Backlog Item"] = Fields("System.Title", "System.TeamProject"),
        };

        private static HistoryOnlyFieldScanner CreateScanner(Func<string, string, bool> isExcluded = null)
            => new HistoryOnlyFieldScanner(
                type => type,
                type => SourceTypes.TryGetValue(type, out var f) ? f : null,
                type => TargetTypes.TryGetValue(type, out var f) ? f : null,
                isExcluded: isExcluded);

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
        public void Scan_LoadsOnlyMovedOrRetypedRevisions()
        {
            var item = WorkItem("1", "Product Backlog Item",
                new Rev { ["Custom.HistOnly"] = "moved" },
                new Rev { ["System.TeamProject"] = "src", ["System.WorkItemType"] = "Bug", ["Custom.HistOnly"] = "retyped" },
                new Rev { ["System.TeamProject"] = "src", ["Custom.HistOnly"] = "current type" });
            var loaded = new List<int>();
            var scanner = new HistoryOnlyFieldScanner(
                type => type,
                type => SourceTypes.TryGetValue(type, out var f) ? f : null,
                type => TargetTypes.TryGetValue(type, out var f) ? f : null,
                (wi, rev) => { loaded.Add(rev.Number); return rev.Fields.Values; });

            var line = scanner.Scan(new[] { item }).GetSummary().Single();

            CollectionAssert.AreEqual(new[] { 1, 2 }, loaded);
            Assert.AreEqual(2, scanner.RevisionsLoaded);
            StringAssert.Contains(line, "2 revision(s) across 1 work item(s)");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_ReportsHistoryOnlyField()
        {
            var item = WorkItem("7173", "Product Backlog Item",
                new Rev { ["System.Title"] = "a", ["Custom.HistOnly"] = "old value" },
                new Rev { ["System.Title"] = "b", ["Custom.HistOnly"] = "old value" },
                new Rev { ["System.Title"] = "c", ["System.TeamProject"] = "src" });

            var result = CreateScanner().Scan(new[] { item });

            Assert.AreEqual(1, result.Count);
            var line = result.GetSummary()[0];
            StringAssert.StartsWith(line, "Custom.HistOnly");
            StringAssert.Contains(line, "2 revision(s) across 1 work item(s) [7173]");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_IgnoresFieldOnCurrentSourceType()
        {
            // Reported by the type validator and can be rescued by a field map, so not a history-only field.
            var item = WorkItem("1", "Product Backlog Item", new Rev { ["Custom.SrcOnly"] = "x" });

            Assert.AreEqual(0, CreateScanner().Scan(new[] { item }).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_IgnoresEmptyValues()
        {
            var item = WorkItem("1", "Product Backlog Item",
                new Rev { ["Custom.HistOnly"] = null },
                new Rev { ["Custom.HistOnly"] = string.Empty });

            Assert.AreEqual(0, CreateScanner().Scan(new[] { item }).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_IgnoresFieldOnTargetType()
        {
            var item = WorkItem("1", "Product Backlog Item", new Rev { ["System.Title"] = "x" });

            Assert.AreEqual(0, CreateScanner().Scan(new[] { item }).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_UsesWorkItemTypeMapping()
        {
            var scanner = new HistoryOnlyFieldScanner(
                type => type == "User Story" ? "Product Backlog Item" : type,
                type => Fields("System.Title", "System.TeamProject"),
                type => TargetTypes.TryGetValue(type, out var f) ? f : null);
            var item = WorkItem("5", "User Story", new Rev { ["Custom.HistOnly"] = "v" });

            var line = scanner.Scan(new[] { item }).GetSummary().Single();

            StringAssert.Contains(line, "source type 'User Story' -> target type 'Product Backlog Item'");
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_SkipsTypesNotValidatedOrMissingInTarget()
        {
            var notValidated = WorkItem("1", "Shared Steps", new Rev { ["Custom.HistOnly"] = "v" });
            var noTarget = new HistoryOnlyFieldScanner(t => "Missing", t => Fields(), t => null)
                .Scan(new[] { WorkItem("2", "Product Backlog Item", new Rev { ["Custom.HistOnly"] = "v" }) });

            Assert.AreEqual(0, CreateScanner().Scan(new[] { notValidated }).Count);
            Assert.AreEqual(0, noTarget.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Scan_ExcludedFieldsAreReportedSeparately()
        {
            var scanner = CreateScanner((type, field) => field == "Custom.HistOnly");
            var item = WorkItem("1", "Product Backlog Item", new Rev { ["Custom.HistOnly"] = "v" });

            Assert.AreEqual(0, scanner.Scan(new[] { item }).Count);
            Assert.AreEqual(1, scanner.Excluded.Count);
        }
    }
}
