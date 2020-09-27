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
using MigrationTools.Core.Configuration.Processing;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Engine.Enrichers;
using MigrationTools.Core.Enrichers;

namespace MigrationTools.Sinks.TfsObjectModel.Enrichers
{
   public class EmbededImagesRepairEnricher : EmbededImagesRepairEnricherBase
    {

        /**
      *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
      */
        public override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {

            Debug.WriteLine($"Searching for urls: {oldTfsurl} and {GetUrlWithOppositeSchema(oldTfsurl)}");
            bool wiUpdated = false;
            bool hasCandidates = false;

            var oldTfsurlOppositeSchema = GetUrlWithOppositeSchema(oldTfsurl);
            string regExSearchForImageUrl = "(?<=<img.*src=\")[^\"]*";

            foreach (Field field in wi.ToWorkItem().Fields)
            {
                if (field.FieldDefinition.FieldType == FieldType.Html)
                {
                    MatchCollection matches = Regex.Matches((string)field.Value, regExSearchForImageUrl);

                    string regExSearchFileName = "(?<=FileName=)[^=]*";
                    foreach (Match match in matches)
                    {
                        if (match.Value.ToLower().Contains(oldTfsurl.ToLower()) || match.Value.ToLower().Contains(oldTfsurlOppositeSchema.ToLower()))
                        {
                            //save image locally and upload as attachment
                            Match newFileNameMatch = Regex.Match(match.Value, regExSearchFileName, RegexOptions.IgnoreCase);
                            if (newFileNameMatch.Success)
                            {
                                Trace.WriteLine($"field '{field.Name}' has match: {System.Net.WebUtility.HtmlDecode(match.Value)}");
                                string fullImageFilePath = Path.GetTempPath() + newFileNameMatch.Value;

                                using (var httpClient = new HttpClient(_httpClientHandler, false))
                                {
                                    if (!string.IsNullOrEmpty(sourcePersonalAccessToken))
                                    {
                                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", sourcePersonalAccessToken))));
                                    }
                                    var result = DownloadFile(httpClient, match.Value, fullImageFilePath);
                                    if (!result.IsSuccessStatusCode)
                                    {
                                        if (_ignore404Errors && result.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            Trace.WriteLine($"Image {match.Value} could not be found in WorkItem {wi.ToWorkItem().Id}, Field {field.Name}");
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
                                    throw new Exception($"Downloaded image [{fullImageFilePath}] from Work Item [{wi.ToWorkItem().Id}] Field: [{field.Name}] could not be identified as an image. Authentication issue?");
                                }

                                int attachmentIndex = wi.ToWorkItem().Attachments.Add(new Attachment(fullImageFilePath));
                                this.SaveMigratedWorkItem(wi);

                                var newImageLink = wi.ToWorkItem().Attachments[attachmentIndex].Uri.ToString();

                                field.Value = field.Value.ToString().Replace(match.Value, newImageLink);
                                wi.ToWorkItem().Attachments.RemoveAt(attachmentIndex);
                                wi.ToWorkItem().Save();
                                wiUpdated = true;
                                File.Delete(fullImageFilePath);
                            }
                        }
                    }
                }
            }
        }

    }
}
