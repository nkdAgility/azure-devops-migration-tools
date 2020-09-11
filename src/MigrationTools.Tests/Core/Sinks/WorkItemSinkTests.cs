using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Sinks;
using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace MigrationTools.Core.Sinks.Tests
{
    [TestClass()]
    public class WorkItemSinkTests
    {
        [TestMethod()]
        public void TestGetWorkItems()
        {
            IWorkItemSink sink = new WorkItemSinkStub();
            var list = sink.GetWorkItems();
            //
            Assert.IsTrue(list.Count() == 5);
        }

        [TestMethod()]
        public void TestPersistNewWorkItem()
        {
            IWorkItemSink sink = new WorkItemSinkStub();
            sink.PersistWorkItem(new WorkItemData { title = "Item 6" });
            //
            var list = sink.GetWorkItems();
            Assert.IsTrue(list.Count() == 6);
        }

        [TestMethod()]
        public void TestpersistExistingItem()
        {
            IWorkItemSink sink = new WorkItemSinkStub();
            var workItem = sink.GetWorkItems().First();
            workItem.title = "New Title";
            sink.PersistWorkItem(workItem);
            //
            var list = sink.GetWorkItems();
            Assert.IsTrue(list.Count() == 5);
            var updatedworkItem = sink.GetWorkItems().First();
            Assert.IsTrue(updatedworkItem.title == workItem.title );
        }

    }
}
