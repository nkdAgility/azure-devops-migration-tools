using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Tests;

namespace MigrationTools.Clients.TfsObjectModel.Tests.Tools
{
    [TestClass]
    public class FieldClearMapServiceRegistrationTests
    {
        [TestMethod, TestCategory("L0")]
        public void FieldClearMap_ServiceRegistration_ShouldResolveFromDI()
        {
            // Arrange
            var services = ServiceProviderHelper.GetServices();

            // Act & Assert - This will throw if FieldClearMap is not registered
            var fieldClearMap = services.GetService<FieldClearMap>();
            Assert.IsNotNull(fieldClearMap, "FieldClearMap should be registered in the DI container and resolvable");
        }
    }
}