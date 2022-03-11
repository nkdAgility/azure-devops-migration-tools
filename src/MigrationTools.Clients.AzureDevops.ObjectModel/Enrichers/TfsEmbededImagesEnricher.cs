﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class TfsEmbededImagesEnricher : EmbededImagesRepairEnricherBase
    {
        private const string RegexPatternForImageUrl = "(?<=<img.*src=\")[^\"]*";
        private const string RegexPatternForImageFileName = "(?<=FileName=)[^=]*";
        private const string TargetDummyWorkItemTitle = "***** DELETE THIS - Migration Tool Generated Dummy Work Item For TfsEmbededImagesEnricher *****";

        private readonly Project _targetProject;
        private readonly TfsTeamProjectConfig _targetConfig;

        private WorkItem _targetDummyWorkItem;

        public IMigrationEngine Engine { get; private set; }

        public TfsEmbededImagesEnricher(IServiceProvider services, ILogger<TfsEmbededImagesEnricher> logger) : base(services, logger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
            _targetProject = Engine.Target.WorkItems.Project.ToProject();
            _targetConfig = Engine.Target.Config.AsTeamProjectConfig();
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

        public override void ProcessorExecutionEnd(IProcessor processor)
        {
            if (_targetDummyWorkItem != null)
            {
                _targetDummyWorkItem.Close();
                _targetProject.Store.DestroyWorkItems(new List<int> { _targetDummyWorkItem.Id });
            }
        }

        /**
      *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
      */

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            Log.LogInformation("EmbededImagesRepairEnricher: Fixing HTML field attachments for work item {Id} from {OldTfsurl} to {NewTfsUrl}", wi.Id, oldTfsurl, newTfsurl);

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

                        Log.LogDebug("EmbededImagesRepairEnricher: field '{fieldName}' has match: {matchValue}", field.Name, System.Net.WebUtility.HtmlDecode(match.Value));
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
                                        Log.LogDebug("EmbededImagesRepairEnricher: Image {MatchValue} could not be found in WorkItem {WorkItemId}, Field {FieldName}", match.Value, wi.Id, field.Name);
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

                            var ar = UploadImageToTarget(wi.ToWorkItem(), fullImageFilePath);
                            field.Value = field.Value.ToString().Replace(match.Value, ar.Url);
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
                    Log.LogError(ex, "EmbededImagesRepairEnricher: Unable to fix HTML field attachments for work item {wiId} from {oldTfsurl} to {newTfsurl}", wi.Id, oldTfsurl, newTfsurl);
                }
            }
        }


        private Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.AttachmentReference UploadImageToTarget(WorkItem wi, string filePath)
        {
            var httpClient = ((TfsConnection)Engine.Target.InternalCollection).GetClient<WorkItemTrackingHttpClient>();

            // uploads and creates the image attachment
            Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.AttachmentReference link = null;
            using (FileStream uploadStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                link = httpClient.CreateAttachmentAsync(uploadStream, fileName: Path.GetFileName(filePath)).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (link == null)
            {
                throw new Exception($"Problem uploading image [{filePath}] for Work Item [{wi.Id}].");
            }


            // Attaches it with dummy work item and removes it just to be able to make the image visible to all the users.
            // VS402330: Unauthorized Read access to the attachment under the areas 
            var payload = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument();
            payload.Add(new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation()
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = "AttachedFile",
                    url = link.Url
                }
            });

            var dummyWi = GetDummyWorkItem(wi.Type);
            var wii = httpClient.UpdateWorkItemAsync(payload, dummyWi.Id).GetAwaiter().GetResult();
            if (wii != null)
            {
                payload[0].Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Remove;
                payload[0].Path = "/relations/" + (wii.Relations.Count - 1);
                payload[0].Value = null;
                wii = httpClient.UpdateWorkItemAsync(payload, dummyWi.Id).GetAwaiter().GetResult();
            }
            else
            {
                throw new Exception($"Problem attaching the uploaded image [{filePath}] with dummy workitem [{dummyWi.Id}] to be able to use for Work Item [{wi.Id}].");
            }

            return link;
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        private WorkItem GetDummyWorkItem(WorkItemType type = null)
        {
            if (_targetDummyWorkItem == null)
            {
                if (_targetProject.WorkItemTypes.Count == 0) return null;

                if (type == null)
                {
                    type = _targetProject.WorkItemTypes["Task"];
                }
                if (type == null)
                {
                    type = _targetProject.WorkItemTypes[0];
                }

                _targetDummyWorkItem = type.NewWorkItem();
                _targetDummyWorkItem.Title = TargetDummyWorkItemTitle;
                _targetDummyWorkItem.Save();
                Log.LogDebug("EmbededImagesRepairEnricher: Dummy workitem {id} created on the target collection.", _targetDummyWorkItem.Id);
                //_targetProject.Store.DestroyWorkItems(new List<int> { _targetDummyWorkItem.Id });
            }
            return _targetDummyWorkItem;
        }
    }
}