using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Options;

namespace MigrationTools.Tests
{
    [TestClass]
    public class TypeExtensionsWithNameStringTests
    {
        private class SampleProcessor { }
        private class SampleProcessorExtended { }
        private class SampleProcessorOther { }

        private class SampleProcessorOptions : IOptions
        {
            public ConfigurationMetadata ConfigurationMetadata => null;
            public bool Enabled { get; set; }
        }

        [TestMethod, TestCategory("L0")]
        public void WithNameString_SameTypeListedTwice_ReturnsIt()
        {
            var types = new[] { typeof(SampleProcessor), typeof(SampleProcessor) };

            Assert.AreEqual(typeof(SampleProcessor), types.WithNameString("SampleProcessor"));
        }

        [TestMethod, TestCategory("L0")]
        public void WithNameString_ExactMatchAndLongerNames_ReturnsExactMatch()
        {
            var types = new[] { typeof(SampleProcessorExtended), typeof(SampleProcessor), typeof(SampleProcessorOther), typeof(SampleProcessor) };

            Assert.AreEqual(typeof(SampleProcessor), types.WithNameString("sampleprocessor"));
        }

        [TestMethod, TestCategory("L0")]
        public void WithNameString_PrefixMatchingOneTypeTwice_ReturnsIt()
        {
            var types = new[] { typeof(SampleProcessorExtended), typeof(SampleProcessorExtended) };

            Assert.AreEqual(typeof(SampleProcessorExtended), types.WithNameString("SampleProcessorExt"));
        }

        [TestMethod, TestCategory("L0")]
        public void WithNameString_PrefixMatchingDifferentTypes_Throws()
        {
            var types = new[] { typeof(SampleProcessorExtended), typeof(SampleProcessorOther) };

            Assert.ThrowsExactly<InvalidOperationException>(() => types.WithNameString("SampleProcessor"));
        }

        [TestMethod, TestCategory("L0")]
        public void WithNameString_NoMatch_ReturnsNull()
        {
            Assert.IsNull(new[] { typeof(SampleProcessor), typeof(SampleProcessor) }.WithNameString("Missing"));
        }

        [TestMethod, TestCategory("L0")]
        public void FromOptions_SameTypeListedTwiceWithLongerName_ReturnsExactMatch()
        {
            var types = new[] { typeof(SampleProcessor), typeof(SampleProcessorExtended), typeof(SampleProcessor) };

            Assert.AreEqual(typeof(SampleProcessor), types.FromOptions(new SampleProcessorOptions()));
        }
    }
}
