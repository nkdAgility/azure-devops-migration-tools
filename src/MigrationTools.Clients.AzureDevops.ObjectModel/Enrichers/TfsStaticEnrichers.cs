using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools.Enrichers
{
    public class TfsStaticEnrichers
    {
        public TfsUserMappingEnricher UserMapping { get; private set; }
        public TfsAttachmentEnricher Attachment { get; private set; }
        public TfsNodeStructure NodeStructure { get; private set; }
        public TfsRevisionManager RevisionManager { get; private set; }
        public TfsWorkItemLinkEnricher WorkItemLink { get; private set; }
        public TfsWorkItemEmbededLinkEnricher WorkItemEmbededLink { get; private set; }
        public TfsValidateRequiredField ValidateRequiredField { get; private set; }
        public TfsTeamSettingsEnricher TeamSettings { get; private set; }

        public TfsEmbededImagesEnricher EmbededImages { get; private set; }

        public TfsGitRepositoryEnricher GitRepository { get; private set; }
        public TfsStaticEnrichers(ITelemetryLogger telemetry,
                                        ILogger<TfsStaticEnrichers> logger,
                                        TfsUserMappingEnricher userMappingEnricher,
                                        TfsAttachmentEnricher attachmentEnricher,
                                        TfsNodeStructure nodeStructureEnricher,
                                        TfsRevisionManager revisionManager,
                                        TfsWorkItemLinkEnricher workItemLinkEnricher,
                                        TfsWorkItemEmbededLinkEnricher workItemEmbeddedLinkEnricher,
                                        TfsValidateRequiredField requiredFieldValidator,
                                        TfsTeamSettingsEnricher teamSettingsEnricher,
                                        TfsEmbededImagesEnricher embededImagesEnricher,
                                        TfsGitRepositoryEnricher tfsGitRepositoryEnricher
            )
        {
            UserMapping = userMappingEnricher;
            Attachment = attachmentEnricher;
            NodeStructure = nodeStructureEnricher;
            RevisionManager = revisionManager;
            WorkItemLink = workItemLinkEnricher;
            WorkItemEmbededLink = workItemEmbeddedLinkEnricher;
            ValidateRequiredField = requiredFieldValidator;
            TeamSettings = teamSettingsEnricher;
            EmbededImages = embededImagesEnricher;
            GitRepository = tfsGitRepositoryEnricher;
        }

    }
}
