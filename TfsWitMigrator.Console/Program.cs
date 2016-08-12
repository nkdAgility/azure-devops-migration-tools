using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTS.DataBulkEditor.Engine;
using VSTS.DataBulkEditor.Engine.ComponentContext;

namespace VSTS.DataBulkEditor.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer
            ////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener(string.Format(@"{0}-{1}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), "MigrationRun"), "myListener"));
            //////////////////////////////////////////////////
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////


            MigrationEngine me = new MigrationEngine();
            Telemetry.EnableTrace = true;

            me.SetSource(new TeamProjectContext(new Uri("https://tfs.test.slb.com/tfs/Drilling/"), "TaiJi"));
            me.SetTarget(new TeamProjectContext(new Uri("https://tfs.test.slb.com/tfs/SLB1/"), "Taiji_New"));
            me.SetReflectedWorkItemIdFieldName("TfsMigrationTool.ReflectedWorkItemId");
            me.AddWorkItemTypeDefinition("User Story", new DescreteWitdMapper("User Story"));
            me.AddWorkItemTypeDefinition("Requirement", new DescreteWitdMapper("Requirement"));
            me.AddWorkItemTypeDefinition("Task", new DescreteWitdMapper("Task"));
            me.AddWorkItemTypeDefinition("Bug", new DescreteWitdMapper("Bug"));
            me.AddWorkItemTypeDefinition("Shared Steps", new DescreteWitdMapper("Shared Steps"));
            me.AddWorkItemTypeDefinition("Shared Parameter", new DescreteWitdMapper("Shared Parameter"));
            me.AddWorkItemTypeDefinition("Test Case", new DescreteWitdMapper("Test Case"));

           // me.AddProcessor<NodeStructuresMigrationContext>();
            me.AddProcessor(new WorkItemMigrationContext(me, @"AND NOT [TfsMigrationTool.ReflectedWorkItemId] contains 'http' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug') "));

            //AND [TfsMigrationTool.ReflectedWorkItemId] = ''

            //Dictionary<string, string> stateMapping = new Dictionary<string, string>();
            //stateMapping.Add("New", "New");
            //stateMapping.Add("Approved", "New");
            //stateMapping.Add("Committed", "Active");
            //stateMapping.Add("In Progress", "Active");
            //stateMapping.Add("To Do", "New");
            //stateMapping.Add("Done", "Closed");

            //Dictionary<string, string> testPlanMapping = new Dictionary<string, string>();
            //testPlanMapping.Add("Yes", "End-User Functionality");
            //testPlanMapping.Add("No", "");
            //testPlanMapping.Add("", "");
            //testPlanMapping.Add("Blank", "");
            //me.AddFieldMap("User Story", new FieldValueMap("Slb.Drilling.NeedTestPlan", "Slb.SIS.UserStoryType", testPlanMapping));

            //me.AddFieldMap("*", new FieldToTagFieldMap("System.State", "OriginalState:{0}"));
            ////me.AddFieldMap("*", new FieldMergeMap("System.Description", "Microsoft.VSTS.Common.BusinessValue", "System.Description", @"Business Value: {1} <br/><br/>{0}"));
            ////me.AddFieldMap("*", new FieldMergeMap("System.Description", "Microsoft.VSTS.Common.AcceptanceCriteria", "System.Description", @"{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"));
            //me.AddFieldMap("*", new FieldValueMap("System.State", "System.State", stateMapping));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank"));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints"));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Common.AcceptanceCriteria", "Slb.SIS.Analysis"));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Common.AcceptanceCriteria", "SLB.SWT.VerifyDetails"));

            //me.AddFieldMap("Bug", new FieldMergeMap("System.Area", "Slb.BGC.TaiJi.ComponentName", "System.Area", @"{0}\{1}"));
            //me.AddFieldMap("Bug", new FieldMergeMap("Microsoft.VSTS.TCM.ReproSteps", "Slb.BGC.TaiJi.ComponentName", "Microsoft.VSTS.TCM.ReproSteps", @"Component Name: {1} <br/><br/>{0}"));
            //me.AddFieldMap("Bug", new FieldMergeMap("Microsoft.VSTS.TCM.ReproSteps", "Microsoft.VSTS.Common.BusinessValue", "Microsoft.VSTS.TCM.ReproSteps", @"Business Value: {1} <br/><br/>{0}"));
            //me.AddFieldMap("*", new FieldToFieldMap("Slb.BGC.Regression", "Slb.SWT.Regression"));
            //me.AddFieldMap("*", new FieldToTagFieldMap("Microsoft.VSTS.Common.BusinessValue", "BV:{0}"));
            //me.AddFieldMap("*", new FieldToTagFieldMap("Slb.BGC.TaiJi.ComponentName", "CN:{0}"));
            //me.AddFieldMap("*", new FieldToFieldMap("Slb.BGC.MustFixBy", "Slb.SWT.MajorTargetedForVersion"));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Common.Severity", "Slb.SWT.UserImpact"));

            //me.AddProcessor(new WorkItemUpdate(me, @" "));// AND [System.Id]=24125")); //


            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.Theme", "Theme:{0}"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.FeatureDocumented", "Documented:{0}"));
            //me.AddFieldMap("Requirement", new FieldToFieldMap("Microsoft.VSTS.CMMI.ImpactAssessmentHtml", "COMPANY.DEVISION.Analysis"));
            // me.AddFieldMap("Requirement", new RegexFieldMap("COMPANY.PRODUCT.Release", "COMPANY.DEVISION.Release", @"PRODUCT (\d{4}).\d{1}"));
            //me.AddFieldMap("Requirement", new RegexFieldMap("COMPANY.PRODUCT.Release", "COMPANY.DEVISION.MinorReleaseVersion", @"PRODUCT \d{4}.(\d{1})"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));

            //me.AddFieldMap("User Story", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));
            //me.AddFieldMap("User Story", new FieldToTagFieldMap("COMPANY.PRODUCT.PortfolioAcceptance", "Portfolio:{0}"));
            //me.AddFieldMap("User Story", new FieldToFieldMap("Microsoft.VSTS.CMMI.ReqType", "COMPANY.DEVISION.UserStoryType"));
            //me.AddFieldMap("User Story", new FieldToFieldMap("Microsoft.VSTS.CMMI.Stakeholder", "COMPANY.DEVISION.Stakeholder"));
            //
            //me.AddFieldMap("User Story", new RegexFieldMap("COMPANY.PRODUCT.Snapshot", "COMPANY.DEVISION.PreCommercialVersion", @"Snapshot ([0-9])", "Alpha 0$1"));
            //me.AddFieldMap("User Story", new RegexFieldMap("COMPANY.PRODUCT.Snapshot", "COMPANY.DEVISION.PreCommercialVersion", @"Snapshot 10", "Alpha 10"));








            //Me.AddWorkItemTypeDefinition("User Story", new DescreteWitdMapper("User Story"));
            // me.AddWorkItemTypeDefinition("Product Backlog Item", new DescreteWitdMapper("Product Backlog Itemy"));
            // me.AddWorkItemTypeDefinition("Task", new DescreteWitdMapper("Task"));
            // me.AddWorkItemTypeDefinition("Bug", new DescreteWitdMapper("Bug"));

            // me.AddProcessor<NodeStructuresMigrationContext>();
            //tfsqc.Query = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug') ORDER BY [System.ChangedDate] ass "; // AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND  [System.WorkItemType] = 'Test Case'  AND  [System.AreaPath] = 'Platform' ";// AND [System.Id] = 452603 ";

            // me.AddProcessor<LinkMigrationContext>();
            //me.AddProcessor<AttachementExportMigrationContext>();
            // me.AddProcessor<AttachementImportMigrationContext>();




            //me.SetTarget(new TeamProjectContext(new Uri("http://tfs01:8080/tfs/col1"), "StructuralGeologyConversion"));

            //me.AddFieldMap("*", new FieldToTagFieldMap("System.State", "ScrumState:{0}"));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank"));
            //me.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints"));
            //me.AddFieldMap("*", new FieldMergeMap("System.Description", "Microsoft.VSTS.Common.AcceptanceCriteria", "System.Description", @"{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"));

            //Dictionary<string, string> stateMapping = new Dictionary<string, string>();
            //stateMapping.Add("New", "New");
            //stateMapping.Add("Approved", "New");
            //stateMapping.Add("Committed", "Active");
            //stateMapping.Add("In Progress", "Active");
            //stateMapping.Add("To Do", "New");
            //stateMapping.Add("Done", "Closed");
            //me.AddFieldMap("*", new FieldValueMap("System.State", "System.State", stateMapping));

            //me.AddProcessor(new WorkItemUpdate(me, @" "));



            // me.AddFieldMap("*", new FieldToTagFieldMap("COMPANY.PRODUCT.Release", "{0}"));
            //  me.AddFieldMap("*", new TreeToTagFieldMap(4));

            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.Theme", "Theme:{0}"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.FeatureDocumented", "Documented:{0}"));
            //me.AddFieldMap("Requirement", new FieldToFieldMap("Microsoft.VSTS.CMMI.ImpactAssessmentHtml", "COMPANY.DEVISION.Analysis"));
            // me.AddFieldMap("Requirement", new RegexFieldMap("COMPANY.PRODUCT.Release", "COMPANY.DEVISION.Release", @"PRODUCT (\d{4}).\d{1}"));
            //me.AddFieldMap("Requirement", new RegexFieldMap("COMPANY.PRODUCT.Release", "COMPANY.DEVISION.MinorReleaseVersion", @"PRODUCT \d{4}.(\d{1})"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));
            //me.AddFieldMap("Requirement", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));

            //me.AddFieldMap("User Story", new FieldToTagFieldMap("COMPANY.PRODUCT.ReqType", "ReqType:{0}"));
            //me.AddFieldMap("User Story", new FieldToTagFieldMap("COMPANY.PRODUCT.PortfolioAcceptance", "Portfolio:{0}"));
            //me.AddFieldMap("User Story", new FieldToFieldMap("Microsoft.VSTS.CMMI.ReqType", "COMPANY.DEVISION.UserStoryType"));
            //me.AddFieldMap("User Story", new FieldToFieldMap("Microsoft.VSTS.CMMI.Stakeholder", "COMPANY.DEVISION.Stakeholder"));
            //me.AddFieldMap("User Story", new FieldToFieldMap("Microsoft.VSTS.CMMI.AcceptanceCriteria", "COMPANY.DEVISION.Analysis"));
            //me.AddFieldMap("User Story", new RegexFieldMap("COMPANY.PRODUCT.Snapshot", "COMPANY.DEVISION.PreCommercialVersion", @"Snapshot ([0-9])", "Alpha 0$1"));
            //me.AddFieldMap("User Story", new RegexFieldMap("COMPANY.PRODUCT.Snapshot", "COMPANY.DEVISION.PreCommercialVersion", @"Snapshot 10", "Alpha 10"));

            //me.AddFieldMap("Stakeholder", new FieldToFieldMap("COMPANY.PRODUCT.BusinessDetail", "COMPANY.DEVISION.BusinessDetail"));

            //me.AddProcessor<NodeStructuresMigrationContext>();

            //me.AddProcessor<WorkItemMigrationContext>();
            //me.AddProcessor<LinkMigrationContext>();
            //me.AddProcessor<AttachementExportMigrationContext>();
            // me.AddProcessor<AttachementImportMigrationContext>();






            //me.AddProcessor<WorkItemPostProcessingContext>();

            //me.AddProcessor(new WorkItemDelete(me));
            //me.AddProcessor(new FixGitCommitLinks(me));
            // me.AddProcessor<FakeProcessor>();
            //
            // 
            //me.AddProcessor<AttachementExportMigrationContext>();
            //me.AddProcessor<AttachementImportMigrationContext>();
            //me.AddProcessor(new WorkItemUpdateAreasAsTagsContext(me, @"PRODUCT\Geophysics\Core-Geophysics"));
            //me.AddProcessor<LinkMigrationContext>();
            //me.AddProcessor<TestVeriablesMigrationContext>();
            //me.AddProcessor<TestConfigurationsMigrationContext>();
            //me.AddProcessor(new TestPlansAndSuitsMigrationContext(me,string.Format(@"{0}", me.Target.Name, me.Source.Name)));
            //me.AddProcessor<TestRunsMigrationContext>();
            //
            //me.AddProcessor (new WorkItemPostProcessingContext (me, @"AND [System.AreaPath] UNDER 'Platform\Ocean Dev Env'"));
            //me.AddProcessor<LinkMigrationContext>();
            //me.AddProcessor<ImportProfilePictureContext>();
            //me.AddProcessor(new ExportProfilePictureFromADContext(me, "DIR", "mhinshelwood", "");
            //me.AddProcessor (new WorkItemUpdate (me, @"and [System.AreaPath] = 'PRODUCT\Geology-Modeling\Geology' and [System.IterationPath] Under 'PRODUCT\Geology-Modeling\Geology'",
            //                                        @"PRODUCT\Geology-Modeling\Geology", @"PRODUCT\Geology-Modeling\Geology"));
            //me.AddProcessor (new WorkItemUpdate (me, @"and [TfsMigrationTool.ReflectedWorkItemId] in ('20479','20493')",
            //                                    @"PRODUCT\Geology-Modeling\Geology", @"PRODUCT\Geology-Modeling\Geology"));
            me.Run();


            //////////////////////////////////////////////////
            Console.WriteLine();
            Console.WriteLine("Freedom");
            Console.ReadKey();
        }

    }
}