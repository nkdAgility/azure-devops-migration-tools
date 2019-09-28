using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Core.Execution.OMatics
{
    class EmbededImagesRepairOMatic
    {

        private readonly HttpClientHandler _httpClientHandler;
        private bool _ignore404Errors = true;

        public EmbededImagesRepairOMatic()
        {
            _httpClientHandler = new HttpClientHandler { AllowAutoRedirect = false, UseDefaultCredentials = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        }

        /**
      *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
      */
        public void FixHtmlAttachmentLinks(WorkItem wi, string oldTfsurl, string newTfsurl)
        {

            Trace.WriteLine($"Searching for urls: {oldTfsurl} and {GetUrlWithOppositeSchema(oldTfsurl)}");
            bool wiUpdated = false;
            bool hasCandidates = false;

            var oldTfsurlOppositeSchema = GetUrlWithOppositeSchema(oldTfsurl);
            string regExSearchForImageUrl = "(?<=<img.*src=\")[^\"]*";

            foreach (Field field in wi.Fields)
            {
                if (field.FieldDefinition.FieldType == FieldType.Html)
                {
                    MatchCollection matches = Regex.Matches((string)field.Value, regExSearchForImageUrl);

                    string regExSearchFileName = "(?<=FileName=)[^=]*";
                    foreach (Match match in matches)
                    {
                        if (match.Value.ToLower().Contains(oldTfsurl.ToLower()) || match.Value.ToLower().Contains(oldTfsurlOppositeSchema.ToLower()) )
                        {
                            //save image locally and upload as attachment
                            Match newFileNameMatch = Regex.Match(match.Value, regExSearchFileName, RegexOptions.IgnoreCase);
                            if (newFileNameMatch.Success)
                            {
                                Trace.WriteLine($"field '{field.Name}' has match: {System.Net.WebUtility.HtmlDecode(match.Value)}");
                                string fullImageFilePath = Path.GetTempPath() + newFileNameMatch.Value;

                                using (var httpClient = new HttpClient(_httpClientHandler, false))
                                {

                                    var result = DownloadFile(httpClient, match.Value, fullImageFilePath);
                                    if (!result.IsSuccessStatusCode)
                                    {
                                        if (_ignore404Errors && result.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            Trace.WriteLine($"Image {match.Value} could not be found in WorkItem {wi.Id}, Field {field.Name}");
                                            continue;
                                        }
                                        else
                                        {
                                            result.EnsureSuccessStatusCode();
                                        }
                                    }
                                }

                                if (GetImageFormat(File.ReadAllBytes(fullImageFilePath)) == ImageFormat.unknown)
                                {
                                    throw new Exception($"Downloaded image [{fullImageFilePath}] from Work Item [{wi.Id}] Field: [{field.Name}] could not be identified as an image. Authentication issue?");
                                }

                                int attachmentIndex = wi.Attachments.Add(new Attachment(fullImageFilePath));
                                wi.Save();

                                var newImageLink = wi.Attachments[attachmentIndex].Uri.ToString();

                                field.Value = field.Value.ToString().Replace(match.Value, newImageLink);
                                wi.Attachments.RemoveAt(attachmentIndex);
                                wi.Save();
                                wiUpdated = true;
                                File.Delete(fullImageFilePath);
                            }
                        }
                    }
                }
            }
        }

        private static HttpResponseMessage DownloadFile(HttpClient httpClient, string url, string destinationPath)
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
        private static ImageFormat GetImageFormat(byte[] bytes)
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

        private string GetUrlWithOppositeSchema(string url)
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

        private enum ImageFormat
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
