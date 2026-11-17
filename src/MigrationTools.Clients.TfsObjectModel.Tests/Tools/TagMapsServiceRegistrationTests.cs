using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Tests;
using MigrationTools.Tools;

namespace MigrationTools.Clients.TfsObjectModel.Tests.Tools
{
    [TestClass]
    public class TagMapsServiceRegistrationTests
    {
        [TestMethod, TestCategory("L0")]
        public void TagMaps_ResolveFromDI()
        {
            var services = ServiceProviderHelper.GetServices();
            Assert.IsNotNull(services.GetService<TagFilterMap>());
            Assert.IsNotNull(services.GetService<TagMappingMap>());
        }

        [TestMethod, TestCategory("L0")]
        public void TagFilterMap_Configure_RejectsInvalidRegex()
        {
            var map = ServiceProviderHelper.GetServices().GetRequiredService<TagFilterMap>();
            var options = new TagFilterMapOptions { includeTags = new List<string> { "regex:(" } };
            Assert.ThrowsExactly<ArgumentException>(() => map.Configure(options));
        }

        [TestMethod, TestCategory("L0")]
        public void TagMappingMap_Configure_RejectsEmptyTarget()
        {
            var map = ServiceProviderHelper.GetServices().GetRequiredService<TagMappingMap>();
            var options = new TagMappingMapOptions { mappings = new List<TagMapping> { new TagMapping { source = "A", target = "" } } };
            Assert.ThrowsExactly<ArgumentException>(() => map.Configure(options));
        }
    }
}
