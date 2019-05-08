using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class HtmlFieldEmbeddedImageMigrationContext : MigrationContextBase
    {
        private readonly HtmlFieldEmbeddedImageMigrationConfig _config;

        private int current;
        private int count = 0;
        private int failures = 0;
        private int updated = 0;
        private int skipped = 0;
        private int candidates = 0;
        private int notfound = 0;

        private readonly HttpClientHandler _httpClientHandler;

        public override string Name => "HtmlFieldEmbeddedImageMigrationContext";

        public HtmlFieldEmbeddedImageMigrationContext(MigrationEngine me, HtmlFieldEmbeddedImageMigrationConfig config)
            : base(me, config)
        {
            _config = config;
            _httpClientHandler = new HttpClientHandler {AllowAutoRedirect = false};
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            TfsQueryContext tfsqc = new TfsQueryContext(targetStore);
            tfsqc.AddParameter("TeamProject", me.Target.Name);
            tfsqc.Query =
                $@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {_config.QueryBit} ORDER BY [System.ChangedDate] desc";
            WorkItemCollection targetWIS = tfsqc.Execute();
            Trace.WriteLine($"Found {targetWIS.Count} work items...", Name);

            current = targetWIS.Count;

            string urlForMatch = me.Source.Collection.Uri.ToString();
            if (_config.FromAnyCollection)
            {
                var url = new Uri(me.Source.Collection.Uri.ToString());
                urlForMatch = url.GetLeftPart(UriPartial.Authority);
            }

            Trace.WriteLine($"Searching for urls: {urlForMatch} and {GetUrlWithOppositeSchema(urlForMatch)} {_config.SourceServerAliases?.Select(alias => "and " + alias)}");
           
            foreach (WorkItem targetWi in targetWIS)
            {
                Trace.WriteLine($"{current} - Fixing: {targetWi.Id}-{targetWi.Type.Name}", Name);

                // Decide on WIT
                if (me.WorkItemTypeDefinitions.ContainsKey(targetWi.Type.Name))
                {
                    FixHtmlAttachmentLinks(targetWi, urlForMatch, me.Target.Collection.Uri.ToString());
                }
                else
                {
                    Trace.WriteLine(
                        $"...the WITD named {targetWi.Type.Name} is not in the list provided in the configuration.json under WorkItemTypeDefinitions. Add it to the list to enable migration of this work item type.", Name);
                    skipped++;
                }

                current--;
                count++;

                Trace.Flush();
            }

            stopwatch.Stop();
            Trace.WriteLine($@"DONE in {stopwatch.Elapsed:%h} hours {stopwatch.Elapsed:%m} minutes {stopwatch.Elapsed:s\:fff} seconds - {targetWIS.Count} Items, {updated} Updated, {skipped} Skipped, {failures} Failures, {candidates} Possible Candidates, {notfound} Not Found", this.Name);
        }

        /**
         *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
         */
        private void FixHtmlAttachmentLinks(WorkItem wi, string oldTfsurl, string newTfsurl)
        {
            bool wiUpdated = false;
            bool hasCandidates = false;

            var oldTfsurlOppositeSchema = GetUrlWithOppositeSchema(oldTfsurl);
            string regExSearchForImageUrl = "(?<=<img.*src=\")[^\"]*";

            foreach (Field field in wi.Fields)
            {
                if (field.FieldDefinition.FieldType == FieldType.Html)
                {
                    MatchCollection matches = Regex.Matches((string) field.Value, regExSearchForImageUrl);

                    string regExSearchFileName = "(?<=FileName=)[^=]*";
                    foreach (Match match in matches)
                    {
                        if (match.Value.ToLower().Contains(oldTfsurl.ToLower()) || match.Value.ToLower().Contains(oldTfsurlOppositeSchema.ToLower()) || (_config.SourceServerAliases != null && _config.SourceServerAliases.Any(i => match.Value.ToLower().Contains(i.ToLower()))))
                        {                     
                            //save image locally and upload as attachment
                            Match newFileNameMatch = Regex.Match(match.Value, regExSearchFileName, RegexOptions.IgnoreCase);
                            if (newFileNameMatch.Success)
                            {
                                Trace.WriteLine($"field '{field.Name}' has match: {match.Value}");
                                string fullImageFilePath = Path.GetTempPath() + newFileNameMatch.Value;

                                using (var httpClient = new HttpClient(_httpClientHandler, false))
                                {
                                    SetAuthorization(httpClient);

                                    var result = DownloadFile(httpClient, match.Value, fullImageFilePath);
                                    if (!result.IsSuccessStatusCode)
                                    {
                                        if (_config.Ignore404Errors && result.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            notfound++;
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

                                if (_config.DeleteTemporaryImageFiles)
                                {
                                    File.Delete(fullImageFilePath);
                                }
                            }
                        }
                        else
                        {
                            hasCandidates = CheckForPossibleCandidates(match, field);
                        }
                    }
                }
            }

            if (wiUpdated)
            {
                updated++;
            }

            if (hasCandidates)
            {
                candidates++;
            }
        }

        private void SetAuthorization(HttpClient httpClient)
        {
            // When alternate credentials are given, use basic authentication with the given credentials
            if (string.IsNullOrWhiteSpace(_config.AlternateCredentialsUsername) 
                || string.IsNullOrWhiteSpace(_config.AlternateCredentialsPassword)) return;

            var credentials = Encoding.ASCII.GetBytes(_config.AlternateCredentialsUsername + ":" + _config.AlternateCredentialsPassword);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
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
            var bmp    = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif    = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png    = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff   = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2  = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg   = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2  = new byte[] { 255, 216, 255, 225 }; // jpeg canon
            var jpeg3  = new byte[] { 255, 216, 255, 237 }; // jpeg
            var jpeg4  = new byte[] { 255, 216, 255, 232 }; // jpeg still picture interchange file format (SPIFF)
            var jpeg5  = new byte[] { 255, 216, 255, 226 }; // jpeg canon

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

        private bool CheckForPossibleCandidates(Match match, Field field)
        {
            if (match.Value.Contains(me.Source.Collection.Uri.Host))
            {
                Trace.WriteLine($"field '{field.Name}' has match: {match.Value}", "Possible Candidate");               
                return true;
            }
            else
                return false;
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