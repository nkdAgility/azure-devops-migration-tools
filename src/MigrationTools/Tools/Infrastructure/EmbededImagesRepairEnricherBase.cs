using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class EmbededImagesRepairToolBase<ToolOptions> : Tool<ToolOptions> where ToolOptions : class, IToolOptions, new()
    {
        protected readonly HttpClientHandler _httpClientHandler;
        protected bool _ignore404Errors = true;

        protected EmbededImagesRepairToolBase(IOptions<ToolOptions> options, IServiceProvider services, ILogger<ITool> logger, ITelemetryLogger telemetry) : base(options, services, logger, telemetry)
        {
            _httpClientHandler = new HttpClientHandler { AllowAutoRedirect = true, UseDefaultCredentials = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        }

        /**
*  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
*/

        protected abstract void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "");

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
            else
            {
                // Log details about non-success responses for debugging
                var logger = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("EmbededImagesRepairEnricher");
                logger.LogDebug("DownloadFile failed for URL {Url}. Status: {StatusCode} ({ReasonPhrase}). Location header: {LocationHeader}",
                    url, (int)response.StatusCode, response.ReasonPhrase, response.Headers.Location?.ToString() ?? "None");
            }

            return response;
        }

        /// <summary>
        /// Retrieve Image Format for a given byte array
        /// </summary>
        /// <param name="bytes">Image to check</param>
        /// <returns>Image format</returns>
        protected static ImageFormat GetImageFormat(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 1)
            {
                // BMP: 42 4D
                var bmp = new byte[] { 0x42, 0x4D };
                if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                    return ImageFormat.bmp;
            }

            if (bytes == null || bytes.Length < 4)
                return ImageFormat.unknown;

            // GIF: GIF87a or GIF89a
            var gif87a = System.Text.Encoding.ASCII.GetBytes("GIF87a");
            var gif89a = System.Text.Encoding.ASCII.GetBytes("GIF89a");

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            // TIFF: II* or MM*
            var tiffLE = new byte[] { 0x49, 0x49, 0x2A, 0x00 };
            var tiffBE = new byte[] { 0x4D, 0x4D, 0x00, 0x2A };

            // JPEG: FF D8
            var jpegSOI = new byte[] { 0xFF, 0xD8 };

            // Check GIF
            if (gif87a.SequenceEqual(bytes.Take(gif87a.Length)) ||
                gif89a.SequenceEqual(bytes.Take(gif89a.Length)))
                return ImageFormat.gif;

            // Check PNG
            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            // Check TIFF
            if (tiffLE.SequenceEqual(bytes.Take(tiffLE.Length)) ||
                tiffBE.SequenceEqual(bytes.Take(tiffBE.Length)))
                return ImageFormat.tiff;

            // Check JPEG
            if (jpegSOI.SequenceEqual(bytes.Take(jpegSOI.Length)))
                return ImageFormat.jpeg;

            var text = Encoding.UTF8.GetString(bytes);
            text = text.TrimStart();

            if (text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) ||
                (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) && text.Contains("<svg")))
            {
                return ImageFormat.svg;
            }

            return ImageFormat.unknown;
        }

        /// <summary>
        /// Returns true when <paramref name="imageUrl"/> is an address under one of the supplied
        /// collection URLs, i.e. the collection URL is a proper prefix ending on a path or query
        /// boundary.
        /// </summary>
        /// <remarks>
        /// A substring check is not enough here: the URL comes from work item content, which is
        /// untrusted, and the caller sends the source credentials with the download request. A
        /// crafted value such as <c>https://attacker.example/?next=https://collection/...</c>
        /// contains the collection URL but does not point to it, and must not receive the
        /// credentials. Requiring a boundary character after the prefix also stops
        /// <c>https://host/CollectionEvil/...</c> from matching <c>https://host/Collection</c>.
        /// </remarks>
        protected static bool IsAttachmentFromSourceCollection(string imageUrl, params string[] collectionUrls)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            // urls taken from HTML fields still carry their entities (&amp;); decode so the
            // comparison sees the address the request would actually be made to.
            string decodedUrl = WebUtility.HtmlDecode(imageUrl).TrimStart();

            foreach (string collectionUrl in collectionUrls)
            {
                if (string.IsNullOrWhiteSpace(collectionUrl))
                    continue;

                string prefix = collectionUrl.TrimEnd('/');

                // the bare collection root is not an attachment, with or without its trailing slash.
                if (decodedUrl.Length <= prefix.Length ||
                    decodedUrl.Equals(prefix + "/", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!decodedUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                char boundary = decodedUrl[prefix.Length];
                if (boundary == '/' || boundary == '?')
                    return true;
            }

            return false;
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
            jpeg,
            svg
        }
    }
}
