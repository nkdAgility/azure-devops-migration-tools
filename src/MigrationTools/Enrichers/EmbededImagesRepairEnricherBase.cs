using MigrationTools.DataContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MigrationTools.Enrichers
{
    public abstract class EmbededImagesRepairEnricherBase : IEmbededImagesRepairEnricher
    {
        protected readonly HttpClientHandler _httpClientHandler;
        protected bool _ignore404Errors = true;

        public EmbededImagesRepairEnricherBase()
        {
            _httpClientHandler = new HttpClientHandler { AllowAutoRedirect = false, UseDefaultCredentials = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        }

        /**
      *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
      */

        public abstract void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "");

        protected static HttpResponseMessage DownloadFile(HttpClient httpClient, string url, string destinationPath)
        {
            var response = httpClient.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                using (var stream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    using (var fileWriter = new FileStream(destinationPath, FileMode.Create))
                    {
                        stream.CopyTo(fileWriter);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Retrieve Image Format for a given byte array
        /// </summary>
        /// <param name="bytes">Image to check</param>
        /// <remarks>From https://stackoverflow.com/a/9446045/1317161</remarks>
        /// <returns>Image format</returns>
        protected static ImageFormat GetImageFormat(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
            var jpeg3 = new byte[] { 255, 216, 255, 237 }; // jpeg
            var jpeg4 = new byte[] { 255, 216, 255, 232 }; // jpeg still picture interchange file format (SPIFF)
            var jpeg5 = new byte[] { 255, 216, 255, 226 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;

            if (jpeg3.SequenceEqual(bytes.Take(jpeg3.Length)))
                return ImageFormat.jpeg;

            if (jpeg4.SequenceEqual(bytes.Take(jpeg4.Length)))
                return ImageFormat.jpeg;

            if (jpeg5.SequenceEqual(bytes.Take(jpeg5.Length)))
                return ImageFormat.jpeg;

            return ImageFormat.unknown;
        }

        protected string GetUrlWithOppositeSchema(string url)
        {
            string oppositeUrl;
            var sourceUrl = new Uri(url);
            if (sourceUrl.Scheme == Uri.UriSchemeHttp)
            {
                oppositeUrl = "https://" + sourceUrl.Host + sourceUrl.AbsolutePath;
            }
            else if (sourceUrl.Scheme == Uri.UriSchemeHttps)
            {
                oppositeUrl = "http://" + sourceUrl.Host + sourceUrl.AbsolutePath;
            }
            else
                oppositeUrl = url;

            return oppositeUrl;
        }

        protected enum ImageFormat
        {
            unknown,
            bmp,
            gif,
            png,
            tiff,
            jpeg
        }

    }
}
