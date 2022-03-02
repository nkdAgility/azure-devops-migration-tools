using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class TfsEmbededImagesEnricher : EmbededImagesRepairEnricherBase
    {
        private const string RegexPatternForImageUrl = "(?<=<img.*src=\")[^\"]*";
        private const string RegexPatternForImageFileName = "(?<=FileName=)[^=]*";

        public IMigrationEngine Engine { get; private set; }

        public TfsEmbededImagesEnricher(IServiceProvider services, ILogger<TfsEmbededImagesEnricher> logger) : base(services, logger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
            //
        }

        [Obsolete]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        [Obsolete("v2 Archtecture: use Configure(bool save = true, bool filter = true) instead", true)]
        public override void Configure(IProcessorEnricherOptions options)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            FixEmbededImages(targetWorkItem, Engine.Source.Config.AsTeamProjectConfig().Collection.ToString(), Engine.Target.Config.AsTeamProjectConfig().Collection.ToString(), Engine.Source.Config.AsTeamProjectConfig().PersonalAccessToken);
            return 0;
        }

        /**
      *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
      */

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            var logTypeName = nameof(TfsEmbededImagesEnricher);
            Log.LogInformation($"{logTypeName}: Fixing HTML field attachments for work item {wi.Id} from {oldTfsurl} to {newTfsurl}");

            var oldTfsurlOppositeSchema = GetUrlWithOppositeSchema(oldTfsurl);
            
            foreach (Field field in wi.ToWorkItem().Fields)
            {
                if (field.FieldDefinition.FieldType != FieldType.Html && field.FieldDefinition.FieldType != FieldType.History)
                    continue;

                try
                {
                    MatchCollection matches = Regex.Matches((string)field.Value, RegexPatternForImageUrl);
                    foreach (Match match in matches)
                    {
                        if (!match.Value.ToLower().Contains(oldTfsurl.ToLower()) && !match.Value.ToLower().Contains(oldTfsurlOppositeSchema.ToLower()))
                            continue;

                        //save image locally and upload as attachment
                        Match newFileNameMatch = Regex.Match(match.Value, RegexPatternForImageFileName, RegexOptions.IgnoreCase);
                        if (!newFileNameMatch.Success)
                            continue;

                        Log.LogDebug($"{logTypeName}: field '{field.Name}' has match: {WebUtility.HtmlDecode(match.Value)}");
                        string fullImageFilePath = Path.GetTempPath() + newFileNameMatch.Value;

                        try
                        {
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
                                        Log.LogDebug($"{logTypeName}: Image {match.Value} could not be found in WorkItem {wi.Id}, Field {field.Name}");
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
                                throw new Exception($"{logTypeName}: Downloaded image [{fullImageFilePath}] from Work Item [{wi.ToWorkItem().Id}] Field: [{field.Name}] could not be identified as an image. Authentication issue?");
                            }

                            var attachmentInfo = new Microsoft.TeamFoundation.WorkItemTracking.Internals.AttachmentInfo(fullImageFilePath);
                            wi.ToWorkItem().UploadAttachment(attachmentInfo);

                            if (attachmentInfo.IsUploaded)
                            {
                                var newImageLink = BuildAttachmentUrl(attachmentInfo);
                                field.Value = field.Value.ToString().Replace(match.Value, newImageLink);
                            }
                            else
                            {
                                throw new Exception($"{logTypeName}: Unable to upload the image [{fullImageFilePath}] to Work Item [{wi.ToWorkItem().Id}] Field: [{field.Name}].");
                            }
                        }
                        finally
                        {
                            if (File.Exists(fullImageFilePath))
                                File.Delete(fullImageFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, $"{logTypeName}: Unable to fix HTML field attachments for work item {wi.Id} from {oldTfsurl} to {newTfsurl}");
                }
            }
        }

        private string BuildAttachmentUrl(Microsoft.TeamFoundation.WorkItemTracking.Internals.AttachmentInfo attachmentInfo)
        {
            //https://{instance}/{collection}/{project}/_apis/wit/attachments/{id}?fileName={fileName}

            var project = Engine.Target.WorkItems.Project.ToProject();
            var config = Engine.Target.Config.AsTeamProjectConfig();

            var uri = new UriBuilder(config.Collection);
            uri.Path += $"{project.Guid}/_apis/wit/attachments/{attachmentInfo.Path}?fileName={attachmentInfo.FileInfo.Name}";
            return uri.ToString();
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }
    }
}