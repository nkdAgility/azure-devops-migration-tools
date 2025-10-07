using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Options;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsEmbededImagesTool : EmbededImagesRepairToolBase<TfsEmbededImagesToolOptions>
    {
        private const string RegexPatternForImageUrl = "(?<=<img.*?src=\")[^\"]*";
        private const string RegexPatternForImageFileName = "(?<=FileName=)[^=]*";
        private const string TargetDummyWorkItemTitle = "***** DELETE THIS - Migration Tool Generated Dummy Work Item For TfsEmbededImagesTool *****";

        private Project _targetProject;

        private readonly IDictionary<string, string> _cachedUploadedUrisBySourceValue;

        private WorkItem _targetDummyWorkItem;

        public TfsEmbededImagesTool(IOptions<TfsEmbededImagesToolOptions> options, IServiceProvider services, ILogger<TfsEmbededImagesTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {
            _cachedUploadedUrisBySourceValue = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void FixEmbededImages(TfsProcessor processor, WorkItemData targetWorkItem)
        {
            static string GenerateAuthToken(string username, string password)
                => Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

            _processor = processor;
            _targetProject = processor.Target.WorkItems.Project.ToProject();

            string? accessToken = null;
            if (processor.Source.Options.Authentication.AuthenticationMode == AuthenticationMode.AccessToken)
            {
                accessToken = GenerateAuthToken(string.Empty, processor.Source.Options.Authentication.AccessToken);
            }
            else if (processor.Source.Options.Authentication.AuthenticationMode == AuthenticationMode.Windows)
            {
                NetworkCredentials credentials = processor.Source.Options.Authentication.NetworkCredentials;
                accessToken = GenerateAuthToken($"{credentials.Domain}\\{credentials.UserName}", credentials.Password);
            }

            FixEmbededImages(targetWorkItem, processor.Source.Options.Collection.AbsoluteUri, processor.Target.Options.Collection.AbsoluteUri, accessToken);
        }

        public void ProcessorExecutionEnd(TfsProcessor processor)
        {
            _processor = processor;
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

                        string newImageLink = "";
                        if (_cachedUploadedUrisBySourceValue.ContainsKey(match.Value))
                        {
                            newImageLink = _cachedUploadedUrisBySourceValue[match.Value];
                        }
                        else
                        {
                            // go upload and get newImageLink
                            newImageLink = UploadedAndRetrieveAttachmentLinkUrl(match.Value, field.Name, wi, sourcePersonalAccessToken);

                            // if unable to store/upload the link, should we cache that result? so the next revision will either just ignore it or try again
                            //   for now, i think the best option is to set it to null so we don't retry an upload, with the assumption being that the next
                            //   upload will most likely fail and just cause the revision process to take longer
                            _cachedUploadedUrisBySourceValue[match.Value] = newImageLink;
                        }

                        if (!string.IsNullOrWhiteSpace(newImageLink))
                        {
                            // the match.Value was either just uploaded or uploaded most likely because of a previous revision. we can replace it
                            field.Value = field.Value.ToString().Replace(match.Value, newImageLink);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "EmbededImagesRepairEnricher: Unable to fix HTML field attachments for work item {wiId} from {oldTfsurl} to {newTfsurl}", wi.Id, oldTfsurl, newTfsurl);
                    Telemetry.TrackException(ex, null);
                }
            }
        }

        private string UploadedAndRetrieveAttachmentLinkUrl(string matchedSourceUri, string sourceFieldName, WorkItemData targetWorkItem, string sourcePersonalAccessToken)
        {
            // save image locally and upload as attachment
            Match newFileNameMatch = Regex.Match(matchedSourceUri, RegexPatternForImageFileName, RegexOptions.IgnoreCase);
            if (!newFileNameMatch.Success) return null;

            Log.LogDebug("EmbededImagesRepairEnricher: field '{fieldName}' has match: {matchValue}", sourceFieldName, WebUtility.HtmlDecode(matchedSourceUri));
            string fullImageFilePath = Path.GetTempPath() + newFileNameMatch.Value;

            try
            {
                using (var httpClient = new HttpClient(_httpClientHandler, false))
                {
                    if (!string.IsNullOrEmpty(sourcePersonalAccessToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", sourcePersonalAccessToken);
                    }

                    var result = DownloadFile(httpClient, matchedSourceUri, fullImageFilePath);
                    if (!result.IsSuccessStatusCode)
                    {
                        if (_ignore404Errors && result.StatusCode == HttpStatusCode.NotFound)
                        {
                            Log.LogDebug("EmbededImagesRepairEnricher: Image {MatchValue} could not be found in WorkItem {WorkItemId}, Field {FieldName}", matchedSourceUri, targetWorkItem.Id, sourceFieldName);
                            return null;
                        }
                        else
                        {
                            // Provide more detailed error information for non-404 failures
                            Log.LogWarning("EmbededImagesRepairEnricher: Failed to download image {MatchValue} from WorkItem {WorkItemId}, Field {FieldName}. Status: {StatusCode} ({ReasonPhrase})",
                                matchedSourceUri, targetWorkItem.Id, sourceFieldName, (int)result.StatusCode, result.ReasonPhrase);
                            
                            result.EnsureSuccessStatusCode();
                        }
                    }
                }

                if (GetImageFormat(File.ReadAllBytes(fullImageFilePath)) == ImageFormat.unknown)
                {
                    throw new Exception($"Downloaded image [{fullImageFilePath}] from Work Item [{targetWorkItem.Id}] Field: [{sourceFieldName}] could not be identified as an image. Authentication issue?");
                }

                var attachRef = UploadImageToTarget(targetWorkItem.ToWorkItem(), fullImageFilePath);
                if (attachRef == null)
                {
                    throw new Exception($"Unable to upload the image [{fullImageFilePath}] to Work Item [{targetWorkItem.Id}] Field: [{sourceFieldName}].");
                }

                return attachRef.Url;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (File.Exists(fullImageFilePath))
                    File.Delete(fullImageFilePath);
            }
        }

        private Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.AttachmentReference UploadImageToTarget(WorkItem wi, string filePath)
        {
            var httpClient = ((TfsConnection)_processor.Target.InternalCollection).GetClient<WorkItemTrackingHttpClient>();

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
            var wii = httpClient.UpdateWorkItemAsync(payload, dummyWi.Id, bypassRules: true).GetAwaiter().GetResult();
            if (wii != null)
            {
                payload[0].Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Remove;
                payload[0].Path = "/relations/" + (wii.Relations.Count - 1);
                payload[0].Value = null;
                wii = httpClient.UpdateWorkItemAsync(payload, dummyWi.Id, bypassRules: true).GetAwaiter().GetResult();
            }
            else
            {
                throw new Exception($"Problem attaching the uploaded image [{filePath}] with dummy workitem [{dummyWi.Id}] to be able to use for Work Item [{wi.Id}].");
            }

            return link;
        }


        private int _DummyWorkItemCount = 0;
        private TfsProcessor _processor;

        private WorkItem GetDummyWorkItem(WorkItemType type = null)
        {
            if (_DummyWorkItemCount > 900)
            {
                Log.LogDebug("EmbededImagesRepairEnricher: Dummy workitem {id} is neering capacity. Creating a new one!", _targetDummyWorkItem.Id);
                _targetDummyWorkItem = null;
                _DummyWorkItemCount = 0;
            }
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

                var fails = _targetDummyWorkItem.Validate();
                if (fails.Count > 0)
                {
                    Log.LogWarning("Dummy Work Item is not ready to save as it has some invalid fields. This may not result in an error. Enable LogLevel as 'Debug' in the config to see more.");
                    Log.LogDebug("--------------------------------------------------------------------------------------------------------------------");
                    Log.LogDebug("--------------------------------------------------------------------------------------------------------------------");
                    foreach (Field f in fails)
                    {
                        Log.LogDebug("Invalid Field Object:\r\n{Field}", f.ToJson());
                    }
                    Log.LogDebug("--------------------------------------------------------------------------------------------------------------------");
                    Log.LogDebug("--------------------------------------------------------------------------------------------------------------------");
                }
                Log.LogTrace("TfsEmbededImagesTool::GetDummyWorkItem::Save()");


                _targetDummyWorkItem.Save();

                if (_targetDummyWorkItem.Id == 0)
                {
                    throw new Exception("The Dummy work Item cant be created due to a save failure. This is likley due to required fields on the Task or First work items type.");
                }
                else
                {
                    Log.LogDebug("TfsEmbededImagesTool: Dummy workitem {id} created on the target collection.", _targetDummyWorkItem.Id);
                    //_targetProject.Store.DestroyWorkItems(new List<int> { _targetDummyWorkItem.Id });
                }
            }
            _DummyWorkItemCount++;
            return _targetDummyWorkItem;
        }
    }
}
