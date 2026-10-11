using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsWorkItemMigrationProcessorTests
    {

        [TestMethod(DisplayName = "TfsWorkItemMigrationProcessorTests_OptionsValidator_Empty"), TestCategory("L0")]
        public void OptionsValidator_Empty()
        {
            var validator = new TfsWorkItemMigrationProcessorOptionsValidator();
            var x = new TfsWorkItemMigrationProcessorOptions();
            Assert.IsTrue(validator.Validate(null, x).Failed);
        }

        [TestMethod(DisplayName = "TfsWorkItemMigrationProcessorTests_OptionsValidator_Valid"), TestCategory("L0")]
        public void OptionsValidator_Valid()
        {
            var validator = new TfsWorkItemMigrationProcessorOptionsValidator();
            var x = new TfsWorkItemMigrationProcessorOptions();
            x.WIQLQuery = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject";
            x.SourceName = "source";
            x.TargetName = "target";
            ValidateOptionsResult result = validator.Validate(null, x);
            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod(DisplayName = "TfsWorkItemMigrationProcessorTests_StopOnFirstError_DefaultsToFalse"), TestCategory("L0")]
        public void StopOnFirstError_DefaultsToFalse()
        {
            var x = new TfsWorkItemMigrationProcessorOptions();
            Assert.IsFalse(x.StopOnFirstError);
            Assert.AreEqual(0, x.MaxGracefulFailures);
        }

        [DataTestMethod, TestCategory("L0")]
        [DataRow(false, 0, 1, false, DisplayName = "Default continues after the first failure")]
        [DataRow(false, 0, 500, false, DisplayName = "Default has no failure limit")]
        [DataRow(true, 0, 1, true, DisplayName = "StopOnFirstError aborts on the first failure")]
        [DataRow(true, 10, 1, true, DisplayName = "StopOnFirstError wins over MaxGracefulFailures")]
        [DataRow(false, 3, 3, false, DisplayName = "MaxGracefulFailures tolerates up to the limit")]
        [DataRow(false, 3, 4, true, DisplayName = "MaxGracefulFailures aborts above the limit")]
        public void ShouldAbortAfterFailure(bool stopOnFirstError, int maxGracefulFailures, int failureCount, bool expected)
        {
            Assert.AreEqual(expected, TfsWorkItemMigrationProcessor.ShouldAbortAfterFailure(stopOnFirstError, maxGracefulFailures, failureCount));
        }

    }
}
