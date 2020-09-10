using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Sinks;
using System;
using System.Collections.Generic;
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

    class WorkItemSinkStub : IWorkItemSink
    {
        List<WorkItemData> list = new List<WorkItemData>();

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            if (list.Count == 0)
            { 
                PopulateList();
            }
            return list;
        }

        public WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            PopulateList();
            var found = list.Find(x => x.id == workItem.id);
            if (found != null)
            {
                // Add Revission
                found.title = workItem.title;
                return workItem;
            } else
            {
                // Create new
                var newid = list.Max(s => int.Parse(s.id))+1;
                list.Add(new WorkItemData { id = newid.ToString(), title = workItem.title });
                return workItem;
            }
        }

        private void PopulateList()
        {
            list.Clear();
            list.Add(new WorkItemData { id = "1", title = "Item 1" });
            list.Add(new WorkItemData { id = "2", title = "Item 2" });
            list.Add(new WorkItemData { id = "3", title = "Item 3" });
            list.Add(new WorkItemData { id = "4", title = "Item 4" });
            list.Add(new WorkItemData { id = "5", title = "Item 5" });
        }
    }
}
