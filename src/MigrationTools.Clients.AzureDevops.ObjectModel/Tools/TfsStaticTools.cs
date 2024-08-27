using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    public class TfsCommonTools : CommonTools
    {
        public TfsCommonTools(
            TfsUserMappingTool userMappingEnricher,
                                        TfsAttachmentTool attachmentEnricher,
                                        TfsNodeStructureTool nodeStructureEnricher,
                                        TfsRevisionManagerTool revisionManager,
                                        TfsWorkItemLinkTool workItemLinkEnricher,
                                        TfsWorkItemEmbededLinkTool workItemEmbeddedLinkEnricher,
                                        TfsValidateRequiredFieldTool requiredFieldValidator,
                                        TfsTeamSettingsTool teamSettingsEnricher,
                                        TfsEmbededImagesTool embededImagesEnricher,
                                        TfsGitRepositoryTool TfsGitRepositoryTool,
                                        IStringManipulatorTool StringManipulatorTool,
                                        IWorkItemTypeMappingTool workItemTypeMapping,
                                        IFieldMappingTool fieldMappingTool
            ) : base(StringManipulatorTool, workItemTypeMapping, fieldMappingTool)
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


    }
}
