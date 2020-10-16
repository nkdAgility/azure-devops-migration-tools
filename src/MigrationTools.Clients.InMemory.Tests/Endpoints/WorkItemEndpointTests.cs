using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Clients.InMemory.Endpoints.Tests
{
    [TestClass()]
    public class WorkItemEndpointTests
    {
        [TestMethod()]
        public void WorkItemEndpointEmptyTest()
        {
            WorkItemEndpoint e = new WorkItemEndpoint();
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod]
        public void WorkItemEndpointConfiguredTest()
        {
            WorkItemEndpoint e = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, "10", null);
            e.Configure(query);
            Assert.AreEqual(10, e.Count);
        }
    }
}