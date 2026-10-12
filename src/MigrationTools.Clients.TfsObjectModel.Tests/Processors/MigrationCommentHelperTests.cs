using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class MigrationCommentHelperTests
    {
        private const string Uri = "https://dev.azure.com/org/proj/_workitems/edit/1";

        [TestMethod, TestCategory("L0")]
        public void AppendTo_EmptyHistory_ReturnsComment()
        {
            Assert.AreEqual(MigrationCommentHelper.Comment(Uri), MigrationCommentHelper.AppendTo("", Uri));
            Assert.AreEqual(MigrationCommentHelper.Comment(Uri), MigrationCommentHelper.AppendTo(null, Uri));
        }

        [TestMethod, TestCategory("L0")]
        public void AppendTo_SourceComment_KeepsItFirst()
        {
            var result = MigrationCommentHelper.AppendTo("<p>source comment</p>", Uri);
            Assert.IsTrue(result.StartsWith("<p>source comment</p>"));
            Assert.IsTrue(result.EndsWith(MigrationCommentHelper.Comment(Uri)));
        }

        [TestMethod, TestCategory("L0")]
        public void AppendTo_AlreadyPresent_NotDuplicated()
        {
            var once = MigrationCommentHelper.AppendTo("x", Uri);
            Assert.AreEqual(once, MigrationCommentHelper.AppendTo(once, Uri));
        }
    }
}
