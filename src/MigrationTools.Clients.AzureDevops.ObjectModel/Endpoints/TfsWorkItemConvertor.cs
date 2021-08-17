using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            context_wid.Title = fieldsOfRevision != null ? fieldsOfRevision["System.Title"].ToString() : context_wi.Title;
            context_wid.ProjectName = context_wi.Project?.Name;
            context_wid.Type = fieldsOfRevision != null ? fieldsOfRevision["System.WorkItemType"].ToString() : context_wi.Type.Name;
            context_wid.Rev = fieldsOfRevision != null ? (int)fieldsOfRevision["System.Rev"].Value : context_wi.Rev;
            context_wid.ChangedDate = fieldsOfRevision != null ? (DateTime)fieldsOfRevision["System.ChangedDate"].Value : context_wi.ChangedDate;

            context_wid.Fields = GetFieldItems(context_wi.Fields);
            context_wid.Links = GetLinkItems(context_wi.Links);
            context_wid.Revisions = fieldsOfRevision == null ? GetRevisionItems(context_wi.Revisions) : null;

        }

        private SortedDictionary<int, RevisionItem> GetRevisionItems(RevisionCollection tfsRevisions)
        {
            return new SortedDictionary<int, RevisionItem>((from Revision x in tfsRevisions
                                                            select new RevisionItem()
                                                            {
                                                                Index = x.Index,
                                                                Number = (int)x.Fields["System.Rev"].Value,
                                                                ChangedDate = (DateTime)x.Fields["System.ChangedDate"].Value,
                                                                Type = x.Fields["System.WorkItemType"].Value as string,
                                                                Fields = GetFieldItems(x.Fields)
                                                            }).ToDictionary(r => r.Number, r => r));
        }

        private Dictionary<string, FieldItem> GetFieldItems(FieldCollection tfsFields)
        {
            return (from Field x in tfsFields
                    select new FieldItem()
                    {
                        Name = x.Name,
                        ReferenceName = x.ReferenceName,
                        Value = x.Value,
                        internalObject = x
                    }).ToDictionary(r => r.ReferenceName, r => r);
        }

        private List<LinkItem> GetLinkItems(LinkCollection tfsLinks)
        {
            var ls = new List<LinkItem>();

            foreach (Link l in tfsLinks)
            {
                if (l is Hyperlink)
                {
                    var lh = (Hyperlink)l;
                    ls.Add(new LinkItem()
                    {
                        LinkType = LinkItemType.Hyperlink,
                        ArtifactLinkType = l.ArtifactLinkType.Name,
                        Comment = lh.Comment,
                        LinkUri = lh.Location,
                        internalObject = l
                    });
                }
                else if (l is ExternalLink)
                {
                    var le = (ExternalLink)l;
                    ls.Add(new LinkItem()
                    {
                        LinkType = LinkItemType.ExternalLink,
                        ArtifactLinkType = l.ArtifactLinkType.Name,
                        Comment = le.Comment,
                        LinkUri = le.LinkedArtifactUri,
                        internalObject = l
                    });
                }
                else if (l is RelatedLink)
                {
                    var lr = (RelatedLink)l;
                    ls.Add(new LinkItem()
                    {
                        LinkType = LinkItemType.RelatedLink,
                        ArtifactLinkType = l.ArtifactLinkType.Name,
                        Comment = lr.Comment,
                        RelatedWorkItem = lr.RelatedWorkItemId,
                        LinkTypeEndImmutableName = lr.LinkTypeEnd == null ? "" : lr.LinkTypeEnd.ImmutableName,
                        LinkTypeEndName = lr.LinkTypeEnd == null ? "" : lr.LinkTypeEnd.Name,
                        internalObject = l
                    });
                }
                else
                {
                    Log.Debug("TfsExtensions::GetLinkData: RelatedLink is of ArtifactLinkType '{ArtifactLinkType}' and Type '{GetTypeName}' on WorkItemId: {WorkItemId}", l.ArtifactLinkType.Name, l.GetType().Name, tfsLinks.WorkItem.Id);
                }
            }
            return ls;
        }

    }
}
