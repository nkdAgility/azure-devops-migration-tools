using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsWorkItemEmbededLinkToolTests
    {
        [TestMethod, TestCategory("L0")]
        public void MapMentionUserName_MappedUser_ReturnsTarget()
        {
            var map = new Dictionary<string, string> { { "Tester", "Test" } };
            Assert.AreEqual("Test", TfsWorkItemEmbededLinkTool.MapMentionUserName("Tester", map));
        }

        [TestMethod, TestCategory("L0")]
        public void MapMentionUserName_IgnoresCase()
        {
            var map = new Dictionary<string, string> { { "Tester", "Test" } };
            Assert.AreEqual("Test", TfsWorkItemEmbededLinkTool.MapMentionUserName("tester", map));
        }

        [TestMethod, TestCategory("L0")]
        public void MapMentionUserName_UnmappedUser_ReturnsOriginal()
        {
            var map = new Dictionary<string, string> { { "Tester", "Test" } };
            Assert.AreEqual("Other", TfsWorkItemEmbededLinkTool.MapMentionUserName("Other", map));
        }

        [TestMethod, TestCategory("L0")]
        public void MapMentionUserName_NoMappings_ReturnsOriginal()
        {
            Assert.AreEqual("Tester", TfsWorkItemEmbededLinkTool.MapMentionUserName("Tester", null));
        }

        [TestMethod, TestCategory("L0")]
        public void MapMentionUserName_IdentityMapDataFormat_MapsDisplayNameAndAccountName()
        {
            var map = TfsUserMappingTool.ToUserMapDictionary(new List<IdentityMapData>
            {
                new IdentityMapData
                {
                    Source = new IdentityItemData { DisplayName = "Tester", AccountName = "DOMAIN\\tester" },
                    Target = new IdentityItemData { DisplayName = "Test User" }
                }
            });
            Assert.AreEqual("Test User", TfsWorkItemEmbededLinkTool.MapMentionUserName("Tester", map));
            Assert.AreEqual("Test User", TfsWorkItemEmbededLinkTool.MapMentionUserName("DOMAIN\\tester", map));
        }
    }
}
