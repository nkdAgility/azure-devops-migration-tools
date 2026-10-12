using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class SharedItemsFirstTests
    {
        private static WorkItemData Item(string id, string type) => new WorkItemData { Id = id, Type = type };

        [TestMethod, TestCategory("L0")]
        public void SharedItemsFirst_MovesSharedStepsAndParametersAheadOfTestCases_KeepingQueryOrderOtherwise()
        {
            var items = new List<WorkItemData>
            {
                Item("1", "Test Case"),
                Item("2", "Shared Steps"),
                Item("3", "Bug"),
                Item("4", "Shared Parameter"),
                Item("5", "Test Case"),
                Item("6", "shared steps"),
            };

            var ordered = TfsWorkItemMigrationProcessor.SharedItemsFirst(items);

            CollectionAssert.AreEqual(new[] { "2", "4", "6", "1", "3", "5" }, ordered.Select(i => i.Id).ToArray());
        }
    }
}
