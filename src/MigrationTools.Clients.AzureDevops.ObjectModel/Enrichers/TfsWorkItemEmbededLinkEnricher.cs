using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class TfsWorkItemEmbededLinkEnricher : WorkItemProcessorEnricher
    {
        private const string RegexPatternLinkAnchorTag = "<a[^>].*?(?:href=\"(?<href>[^\"]*)\".*?|(?<version>data-vss-mention=\"[^\"]*\").*?)*>(?<value>.*?)<\\/a?>";
        private const string RegexPatternWorkItemUrl = "http[s]*://.*?/_workitems/edit/(?<id>\\d+)";
        private readonly Lazy<List<TeamFoundationIdentity>> _targetTeamFoundationIdentities;

        private IMigrationEngine Engine;
        
        public TfsWorkItemEmbededLinkEnricher(IServiceProvider services, ILogger<TfsWorkItemEmbededLinkEnricher> logger)
            : base(services, logger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
            _targetTeamFoundationIdentities = new Lazy<List<TeamFoundationIdentity>>(() =>
            {
                TfsTeamService teamService = Engine.Target.GetService<TfsTeamService>();
                TfsConnection connection = (TfsConnection)Engine.Target.InternalCollection;

                var identityService = Engine.Target.GetService<IIdentityManagementService>();
                var tfi = identityService.ReadIdentity(IdentitySearchFactor.General, "Project Collection Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);
                return identityService.ReadIdentities(tfi.Members, MembershipQuery.None, ReadIdentityOptions.None).ToList();
            });
        }

        [Obsolete]
        public override void Configure(bool save = false, bool filterWorkItemsThatAlreadyExistInTarget = true)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            string oldTfsurl = Engine.Source.Config.AsTeamProjectConfig().Collection.ToString();
            string newTfsurl = Engine.Target.Config.AsTeamProjectConfig().Collection.ToString();

            var logTypeName = nameof(TfsWorkItemEmbededLinkEnricher);
            Log.LogInformation($"{logTypeName}: Fixing embedded mention links on target work item {targetWorkItem.Id} from {oldTfsurl} to {newTfsurl}");

            foreach (Field field in targetWorkItem.ToWorkItem().Fields)
            {
                if (field.Value == null
                    || string.IsNullOrWhiteSpace(field.Value.ToString())
                    || (field.FieldDefinition.FieldType != FieldType.Html && field.FieldDefinition.FieldType != FieldType.History))
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
                            Log.LogDebug($"{logTypeName}: Source work item {workItemId} mention link traced on field {field.Name} on target work item {targetWorkItem.Id}.");
                            var sourceLinkWi = Engine.Source.WorkItems.GetWorkItem(workItemId);
                            if (sourceLinkWi != null)
                            {
                                var linkWI = Engine.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceLinkWi);
                                if (linkWI != null)
                                {
                                    var replaceValue = anchorTagMatch.Value.Replace(workItemId, linkWI.Id);
                                    field.Value = field.Value.ToString().Replace(anchorTagMatch.Value, replaceValue);
                                    Log.LogInformation($"{logTypeName}: Source work item {workItemId} mention link was successfully replaced with target work item {linkWI.Id} mention link on field {field.Name} on target work item {targetWorkItem.Id}.");
                                }
                                else
                                {
                                    Log.LogError($"{logTypeName}: Matching target work item mention link for source work item {workItemId} mention link on field {field.Name} on target work item {targetWorkItem.Id} was not found on the target collection.");
                                }
                            }
                            else
                            {
                                Log.LogInformation($"{logTypeName}: [SKIP] Source work item {workItemId} mention link on field {field.Name} on target work item {targetWorkItem.Id} was not found on the source collection.");
                            }
                        }
                        else if ((href.StartsWith("mailto:") || href.StartsWith("#")) && value.StartsWith("@"))
                        {
                            var displayName = value.Substring(1);
                            Log.LogDebug($"{logTypeName}: User identity {displayName} mention traced on field {field.Name} on target work item {targetWorkItem.Id}.");
                            var identity = _targetTeamFoundationIdentities.Value.FirstOrDefault(i => i.DisplayName == displayName);
                            if (identity != null)
                            {
                                var replaceValue = anchorTagMatch.Value.Replace(href, "#").Replace(version, $"data-vss-mention=\"version:2.0,{identity.TeamFoundationId}\"");
                                field.Value = field.Value.ToString().Replace(anchorTagMatch.Value, replaceValue);
                                Log.LogInformation($"{logTypeName}: User identity {displayName} mention was successfully matched on field {field.Name} on target work item {targetWorkItem.Id}.");
                            }
                            else
                            {
                                Log.LogInformation($"{logTypeName}: [SKIP] Matching user identity {displayName} mention was not found on field {field.Name} on target work item {targetWorkItem.Id}.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, $"{logTypeName}: Unable to fix embedded mention links on field {field.Name} on target work item {targetWorkItem.Id} from {oldTfsurl} to {newTfsurl}");
                }
            }

            return 0;
        }

        [Obsolete("v2 Archtecture: use Configure(bool save = true, bool filter = true) instead", true)]
        public override void Configure(IProcessorEnricherOptions options)
        {
            throw new NotImplementedException();
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