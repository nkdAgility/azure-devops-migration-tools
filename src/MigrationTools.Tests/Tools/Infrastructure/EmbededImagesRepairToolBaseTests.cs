using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Options;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tests.Tools.Infrastructure
{
    [TestClass]
    public class EmbededImagesRepairToolBaseTests
    {
        private static byte[] LoadAsset(string name)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "Infrastructure", "Assets", name);
            return File.ReadAllBytes(path);
        }

        [TestMethod]
        public void ShouldDetectJpegWithPhotoshopMetadata_FromFile()
        {
            byte[] bytes = LoadAsset("jpgsample.jpg");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(bytes);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectPng_FromFile()
        {
            byte[] bytes = LoadAsset("pngsample.png");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(bytes);
            Assert.AreEqual("png", format);
        }

        [TestMethod]
        public void ShouldDetectTiff_FromFile()
        {
            byte[] bytes = LoadAsset("tiffsample.tiff");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(bytes);
            Assert.AreEqual("tiff", format);
        }

        [TestMethod]
        public void ShouldDetectGif_FromFile()
        {
            byte[] bytes = LoadAsset("gifsample.gif");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(bytes);
            Assert.AreEqual("gif", format);
        }

        [TestMethod]
        public void ShouldDetectSvg_FromFile()
        {
            byte[] bytes = LoadAsset("svgsample.svg");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(bytes);
            Assert.AreEqual("svg", format);
        }

        [TestMethod]
        public void ShouldDetectBmp()
        {
            byte[] bmpHeader = new byte[] { 66, 77 }; // "BM"
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(bmpHeader);
            Assert.AreEqual("bmp", format);
        }

        [TestMethod]
        public void ShouldDetectSVG()
        {
            // Arrange: SVG header (first few bytes of an SVG file)
            byte[] svgHeader = Encoding.ASCII.GetBytes("<svg");

            // Act
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(svgHeader);

            // Assert
            Assert.AreEqual("svg", format);
        }

        [TestMethod]
        public void ShouldDetectGIF87a()
        {
            byte[] gifHeader = Encoding.ASCII.GetBytes("GIF87a");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(gifHeader);
            Assert.AreEqual("gif", format);
        }

        [TestMethod]
        public void ShouldDetectGIF89a()
        {
            byte[] gifHeader = Encoding.ASCII.GetBytes("GIF89a");
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(gifHeader);
            Assert.AreEqual("gif", format);
        }

        [TestMethod]
        public void ShouldDetectPng()
        {
            byte[] pngHeader = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(pngHeader);
            Assert.AreEqual("png", format);
        }

        [TestMethod]
        public void ShouldDetectTiffLittleEndian()
        {
            byte[] tiffHeader = new byte[] { 73, 73, 42, 0 }; // II followed by 2A 00
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(tiffHeader);
            Assert.AreEqual("tiff", format);
        }

        [TestMethod]
        public void ShouldDetectTiffBigEndian()
        {
            byte[] tiffHeader = new byte[] { 77, 77, 0, 42 }; // MM followed by 00 2
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(tiffHeader);
            Assert.AreEqual("tiff", format);
        }

        [TestMethod]
        public void ShouldDetectJpegStandard()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 224 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegCanon()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 225 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithSPIFF()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 232 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithAdobeMarker()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 239 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithICCProfile()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 227 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithIRBMetadata()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 237 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithQuantizationTable()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 219 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithPhotoshopMetadata()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 238 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

        [TestMethod]
        public void ShouldDetectJpegWithCanonMarker()
        {
            byte[] jpegHeader = new byte[] { 255, 216, 255, 226 };
            var format = TestableEmbededImagesRepairTool.CallGetImageFormat(jpegHeader);
            Assert.AreEqual("jpeg", format);
        }

    }

    /// <summary>
    /// Wrapper class to expose GetImageFormat from EmbededImagesRepairToolBase
    /// </summary>
    public class TestableEmbededImagesRepairTool : EmbededImagesRepairToolBase<TestToolOptions>
    {
        public TestableEmbededImagesRepairTool(IOptions<TestToolOptions> options, IServiceProvider services, ILogger<ITool> logger, ITelemetryLogger telemetry) : base(options, services, logger, telemetry)
        {
        }

        public static string CallGetImageFormat(byte[] bytes)
        {
            return GetImageFormat(bytes).ToString();
        }

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            throw new NotImplementedException();
        }
    }

    public class TestToolOptions : IToolOptions
    {
        public ConfigurationMetadata ConfigurationMetadata => throw new NotImplementedException();

        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}