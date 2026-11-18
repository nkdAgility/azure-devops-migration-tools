using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Services;

namespace MigrationTools.Tests.Clients
{
    [TestClass]
    public class TfsWorkItemMigrationClientTests
    {
        [TestMethod, TestCategory("L0")]
        public void GetIfExists_ReturnsTheWorkItem()
        {
            var item = new WorkItemData { Id = "7" };

            Assert.AreSame(item, TfsWorkItemMigrationClient.GetIfExists(7, _ => item));
        }

        [TestMethod, TestCategory("L0")]
        public void GetIfExists_ReturnsNull_WhenTheWorkItemDoesNotExistOrCannotBeRead()
        {
            // TF26198, raised for a link target that is deleted or in a project the identity cannot read (#3271).
            var result = TfsWorkItemMigrationClient.GetIfExists(7, _ => throw new DeniedOrNotExistException());

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("L0")]
        public void GetIfExists_Throws_OtherErrors()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                TfsWorkItemMigrationClient.GetIfExists(7, _ => throw new InvalidOperationException("server unavailable")));
        }

        [TestMethod, TestCategory("L0")]
        public void ActivitySourceProvider_HasNoFlushTelemetery()
        {
            // FlushTelemetery threw NotImplementedException from error paths and hid the real error (#3271).
            Assert.DoesNotContain("FlushTelemetery", typeof(ActivitySourceProvider).GetMethods().Select(m => m.Name));
        }
    }
}
