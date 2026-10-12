using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Services.Shadows;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;
using Moq;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class MissingTargetFieldPolicyTests
    {
        private static readonly Dictionary<string, string[]> SourceTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Product Backlog Item"] = new[] { "System.Title", "Custom.SrcOnly" },
        };

        private static IFieldMap FieldToField(string source, string target, FieldMapMode mode = FieldMapMode.SourceToTarget)
        {
            var map = new FieldToFieldMap(NullLogger<FieldToFieldMap>.Instance, new TelemetryLoggerFake());
            map.Configure(new FieldToFieldMapOptions { sourceField = source, targetField = target, fieldMapMode = mode });
            return map;
        }

        private static IFieldMap MultiMap(string source, string target)
        {
            var map = new FieldToFieldMultiMap(NullLogger<FieldToFieldMultiMap>.Instance, new TelemetryLoggerFake());
            map.Configure(new FieldToFieldMultiMapOptions { SourceToTargetMappings = new Dictionary<string, string> { [source] = target } });
            return map;
        }

        private static MissingTargetFieldPolicy Create(Dictionary<string, List<IFieldMap>> mapsBySourceType, List<string> loadedTypes = null)
        {
            var tool = new Mock<IFieldMappingTool>();
            tool.Setup(t => t.GetFieldMappings(It.IsAny<string>()))
                .Returns((string wit) => mapsBySourceType.TryGetValue(wit, out var maps) ? maps : new List<IFieldMap>());
            return new MissingTargetFieldPolicy(
                type =>
                {
                    loadedTypes?.Add(type);
                    return SourceTypes.TryGetValue(type, out var fields) ? fields : Enumerable.Empty<string>();
                },
                tool.Object);
        }

        [TestMethod, TestCategory("L0")]
        public void IsDropped_FieldOnSourceTypeWithFieldToFieldMap_IsNotDropped()
        {
            var policy = Create(new() { ["Product Backlog Item"] = new() { FieldToField("Custom.SrcOnly", "Custom.TgtText") } });

            Assert.IsFalse(policy.IsDropped("Product Backlog Item", "Custom.SrcOnly"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsDropped_FieldOnSourceTypeWithMultiMap_IsNotDropped()
        {
            var policy = Create(new() { ["Product Backlog Item"] = new() { MultiMap("custom.srconly", "Custom.TgtText") } });

            Assert.IsFalse(policy.IsDropped("Product Backlog Item", "Custom.SrcOnly"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsDropped_FieldOnSourceTypeWithoutMap_IsDropped()
        {
            Assert.IsTrue(Create(new()).IsDropped("Product Backlog Item", "Custom.SrcOnly"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsDropped_HistoryOnlyFieldIsDroppedEvenWhenMapped()
        {
            // Field maps validate against the current source type, so they cannot read a history-only field.
            var policy = Create(new() { ["Product Backlog Item"] = new() { FieldToField("Custom.HistOnly", "Custom.TgtText") } });

            Assert.IsFalse(policy.IsFieldOnCurrentSourceType("Product Backlog Item", "Custom.HistOnly"));
            Assert.IsTrue(policy.IsSourceFieldHandledByFieldMap("Product Backlog Item", "Custom.HistOnly"));
            Assert.IsTrue(policy.IsDropped("Product Backlog Item", "Custom.HistOnly"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsDropped_MapForAnotherTypeOrModeDoesNotApply()
        {
            // Maps are selected by the source type (#3151), and only SourceToTarget maps read the source field.
            var policy = Create(new()
            {
                ["Bug"] = new() { FieldToField("Custom.SrcOnly", "Custom.TgtText") },
                ["Product Backlog Item"] = new() { FieldToField("Custom.SrcOnly", "Custom.TgtText", FieldMapMode.TargetToTarget) },
            });

            Assert.IsTrue(policy.IsDropped("Product Backlog Item", "Custom.SrcOnly"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsFieldOnCurrentSourceType_TypeMissingFromSourceProject_IsFalse()
        {
            Assert.IsFalse(Create(new()).IsFieldOnCurrentSourceType("Issue", "System.Title"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsFieldOnCurrentSourceType_LoadsEachTypeOnceIgnoringCase()
        {
            var loaded = new List<string>();
            var policy = Create(new(), loaded);

            Assert.IsTrue(policy.IsFieldOnCurrentSourceType("Product Backlog Item", "system.title"));
            Assert.IsTrue(policy.IsFieldOnCurrentSourceType("product backlog item", "Custom.SrcOnly"));

            Assert.AreEqual(1, loaded.Count);
        }
    }
}
