using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools._EngineV1.Enrichers;
using FieldType = Microsoft.TeamFoundation.WorkItemTracking.Client.FieldType;
using Link = Microsoft.TeamFoundation.WorkItemTracking.Client.Link;
using WorkItemLink = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemLink;

namespace MigrationTools.Enrichers
{
    public class TfsEmbededImagesEnricher : EmbededImagesRepairEnricherBase
    {
        public TfsEmbededImagesEnricher(IMigrationEngine engine, ILogger<TfsEmbededImagesEnricher> logger) : base(engine, logger)
        {
        }

        public override void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        [Obsolete("v2 Archtecture: use Configure(bool save = true, bool filter = true) instead", true)]
        public override void Configure(IProcessorEnricherOptions options)
        {
            throw new NotImplementedException();
        }

        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, WorkItemTrackingHttpClient witClient, string project)
        {
            FixEmbededImages(targetWorkItem, Engine.Source.Config.AsTeamProjectConfig().Collection.ToString(), Engine.Target.Config.AsTeamProjectConfig().Collection.ToString(), witClient, project, Engine.Source.Config.AsTeamProjectConfig().PersonalAccessToken);
            return 0;
        }

        /**
      *  from https://gist.github.com/pietergheysens/792ed505f09557e77ddfc1b83531e4fb
      */

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl,
            WorkItemTrackingHttpClient witClient, string project, string sourcePersonalAccessToken = "")
        {
            Log.LogInformation("EmbededImagesRepairEnricher: Fixing HTML field attachments for work item {Id} from {OldTfsurl} to {NewTfsUrl}", wi.Id, oldTfsurl, GetUrlWithOppositeSchema(oldTfsurl));

            var oldTfsurlOppositeSchema = GetUrlWithOppositeSchema(oldTfsurl);
            string regExSearchForImageUrl = "(?<=<img.*src=\")[^\"]*";

            var linkList = new List<Link>();
            foreach (Link link in wi.ToWorkItem().Links)
            {
                linkList.Add(link);
            }

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
                                Log.LogDebug("EmbededImagesRepairEnricher: field '{fieldName}' has match: {matchValue}", field.Name, System.Net.WebUtility.HtmlDecode(match.Value));
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

                                int attachmentIndex = wi.ToWorkItem().Attachments.Add(new Attachment(fullImageFilePath));
                                wi.SaveToAzureDevOps();

                                var newImageLink = wi.ToWorkItem().Attachments[attachmentIndex].Uri.ToString();

                                field.Value = field.Value.ToString().Replace(match.Value, newImageLink);
                                wi.ToWorkItem().Attachments.RemoveAt(attachmentIndex);
                                //wi.ToWorkItem().Fields["System.ChangedDate"].Value = lastRevChangedDate;
                                wi.SaveToAzureDevOps();
                                try
                                {
                                    File.Delete(fullImageFilePath);
                                }
                                catch (Exception)
                                {

                                }

                            }
                        }
                    }

                    //find links in field, match #12345
                    //<!--<a href="https://dev.azure.com/GEBmanDEV/b88f3c69-ad72-476f-aec4-25cdd6db838d/_workitems/edit/11933" data-vss-mention="version:1.0">#188</a>-->
                    string regExSearchForIssueUrl = "(?<=#)[0-9]*";
                    matches = Regex.Matches((string)field.Value, regExSearchForIssueUrl);
                    
                    foreach (Match match in matches)
                    {

                        foreach (Link item in linkList)
                        {
                            try
                            {
                                Log.LogInformation("Migrating link for {sourceWorkItemLinkStartId} of type {ItemGetTypeName}", wi.Id, item.GetType().Name);
                                if (item is RelatedLink rlink)
                                {
                                    var workItem = Engine.Target.WorkItems.GetWorkItem(rlink.RelatedWorkItemId);
                                    var reflectedWorkItemId = Engine.Target.WorkItems.GetReflectedWorkItemId(workItem) as TfsReflectedWorkItemId;
                                    if (string.Equals(reflectedWorkItemId?.WorkItemId, match.Value))
                                    {
                                        field.Value = Regex.Replace(field.Value.ToString(), $"(?<=href=\"){oldTfsurl}.*?" + $"/{match.Value}/?(?=\")", new TfsReflectedWorkItemId(workItem).ToString(),RegexOptions.IgnoreCase);

                                        field.Value = Regex.Replace(field.Value.ToString(), "(?<=#)"+match.Value, workItem.Id);
                                        wi.SaveToAzureDevOps();
                                    }
                                }
                                else
                                {
                                    Log.LogError($"Not implemented Link conversion for {match.Value} [{item.BaseType.ToString()}]");
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.LogError(ex, $"[UnexpectedErrorException] Replacing Link for {match.Value}");
                            }
                        }
                    }

                    var oldProject = Engine.Config.Source.AsTeamProjectConfig().Project;
                    string regExSearchForIssueLinkUncert = $"(?<={oldTfsurl}/?{oldProject}/_workitems/edit/)";
                    string regExSearchForIssueLink = regExSearchForIssueLinkUncert + $"[0-9]*";
                    matches = Regex.Matches((string)field.Value, regExSearchForIssueLink);

                    foreach (Match match in matches)
                    {

                        foreach (Link item in linkList)
                        {
                            try
                            {
                                Log.LogInformation("Migrating link for {sourceWorkItemLinkStartId} of type {ItemGetTypeName}", wi.Id, item.GetType().Name);
                                if (item is RelatedLink rlink)
                                {
                                    var workItem = Engine.Target.WorkItems.GetWorkItem(rlink.RelatedWorkItemId);
                                    var reflectedWorkItemId = Engine.Target.WorkItems.GetReflectedWorkItemId(workItem) as TfsReflectedWorkItemId;
                                    if (string.Equals(reflectedWorkItemId?.WorkItemId, match.Value))
                                    {
                                        field.Value = Regex.Replace(field.Value.ToString(), reflectedWorkItemId.ToString(), new TfsReflectedWorkItemId(workItem).ToString());

                                        field.Value = Regex.Replace(field.Value.ToString(), "(?<=<a.*?)" + match.Value + "(?=.*?</a>)", rlink.RelatedWorkItemId.ToString());
                                        wi.SaveToAzureDevOps();
                                    }
                                }
                                else
                                {
                                    Log.LogError($"Not implemented Link conversion for {match.Value} [{item.BaseType.ToString()}]");
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.LogError(ex, $"[UnexpectedErrorException] Replacing Link for {match.Value}");
                            }
                        }
                    }

                }
            }

            var comments = witClient.GetCommentsAsync(project, int.Parse(wi.Id)).Result;

            var attIndices = new List<Tuple<Attachment, string, string>>();

            foreach (Comment comment in comments.Comments)
            {
                MatchCollection matches = Regex.Matches((string)comment.Text, regExSearchForImageUrl);
                var commentUpdate = new CommentUpdate();
                commentUpdate.Text = comment.Text;
                string regExSearchFileName = "(?<=FileName=)[^=]*";
                foreach (Match match in matches)
                {
                    if (match.Value.ToLower().Contains(oldTfsurl.ToLower()) || match.Value.ToLower().Contains(oldTfsurlOppositeSchema.ToLower()))
                    {
                        //save image locally and upload as attachment
                        Match newFileNameMatch = Regex.Match(match.Value, regExSearchFileName, RegexOptions.IgnoreCase);
                        if (newFileNameMatch.Success)
                        {
                            Log.LogDebug("EmbededImagesRepairEnricher: History has match: {matchValue}", System.Net.WebUtility.HtmlDecode(match.Value));
                            string fullImageFilePath = Path.GetTempPath() + Guid.NewGuid().ToString("N") + Path.GetExtension(newFileNameMatch.Value);//newFileNameMatch.Value;

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
                                        Log.LogDebug("EmbededImagesRepairEnricher: Image {MatchValue} could not be found in WorkItem {WorkItemId}, Field History", match.Value, wi.Id);
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
                                throw new Exception($"Downloaded image [{fullImageFilePath}] from Work Item [{wi.ToWorkItem().Id}] Field: History could not be identified as an image. Authentication issue?");
                            }

                            var attachment = new Attachment(fullImageFilePath);
                            int attachmentIndex = wi.ToWorkItem().Attachments.Add(attachment);
                            attIndices.Add(new Tuple<Attachment, string, string>(attachment, match.Value, fullImageFilePath));

                        }
                    }
                }

                if (matches.Count > 0)
                {
                    wi.SaveToAzureDevOps();
                    // TODO: 2D Array for attindices, this cycles all the attachments for the comments, not just for the comment, but it is minor
                    foreach (var attIndex in attIndices)
                    {
                        var newImageLink = attIndex.Item1.Uri.ToString();

                        commentUpdate.Text = commentUpdate.Text.Replace(attIndex.Item2, newImageLink);
                    }
                }

                //find links in field, match #12345
                //<!--<a href="https://dev.azure.com/GEBmanDEV/b88f3c69-ad72-476f-aec4-25cdd6db838d/_workitems/edit/11933" data-vss-mention="version:1.0">#188</a>-->
                string regExSearchForIssueUrl = "(?<=#)[0-9]*";
                matches = Regex.Matches(comment.Text, regExSearchForIssueUrl);

                foreach (Match match in matches)
                {

                    foreach (Link item in linkList)
                    {
                        try
                        {
                            Log.LogInformation("Migrating link for {sourceWorkItemLinkStartId} of type {ItemGetTypeName}", wi.Id, item.GetType().Name);
                            if (item is RelatedLink rlink)
                            {
                                var workItem = Engine.Target.WorkItems.GetWorkItem(rlink.RelatedWorkItemId);
                                var reflectedWorkItemId = Engine.Target.WorkItems.GetReflectedWorkItemId(workItem) as TfsReflectedWorkItemId;
                                if (string.Equals(reflectedWorkItemId?.WorkItemId, match.Value))
                                {
                                    commentUpdate.Text = Regex.Replace(commentUpdate.Text, $"(?<=href=\"){oldTfsurl}.*?" + $"/{match.Value}/?(?=\")", new TfsReflectedWorkItemId(workItem).ToString(), RegexOptions.IgnoreCase);

                                    commentUpdate.Text = Regex.Replace(commentUpdate.Text, "(?<=#)" + match.Value, workItem.Id);
                                }
                            }
                            else
                            {
                                Log.LogError($"Not implemented Link conversion for {match.Value} [{item.BaseType.ToString()}]");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogError(ex, $"[UnexpectedErrorException] Replacing Link for {match.Value}");
                        }
                    }
                }

                var oldProject = Engine.Config.Source.AsTeamProjectConfig().Project;
                string regExSearchForIssueLinkUncert = $"(?<={oldTfsurl}/?{oldProject}/_workitems/edit/)";
                string regExSearchForIssueLink = regExSearchForIssueLinkUncert + $"[0-9]*";
                matches = Regex.Matches(comment.Text, regExSearchForIssueLink);

                foreach (Match match in matches)
                {

                    foreach (Link item in linkList)
                    {
                        try
                        {
                            Log.LogInformation("Migrating link for {sourceWorkItemLinkStartId} of type {ItemGetTypeName}", wi.Id, item.GetType().Name);
                            if (item is RelatedLink rlink)
                            {
                                var workItem = Engine.Target.WorkItems.GetWorkItem(rlink.RelatedWorkItemId);
                                var reflectedWorkItemId = Engine.Target.WorkItems.GetReflectedWorkItemId(workItem) as TfsReflectedWorkItemId;
                                if (string.Equals(reflectedWorkItemId?.WorkItemId, match.Value))
                                {
                                    commentUpdate.Text = Regex.Replace(commentUpdate.Text, reflectedWorkItemId.ToString(), new TfsReflectedWorkItemId(workItem).ToString());

                                    commentUpdate.Text = Regex.Replace(commentUpdate.Text, "(?<=<a.*?)" + match.Value + "(?=.*?</a>)", rlink.RelatedWorkItemId.ToString());
                                }
                            }
                            else
                            {
                                Log.LogError($"Not implemented Link conversion for {match.Value} [{item.BaseType.ToString()}]");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogError(ex, $"[UnexpectedErrorException] Replacing Link for {match.Value}");
                        }
                    }
                }

                if(!string.Equals(commentUpdate.Text,comment.Text))
                    witClient.UpdateCommentAsync(commentUpdate, project, int.Parse(wi.Id), comment.Id).Wait();
            }
            if (attIndices.Any())
            {

                foreach (var attIndex in attIndices)
                {
                    wi.ToWorkItem().Attachments.Remove(attIndex.Item1);
                }
                wi.SaveToAzureDevOps();
                foreach (var attIndex in attIndices)
                {

                    try
                    {
                        File.Delete(attIndex.Item3);
                    }
                    catch (Exception)
                    {

                    }
                }

            }
        }
    }
}