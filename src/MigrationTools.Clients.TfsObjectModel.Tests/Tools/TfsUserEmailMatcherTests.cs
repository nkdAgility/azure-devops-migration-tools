using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tools;
using Newtonsoft.Json;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsUserEmailMatcherTests
    {
        private static IdentityItemData U(string name, string mail) => new() { DisplayName = name, MailAddress = mail };

        private static readonly List<IdentityItemData> Targets =
        [
            U("Jane Doe (New)", "jane.doe@new.com"),
            U("John Smith", "john.smith@new.com"),
            U("John Smith 2", "john.smith@other.com"),
            U("No Mail", null),
        ];

        [TestMethod, TestCategory("L0")]
        public void ExactEmailMatch_StillWorks()
        {
            var r = TfsUserEmailMatcher.FindTarget(U("Jane", "JANE.DOE@new.com"), Targets, null, false);
            Assert.AreEqual("Jane Doe (New)", r?.DisplayName);
        }

        [TestMethod, TestCategory("L0")]
        public void DomainChange_WithoutOptions_NoMatch()
        {
            Assert.IsNull(TfsUserEmailMatcher.FindTarget(U("Jane", "jane.doe@old.com"), Targets, null, false));
        }

        [TestMethod, TestCategory("L0")]
        public void DomainMapping_RewritesDomain()
        {
            var map = new Dictionary<string, string> { { "OLD.com", "@new.com" } };
            var r = TfsUserEmailMatcher.FindTarget(U("Jane", "jane.doe@old.com"), Targets, map, false);
            Assert.AreEqual("Jane Doe (New)", r?.DisplayName);
        }

        [TestMethod, TestCategory("L0")]
        public void DomainMapping_DisambiguatesLocalPartCollision()
        {
            var map = new Dictionary<string, string> { { "old.com", "new.com" } };
            var r = TfsUserEmailMatcher.FindTarget(U("John", "john.smith@old.com"), Targets, map, true);
            Assert.AreEqual("John Smith", r?.DisplayName);
        }

        [TestMethod, TestCategory("L0")]
        public void LocalPart_MatchesUnique()
        {
            var r = TfsUserEmailMatcher.FindTarget(U("Jane", "jane.doe@old.com"), Targets, null, true);
            Assert.AreEqual("Jane Doe (New)", r?.DisplayName);
        }

        [TestMethod, TestCategory("L0")]
        public void LocalPart_Ambiguous_ReturnsNull()
        {
            Assert.IsNull(TfsUserEmailMatcher.FindTarget(U("John", "john.smith@old.com"), Targets, null, true));
        }

        [TestMethod, TestCategory("L0")]
        public void NoSourceEmail_ReturnsNull()
        {
            Assert.IsNull(TfsUserEmailMatcher.FindTarget(U("X", ""), Targets, null, true));
        }

        [TestMethod, TestCategory("L0")]
        public void Options_Deserialize_NewSettings_AndDefaultsAreBackwardsCompatible()
        {
            var o = JsonConvert.DeserializeObject<TfsUserMappingToolOptions>(
                "{\"MatchUsersByEmail\":true,\"EmailDomainMappings\":{\"old.com\":\"new.com\"},\"MatchUsersByEmailLocalPart\":true}");
            Assert.AreEqual("new.com", o.EmailDomainMappings["old.com"]);
            Assert.IsTrue(o.MatchUsersByEmailLocalPart);
            var d = JsonConvert.DeserializeObject<TfsUserMappingToolOptions>("{}");
            Assert.IsNull(d.EmailDomainMappings);
            Assert.IsFalse(d.MatchUsersByEmailLocalPart);
        }
    }
}
