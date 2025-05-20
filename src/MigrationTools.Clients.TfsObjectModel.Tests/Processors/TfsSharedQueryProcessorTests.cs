using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;
using MigrationTools.Processors.Infrastructure;
using System.Reflection;
using System;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsSharedQueryProcessorTests : TfsProcessorTests
    {
        [TestMethod("TfsSharedQueryProcessorTests_Incantate"), TestCategory("L0")]
        public void Incantate()
        {
            var x = GetTfsSharedQueryProcessor();
            Assert.IsNotNull(x);
        }

        [TestMethod("TfsSharedQueryProcessorTests_BasicConfigure"), TestCategory("L0")]
        public void BasicConfigure()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                RefName = "fortyTwo",
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = GetTfsSharedQueryProcessor(y);
            Assert.IsNotNull(x);
            Assert.AreEqual("fortyTwo", x.Options.RefName);
        }

        [TestMethod("TfsSharedQueryProcessorTests_BasicRun"), TestCategory("L0")]
        public void BasicRun()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = GetTfsSharedQueryProcessor(y);
            x.Execute();
            Assert.IsNotNull(x);
        }
        
        [TestMethod("TfsSharedQueryProcessorTests_PreserveMacros"), TestCategory("L0")]
        public void PreserveMacros_WithCurrentIterationAndAreaPath_PreservesQueryStructure()
        {
            // This test verifies that @CurrentIteration macros with area path selections are preserved
            var processor = GetTfsSharedQueryProcessor();
            
            // The original query text with both @CurrentIteration and area path
            string originalQuery = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] = 'ProjectArea' AND [System.IterationPath] = @CurrentIteration";
            
            // Use reflection to access the private PreserveMacros method
            MethodInfo preserveMacrosMethod = typeof(TfsSharedQueryProcessor).GetMethod("PreserveMacros", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Invoke the method to test it
            string result = (string)preserveMacrosMethod.Invoke(processor, new object[] { originalQuery });
            
            // Verify that the area path condition is preserved in the result
            Assert.IsTrue(result.Contains("[System.AreaPath]"), "Area path condition should be preserved");
            Assert.IsTrue(result.Contains("@CurrentIteration"), "@CurrentIteration macro should be preserved");
        }
        
        [TestMethod("TfsSharedQueryProcessorTests_PreserveMacros_Reordering"), TestCategory("L0")]
        public void PreserveMacros_ReordersConditionsCorrectly()
        {
            // This test verifies that conditions are properly reordered when area path comes after iteration path
            var processor = GetTfsSharedQueryProcessor();
            
            // Original query with iteration path before area path (which causes the issue)
            string originalQuery = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.IterationPath] = @CurrentIteration AND [System.AreaPath] = 'ProjectArea'";
            
            // Use reflection to access the private PreserveMacros method
            MethodInfo preserveMacrosMethod = typeof(TfsSharedQueryProcessor).GetMethod("PreserveMacros", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Invoke the method to test it
            string result = (string)preserveMacrosMethod.Invoke(processor, new object[] { originalQuery });
            
            // Verify that the area path condition is preserved and now comes before iteration path
            Assert.IsTrue(result.Contains("[System.AreaPath]"), "Area path condition should be preserved");
            Assert.IsTrue(result.Contains("@CurrentIteration"), "@CurrentIteration macro should be preserved");
            
            // Check that area path now comes before iteration path in the reordered query
            int areaPathIndex = result.IndexOf("[System.AreaPath]");
            int iterationPathIndex = result.IndexOf("[System.IterationPath]");
            
            Assert.IsTrue(areaPathIndex < iterationPathIndex, 
                "Area path condition should come before iteration path in the reordered query");
        }
    }
}