using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Core.Configuration.Processing;
using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration
{
    public class EngineConfiguration
    {
        public string Version { get; set; }
        public bool TelemetryEnableTrace { get; set; }
        public bool workaroundForQuerySOAPBugEnabled { get; set; }
        public TeamProjectConfig Source { get; set; }
        public TeamProjectConfig Target { get; set; }
        public List<IFieldMapConfig> FieldMaps { get; set; }
        public Dictionary<string, string> WorkItemTypeDefinition { get; set; }
        public Dictionary<string, string> GitRepoMapping { get; set; }
        public List<ITfsProcessingConfig> Processors { get; set; }

        public static EngineConfiguration GetDefault()
        {
            EngineConfiguration ec = CreateEmptyConfig();
            AddFieldMapps(ec);
            AddWorkItemMigrationDefault(ec);
            AddTestPlansMigrationDefault(ec);
            ec.Processors.Add(new ImportProfilePictureConfig());
            ec.Processors.Add(new ExportProfilePictureFromADConfig());
            ec.Processors.Add(new FixGitCommitLinksConfig() { TargetRepository = ec.Target.Project });
            ec.Processors.Add(new WorkItemUpdateConfig());
            ec.Processors.Add(new WorkItemPostProcessingConfig() { WorkItemIDs = new List<int> { 1, 2, 3 } });
            ec.Processors.Add(new WorkItemDeleteConfig());
            ec.Processors.Add(new WorkItemQueryMigrationConfig() { SourceToTargetFieldMappings = new Dictionary<string, string>() { {"SourceFieldRef", "TargetFieldRef" } } });
            ec.Processors.Add(new TeamMigrationConfig());
            return ec;
        }

        public static EngineConfiguration GetWorkItemMigration()
        {
            EngineConfiguration ec = CreateEmptyConfig();
            AddFieldMapps(ec);
            AddWorkItemMigrationDefault(ec);
            return ec;
        }

        private static void AddWorkItemMigrationDefault(EngineConfiguration ec)
        {
            ec.Processors.Add(new NodeStructuresMigrationConfig()
            {
                BasePaths = new[] { "Product\\Area\\Path1", "Product\\Area\\Path2" }
            });
            ec.Processors.Add(new WorkItemMigrationConfig());
        }

        private static EngineConfiguration CreateEmptyConfig()
        {
            EngineConfiguration ec = new EngineConfiguration();
            ec.TelemetryEnableTrace = false;
            ec.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
            ec.Source = new TeamProjectConfig() { Project = "migrationSource1", 
                AllowCrossProjectLinking = false, 
                Collection = new Uri("https://dev.azure.com/nkdagility-preview/"), 
                ReflectedWorkItemIDFieldName = "Custom.ReflectedWorkItemId", 
                PersonalAccessToken = "",
                LanguageMaps = new LanguageMaps() { AreaPath = "Area", IterationPath = "Iteration" }
            };
            ec.Target = new TeamProjectConfig() { Project = "migrationTarget1", 
                AllowCrossProjectLinking = false, 
                Collection = new Uri("https://dev.azure.com/nkdagility-preview/"), 
                ReflectedWorkItemIDFieldName = "Custom.ReflectedWorkItemId", 
                PersonalAccessToken = "",
                LanguageMaps = new LanguageMaps() { AreaPath ="Area", IterationPath ="Iteration"}
            };
            ec.FieldMaps = new List<IFieldMapConfig>();
            ec.WorkItemTypeDefinition = new Dictionary<string, string> {
                    { "sourceWorkItemTypeName", "targetWorkItemTypeName" }
            };
            ec.Processors = new List<ITfsProcessingConfig>();
            return ec;
        }

        private static void AddTestPlansMigrationDefault(EngineConfiguration ec)
        {
            ec.Processors.Add(new TestVariablesMigrationConfig());
            ec.Processors.Add(new TestConfigurationsMigrationConfig());
            ec.Processors.Add(new TestPlansAndSuitesMigrationConfig());
            //ec.Processors.Add(new TestRunsMigrationConfig());
        }

        private static void AddFieldMapps(EngineConfiguration ec)
        {
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
            ec.FieldMaps.Add(new FieldBlankMapConfig()
            {
                WorkItemTypeName = "*",
                targetField = "TfsMigrationTool.ReflectedWorkItemId"
            });
            ec.FieldMaps.Add(new FieldValueMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "System.State",
                targetField = "System.State",
                defaultValue = "New",
                valueMapping = new Dictionary<string, string> {
                    { "Approved", "New" },
                    { "New", "New" },
                    { "Committed", "Active" },
                    { "In Progress", "Active" },
                    { "To Do", "New" },
                    { "Done", "Closed" },
                    { "Removed", "Removed" }
                }
            });
            ec.FieldMaps.Add(new FieldtoFieldMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "Microsoft.VSTS.Common.BacklogPriority",
                targetField = "Microsoft.VSTS.Common.StackRank"
            });
            ec.FieldMaps.Add(new FieldtoFieldMultiMapConfig()
            {
                WorkItemTypeName = "*",
                SourceToTargetMappings = new Dictionary<string, string>
                {
                    {"SourceField1", "TargetField1" },
                    {"SourceField2", "TargetField2" }
                }
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
        }


    }
}
