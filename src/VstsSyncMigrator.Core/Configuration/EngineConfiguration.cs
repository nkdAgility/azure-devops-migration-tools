using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VstsSyncMigrator.Engine.Configuration.FieldMap;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine.Configuration
{
    public class EngineConfiguration
    {
        public bool TelemetryEnableTrace { get; set; }
        public TeamProjectConfig Source { get; set; }
        public TeamProjectConfig Target { get; set; }
        public string ReflectedWorkItemIDFieldName { get; set; }
        public List<IFieldMapConfig> FieldMaps { get; set; }
        public Dictionary<string, string> WorkItemTypeDefinition { get; set; }
        public List<ITfsProcessingConfig> Processors { get; set; }

        public static EngineConfiguration GetDefault()
        {
            EngineConfiguration ec = new EngineConfiguration();
            ec.TelemetryEnableTrace = true;
            ec.Source = new TeamProjectConfig() { Name = "DemoProjs", Collection = new Uri("https://sdd2016.visualstudio.com/") };
            ec.Target = new TeamProjectConfig() { Name = "DemoProjt", Collection = new Uri("https://sdd2016.visualstudio.com/") };
            ec.ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId";
            ec.FieldMaps = new List<IFieldMapConfig>();
            ec.FieldMaps.Add(new MultiValueConditionalMapConfig()
            {
                WorkItemTypeName = "*",
                sourceFieldsAndValues = new Dictionary<string, string>
                 {
                     { "Field1", "Value1" },
                     { "Field2", "Value2" }
                 },
                targetFieldsAndValues = new Dictionary<string, string>
                 {
                     { "Field1", "Value1" },
                     { "Field2", "Value2" }
                 }
            });
            ec.FieldMaps.Add(new FieldBlankMapConfig() {
                WorkItemTypeName = "*",
                targetField = "TfsMigrationTool.ReflectedWorkItemId"
            });
            ec.FieldMaps.Add(new FieldValueMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "System.State",
                targetField = "System.State",
                valueMapping = new Dictionary<string, string> {
                    { "Approved", "New" },
                    { "New", "New" },
                    { "Committed", "Active" },
                    { "In Progress", "Active" },
                    { "To Do", "New" },
                    { "Done", "Closed" }
                }
            });
            ec.FieldMaps.Add(new FieldtoFieldMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "Microsoft.VSTS.Common.BacklogPriority",
                targetField = "Microsoft.VSTS.Common.StackRank"
            });
            ec.FieldMaps.Add(new FieldtoTagMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "System.State",
                formatExpression = "ScrumState:{0}"
            });
            ec.FieldMaps.Add(new FieldMergeMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField1 = "System.Description",
                sourceField2 = "Microsoft.VSTS.Common.AcceptanceCriteria",
                targetField = "System.Description",
                formatExpression = @"{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"
            });
            ec.FieldMaps.Add(new RegexFieldMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "COMPANY.PRODUCT.Release",
                targetField = "COMPANY.DEVISION.MinorReleaseVersion",
                pattern = @"PRODUCT \d{4}.(\d{1})",
                replacement = "$1"
            });
            ec.FieldMaps.Add(new FieldValuetoTagMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "Microsoft.VSTS.CMMI.Blocked",
                pattern = @"Yes",
                formatExpression = "{0}"
            });
            ec.FieldMaps.Add(new TreeToTagMapConfig()
            {
                WorkItemTypeName = "*",
                timeTravel = 1,
                toSkip = 3
            });
            ec.WorkItemTypeDefinition = new Dictionary<string, string> {
                    { "Bug", "Bug" },
                    { "Product Backlog Item", "Product Backlog Item" }
            };
            ec.Processors = new List<ITfsProcessingConfig>();
            ec.Processors.Add(new WorkItemMigrationConfig() { Enabled = false, UpdateCreatedBy=true, PrefixProjectToNodes = true, UpdateCreatedDate=true, UpdateSoureReflectedId=true, QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')" });
            ec.Processors.Add(new WorkItemRevisionReplayMigrationConfig() { Enabled = false, PrefixProjectToNodes = true, UpdateSoureReflectedId=true, QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')" });
            ec.Processors.Add(new WorkItemUpdateConfig() { Enabled = false, QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')" });
            ec.Processors.Add(new NodeStructuresMigrationConfig() { Enabled = false });
            ec.Processors.Add(new LinkMigrationConfig() { Enabled = false, QueryBit= @"AND ([System.ExternalLinkCount] > 0 OR [System.RelatedLinkCount] > 0)" });
            ec.Processors.Add(new WorkItemPostProcessingConfig() { Enabled = false, QueryBit= "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ", WorkItemIDs = new List<int> {1,2,3} });
            ec.Processors.Add(new WorkItemDeleteConfig() { Enabled = false });
            ec.Processors.Add(new AttachementExportMigrationConfig() { Enabled = false, QueryBit = @"AND [System.AttachedFileCount] > 0" });
            ec.Processors.Add(new AttachementImportMigrationConfig() { Enabled = false });
            ec.Processors.Add(new WorkItemQueryMigrationConfig() { Enabled = false, PrefixProjectToNodes = false });
            ec.Processors.Add(new TestVariablesMigrationConfig() { Enabled = false });
            ec.Processors.Add(new TestConfigurationsMigrationConfig() { Enabled = false });
            ec.Processors.Add(new TestPlansAndSuitsMigrationConfig() { Enabled = false, PrefixProjectToNodes = true });
            ec.Processors.Add(new TestRunsMigrationConfig() { Enabled = false });
            ec.Processors.Add(new ImportProfilePictureConfig() { Enabled = false });
            ec.Processors.Add(new ExportProfilePictureFromADConfig() { Enabled = false });
            ec.Processors.Add(new FixGitCommitLinksConfig() { Enabled = false, TargetRepository = ec.Target.Name });
            return ec;
        }

    }
}
