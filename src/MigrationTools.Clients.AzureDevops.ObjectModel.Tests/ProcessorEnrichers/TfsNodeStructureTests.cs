using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;
using MigrationTools.TestExtensions;
using System.Threading.Tasks;
using System;
using MigrationTools.Tools;

namespace MigrationTools.ProcessorEnrichers.Tests
{
    [TestClass()]
    public class TfsNodeStructureToolTests
    {

        [TestMethod(), TestCategory("L0")]
        public void GetTfsNodeStructureTool_WithDifferentAreaPath()
        {
            var options = new TfsNodeStructureToolOptions();
            options.Enabled = true;
            options.Areas = new NodeOptions()
            {
                Mappings = new Dictionary<string, string>()
                {
                    { @"^SourceProject\\PUL", "TargetProject\\test\\PUL" }
                }
            };
            var nodeStructure = GetTfsNodeStructureTool(options);

            nodeStructure.ApplySettings(new TfsNodeStructureToolSettings
            {
                SourceProjectName = "SourceProject",
                TargetProjectName = "TargetProject",
                FoundNodes = new Dictionary<string, bool>
                {
                    { @"TargetProject\Area\test\PUL", true }
                }
            });

            const string sourceNodeName = @"SourceProject\PUL";
            const TfsNodeStructureType nodeStructureType = TfsNodeStructureType.Area;

            var newNodeName = nodeStructure.GetNewNodeName(sourceNodeName, nodeStructureType);

            Assert.AreEqual(@"TargetProject\test\PUL", newNodeName);

        }

        [TestMethod, TestCategory("L0")]
        public void TestFixAreaPath_WhenNoAreaPathOrIterationPath_DoesntChangeQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }


        [TestMethod, TestCategory("L0")]
        public void TestFixAreaPath_WhenAreaPathInQuery_ChangesQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();


            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'TargetServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L1")]
        public void TestFixAreaPath_WhenAreaPathInQuery_WithPrefixProjectToNodesEnabled_ChangesQuery()
        {
            var options = new TfsNodeStructureToolOptions();
            options.Enabled = true;
            options.Areas = new NodeOptions()
            {
                Mappings = new Dictionary<string, string>()
                {
                    { "^SourceServer\\\\(.*)" , "TargetServer\\SourceServer\\$1" }
                }
            };
            options.Iterations = new NodeOptions()
            {
                Mappings = new Dictionary<string, string>()
                            {
                                { "^SourceServer\\\\(.*)" , "TargetServer\\SourceServer\\$1" }
                            }
            };
            var nodeStructure = GetTfsNodeStructureTool(options);

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'TargetServer\SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQuery = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQuery);
        }

        [TestMethod, TestCategory("L1")]
        public void TestFixAreaPath_WhenAreaPathInQuery_WithPrefixProjectToNodesDisabled_SupportsWhitespaces()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'TargetServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L1")]
        public void TestFixAreaPath_WhenAreaPathInQuery_WithPrefixProjectToNodesEnabled_SupportsWhitespaces()
        {
            var options = new TfsNodeStructureToolOptions();
            options.Enabled = true;
            options.Areas = new NodeOptions()
            {
                Mappings = new Dictionary<string, string>()
                {
                    { "^Source Project\\\\(.*)" , "Target Project\\Source Project\\$1" }
                }
            };

            options.Iterations = new NodeOptions()
            {
                Mappings = new Dictionary<string, string>()
                {
                    { "^Source Project\\\\(.*)" , "Target Project\\Source Project\\$1" }
                }
            };
            var settings = new TfsNodeStructureToolSettings() { SourceProjectName = "Source Project", TargetProjectName = "Target Project", FoundNodes = new Dictionary<string, bool>() };
            var nodeStructure = GetTfsNodeStructureTool(options, settings);

            var WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'Source Project\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            var expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'Target Project\Source Project\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            var targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "Source Project", "Target Project", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L0")]
        public void TestFixAreaPath_WhenMultipleAreaPathInQuery_ChangesQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L0")]
        public void TestFixAreaPath_WhenAreaPathAtEndOfQuery_ChangesQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'SourceServer\Area\Path1'";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'TargetServer\Area\Path1'";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L0")]
        public void TestFixIterationPath_WhenInQuery_ChangesQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.IterationPath] = 'SourceServer\Iteration\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.IterationPath] = 'TargetServer\Iteration\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L0")]
        public void TestFixAreaPathAndIteration_WhenMultipleOccuranceInQuery_ChangesQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND ([System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2') AND ([System.IterationPath] = 'SourceServer\Iteration\Path1' OR [System.IterationPath] = 'SourceServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND ([System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2') AND ([System.IterationPath] = 'TargetServer\Iteration\Path1' OR [System.IterationPath] = 'TargetServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod, TestCategory("L0")]
        public void TestFixAreaPathAndIteration_WhenMultipleOccuranceWithMixtureOrEqualAndUnderOperatorsInQuery_ChangesQuery()
        {
            var nodeStructure = GetTfsNodeStructureTool();

            string WIQLQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND ([System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] UNDER 'SourceServer\Area\Path2') AND ([System.IterationPath] UNDER 'SourceServer\Iteration\Path1' OR [System.IterationPath] = 'SourceServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND ([System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] UNDER 'TargetServer\Area\Path2') AND ([System.IterationPath] UNDER 'TargetServer\Iteration\Path1' OR [System.IterationPath] = 'TargetServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = nodeStructure.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        private static TfsNodeStructureTool GetTfsNodeStructureTool(TfsNodeStructureToolOptions options)
        {
            if (options == null)
            {
                throw new Exception();
            }
            var settings = new TfsNodeStructureToolSettings() { SourceProjectName = "SourceProject", TargetProjectName = "TargetProject", FoundNodes = new Dictionary<string, bool>() };
            return GetTfsNodeStructureTool(options, settings);
        }

        private static TfsNodeStructureTool GetTfsNodeStructureTool()
        {
            var options = new TfsNodeStructureToolOptions() { Enabled = true, Areas = new NodeOptions { Mappings = new Dictionary<string, string>() }, Iterations = new NodeOptions { Mappings = new Dictionary<string, string>() } };
            var settings = new TfsNodeStructureToolSettings() { SourceProjectName = "SourceServer", TargetProjectName = "TargetServer", FoundNodes = new Dictionary<string, bool>() };
            return GetTfsNodeStructureTool(options, settings);
        }

        private static TfsNodeStructureTool GetTfsNodeStructureTool(TfsNodeStructureToolOptions options, TfsNodeStructureToolSettings settings)
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            services.AddSingleton<TfsNodeStructureTool>();
            services.Configure<TfsNodeStructureToolOptions>(o =>
            {
                o.Enabled = options.Enabled;
                o.Areas = options.Areas;
                o.Iterations = options.Iterations;
            });

            var nodeStructure = services.BuildServiceProvider().GetService<TfsNodeStructureTool>();

            nodeStructure.ApplySettings(new TfsNodeStructureToolSettings
            {
                SourceProjectName = settings.SourceProjectName,
                TargetProjectName = settings.TargetProjectName,
                FoundNodes = settings.FoundNodes
            });

            return nodeStructure;
        }

    }
}