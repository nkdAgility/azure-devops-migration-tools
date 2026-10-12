using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Options;
using MigrationTools.Tools;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Tests.Options
{
    [TestClass]
    public class OptionsConfigurationUpgraderTests
    {
        private const string V160Config = @"{
  ""MigrationTools"": {
    ""Version"": ""16.0"",
    ""Endpoints"": {
      ""Source"": { ""EndpointType"": ""TfsTeamProjectEndpoint"", ""Collection"": ""https://dev.azure.com/source/"", ""Project"": ""SourceProject"" },
      ""Target"": { ""EndpointType"": ""TfsTeamProjectEndpoint"", ""Collection"": ""https://dev.azure.com/target/"", ""Project"": ""TargetProject"" }
    },
    ""CommonTools"": {
      ""TfsWorkItemTypeValidatorTool"": {
        ""Enabled"": true,
        ""ExcludeWorkItemtypes"": [ ""Program Epic"" ],
        ""ExcludeSourceFields"": { ""*"": [ ""Custom.FieldA"", ""Custom.FieldB"" ] }
      },
      ""TfsChangeSetMappingTool"": {
        ""Enabled"": false
      }
    },
    ""Processors"": []
  }
}";

        private const string V150Config = @"{
  ""Version"": ""15.0"",
  ""ChangeSetMappingFile"": ""C:\\temp\\changesetmapping.json"",
  ""Source"": { ""$type"": ""TfsTeamProjectConfig"", ""Collection"": ""https://dev.azure.com/source/"", ""Project"": ""SourceProject"" },
  ""Target"": { ""$type"": ""TfsTeamProjectConfig"", ""Collection"": ""https://dev.azure.com/target/"", ""Project"": ""TargetProject"" },
  ""FieldMaps"": [],
  ""Processors"": []
}";

        private static JObject Upgrade(string json)
        {
            var file = Path.GetTempFileName();
            try
            {
                File.WriteAllText(file, json);
                var configuration = new ConfigurationBuilder().Build();
                var services = new ServiceCollection().BuildServiceProvider();
                var logger = NullLogger<OptionsConfigurationBuilder>.Instance;
                var builder = new OptionsConfigurationBuilder(configuration, logger, null);
                var upgrader = new OptionsConfigurationUpgrader(configuration, logger, null, services);
                upgrader.UpgradeConfiguration(builder, file);
                return JObject.Parse(builder.Build());
            }
            finally
            {
                File.Delete(file);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void UpgradeConfiguration_V160_DoesNotDuplicateListEntries()
        {
            // Ensures the TfsWorkItemTypeValidatorToolOptions assembly is loaded for type discovery.
            Assert.IsNotNull(typeof(TfsWorkItemTypeValidatorToolOptions));

            var result = Upgrade(V160Config);

            var tool = result["MigrationTools"]["CommonTools"]["TfsWorkItemTypeValidatorTool"];
            Assert.IsNotNull(tool, result.ToString());
            CollectionAssert.AreEqual(new[] { "Program Epic" }, tool["ExcludeWorkItemTypes"].Values<string>().ToArray());
            CollectionAssert.AreEqual(new[] { "Custom.FieldA", "Custom.FieldB" }, tool["ExcludeSourceFields"]["*"].Values<string>().ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void UpgradeConfiguration_V160_DisabledChangeSetMappingTool_DoesNotEmitNullFile()
        {
            Assert.IsNotNull(typeof(TfsChangeSetMappingToolOptions));

            var result = Upgrade(V160Config);

            var tool = (JObject)result["MigrationTools"]["CommonTools"]["TfsChangeSetMappingTool"];
            Assert.IsNotNull(tool, result.ToString());
            Assert.IsFalse(tool.Value<bool>("Enabled"));
            Assert.IsNull(tool.Property("ChangeSetMappingFile"), tool.ToString());
        }

        [TestMethod, TestCategory("L0")]
        public void UpgradeConfiguration_V150_MapsChangeSetMappingFile()
        {
            Assert.IsNotNull(typeof(TfsChangeSetMappingToolOptions));

            var result = Upgrade(V150Config);

            var tool = result["MigrationTools"]["CommonTools"]["TfsChangeSetMappingTool"];
            Assert.IsNotNull(tool, result.ToString());
            Assert.IsTrue(tool.Value<bool>("Enabled"));
            Assert.AreEqual(@"C:\temp\changesetmapping.json", tool.Value<string>("ChangeSetMappingFile"));
        }
    }
}
