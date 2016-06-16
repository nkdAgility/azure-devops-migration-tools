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
using TfsWitMigrator.Core;
using TfsWitMigrator.Core.ComponentContext;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer



            ////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener(string.Format(@"logs\{0}-{1}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), "MigrationRun"), "myListener"));
            //////////////////////////////////////////////////
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            MigrationEngine me = new MigrationEngine();


            me.SetSource(new TeamProjectContext(new Uri("https://sdd2016.visualstudio.com/"), "DemoProj1"));
            me.SetTarget(new TeamProjectContext(new Uri("https://sdd2016.visualstudio.com/"), "test1"));
            me.SetReflectedWorkItemIdFieldName("ReflectedWorkItemId");
            me.AddWorkItemTypeDefinition("User Story", new DescreteWitdMapper("User Story"));
            me.AddWorkItemTypeDefinition("Product Backlog Item", new DescreteWitdMapper("Product Backlog Itemy"));
            me.AddWorkItemTypeDefinition("Task", new DescreteWitdMapper("Task"));
            me.AddWorkItemTypeDefinition("Bug", new DescreteWitdMapper("Bug"));

            me.AddProcessor<NodeStructuresMigrationContext>();
            me.AddProcessor<WorkItemMigrationContext>();
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