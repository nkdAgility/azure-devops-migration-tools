using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class FieldReferenceNameMappingToolOptionsTests
    {
        [TestMethod]
        public void MappingsMustNotBeNullWhenNormalized()
        {
            FieldReferenceNameMappingToolOptions options = new() { Enabled = true };
            options.Mappings = null;

            FieldReferenceNameMappingToolOptions normalized = options.Normalize();
            Assert.IsNotNull(normalized.Mappings);
        }

        [TestMethod]
        public void ShouldReturnSourceValueWhenNotEnabled()
        {
            FieldReferenceNameMappingToolOptions options = new()
            {
                Enabled = false,
                Mappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    }
                }
            };
            FieldReferenceNameMappingToolOptions normalized = options.Normalize();

            Assert.AreEqual("source", normalized.GetTargetFieldName("wit", "source", out bool _));
        }

        [TestMethod]
        public void ShouldLookupValueCaseInsensitively()
        {
            FieldReferenceNameMappingToolOptions options = new()
            {
                Enabled = true,
                Mappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    }
                }
            };
            FieldReferenceNameMappingToolOptions normalized = options.Normalize();

            Assert.AreEqual("target", normalized.GetTargetFieldName("wit", "source", out bool _));
            Assert.AreEqual("target", normalized.GetTargetFieldName("WIT", "source", out bool _));
            Assert.AreEqual("target", normalized.GetTargetFieldName("wit", "SOURCE", out bool _));
            Assert.AreEqual("target", normalized.GetTargetFieldName("WIT", "SOURCE", out bool _));
        }

        [TestMethod]
        public void ShouldReturnSourceValueIfNotMapped()
        {
            FieldReferenceNameMappingToolOptions options = new()
            {
                Enabled = true,
                Mappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    }
                }
            };
            FieldReferenceNameMappingToolOptions normalized = options.Normalize();

            Assert.AreEqual("not-mapped-source", normalized.GetTargetFieldName("wit", "not-mapped-source", out bool _));
            Assert.AreEqual("source", normalized.GetTargetFieldName("not-mapped-wit", "source", out bool _));
        }

        [TestMethod]
        public void ShouldReturnEmptyStringIfMappedToEmptyString()
        {
            FieldReferenceNameMappingToolOptions options = new()
            {
                Enabled = true,
                Mappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target",
                        ["source-null"] = ""
                    }
                }
            };

            FieldReferenceNameMappingToolOptions normalized = options.Normalize();

            Assert.AreEqual("target", normalized.GetTargetFieldName("wit", "source", out bool _));
            Assert.IsEmpty(normalized.GetTargetFieldName("wit", "source-null", out bool _));
        }

        [TestMethod]
        public void ShouldHandleAllWorkItemTypes()
        {
            FieldReferenceNameMappingToolOptions options = new()
            {
                Enabled = true,
                Mappings = new()
                {
                    [FieldReferenceNameMappingToolOptions.AllWorkItemTypes] = new()
                    {
                        ["source-all"] = "target-all"
                    },
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    },
                    ["wit2"] = new()
                    {
                        ["source-all"] = "target-wit2"
                    }
                }
            };
            FieldReferenceNameMappingToolOptions normalized = options.Normalize();

            Assert.AreEqual("target-wit2", normalized.GetTargetFieldName("wit2", "source-all", out bool _));
            Assert.AreEqual("target-all", normalized.GetTargetFieldName("wit", "source-all", out bool _));
            Assert.AreEqual("target-all", normalized.GetTargetFieldName("not-mapped-wit", "source-all", out bool _));
        }
    }
}
