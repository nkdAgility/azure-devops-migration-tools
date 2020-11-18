using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.Tests
{
    [TestClass()]
    public class WorkItemMigrationClientTests
    {
        [TestMethod(), TestCategory("L0")]
        public void TestGetWorkItems()
        {
            IWorkItemMigrationClient sink = new WorkItemMigrationClientMock();
            var list = sink.GetWorkItems();
            //
            Assert.IsTrue(list.Count() == 5);
        }

        [TestMethod(), TestCategory("L0")]
        public void TestPersistNewWorkItem()
        {
            IWorkItemMigrationClient sink = new WorkItemMigrationClientMock();
            sink.PersistWorkItem(new WorkItemData { Title = "Item 6" });
            //
            var list = sink.GetWorkItems();
            Assert.IsTrue(list.Count() == 6);
        }

        [TestMethod(), TestCategory("L0")]
        public void TestpersistExistingItem()
        {
            IWorkItemMigrationClient sink = new WorkItemMigrationClientMock();
            var workItem = sink.GetWorkItems().First();
            workItem.Title = "New Title";
            sink.PersistWorkItem(workItem);
            //
            var list = sink.GetWorkItems();
            Assert.IsTrue(list.Count() == 5);
            var updatedworkItem = sink.GetWorkItems().First();
            Assert.IsTrue(updatedworkItem.Title == workItem.Title);
        }
    }
}