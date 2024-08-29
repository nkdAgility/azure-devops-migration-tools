using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsWorkItemEmbededLinkTool : Tool<TfsWorkItemEmbededLinkToolOptions>
    {
        private const string LogTypeName = nameof(TfsWorkItemEmbededLinkTool);
        private const string RegexPatternLinkAnchorTag = "<a[^>].*?(?:href=\"(?<href>[^\"]*)\".*?|(?<version>data-vss-mention=\"[^\"]*\").*?)*>(?<value>.*?)<\\/a?>";
        private const string RegexPatternWorkItemUrl = "http[s]*://.*?/_workitems/edit/(?<id>\\d+)";
        private  Lazy<List<TeamFoundationIdentity>> _targetTeamFoundationIdentitiesLazyCache;

        public TfsWorkItemEmbededLinkTool(IOptions<TfsWorkItemEmbededLinkToolOptions> options, IServiceProvider services, ILogger<TfsWorkItemEmbededLinkTool> logger, ITelemetryLogger telemetryLogger)
            : base(options, services, logger, telemetryLogger)
        {

           
        }

        public  int Enrich(TfsProcessor processor, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            _targetTeamFoundationIdentitiesLazyCache = new Lazy<List<TeamFoundationIdentity>>(() =>
            {
                try
                {
                    TfsTeamService teamService = processor.Target.GetService<TfsTeamService>();
                    TfsConnection connection = (TfsConnection)processor.Target.InternalCollection;

                    var identityService = processor.Target.GetService<IIdentityManagementService>();
                    var tfi = identityService.ReadIdentity(IdentitySearchFactor.General, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);
                    return identityService.ReadIdentities(tfi.Members, MembershipQuery.None, ReadIdentityOptions.None).ToList();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "{LogTypeName}: Unable load list of identities from target collection.", LogTypeName);
                    Telemetry.TrackException(ex, null, null);
                    return new List<TeamFoundationIdentity>();
                }
            });


            string oldTfsurl = processor.Source.Options.Collection.ToString();
            string newTfsurl = processor.Target.Options.Collection.ToString();

            string oldTfsProject = processor.Source.Options.Project;
            string newTfsProject = processor.Target.Options.Project;

            Log.LogInformation("{LogTypeName}: Fixing embedded mention links on target work item {targetWorkItemId} from {oldTfsurl} to {newTfsurl}", LogTypeName, targetWorkItem.Id, oldTfsurl, newTfsurl);

            foreach (Field field in targetWorkItem.ToWorkItem().Fields)
            {
                if (field.Value == null
                    || string.IsNullOrWhiteSpace(field.Value.ToString())
                    || field.FieldDefinition.FieldType != FieldType.Html && field.FieldDefinition.FieldType != FieldType.History)
                {
                    continue;
                }

                try
                {
                    var anchorTagMatches = Regex.Matches((string)field.Value, RegexPatternLinkAnchorTag);
                    foreach (Match anchorTagMatch in anchorTagMatches)
                    {
                        if (!anchorTagMatch.Success) continue;

                        var href = anchorTagMatch.Groups["href"].Value;
                        var version = anchorTagMatch.Groups["version"].Value;
                        var value = anchorTagMatch.Groups["value"].Value;

                        if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(value))
                            continue;

                        var workItemLinkMatch = Regex.Match(href, RegexPatternWorkItemUrl);
                        if (workItemLinkMatch.Success)
                        {
                            var workItemId = workItemLinkMatch.Groups["id"].Value;
                            Log.LogDebug("{LogTypeName}: Source work item {workItemId} mention link traced on field {fieldName} on target work item {targetWorkItemId}.", LogTypeName, workItemId, field.Name, targetWorkItem.Id);
                            var sourceLinkWi = processor.Source.WorkItems.GetWorkItem(workItemId, false);
                            if (sourceLinkWi != null)
                            {
                                var linkWI = processor.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceLinkWi);
                                if (linkWI != null)
                                {
                                    var replaceValue = anchorTagMatch.Value
                                        .Replace(workItemId, linkWI.Id)
                                        .Replace(oldTfsProject, newTfsProject)
                                        .Replace(oldTfsurl, newTfsurl);
                                    field.Value = field.Value.ToString().Replace(anchorTagMatch.Value, replaceValue);
                                    Log.LogInformation("{LogTypeName}: Source work item {workItemId} mention link was successfully replaced with target work item {linkWIId} mention link on field {fieldName} on target work item {targetWorkItemId}.", LogTypeName, workItemId, linkWI.Id, field.Name, targetWorkItem.Id);
                                }
                                else
                                {
                                    var replaceValue = value;
                                    field.Value = field.Value.ToString().Replace(anchorTagMatch.Value, replaceValue);
                                    Log.LogWarning("{LogTypeName}: [SKIP] Matching target work item mention link for source work item {workItemId} mention link on field {fieldName} on target work item {targetWorkItemId} was not found on the target collection. So link is replaced with just simple text.", LogTypeName, workItemId, field.Name, targetWorkItem.Id);
                                }
                            }
                            else
                            {
                                var replaceValue = value;
                                field.Value = field.Value.ToString().Replace(anchorTagMatch.Value, replaceValue);
                                Log.LogInformation("{LogTypeName}: [SKIP] Source work item {workItemId} mention link on field {fieldName} was not found on the source collection.", LogTypeName, workItemId, field.Name, targetWorkItem.Id);
                            }
                        }
                        else if ((href.StartsWith("mailto:") || href.StartsWith("#")) && value.StartsWith("@"))
                        {
                            var displayName = value.Substring(1);
                            Log.LogDebug("{LogTypeName}: User identity {displayName} mention traced on field {fieldName} on target work item {targetWorkItemId}.", LogTypeName, displayName, field.Name, targetWorkItem.Id);
                            var identity = _targetTeamFoundationIdentitiesLazyCache.Value.FirstOrDefault(i => i.DisplayName == displayName);
                            if (identity != null)
                            {
                                var replaceValue = anchorTagMatch.Value.Replace(href, "#").Replace(version, $"data-vss-mention=\"version:2.0,{identity.TeamFoundationId}\"");
                                field.Value = field.Value.ToString().Replace(anchorTagMatch.Value, replaceValue);
                                Log.LogInformation("{LogTypeName}: User identity {displayName} mention was successfully matched on field {fieldName} on target work item {targetWorkItemId}.", LogTypeName, displayName, field.Name, targetWorkItem.Id);
                            }
                            else
                            {
                                Log.LogInformation("{LogTypeName}: [SKIP] Matching user identity {displayName} mention was not found on field {fieldName} on target work item {targetWorkItemId}. So left it as it is.", LogTypeName, displayName, field.Name, targetWorkItem.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "{LogTypeName}: Unable to fix embedded mention links on field {fieldName} on target work item {targetWorkItemId} from {oldTfsurl} to {newTfsurl}", LogTypeName, field.Name, targetWorkItem.Id, oldTfsurl, newTfsurl);
                    Telemetry.TrackException(ex, null, null);
                }
            }

            return 0;
        }


    }
}