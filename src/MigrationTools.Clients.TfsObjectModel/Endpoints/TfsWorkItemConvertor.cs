using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemConvertor
    {
        public void MapWorkItemtoWorkItemData(WorkItemData context_wid, WorkItem context_wi, Dictionary<string, FieldItem> fieldsOfRevision = null)
        {
            context_wid.Id = context_wi.Id.ToString();
            context_wid.Title = fieldsOfRevision != null ? fieldsOfRevision["System.Title"].Value.ToString() : context_wi.Title;
            context_wid.ProjectName = context_wi.Project?.Name;
            context_wid.Type = fieldsOfRevision != null ? fieldsOfRevision["System.WorkItemType"].Value.ToString() : context_wi.Type.Name;
            context_wid.Rev = fieldsOfRevision != null ? (int)fieldsOfRevision["System.Rev"].Value : context_wi.Rev;
            context_wid.ChangedDate = fieldsOfRevision != null ? (DateTime)fieldsOfRevision["System.ChangedDate"].Value : context_wi.ChangedDate;

            var fieldItems = GetFieldItems(context_wi.Fields);
            if (fieldsOfRevision != null)
            {
                foreach (var revField in fieldsOfRevision)
                {
                    fieldItems[revField.Key] = new FieldItem
                    {
                        Name = revField.Value.Name,
                        FieldType = revField.Value.FieldType,
                        ReferenceName = revField.Value.ReferenceName,
                        Value = revField.Value.Value,
                        internalObject = revField.Value.internalObject,
                    };
                }
            }

            context_wid.Fields = fieldItems;
            context_wid.Links = GetLinkItems(context_wi.Links);
            context_wid.Revisions = fieldsOfRevision == null ? GetRevisionItems(context_wi.Revisions) : null;

        }

        private SortedDictionary<int, RevisionItem> GetRevisionItems(RevisionCollection tfsRevisions)
        {
            var items = tfsRevisions.OfType<Revision>().Select(x => new RevisionItem()
            {
                WorkItemId = x.WorkItem.Id,
                Index = x.Index,
                Number = (int)x.Fields["System.Rev"].Value,
                ChangedDate = (DateTime)x.Fields["System.ChangedDate"].Value,
                OriginalChangedDate = (DateTime)x.Fields["System.ChangedDate"].Value,
                CreatedDate = (DateTime)x.Fields["System.CreatedDate"].Value,
                OriginalCreatedDate = (DateTime)x.Fields["System.CreatedDate"].Value,
                Type = x.Fields["System.WorkItemType"].Value as string,
                Fields = GetFieldItems(x.Fields)
            }).ToList();

            try
            {
                var dictionary = items.ToDictionary(item => item.Number);
                return new SortedDictionary<int, RevisionItem>(dictionary);
            }
            catch (ArgumentException)
            {
                Log.Warning("For some Reason there are multiple Revisions on {WorkItemId} with the same System.Rev. We will create a renumbered list...", items[0].WorkItemId);
                var currentNumber = -1;
                foreach (var item in items)
                {
                    if (item.Number == currentNumber)
                    {
                        item.Number += 1;
                    }
                    currentNumber = item.Number;
                }
                var dictionary = items.ToDictionary(item => item.Number);
                return new SortedDictionary<int, RevisionItem>(dictionary);
            }
        }

        private Dictionary<string, FieldItem> GetFieldItems(FieldCollection tfsFields)
        {
            return tfsFields.OfType<Field>().Select(x => new FieldItem()
            {
                Name = x.Name,
                ReferenceName = x.ReferenceName,
                Value = x.Value,
                FieldType = x.FieldDefinition.FieldType.ToString(),
                IsIdentity = x.FieldDefinition.IsIdentity,
                internalObject = x
            })
            .ToDictionary(r => r.ReferenceName);
        }

        private List<LinkItem> GetLinkItems(LinkCollection tfsLinks)
        {
            var ls = new List<LinkItem>();

            foreach (Link link in tfsLinks)
            {
                switch (link)
                {
                    case Hyperlink lh:
                        ls.Add(new LinkItem()
                        {
                            LinkType = LinkItemType.Hyperlink,
                            ArtifactLinkType = link.ArtifactLinkType.Name,
                            Comment = lh.Comment,
                            LinkUri = lh.Location,
                            internalObject = link
                        });
                        break;
                    case ExternalLink le:
                        ls.Add(new LinkItem()
                        {
                            LinkType = LinkItemType.ExternalLink,
                            ArtifactLinkType = link.ArtifactLinkType.Name,
                            Comment = le.Comment,
                            LinkUri = le.LinkedArtifactUri,
                            internalObject = link
                        });
                        break;
                    case RelatedLink lr:
                        ls.Add(new LinkItem()
                        {
                            LinkType = LinkItemType.RelatedLink,
                            ArtifactLinkType = link.ArtifactLinkType.Name,
                            Comment = lr.Comment,
                            RelatedWorkItem = lr.RelatedWorkItemId,
                            LinkTypeEndImmutableName = lr.LinkTypeEnd == null ? "" : lr.LinkTypeEnd.ImmutableName,
                            LinkTypeEndName = lr.LinkTypeEnd == null ? "" : lr.LinkTypeEnd.Name,
                            internalObject = link
                        });
                        break;
                    default:
                        Log.Debug("TfsExtensions::GetLinkData: RelatedLink is of ArtifactLinkType '{ArtifactLinkType}' and Type '{GetTypeName}' on WorkItemId: {WorkItemId}", link.ArtifactLinkType.Name, link.GetType().Name, tfsLinks.WorkItem.Id);
                        break;
                }
            }
            return ls;
        }

    }
}
