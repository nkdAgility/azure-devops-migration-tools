using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Extended collection of tools specific to Team Foundation Server and Azure DevOps migrations, providing TFS-specific functionality beyond the common tools.
    /// </summary>
    public class TfsCommonTools : CommonTools
    {
        /// <summary>
        /// Initializes a new instance of the TfsCommonTools class.
        /// </summary>
        /// <param name="userMappingEnricher">Tool for mapping users between source and target</param>
        /// <param name="attachmentEnricher">Tool for handling work item attachments</param>
        /// <param name="nodeStructureEnricher">Tool for managing area and iteration path structures</param>
        /// <param name="revisionManager">Tool for managing work item revision history</param>
        /// <param name="workItemLinkEnricher">Tool for processing work item links</param>
        /// <param name="workItemEmbeddedLinkEnricher">Tool for processing embedded links in work items</param>
        /// <param name="requiredFieldValidator">Tool for validating required fields</param>
        /// <param name="teamSettingsEnricher">Tool for migrating team settings</param>
        /// <param name="embededImagesEnricher">Tool for processing embedded images</param>
        /// <param name="TfsGitRepositoryTool">Tool for git repository operations</param>
        /// <param name="StringManipulatorTool">Tool for string field manipulation</param>
        /// <param name="workItemTypeMapping">Tool for work item type mapping</param>
        /// <param name="workItemMapping">Tool for work item mapping.</param>
        /// <param name="workItemTypeValidatorTool">Tool for work item type validation.</param>
        /// <param name="fieldMappingTool">Tool for field mapping operations</param>
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
            IWorkItemMappingTool workItemMapping,
            TfsWorkItemTypeValidatorTool workItemTypeValidatorTool,
            IFieldMappingTool fieldMappingTool
            ) : base(StringManipulatorTool, workItemTypeMapping, workItemMapping, fieldMappingTool)
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
            WorkItemTypeValidatorTool = workItemTypeValidatorTool;
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

        /// <summary>
        /// Tool for work item type validation.
        /// </summary>
        public TfsWorkItemTypeValidatorTool WorkItemTypeValidatorTool { get; private set; }
    }
}
