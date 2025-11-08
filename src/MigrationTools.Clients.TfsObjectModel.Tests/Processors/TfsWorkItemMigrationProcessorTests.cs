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

    }
}
