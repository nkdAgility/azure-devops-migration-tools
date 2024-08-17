using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MigrationTools.Tools
{
    public class TfsStaticTools
    {
        public TfsUserMappingTool UserMapping { get; private set; }
        public TfsAttachmentTool Attachment { get; private set; }
        public TfsNodeStructureTool NodeStructure { get; private set; }
        public TfsRevisionManagerTool RevisionManager { get; private set; }
        public TfsWorkItemLinkTool WorkItemLink { get; private set; }
        public TfsWorkItemEmbededLinkTool WorkItemEmbededLink { get; private set; }
        public TfsValidateRequiredFieldTool ValidateRequiredField { get; private set; }
        public TfsTeamSettingsTool TeamSettings { get; private set; }

        public TfsEmbededImagesTool EmbededImages { get; private set; }

        public TfsGitRepositoryTool GitRepository { get; private set; }
        public TfsStaticTools(ITelemetryLogger telemetry,
                                        ILogger<TfsStaticTools> logger,
                                        TfsUserMappingTool userMappingEnricher,
                                        TfsAttachmentTool attachmentEnricher,
                                        TfsNodeStructureTool nodeStructureEnricher,
                                        TfsRevisionManagerTool revisionManager,
                                        TfsWorkItemLinkTool workItemLinkEnricher,
                                        TfsWorkItemEmbededLinkTool workItemEmbeddedLinkEnricher,
                                        TfsValidateRequiredFieldTool requiredFieldValidator,
                                        TfsTeamSettingsTool teamSettingsEnricher,
                                        TfsEmbededImagesTool embededImagesEnricher,
                                        TfsGitRepositoryTool TfsGitRepositoryTool
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
            GitRepository = TfsGitRepositoryTool;
        }

    }
}
