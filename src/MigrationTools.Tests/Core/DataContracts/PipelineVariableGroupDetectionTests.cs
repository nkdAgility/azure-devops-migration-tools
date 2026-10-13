using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts.Pipelines;

namespace MigrationTools.DataContracts.Tests
{
    [TestClass()]
    public class PipelineVariableGroupDetectionTests
    {
        private static ReleaseDefinition Release(int[] definitionGroups, params int[][] environmentGroups)
        {
            var environments = new Environment[environmentGroups.Length];
            for (int i = 0; i < environmentGroups.Length; i++)
            {
                environments[i] = new Environment { VariableGroups = environmentGroups[i] };
            }
            return new ReleaseDefinition { VariableGroups = definitionGroups, Environments = environments };
        }

        [TestMethod(), TestCategory("L0")]
        public void Release_EmptyGroupArrays_HasNoVariableGroups()
        {
            Assert.IsFalse(Release(new int[0], new int[0], new int[0]).HasVariableGroups());
        }

        [TestMethod(), TestCategory("L0")]
        public void Release_NullGroupArrays_HasNoVariableGroups()
        {
            Assert.IsFalse(Release(null, new int[][] { null }).HasVariableGroups());
        }

        [TestMethod(), TestCategory("L0")]
        public void Release_DefinitionGroup_HasVariableGroups()
        {
            Assert.IsTrue(Release(new[] { 1 }, new int[0]).HasVariableGroups());
        }

        [TestMethod(), TestCategory("L0")]
        public void Release_EnvironmentGroup_HasVariableGroups()
        {
            Assert.IsTrue(Release(new int[0], new int[0], new[] { 2 }).HasVariableGroups());
        }

        [TestMethod(), TestCategory("L0")]
        public void Build_NoGroups_HasNoVariableGroups()
        {
            Assert.IsFalse(new BuildDefinition().HasVariableGroups());
            Assert.IsFalse(new BuildDefinition { VariableGroups = new VariableGroupList[0] }.HasVariableGroups());
        }

        [TestMethod(), TestCategory("L0")]
        public void Build_Group_HasVariableGroups()
        {
            Assert.IsTrue(new BuildDefinition { VariableGroups = new[] { new VariableGroupList() } }.HasVariableGroups());
        }
    }
}
