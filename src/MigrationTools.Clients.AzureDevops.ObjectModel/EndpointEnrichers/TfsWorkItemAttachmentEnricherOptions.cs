using System;

namespace MigrationTools.EndpointEnrichers
{
    public class TfsWorkItemAttachmentEnricherOptions : WorkItemAttachmentEnricherOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemAttachmentEnricher);
    }
}