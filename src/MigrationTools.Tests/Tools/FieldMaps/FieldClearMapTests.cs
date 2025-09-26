using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools.FieldMaps
{
    [TestClass]
    public class FieldClearMapTests
    {
        [TestMethod]
        public void FieldClearMapOptions_SetExampleConfigDefaults_ShouldSetCorrectValues()
        {
            // Arrange
            var options = new FieldClearMapOptions();

            // Act
            options.SetExampleConfigDefaults();

            // Assert
            Assert.AreEqual("System.Description", options.targetField);
            CollectionAssert.Contains(options.ApplyTo, "*");
        }
    }
}