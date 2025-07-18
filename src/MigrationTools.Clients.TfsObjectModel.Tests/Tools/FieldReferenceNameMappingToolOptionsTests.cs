using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class FieldReferenceNameMappingToolOptionsTests
    {
        [TestMethod]
        public void FieldMappingsMustNotBeNullWhenNormalized()
        {
            TfsWorkItemTypeValidatorToolOptions options = new() { Enabled = true };
            options.FieldMappings = null;
            options.Normalize();

            Assert.IsNotNull(options.FieldMappings);
        }

        [TestMethod]
        public void ShouldReturnSourceValueWhenNotEnabled()
        {
            TfsWorkItemTypeValidatorToolOptions options = new()
            {
                Enabled = false,
                FieldMappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    }
                }
            };
            options.Normalize();

            Assert.AreEqual("source", options.GetTargetFieldName("wit", "source", out bool _));
        }

        [TestMethod]
        public void ShouldLookupValueCaseInsensitively()
        {
            TfsWorkItemTypeValidatorToolOptions options = new()
            {
                Enabled = true,
                FieldMappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    }
                }
            };
            options.Normalize();

            Assert.AreEqual("target", options.GetTargetFieldName("wit", "source", out bool _));
            Assert.AreEqual("target", options.GetTargetFieldName("WIT", "source", out bool _));
            Assert.AreEqual("target", options.GetTargetFieldName("wit", "SOURCE", out bool _));
            Assert.AreEqual("target", options.GetTargetFieldName("WIT", "SOURCE", out bool _));
        }

        [TestMethod]
        public void ShouldReturnSourceValueIfNotMapped()
        {
            TfsWorkItemTypeValidatorToolOptions options = new()
            {
                Enabled = true,
                FieldMappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target"
                    }
                }
            };
            options.Normalize();

            Assert.AreEqual("not-mapped-source", options.GetTargetFieldName("wit", "not-mapped-source", out bool _));
            Assert.AreEqual("source", options.GetTargetFieldName("not-mapped-wit", "source", out bool _));
        }

        [TestMethod]
        public void ShouldReturnEmptyStringIfMappedToEmptyString()
        {
            TfsWorkItemTypeValidatorToolOptions options = new()
            {
                Enabled = true,
                FieldMappings = new()
                {
                    ["wit"] = new()
                    {
                        ["source"] = "target",
                        ["source-null"] = ""
                    }
                }
            };

            options.Normalize();

            Assert.AreEqual("target", options.GetTargetFieldName("wit", "source", out bool _));
            Assert.IsEmpty(options.GetTargetFieldName("wit", "source-null", out bool _));
        }

        [TestMethod]
        public void ShouldHandleAllWorkItemTypes()
        {
            TfsWorkItemTypeValidatorToolOptions options = new()
            {
                Enabled = true,
                FieldMappings = new()
                {
                    [TfsWorkItemTypeValidatorToolOptions.AllWorkItemTypes] = new()
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
            options.Normalize();

            Assert.AreEqual("target-wit2", options.GetTargetFieldName("wit2", "source-all", out bool _));
            Assert.AreEqual("target-all", options.GetTargetFieldName("wit", "source-all", out bool _));
            Assert.AreEqual("target-all", options.GetTargetFieldName("not-mapped-wit", "source-all", out bool _));
        }
    }
}
