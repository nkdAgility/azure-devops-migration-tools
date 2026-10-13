using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class SharedQueryWiqlRewriterTests
    {
        [TestMethod(), TestCategory("L0")]
        public void Rewrite_CurrentIterationTeamContext_PointsAtTargetProject()
        {
            var wiql = @"select [System.Id] from WorkItems where [System.IterationPath] = @CurrentIteration('[Src]\Team B') + 1";
            var result = SharedQueryWiqlRewriter.Rewrite(wiql, "Src", "Tgt", false, null);
            Assert.AreEqual(@"select [System.Id] from WorkItems where [System.IterationPath] = @CurrentIteration('[Tgt]\Team B') + 1", result);
        }

        [TestMethod(), TestCategory("L0")]
        public void Rewrite_IgnoresProjectCasing()
        {
            var wiql = @"[System.AreaPath] under 'src\Area' and [System.IterationPath] = @currentiteration('[SRC]\Team')";
            Assert.AreEqual(@"[System.AreaPath] under 'Tgt\Area' and [System.IterationPath] = @currentiteration('[Tgt]\Team')", SharedQueryWiqlRewriter.Rewrite(wiql, "Src", "Tgt", false, null));
        }

        [TestMethod(), TestCategory("L0")]
        public void Rewrite_LeavesBracketedLiteralsOutsideTeamMacrosAlone()
        {
            var wiql = @"[System.Title] = '[Src]\Team B' and [System.AreaPath] in @TeamAreas('[Src]\Team B')";
            Assert.AreEqual(@"[System.Title] = '[Src]\Team B' and [System.AreaPath] in @TeamAreas('[Tgt]\Team B')", SharedQueryWiqlRewriter.Rewrite(wiql, "Src", "Tgt", false, null));
        }

        [TestMethod(), TestCategory("L0")]
        public void Rewrite_DropsSourceTeamId()
        {
            var wiql = @"= @CurrentIteration('[Src]\Team B <id:daa8efa0-213c-47f3-be65-0fcd439af9ef>')";
            Assert.AreEqual(@"= @CurrentIteration('[Tgt]\Team B')", SharedQueryWiqlRewriter.Rewrite(wiql, "Src", "Tgt", false, null));
        }

        [TestMethod(), TestCategory("L0")]
        public void ReplaceTeam_SwapsTeamInTargetProject()
        {
            var wiql = @"= @CurrentIteration('[Tgt]\Src Team') + 1";
            Assert.AreEqual(@"= @CurrentIteration('[Tgt]\Tgt Team') + 1", SharedQueryWiqlRewriter.ReplaceTeam(wiql, "Tgt", "Src Team", "Tgt Team"));
        }

        [TestMethod(), TestCategory("L0")]
        public void Rewrite_AreaPath_PointsAtTargetProject()
        {
            var wiql = @"select [System.Id] from WorkItems where [System.AreaPath] under 'Src\Area'";
            Assert.AreEqual(@"select [System.Id] from WorkItems where [System.AreaPath] under 'Tgt\Area'", SharedQueryWiqlRewriter.Rewrite(wiql, "Src", "Tgt", false, null));
        }

        [TestMethod(), TestCategory("L0")]
        public void Rewrite_PrefixProjectToNodes_LeavesTeamNameAlone()
        {
            var wiql = @"where [System.AreaPath] under 'Src\Area' and [System.IterationPath] = @CurrentIteration('[Src]\Team')";
            var result = SharedQueryWiqlRewriter.Rewrite(wiql, "Src", "Tgt", true, null);
            Assert.AreEqual(@"where [System.AreaPath] under 'Tgt\Src\Area' and [System.IterationPath] = @CurrentIteration('[Tgt]\Team')", result);
        }

        [TestMethod(), TestCategory("L0")]
        public void Rewrite_AppliesFieldMappings()
        {
            var result = SharedQueryWiqlRewriter.Rewrite("select [Custom.Old] from WorkItems", "Src", "Tgt", false, new Dictionary<string, string> { { "Custom.Old", "Custom.New" } });
            Assert.AreEqual("select [Custom.New] from WorkItems", result);
        }

        [TestMethod(), TestCategory("L0")]
        public void GetReferencedTeams_ReturnsDistinctTeamsForProject()
        {
            var wiql = @"@CurrentIteration('[Tgt]\Team B') and @CurrentIteration('[Tgt]\Team B') - 1 and @TeamAreas('[Other]\X')";
            CollectionAssert.AreEqual(new[] { "Team B" }, SharedQueryWiqlRewriter.GetReferencedTeams(wiql, "Tgt").ToArray());
        }
    }
}
