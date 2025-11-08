using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.Tests
{
    [TestClass()]
    public class WorkItemMigrationClientTests
    {
        [TestMethod(), TestCategory("L0")]
        public void WorkItemMigrationClientGetWorkItemsTest()
        {
            IWorkItemMigrationClient sink = new WorkItemMigrationClientMock();
            var list = sink.GetWorkItems();
            //
            Assert.HasCount(5, list);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemMigrationClientPersistNewWorkItemTest()
        {
            IWorkItemMigrationClient sink = new WorkItemMigrationClientMock();
            sink.PersistWorkItem(new WorkItemData { Title = "Item 6" });
            //
            var list = sink.GetWorkItems();
            Assert.HasCount(6, list);
        }

        [TestMethod(), TestCategory("L0")]
        public void WorkItemMigrationClientPersistExistingItemTest()
        {
            IWorkItemMigrationClient sink = new WorkItemMigrationClientMock();
            var workItem = sink.GetWorkItems().First();
            workItem.Title = "New Title";
            sink.PersistWorkItem(workItem);
            //
            var list = sink.GetWorkItems();
            Assert.HasCount(5, list);
            var updatedworkItem = sink.GetWorkItems().First();
            Assert.AreEqual(workItem.Title, updatedworkItem.Title);
        }
    }
}
