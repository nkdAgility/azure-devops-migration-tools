using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Tests;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core.Tests
{
    [TestClass]
    public class WorkItemMigrationTests
    {
        private ServiceProvider _services;
        private WorkItemMigrationContext _underTest;

        [TestInitialize]
        public void Setup()
        {
            _services = ServiceProviderHelper.GetServices();
            var nodeStructure = _services.GetRequiredService<TfsNodeStructure>();
            nodeStructure.ApplySettings(new TfsNodeStructureSettings
            {
                FoundNodes = new Dictionary<string, bool>(),
                SourceProjectName = "Path1",
                TargetProjectName = "Path1",
            });
            nodeStructure.Configure(new TfsNodeStructureOptions
            {
                AreaMaps = new Dictionary<string, string>
                {
                    { "SourceServer", "TargetServer" }
                },
                IterationMaps = new Dictionary<string, string>
                {
                    { "SourceServer", "TargetServer" }
                },
            });

            _underTest = new WorkItemMigrationContext(_services.GetRequiredService<IMigrationEngine>(), _services,
                _services.GetRequiredService<ITelemetryLogger>(),
                _services.GetRequiredService<ILogger<WorkItemMigrationContext>>());
            _underTest.Configure(new WorkItemMigrationConfig
            {
                AreaMaps = new Dictionary<string, string>
                {
                    {"SourceServer", "TargetServer"}
                },
                IterationMaps = new Dictionary<string, string>
                {
                    {"SourceServer", "TargetServer"}
                },
            });
        }

        [TestMethod]
        public void TestFixAreaPath_WhenNoAreaPathOrIterationPath_DoesntChangeQuery()
        {
            string WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }


        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathInQuery_ChangesQuery()
        {
            string WIQLQueryBit =         @"AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenMultipleAreaPathInQuery_ChangesQuery()
        {
            string WIQLQueryBit =         @"AND [System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathAtEndOfQuery_ChangesQuery()
        {
            string WIQLQueryBit =         @"AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'SourceServer\Area\Path1'";
            string expectTargetQueryBit = @"AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'TargetServer\Area\Path1'";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixIterationPath_WhenInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND [System.IterationPath] = 'SourceServer\Iteration\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.IterationPath] = 'TargetServer\Iteration\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPathAndIteration_WhenMultipleOccuranceInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND ([System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2') AND ([System.IterationPath] = 'SourceServer\Iteration\Path1' OR [System.IterationPath] = 'SourceServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND ([System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2') AND ([System.IterationPath] = 'TargetServer\Iteration\Path1' OR [System.IterationPath] = 'TargetServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPathAndIteration_WhenMultipleOccuranceWithMixtureOrEqualAndUnderOperatorsInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND ([System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] UNDER 'SourceServer\Area\Path2') AND ([System.IterationPath] UNDER 'SourceServer\Iteration\Path1' OR [System.IterationPath] = 'SourceServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND ([System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] UNDER 'TargetServer\Area\Path2') AND ([System.IterationPath] UNDER 'TargetServer\Iteration\Path1' OR [System.IterationPath] = 'TargetServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = _underTest.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }
    }
}