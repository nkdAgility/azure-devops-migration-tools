using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Processors;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsUserIdentityReferenceTests
    {
        private static IdentityItemData U(string name, string account) => new() { DisplayName = name, AccountName = account };

        private static Dictionary<string, string> Map(params (string Key, string Value)[] entries)
        {
            var map = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var (key, value) in entries)
            {
                map[key] = value;
            }
            return map;
        }

        [TestMethod, TestCategory("L0")]
        public void TryParse_DisplayNameWithUpn()
        {
            Assert.IsTrue(TfsUserIdentityReference.TryParse(" John Smith <john.smith@new.com> ", out var name, out var unique));
            Assert.AreEqual("John Smith", name);
            Assert.AreEqual("john.smith@new.com", unique);
        }

        [TestMethod, TestCategory("L0")]
        public void TryParse_PlainDisplayName_ReturnsFalse()
        {
            Assert.IsFalse(TfsUserIdentityReference.TryParse("John Smith", out _, out _));
            Assert.IsFalse(TfsUserIdentityReference.TryParse("John Smith <>", out _, out _));
        }

        [TestMethod, TestCategory("L0")]
        public void Format_PrefersAccountName_ThenMail()
        {
            Assert.AreEqual("Jane <jane@new.com>", TfsUserIdentityReference.Format(U("Jane", "jane@new.com")));
            Assert.AreEqual("Jane <j@mail.com>", TfsUserIdentityReference.Format(new IdentityItemData { DisplayName = "Jane", MailAddress = "j@mail.com" }));
            Assert.AreEqual("Jane", TfsUserIdentityReference.Format(new IdentityItemData { DisplayName = "Jane" }));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_PlainKey_StillWorks()
        {
            var map = Map(("Jane Doe", "Jane Doe (New)"));
            Assert.AreEqual("Jane Doe (New)", TfsUserIdentityReference.FindMappedTarget(map, "jane doe"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_ValueWithUpn_IsReturnedVerbatim()
        {
            var map = Map(("Jane Doe", "Jane Doe <jane.doe@new.com>"));
            Assert.AreEqual("Jane Doe <jane.doe@new.com>", TfsUserIdentityReference.FindMappedTarget(map, "Jane Doe"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_QualifiedSource_MatchesKeyByUpn()
        {
            var map = Map(("John Smith <john.smith@old.com>", "John Smith <john.smith@new.com>"),
                          ("John Smith <jsmith@old.com>", "John Smith <jsmith@new.com>"));
            Assert.AreEqual("John Smith <jsmith@new.com>", TfsUserIdentityReference.FindMappedTarget(map, "John Smith <JSMITH@old.com>"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_PlainSource_SingleQualifiedKey_Matches()
        {
            var map = Map(("Jane Doe <jane.doe@old.com>", "Jane Doe <jane.doe@new.com>"));
            Assert.AreEqual("Jane Doe <jane.doe@new.com>", TfsUserIdentityReference.FindMappedTarget(map, "Jane Doe"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_PlainSource_AmbiguousQualifiedKeys_ReturnsNull()
        {
            var map = Map(("John Smith <john.smith@old.com>", "A"), ("John Smith <jsmith@old.com>", "B"));
            Assert.IsNull(TfsUserIdentityReference.FindMappedTarget(map, "John Smith"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_QualifiedSource_FallsBackToPlainKey()
        {
            var map = Map(("Jane Doe", "Jane Doe (New)"));
            Assert.AreEqual("Jane Doe (New)", TfsUserIdentityReference.FindMappedTarget(map, "Jane Doe <jane.doe@old.com>"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMappedTarget_NoMatch_ReturnsNull()
        {
            Assert.IsNull(TfsUserIdentityReference.FindMappedTarget(Map(("A", "B")), "C"));
            Assert.IsNull(TfsUserIdentityReference.FindMappedTarget(null, "C"));
        }

        [TestMethod, TestCategory("L0")]
        public void BuildSimpleDictionary_QualifiesOnlyDuplicateDisplayNames()
        {
            var jsOld = U("John Smith", "john.smith@old.com");
            var js2Old = U("John Smith", "jsmith@old.com");
            var janeOld = U("Jane Doe", "jane.doe@old.com");
            var jsNew = U("John Smith", "john.smith@new.com");
            var js2New = U("John Smith", "jsmith@new.com");
            var janeNew = U("Jane D", "jane.doe@new.com");
            var map = new List<IdentityMapData>
            {
                new() { Source = jsOld, Target = jsNew },
                new() { Source = js2Old, Target = js2New },
                new() { Source = janeOld, Target = janeNew },
            };

            var result = TfsExportUsersForMappingProcessor.BuildSimpleDictionary(map, [jsOld, js2Old, janeOld], [jsNew, js2New, janeNew]);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("John Smith <john.smith@new.com>", result["John Smith <john.smith@old.com>"]);
            Assert.AreEqual("John Smith <jsmith@new.com>", result["John Smith <jsmith@old.com>"]);
            Assert.AreEqual("Jane D", result["Jane Doe"]);
        }
    }
}
