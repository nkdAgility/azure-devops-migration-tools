using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;

using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using Environment = System.Environment;
using Microsoft.Extensions.Options;
using MigrationTools.Tools;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Enrichers;
using System.Diagnostics.Metrics;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Suites &amp; Plans</processingtarget>
    public class TestPlansAndSuitesMigrationProcessor : TfsProcessor
    {
        private int __currentSuite = 0;
        private int __totalSuites = 0;
        private int _currentPlan = 0;
        private int _currentTestCases = 0;
        private IIdentityManagementService _sourceIdentityManagementService;
        private ITestConfigurationCollection _sourceTestConfigs;
        private TestManagementContext _sourceTestStore;
        private IIdentityManagementService _targetIdentityManagementService;
        private ITestConfigurationCollection _targetTestConfigs;
        private TestManagementContext _targetTestStore;
        private int _totalPlans = 0;
        private int _totalTestCases = 0;


        private static readonly Meter _meter = new Meter("MigrationTools.TestPlansAndSuitesMigrationProcessor", "1.0.0");
        private static readonly Counter<int> _testPlansCounter = _meter.CreateCounter<int>("test_plans_count");
        private static readonly Counter<int> _testSuitesCounter = _meter.CreateCounter<int>("test_suites_count");
        private static readonly Counter<int> _testCasesCounter = _meter.CreateCounter<int>("test_cases_count");

        public TestPlansAndSuitesMigrationProcessor(IOptions<TestPlansAndSuitesMigrationProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
       
        }

        new TestPlansAndSuitesMigrationProcessorOptions Options => (TestPlansAndSuitesMigrationProcessorOptions)base.Options;

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;



        protected override void InternalExecute()
        {
            _sourceTestStore = new TestManagementContext(Source, Options.TestPlanQuery);
            _targetTestStore = new TestManagementContext(Target);
            _sourceTestConfigs = _sourceTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            _targetTestConfigs = _targetTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            _sourceIdentityManagementService = Source.GetService<IIdentityManagementService>();
            _targetIdentityManagementService = Target.GetService<IIdentityManagementService>();

            CommonTools.NodeStructure.ProcessorExecutionBegin(this);

            bool filterByCompleted = Options.FilterCompleted;

            var stopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            var sourcePlans = _sourceTestStore.GetTestPlans();
            List<ITestPlan> toProcess;
            if (filterByCompleted)
            {
                var targetPlanNames = _targetTestStore.GetTestPlans().Select(tp => tp.Name).ToList();
                toProcess = sourcePlans.Where(tp => !targetPlanNames.Contains(tp.Name)).ToList();
            }
            else
            {
                toProcess = sourcePlans;
            }

            Log.LogInformation("TestPlansAndSuitesMigrationContext: Plan to copy {0} Plans?", toProcess.Count());
            _currentPlan = 0;
            _totalPlans = toProcess.Count;

            foreach (ITestPlan sourcePlan in toProcess)
            {
                _currentPlan++;
                _testPlansCounter.Add(1);
                if (CanSkipElementBecauseOfTags(sourcePlan.Id))
                {
                    Log.LogInformation("TestPlansAndSuitesMigrationContext: Skipping Test Plan {Id}:'{Name}' as is not tagged with '{Tag}'.", sourcePlan.Id, sourcePlan.Name, Options.OnlyElementsWithTag);
                    continue;
                }
                ProcessTestPlan(sourcePlan);
            }
            _currentPlan = 0;
            _totalPlans = 0;
            stopwatch.Stop();
            Log.LogInformation("TestPlansAndSuitesMigrationContext:  DONE in {Elapsed}", stopwatch.Elapsed.ToString("c"));
        }

        private void AddChildTestCases(ITestSuiteBase source, ITestSuiteBase target, ITestPlan targetPlan)
        {
            //////////////////////////////////////////
            var stopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            var metrics = new Dictionary<string, double>();
            var parameters = new Dictionary<string, string>();
            AddParameter("SuiteId", parameters, source.Id.ToString());
            AddParameter("PlanId", parameters, targetPlan.Id.ToString());
            ////////////////////////////////////
            target.Refresh();
            targetPlan.Refresh();
            targetPlan.RefreshRootSuite();

            if (CanSkipElementBecauseOfTags(source.Id))
            {
                Log.LogInformation("TestPlansAndSuitesMigrationContext::AddChildTestCases: Skipping Test Case {Id}:'{Name}' as is not tagged with '{Tag}'.", source.Id, source.Title, Options.OnlyElementsWithTag);
                return;
            }


            _totalTestCases = source.TestCases.Count;
            _currentTestCases = 0;
            AddMetric("TestCaseCount", metrics, _totalTestCases);
            InnerLog(source, string.Format("            Suite has {0} test cases", _totalTestCases), 15);
            int index = 0;
            foreach (ITestSuiteEntry sourceTestCaseEntry in source.TestCases)
            {
                _currentTestCases++;
                InnerLog(source, $"Work item: {sourceTestCaseEntry.Id}", 15);

                if (CanSkipElementBecauseOfTags(sourceTestCaseEntry.Id))
                {
                    Log.LogInformation("TestPlansAndSuitesMigrationContext::AddChildTestCases: Skipping Test Suite {Id}:'{Name}' as is not tagged with '{Tag}'.", sourceTestCaseEntry.Id, sourceTestCaseEntry.Title, Options.OnlyElementsWithTag);
                    continue;
                }

                InnerLog(source, string.Format("    Processing {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                WorkItemData wi = Target.WorkItems.FindReflectedWorkItem(sourceTestCaseEntry.TestCase.WorkItem.AsWorkItemData(), false);
                if (wi == null)
                {
                    InnerLog(source, string.Format("    Can't find work item for Test Case. Has it been migrated? {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                    continue;
                }
                var exists = (from tc in target.TestCases
                              where tc.TestCase.WorkItem.Id.ToString() == wi.Id
                              select tc).SingleOrDefault();

                if (exists != null)
                {
                    InnerLog(source, string.Format("    Test case already in suite {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                }
                else
                {
                    ITestCase targetTestCase = _targetTestStore.Project.TestCases.Find(int.Parse(wi.Id));
                    if (targetTestCase == null)
                    {
                        InnerLog(source, string.Format("    ERROR: Test case not found {0} : {1} - {2} ", sourceTestCaseEntry.EntryType, sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                    }
                    else
                    {
                        target.TestCases.InsertCases(index, new List<ITestCase> { targetTestCase });
                        InnerLog(source, string.Format("    Adding {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                    }
                }
                index++;
            }

            targetPlan.Save();
            InnerLog(source, string.Format("    SAVED {0} : {1} - {2} ", target.TestSuiteType.ToString(), target.Id, target.Title), 15);

            stopwatch.Stop();
            _totalTestCases = 0;
            _currentTestCases = 0;
        }

        /// <summary>
        /// Apply configurations to a single test case entry on the target, by copying from the source
        /// </summary>
        /// <param name="sourceEntry"></param>
        /// <param name="targetEntry"></param>
        private void ApplyConfigurations(ITestSuiteEntry sourceEntry, ITestSuiteEntry targetEntry)
        {
            int sourceConfigCount = sourceEntry.Configurations != null ? sourceEntry.Configurations.Count : 0;
            int targetConfigCount = targetEntry.Configurations != null ? targetEntry.Configurations.Count : 0;
            var deviations = sourceConfigCount > 0 && targetConfigCount > 0 && sourceEntry.Configurations.Select(x => x.Name).Intersect(targetEntry.Configurations.Select(x => x.Name)).Count() < sourceConfigCount;

            if ((sourceConfigCount != targetConfigCount) || deviations)
            {
                Log.LogInformation("   CONFIG MISMATCH FOUND --- FIX ATTEMPTING");
                IList<IdAndName> targetConfigs = new List<IdAndName>();
                foreach (var config in sourceEntry.Configurations)
                {
                    var targetFound = (from tc in _targetTestConfigs
                                       where tc.Name == config.Name
                                       select tc).SingleOrDefault();
                    if (targetFound != null)
                    {
                        targetConfigs.Add(new IdAndName(targetFound.Id, targetFound.Name));
                    }
                }
                try
                {
                    targetEntry.SetConfigurations(targetConfigs);
                }
                catch (Exception ex)
                {
                    // SOmetimes this will error out for no reason.
                    Log.LogWarning(ex, "Applying Configurations");
                }
            }
        }

        private void ApplyConfigurationsAndAssignTesters(ITestSuiteBase sourceSuite, ITestSuiteBase targetSuite)
        {
            _totalTestCases = sourceSuite.TestCases.Count();
            InnerLog(sourceSuite, $"Applying configurations for test cases in source suite {sourceSuite.Title}", 5);
            foreach (ITestSuiteEntry sourceTce in sourceSuite.TestCases)
            {
                var stopwatch = Stopwatch.StartNew();
                var starttime = DateTime.Now;
                _currentTestCases++;
                WorkItemData wi = Target.WorkItems.FindReflectedWorkItem(sourceTce.TestCase.WorkItem.AsWorkItemData(), false);
                ITestSuiteEntry targetTce;
                if (wi != null)
                {
                    targetTce = (from tc in targetSuite.TestCases
                                 where tc.TestCase.WorkItem.Id.ToString() == wi.Id
                                 select tc).SingleOrDefault();
                    if (targetTce != null)
                    {
                        InnerLog(sourceSuite, $"ApplyConfigurations Test Case ${sourceTce.Title} ", 5);
                        ApplyConfigurations(sourceTce, targetTce);
                    }
                    else
                    {
                        InnerLog(sourceSuite, $"Test Case ${sourceTce.Title} from source is not included in target. Cannot apply configuration for it.", 5);
                    }
                }
                else
                {
                    InnerLog(sourceSuite, $"Work Item for Test Case {sourceTce.Title} cannot be found in target. Has it been migrated?", 5);
                }
            }
            _totalTestCases = 0;
            _currentTestCases = 0;

            AssignTesters(sourceSuite, targetSuite);

            //Loop over child suites and set configurations for test case entries there
            if (HasChildSuites(sourceSuite))
            {
                foreach (ITestSuiteEntry sourceSuiteChild in ((IStaticTestSuite)sourceSuite).Entries.Where(
                    e => e.EntryType == TestSuiteEntryType.DynamicTestSuite
                    || e.EntryType == TestSuiteEntryType.RequirementTestSuite
                    || e.EntryType == TestSuiteEntryType.StaticTestSuite))
                {
                    //Find migrated suite in target
                    WorkItemData sourceSuiteWi = Source.WorkItems.GetWorkItem(sourceSuiteChild.Id.ToString());
                    WorkItemData targetSuiteWi = Target.WorkItems.FindReflectedWorkItem(sourceSuiteWi, false);
                    if (targetSuiteWi != null)
                    {
                        ITestSuiteEntry targetSuiteChild = (from tc in ((IStaticTestSuite)targetSuite).Entries
                                                            where tc.Id.ToString() == targetSuiteWi.Id
                                                            select tc).FirstOrDefault();
                        if (targetSuiteChild != null)
                        {
                            ApplyConfigurationsAndAssignTesters(sourceSuiteChild.TestSuite, targetSuiteChild.TestSuite);
                        }
                        else
                        {
                            InnerLog(sourceSuite, $"Test Suite {sourceSuiteChild.Title} from source cannot be found in target. Has it been migrated?", 5);
                        }
                    }
                    else
                    {
                        InnerLog(sourceSuite, $"Test Suite {sourceSuiteChild.Title} from source cannot be found in target. Has it been migrated?", 5);
                    }
                }
            }
        }

        /// <summary>
        /// Sets default configurations on migrated test suites.
        /// </summary>
        /// <param name="source">The test suite to take as a source.</param>
        /// <param name="target">The test suite to apply the default configurations to.</param>
        private void ApplyDefaultConfigurations(ITestSuiteBase source, ITestSuiteBase target)
        {
            if (source.DefaultConfigurations != null)
            {
                InnerLog(source, $"   Setting default configurations for suite {target.Title}", 15);
                IList<IdAndName> targetConfigs = new List<IdAndName>();
                foreach (var config in source.DefaultConfigurations)
                {
                    var targetFound = (from tc in _targetTestConfigs
                                       where tc.Name == config.Name
                                       select tc).SingleOrDefault();
                    if (targetFound != null)
                    {
                        targetConfigs.Add(new IdAndName(targetFound.Id, targetFound.Name));
                    }
                }
                try
                {
                    target.SetDefaultConfigurations(targetConfigs);
                }
                catch (Exception)
                {
                    // SOmetimes this will error out for no reason.
                }
            }
            else
            {
                target.ClearDefaultConfigurations();
            }
        }

        private void ApplyFieldMappings(int sourceWIId, int targetWIId)
        {
            var sourceWI = Source.WorkItems.GetWorkItem(sourceWIId.ToString());
            var targetWI = Target.WorkItems.GetWorkItem(targetWIId.ToString());

            targetWI.ToWorkItem().AreaPath = CommonTools.NodeStructure.GetNewNodeName(sourceWI.ToWorkItem().AreaPath, TfsNodeStructureType.Area);
            targetWI.ToWorkItem().IterationPath = CommonTools.NodeStructure.GetNewNodeName(sourceWI.ToWorkItem().IterationPath, TfsNodeStructureType.Iteration);

            CommonTools.FieldMappingTool.ApplyFieldMappings(sourceWI, targetWI);
            targetWI.SaveToAzureDevOps();
        }

        private void ApplyTestSuiteQuery(ITestSuiteBase source, IDynamicTestSuite targetSuiteChild, TestManagementContext targetTestStore)
        {
            targetSuiteChild.Query = ((IDynamicTestSuite)source).Query;

            // Postprocessing common errors
            FixQueryForTeamProjectNameChange(source, targetSuiteChild, targetTestStore);
            FixWorkItemIdInQuery(targetSuiteChild);
        }

        private void AssignReflectedWorkItemId(int sourceWIId, int targetWIId)
        {
            var sourceWI = Source.WorkItems.GetWorkItem(sourceWIId.ToString());
            var targetWI = Target.WorkItems.GetWorkItem(targetWIId.ToString());
            targetWI.ToWorkItem().Fields[Target.Options.ReflectedWorkItemIDFieldName].Value = Source.WorkItems.CreateReflectedWorkItemId(sourceWI).ToString();
            targetWI.SaveToAzureDevOps();
        }

        private void AssignTesters(ITestSuiteBase sourceSuite, ITestSuiteBase targetSuite)
        {
            if (targetSuite == null)
            {
                Log.LogError("Target Suite is NULL");
            }

            List<ITestPointAssignment> assignmentsToAdd = new List<ITestPointAssignment>();
            //loop over all source test case entries
            _totalTestCases = sourceSuite.TestCases.Count();
            _currentTestCases = 0;
            foreach (ITestSuiteEntry sourceTce in sourceSuite.TestCases)
            {
                _currentTestCases++;
                // find target testcase id for this source tce
                WorkItemData targetTc = Target.WorkItems.FindReflectedWorkItem(sourceTce.TestCase.WorkItem.AsWorkItemData(), false);

                if (targetTc == null)
                {
                    Log.LogError("Target Reflected Work Item Not found for source WorkItem ID: {sourceTceTestCaseWorkItemId}", sourceTce.TestCase.WorkItem.Id);
                }
                else
                {
                    InnerLog(sourceSuite, $"Test Point Assignement for wi{targetTc.Id}", 15);
                    //figure out test point assignments for each source tce
                    foreach (ITestPointAssignment tpa in sourceTce.PointAssignments)
                    {
                        int sourceConfigurationId = tpa.ConfigurationId;

                        TeamFoundationIdentity targetIdentity = null;

                        if (tpa.AssignedTo != null)
                        {

                            targetIdentity = GetTargetIdentity(tpa.AssignedTo.Descriptor);

                            if (targetIdentity == null)
                            {
                                if (TryToRefreshTesterIdentity(targetSuite, tpa))
                                {
                                    targetIdentity = GetTargetIdentity(tpa.AssignedTo.Descriptor);
                                }
                            }
                        }

                        // translate source configuration id to target configuration id and name
                        //// Get source configuration name
                        string sourceConfigName = (from tc in _sourceTestConfigs
                                                   where tc.Id == sourceConfigurationId
                                                   select tc.Name).FirstOrDefault();

                        //// Find source configuration name in target and get the id for it
                        int targetConfigId = (from tc in _targetTestConfigs
                                              where tc.Name == sourceConfigName
                                              select tc.Id).FirstOrDefault();

                        if (targetConfigId != 0)
                        {
                            IdAndName targetConfiguration = new IdAndName(targetConfigId, sourceConfigName);

                            var targetUserId = Guid.Empty;
                            if (targetIdentity != null)
                            {
                                targetUserId = targetIdentity.TeamFoundationId;
                            }

                            // Create a test point assignment with target test case id, target configuration (id and name) and target identity
                            var newAssignment = targetSuite?.CreateTestPointAssignment(
                                int.Parse(targetTc.Id),
                                targetConfiguration,
                                targetUserId);

                            // add the test point assignment to the list
                            assignmentsToAdd.Add(newAssignment);
                        }
                        else
                        {
                            Log.LogInformation("Cannot find configuration with name [{sourceConfigName}] in target. Cannot assign tester to it.", sourceConfigName);
                        }
                    }
                }
            }

            // assign the list to the suite
            targetSuite?.AssignTestPoints(assignmentsToAdd);
        }

        private bool TryToRefreshTesterIdentity(ITestSuiteBase targetSuite, ITestPointAssignment tpa)
        {
            try
            {
                _sourceIdentityManagementService.RefreshIdentity(tpa.AssignedTo.Descriptor);
                return true;
            }
            catch (Exception ex)
            {
                InnerLog(targetSuite, string.Format("            Unable to refresh tester identity: {0}", ex.Message), 10);
                Log.LogWarning("Cannot refresh identity of [{tester}] in target. Cannot assign tester to it.", tpa.AssignedTo.DisplayName);
                return false;
            }
        }

        private bool CanSkipElementBecauseOfTags(int workItemId)
        {
            if (Options.OnlyElementsWithTag == null)
            {
                return false;
            }
            var sourcePlanWorkItem = Source.WorkItems.GetWorkItem(workItemId.ToString());
            var tagWhichMustBePresent = Options.OnlyElementsWithTag;
            return !sourcePlanWorkItem.ToWorkItem().Tags.Contains(tagWhichMustBePresent);
        }

        private ITestSuiteBase CreateNewDynamicTestSuite(ITestSuiteBase source)
        {
            IDynamicTestSuite targetSuiteChild = _targetTestStore.Project.TestSuites.CreateDynamic();
            targetSuiteChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            if (targetSuiteChild is ITestSuiteBase2)
            {
                ((ITestSuiteBase2)targetSuiteChild).Status = ((ITestSuiteBase2)source).Status;
            }
            ApplyTestSuiteQuery(source, targetSuiteChild, _targetTestStore);

            return targetSuiteChild;
        }

        private ITestSuiteBase CreateNewRequirementTestSuite(ITestSuiteBase source, WorkItemData requirement)
        {
            IRequirementTestSuite targetSuiteChild;
            try
            {
                string token = Target.Options.Authentication.AccessToken;
                string project = Target.Options.Project;
                Uri collectionUri = Target.Options.Collection;
                VssConnection connection = new VssConnection(collectionUri, new VssClientCredentials());
                if (!string.IsNullOrEmpty(token)) connection = new VssConnection(collectionUri, new VssBasicCredential(string.Empty, token));
                TestPlanHttpClient testPlanHttpClient = connection.GetClient<TestPlanHttpClient>();
                WorkItemData sourceSuite = Source.WorkItems.GetWorkItem(source.Parent.Id);
                WorkItemData targetSuite = Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceSuite);
                WorkItemData sourcePlan = Source.WorkItems.GetWorkItem(source.Plan.Id);
                WorkItemData targetPlan = Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourcePlan);
                TestSuiteCreateParams testSuiteCreateParams = new TestSuiteCreateParams()
                {
                    RequirementId = int.Parse(requirement.Id),
                    SuiteType = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuiteType.RequirementTestSuite,
                    ParentSuite = new TestSuiteReference()
                    {
                        Id = int.Parse(targetSuite.Id)
                    }
                };
                Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite suite = testPlanHttpClient.CreateTestSuiteAsync(testSuiteCreateParams, project, int.Parse(targetPlan.Id)).Result;
                targetSuiteChild = (IRequirementTestSuite)_targetTestStore.Project.TestSuites.Find(suite.Id);
                //Replaced soap api with rest api because work item type Bug is no longer part of the Microsoft.Requirement.Category
                //targetSuiteChild = _targetTestStore.Project.TestSuites.CreateRequirement(requirement.ToWorkItem());
            }
            catch (TestManagementValidationException ex)
            {
                InnerLog(source, string.Format("            Unable to Create Requirement based Test Suite: {0}", ex.Message), 10);
                return null;
            }
            targetSuiteChild.Title = source.Title;
            return targetSuiteChild;
        }

        private ITestSuiteBase CreateNewStaticTestSuite(ITestSuiteBase source)
        {
            ITestSuiteBase targetSuiteChild = _targetTestStore.Project.TestSuites.CreateStatic();
            targetSuiteChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            targetSuiteChild.State = TestSuiteState.InPlanning;
            if (targetSuiteChild is ITestSuiteBase2)
            {
                ((ITestSuiteBase2)targetSuiteChild).Status = ((ITestSuiteBase2)source).Status;
            }
            return targetSuiteChild;
        }

        private ITestPlan CreateNewTestPlanFromSource(ITestPlan sourcePlan, string newPlanName)
        {
            ITestPlan targetPlan;
            targetPlan = _targetTestStore.CreateTestPlan();
            targetPlan.CopyPropertiesFrom(sourcePlan);
            targetPlan.Name = newPlanName;
            targetPlan.StartDate = sourcePlan.StartDate != DateTime.MinValue ? sourcePlan.StartDate : DateTime.Now;
            targetPlan.EndDate = sourcePlan.EndDate != DateTime.MinValue ? sourcePlan.EndDate : targetPlan.StartDate.AddDays(1);
            targetPlan.Description = sourcePlan.Description;
            if (targetPlan is ITestPlan2)
            {
                ((ITestPlan2)targetPlan).Status = ((ITestPlan2)sourcePlan).Status;
            }

            // Set area and iteration to root of the target project.
            // We will set the correct values later, when we actually have a work item available
            targetPlan.Iteration = Target.Options.Project;
            targetPlan.AreaPath = Target.Options.Project;

            // Remove testsettings reference because VSTS Sync doesn't support migrating these artifacts
            if (targetPlan.ManualTestSettingsId != 0)
            {
                targetPlan.ManualTestSettingsId = 0;
                targetPlan.AutomatedTestSettingsId = 0;
                Log.LogWarning("Ignoring migration of Testsettings. Azure DevOps Migration Tools don't support migration of this artifact type.");
            }

            // Remove reference to build uri because VSTS Sync doesn't support migrating these artifacts
            if (targetPlan.BuildUri != null)
            {
                targetPlan.BuildUri = null;
                Log.LogWarning("Ignoring migration of assigned Build artifact {0}. Azure DevOps Migration Tools don't support migration of this artifact type.", sourcePlan.BuildUri);
            }
            return targetPlan;
        }

        private ITestSuiteBase FindSuiteEntry(IStaticTestSuite staticSuite, string titleToFind, ITestSuiteBase sourceSuit)
        {
            Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry");
            ITestSuiteBase testSuit = (from s in staticSuite.SubSuites where s.Title == titleToFind select s).SingleOrDefault();
            if (testSuit != null)
            {
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry::FOUND Test Suit {id}", testSuit.Id);
                //Get Source ReflectedWorkItemId
                var sourceWI = Source.WorkItems.GetWorkItem(sourceSuit.Id.ToString());
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry::SourceWorkItem[{workItemId]", sourceWI.Id);
                string expectedReflectedId = Source.WorkItems.CreateReflectedWorkItemId(sourceWI).ToString();
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry::SourceWorkItem[{workItemId] ~[{ReflectedWorkItemId]", sourceWI.Id, expectedReflectedId);
                //Get Target ReflectedWorkItemId
                var targetWI = Target.WorkItems.GetWorkItem(testSuit.Id.ToString());
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry::TargetWorkItem[{workItemId]", targetWI.Id);
                string workItemReflectedId = (string)targetWI.Fields[Target.Options.ReflectedWorkItemIDFieldName].Value;
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry::TargetWorkItem[{workItemId] [{ReflectedWorkItemId]", targetWI.Id, workItemReflectedId);
                //Compaire
                if (workItemReflectedId != expectedReflectedId)
                {
                    Log.LogDebug("TestPlansAndSuitesMigrationContext::FindSuiteEntry: Source ReflectedWorkItemId and Target ReflectedWorkItemId do not match");
                    testSuit = null;
                }
            }
            return testSuit;
        }

        private ITestPlan FindTestPlan(string planName, int sourcePlanId)
        {
            Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan");
            ITestPlan testPlan = (from p in _targetTestStore.Project.TestPlans.Query("Select * From TestPlan") where p.Name == planName select p).SingleOrDefault();
 
            if (testPlan != null)
            {
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan:: FOUND Test Plan with {name}", planName);
                //Get Source ReflectedWorkItemId
                var sourceWI = Source.WorkItems.GetWorkItem(sourcePlanId);
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan::SourceWorkItem[{workItemId]", sourceWI.Id);
                string expectedReflectedId = Source.WorkItems.CreateReflectedWorkItemId(sourceWI).ToString();
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan::SourceWorkItem[{workItemId] ~[{ReflectedWorkItemId]", sourceWI.Id, expectedReflectedId);
                //Get Target ReflectedWorkItemId
                var targetWI = Target.WorkItems.GetWorkItem(testPlan.Id.ToString());
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan::TargetWorkItem[{workItemId]", targetWI.Id);
                if (!targetWI.Fields.ContainsKey(Target.Options.ReflectedWorkItemIDFieldName))
                {
                    Log.LogError("TestPlansAndSuitesMigrationContext::FindTestPlan::TargetWorkItem[{workItemId} does not have ReflectedWorkItemId field {ReflectedWorkItemIDFieldName}", targetWI.Id, Target.Options.ReflectedWorkItemIDFieldName);
                    Environment.Exit(-1);
                }
                string workItemReflectedId = (string)targetWI.Fields[Target.Options.ReflectedWorkItemIDFieldName].Value;
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan::TargetWorkItem[{workItemId] [{ReflectedWorkItemId]", targetWI.Id, workItemReflectedId);
                //Compaire
                if (workItemReflectedId != expectedReflectedId)
                {
                    Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan: Source ReflectedWorkItemId and Target ReflectedWorkItemId do not match");
                    testPlan = null;
                }
            }
            else
            {
                Log.LogDebug("TestPlansAndSuitesMigrationContext::FindTestPlan:: NOT FOUND Test Plan with {name}", planName);
            }
            return testPlan;
        }

        private void FixAssignedToValue(int sourceWIId, int targetWIId)
        {
            var sourceWI = Source.WorkItems.GetWorkItem(sourceWIId.ToString());
            var targetWI = Target.WorkItems.GetWorkItem(targetWIId.ToString());
            targetWI.ToWorkItem().Fields["System.AssignedTo"].Value = sourceWI.ToWorkItem().Fields["System.AssignedTo"].Value;
            targetWI.SaveToAzureDevOps();
        }

        private void FixIterationNotFound(Exception exception, ITestSuiteBase source, IDynamicTestSuite targetSuiteChild, TestManagementContext targetTestStore)
        {
            if (exception.Message.Contains("The specified iteration path does not exist."))
            {
                Regex regEx = new Regex(@"'(.*?)'");

                var missingIterationPath = regEx.Match(exception.Message).Groups[0].Value;
                missingIterationPath = missingIterationPath.Substring(missingIterationPath.IndexOf(@"\") + 1, missingIterationPath.Length - missingIterationPath.IndexOf(@"\") - 2);

                Log.LogInformation("Found a orphaned iteration path in test suite query.");
                Log.LogInformation("Invalid iteration path {0}:", missingIterationPath);
                Log.LogInformation("Replacing the orphaned iteration path from query with root iteration path. Please fix the query after the migration.");

                targetSuiteChild.Query = targetSuiteChild.Project.CreateTestQuery(
                    targetSuiteChild.Query.QueryText.Replace(
                        string.Format(@"'{0}\{1}'", source.Plan.Project.TeamProjectName, missingIterationPath),
                        string.Format(@"'{0}'", targetTestStore.Project.TeamProjectName)
                    ));

                targetSuiteChild.Query = targetSuiteChild.Project.CreateTestQuery(
                    targetSuiteChild.Query.QueryText.Replace(
                        string.Format(@"'{0}\{1}'", targetTestStore.Project.TeamProjectName, missingIterationPath),
                        string.Format(@"'{0}'", targetTestStore.Project.TeamProjectName)
                    ));
            }
        }

        private void FixQueryForTeamProjectNameChange(ITestSuiteBase source, IDynamicTestSuite targetSuiteChild, TestManagementContext targetTestStore)
        {
            // Replacing old projectname in queries with new projectname
            // The target team project name is only available via target test store because the dyn. testsuite isnt saved at this point in time
            if (!source.Plan.Project.TeamProjectName.Equals(targetTestStore.Project.TeamProjectName))
            {
                Log.LogInformation("Team Project names dont match. We need to fix the query in dynamic test suite {0} - {1}.", source.Id, source.Title);
                Log.LogInformation("Replacing old project name {1} in query {0} with new team project name {2}", targetSuiteChild.Query.QueryText, source.Plan.Project.TeamProjectName, targetTestStore.Project.TeamProjectName);
                // First need to check is prefix project nodes has been applied for the migration

                    // If we are not profixing project nodes then we just need to take the old value for the project and replace it with the new project value
                    targetSuiteChild.Query = targetSuiteChild.Project.CreateTestQuery(targetSuiteChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", source.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}", targetTestStore.Project.TeamProjectName)));

                Log.LogInformation("New query is now {0}", targetSuiteChild.Query.QueryText);
            }
        }

        /// <summary>
        /// Fix work item ID's in query based suites
        /// </summary>
        private void FixWorkItemIdInQuery(ITestSuiteBase targetSuitChild)
        {
            var targetPlan = targetSuitChild.Plan;
            if (targetSuitChild.TestSuiteType == Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.DynamicTestSuite)
            {
                var dynamic = (IDynamicTestSuite)targetSuitChild;

                if (
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(dynamic.Query.QueryText, "[System.Id]",
                        CompareOptions.IgnoreCase) >= 0)
                {
                    string regExSearchForSystemId = @"(\[System.Id\]\s*=\s*[\d]*)";

                    MatchCollection matches = Regex.Matches(dynamic.Query.QueryText, regExSearchForSystemId, RegexOptions.IgnoreCase);

                    foreach (Match match in matches)
                    {
                        var qid = match.Value.Split('=')[1].Trim();
                        TfsReflectedWorkItemId reflectedString = new TfsReflectedWorkItemId(int.Parse(qid), Source.Options.Project, Source.Options.Collection);
                        var targetWi = Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(reflectedString.ToString());

                        if (targetWi == null)
                        {
                            InnerLog(targetSuitChild, "Target WI does not exist. We are skipping this item. Please fix it manually.", 10);
                        }
                        else
                        {
                            InnerLog(targetSuitChild, "Fixing [System.Id] in query in test suite '" + dynamic.Title + "' from " + qid + " to " + targetWi.Id, 10);
                            if (targetPlan != null)
                            {
                                dynamic.Refresh();
                                dynamic.Repopulate();
                            }
                            dynamic.Query = _targetTestStore.Project.CreateTestQuery(dynamic.Query.QueryText.Replace(match.Value, string.Format("[System.Id] = {0}", targetWi.Id)));
                            if (targetPlan != null)
                            {
                                targetPlan.Save();
                            }
                        }
                    }
                }
            }
        }

        private string GetLogTag(string name, int current, int total)
        {
            var currentString = current.ToString();
            var totalString = total.ToString();
            return $"{name}[{currentString.PadLeft(totalString.Length)}/{totalString}]";
        }

        private string GetLogTags()
        {
            return $"{GetLogTag("Plan", _currentPlan, _totalPlans)} {GetLogTag("Suite", __currentSuite, __totalSuites)} {GetLogTag("Cases", _currentTestCases, _totalTestCases)} ";
        }

        /// <summary>
        /// Retrieve the target identity for a given source descriptor
        /// </summary>
        /// <param name="sourceIdentityDescriptor">Source identity Descriptor</param>
        /// <returns>Target Identity</returns>
        private TeamFoundationIdentity GetTargetIdentity(IdentityDescriptor sourceIdentityDescriptor)
        {
            var sourceIdentity = _sourceIdentityManagementService.ReadIdentity(
                sourceIdentityDescriptor,
                MembershipQuery.Direct,
                ReadIdentityOptions.ExtendedProperties);
            string sourceIdentityMail = sourceIdentity.GetProperty("Mail") as string;

            // Try refresh the Identity if we are missing the Mail property
            if (string.IsNullOrEmpty(sourceIdentityMail))
            {
                sourceIdentity = _sourceIdentityManagementService.ReadIdentity(
                    sourceIdentityDescriptor,
                    MembershipQuery.Direct,
                    ReadIdentityOptions.ExtendedProperties);

                sourceIdentityMail = sourceIdentity.GetProperty("Mail") as string;
            }

            if (!string.IsNullOrEmpty(sourceIdentityMail))
            {
                // translate source assignedto name to target identity
                TeamFoundationIdentity targetIdentity = null;
                try
                {
                    targetIdentity = _targetIdentityManagementService.ReadIdentity(
                        IdentitySearchFactor.MailAddress,
                        sourceIdentityMail,
                        MembershipQuery.Direct,
                        ReadIdentityOptions.None);
                }
                catch (MultipleIdentitiesFoundException)
                {
                    Log.LogInformation("Multiple identities found with email [{sourceIdentityMail}] in target system. Trying Account Name.", sourceIdentityMail);
                }

                if (targetIdentity == null)
                {
                    targetIdentity = _targetIdentityManagementService.ReadIdentity(
                        IdentitySearchFactor.AccountName,
                        sourceIdentityMail,
                        MembershipQuery.Direct,
                        ReadIdentityOptions.None);
                }

                if (targetIdentity == null)
                {
                    Log.LogInformation("Cannot find tester with e-mail [{sourceIdentityMail}] in target system. Cannot assign.", sourceIdentityMail);
                    return null;
                }

                return targetIdentity;
            }
            else
            {
                Log.LogInformation("No e-mail address known in source system for [{sourceIdentityDisplayName}]. Cannot translate to target.", sourceIdentity.DisplayName);
                return null;
            }
        }

        private bool HasChildSuites(ITestSuiteBase sourceSuite)
        {
            bool hasChildren = false;
            if (sourceSuite != null && sourceSuite.TestSuiteType == Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.StaticTestSuite)
            {
                hasChildren = (((IStaticTestSuite)sourceSuite).Entries.Count > 0);
            }
            return hasChildren;
        }

        private bool HasChildTestCases(ITestSuiteBase sourceSuite)
        {
            return sourceSuite.TestCaseCount > 0;
        }

        private void ProcessTestPlan(ITestPlan sourcePlan)
        {
            var stopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            ////////////////////////////////////
            var newPlanName = $"{sourcePlan.Name}";
            InnerLog(sourcePlan, $"Process Plan {newPlanName}", 0, true);
            var targetPlan = FindTestPlan(newPlanName, sourcePlan.Id);
            //if (targetPlan != null && TargetPlanContansTag(targetPlan.Id))
            //{
            //    return;
            //}
            if (targetPlan == null)
            {
                InnerLog(sourcePlan, $" Creating Plan {newPlanName}", 5);
                targetPlan = CreateNewTestPlanFromSource(sourcePlan, newPlanName);


                RemoveInvalidLinks(targetPlan);
                if (Options.RemoveAllLinks)
                {
                    targetPlan.Links.Clear();
                }

                InnerLog(sourcePlan, $" Starte Date {targetPlan.StartDate}", 5);
                InnerLog(sourcePlan, $" EndDate Plan {targetPlan.EndDate}", 5);

                targetPlan.Save();

                ApplyFieldMappings(sourcePlan.Id, targetPlan.Id);
                AssignReflectedWorkItemId(sourcePlan.Id, targetPlan.Id);
                FixAssignedToValue(sourcePlan.Id, targetPlan.Id);

                ApplyDefaultConfigurations(sourcePlan.RootSuite, targetPlan.RootSuite);

                ApplyFieldMappings(sourcePlan.RootSuite.Id, targetPlan.RootSuite.Id);
                AssignReflectedWorkItemId(sourcePlan.RootSuite.Id, targetPlan.RootSuite.Id);
                FixAssignedToValue(sourcePlan.RootSuite.Id, targetPlan.RootSuite.Id);
                // Add Test Cases & apply configurations
                AddChildTestCases(sourcePlan.RootSuite, targetPlan.RootSuite, targetPlan);
            }
            else
            {
                InnerLog(sourcePlan, $"Found Plan {newPlanName}", 5);
            }
            targetPlan.Save();
            targetPlan.Refresh();
            if (HasChildSuites(sourcePlan.RootSuite))
            {
                __currentSuite = 0;
                __totalSuites = sourcePlan.RootSuite.Entries.Count;
                InnerLog(sourcePlan, $"Source Plan has {__totalSuites} Suites", 5);
                foreach (var sourceSuiteChild in sourcePlan.RootSuite.SubSuites)
                {
                    __currentSuite++;
                    InnerLog(sourceSuiteChild, $"", 5, true);

                    ProcessTestSuite(sourceSuiteChild, targetPlan.RootSuite, targetPlan);
                }
                __currentSuite = 0;
                __totalSuites = 0;
            }

            targetPlan.Save();
            // Load the plan again, because somehow it doesn't let me set configurations on the already loaded plan
            InnerLog(sourcePlan, $"ApplyConfigurationsAndAssignTesters {targetPlan.Name}", 5); ;
            ITestPlan targetPlan2 = FindTestPlan(targetPlan.Name, sourcePlan.Id);
            if (targetPlan2 !=  null)
            {
                ApplyConfigurationsAndAssignTesters(sourcePlan.RootSuite, targetPlan2.RootSuite);
                //////////////////////////////
                TagCompletedTargetPlan(targetPlan.Id);
            }
            else
            {
                Log.LogError("Unable to ApplyConfigurationsAndAssignTesters: When loading the test plan again with the name it was found that they do not match!");
            }
           
            ///////////////////////////////////////////////
        }

        private void ProcessTestSuite(ITestSuiteBase sourceSuite, ITestSuiteBase targetParent, ITestPlan targetPlan)
        {
            _testSuitesCounter.Add(1);
            if (CanSkipElementBecauseOfTags(sourceSuite.Id))
                return;
            //////////////////////////////////////////
            var stopwatch = Stopwatch.StartNew();
            if (Options.MigrationDelay > 0)
            {
                System.Threading.Thread.Sleep(Options.MigrationDelay);
            }
            var starttime = DateTime.Now;
            ////////////////////////////////////

            InnerLog(sourceSuite, $"    Processing {sourceSuite.TestSuiteType} : {sourceSuite.Id} - {sourceSuite.Title} ", 5);
            var targetSuiteChild = FindSuiteEntry((IStaticTestSuite)targetParent, sourceSuite.Title, sourceSuite);

            // Target suite is not found in target system. We should create it.
            if (targetSuiteChild == null)
            {
                switch (sourceSuite.TestSuiteType)
                {
                    case Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.None:
                        throw new NotImplementedException();
                    //break;
                    case Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.DynamicTestSuite:
                        targetSuiteChild = CreateNewDynamicTestSuite(sourceSuite);
                        break;

                    case Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.StaticTestSuite:
                        targetSuiteChild = CreateNewStaticTestSuite(sourceSuite);
                        break;

                    case Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.RequirementTestSuite:
                        int sourceRid = ((IRequirementTestSuite)sourceSuite).RequirementId;
                        WorkItemData sourceReq = null;
                        WorkItemData targetReq = null;
                        try
                        {
                            sourceReq = Source.WorkItems.GetWorkItem(sourceRid.ToString());
                            if (sourceReq == null)
                            {
                                InnerLog(sourceSuite, "            Source work item not found", 5);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            InnerLog(sourceSuite, "            Source work item cannot be loaded", 5);
                            break;
                        }
                        try
                        {
                            targetReq = Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceReq);

                            if (targetReq == null)
                            {
                                InnerLog(sourceSuite, "            Target work item not found", 5);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            InnerLog(sourceSuite, "            Source work item not migrated to target, cannot be found", 5);
                            Log.LogError("Unable to locate SourceWI:{SourceWI} in the Target. ReflectedWorkItemId:{ReflectedWorkItemId}", sourceReq.Id, Target.WorkItems.CreateReflectedWorkItemId(sourceReq), ex);
                            break;
                        }
                        targetSuiteChild = CreateNewRequirementTestSuite(sourceSuite, targetReq);
                        break;

                    default:
                        throw new NotImplementedException();
                        //break;
                }
                if (targetSuiteChild != null)
                {
                    // Apply default configurations, Add to target and Save
                    ApplyDefaultConfigurations(sourceSuite, targetSuiteChild);

                    if (targetSuiteChild.Plan == null)
                    {
                        SaveNewTestSuiteToPlan(targetPlan, (IStaticTestSuite)targetParent, targetSuiteChild);
                    }
                    ApplyFieldMappings(sourceSuite.Id, targetSuiteChild.Id);
                    AssignReflectedWorkItemId(sourceSuite.Id, targetSuiteChild.Id);
                    FixAssignedToValue(sourceSuite.Id, targetSuiteChild.Id);
                }
            }
            else
            {
                // The target suite already exists, take it from here
                InnerLog(sourceSuite, "            Suite Exists", 5);
                ApplyDefaultConfigurations(sourceSuite, targetSuiteChild);
                if (targetSuiteChild.IsDirty)
                {
                    targetPlan.Save();
                }
            }

            // Recurse if Static Suite
            if (sourceSuite.TestSuiteType == Microsoft.TeamFoundation.TestManagement.Client.TestSuiteType.StaticTestSuite && targetSuiteChild != null)
            {
                // Add Test Cases
                AddChildTestCases(sourceSuite, targetSuiteChild, targetPlan);

                if (HasChildSuites(sourceSuite))
                {
                    InnerLog(sourceSuite, $"            Suite has {((IStaticTestSuite)sourceSuite).Entries.Count} children", 5);
                    foreach (var sourceSuitChild in ((IStaticTestSuite)sourceSuite).SubSuites)
                    {
                        ProcessTestSuite(sourceSuitChild, targetSuiteChild, targetPlan);
                    }
                }
            }
            ///////////////////////////////////////////////
        }

        /// <summary>
        /// Remove invalid links
        /// </summary>
        /// <remarks>
        /// VSTS cannot store some links which have an invalid URI Scheme. You will get errors like "The URL specified has a potentially unsafe URL protocol"
        /// For myself, the issue were urls that pointed to TFVC:    "vstfs:///VersionControl/Changeset/19415"
        /// Unfortunately the API does not seem to allow access to the "raw" data, so there's nowhere to retrieve this as far as I can find.
        /// Should take care of https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </remarks>
        /// <param name="targetPlan">The plan to remove invalid links drom</param>
        private void RemoveInvalidLinks(ITestPlan targetPlan)
        {
            var linksToRemove = new List<ITestExternalLink>();
            foreach (var link in targetPlan.Links)
            {
                try
                {
                    link.Uri.ToString();
                }
                catch (UriFormatException)
                {
                    linksToRemove.Add(link);
                }
            }

            if (linksToRemove.Any())
            {
                if (!Options.RemoveInvalidTestSuiteLinks)
                {
                    InnerLog(targetPlan, "We have detected test suite links that probably can't be migrated. You might receive an error 'The URL specified has a potentially unsafe URL protocol' when migrating to VSTS.", 5);
                    InnerLog(targetPlan, "Please see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178 for more details.", 5);
                }
                else
                {
                    InnerLog(targetPlan, $"Link count before removal of invalid: [{targetPlan.Links.Count}]", 5);
                    foreach (var link in linksToRemove)
                    {
                        InnerLog(targetPlan,
                            $"Link with Description [{link.Description}] could not be migrated, as the URI is invalid. (We can't display the URI because of limitations in the TFS/VSTS/DevOps API.) Removing link.", 0);
                        targetPlan.Links.Remove(link);
                    }

                    InnerLog(targetPlan, $"Link count after removal of invalid: [{targetPlan.Links.Count}]", 0);
                }
            }
        }

        private void SaveNewTestSuiteToPlan(ITestPlan testPlan, IStaticTestSuite parent, ITestSuiteBase newTestSuite)
        {
            InnerLog(newTestSuite,
                $"       Saving {newTestSuite.TestSuiteType} : {newTestSuite.Id} - {newTestSuite.Title} ", 10);
            try
            {
                ((IStaticTestSuite)parent).Entries.Add(newTestSuite);
            }
            catch (TestManagementServerException ex)
            {
                Log.LogError(ex, " FAILED {TestSuiteType} : {Id} - {Title}", newTestSuite.TestSuiteType.ToString(), newTestSuite.Id.ToString(), newTestSuite.Title,
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", Target.Options.Project},
                          { "Target Collection", Target.Options.Collection.ToString() },
                          { "Source Project", Source.Options.Project},
                          { "Source Collection", Source.Options.Collection.ToString() },
                          { "Status", Status.ToString() },
                          { "Task", "SaveNewTestSuitToPlan" },
                          { "Id", newTestSuite.Id.ToString()},
                          { "Title", newTestSuite.Title},
                          { "TestSuiteType", newTestSuite.TestSuiteType.ToString()}
                      });
                ITestSuiteBase ErrorSuiteChild = _targetTestStore.Project.TestSuites.CreateStatic();
                ErrorSuiteChild.TestSuiteEntry.Title = string.Format(@"BROKEN: {0} | {1}", newTestSuite.Title, ex.Message);
                ((IStaticTestSuite)parent).Entries.Add(ErrorSuiteChild);
            }

            testPlan.Save();
        }

        private void TagCompletedTargetPlan(int workItemId)
        {
            // Remvoed to fix bug #852
            //var targetPlanWorkItem = Target.WorkItems.GetWorkItem(workItemId.ToString());
            //targetPlanWorkItem.ToWorkItem().Tags = targetPlanWorkItem.ToWorkItem().Tags + ";migrated";
            //targetPlanWorkItem.SaveToAzureDevOps();
        }

        private bool TargetPlanContansTag(int workItemId)
        {
            var targetPlanWorkItem = Target.WorkItems.GetWorkItem(workItemId.ToString());
            return targetPlanWorkItem.ToWorkItem().Tags.Contains("migrated");
        }

        private void InnerLog(ITestPlan sourcePlan, string message = "", int indent = 0, bool header = false)
        {
            if (header)
            {
                Log.LogInformation("===============================================================================================".PadLeft(indent));
                Log.LogInformation("===============================================================================================".PadLeft(indent));
                Log.LogInformation("===============================================================================================".PadLeft(indent));
                Log.LogInformation("==      Suite Name: {SourcePlanName}=============================".PadLeft(indent), sourcePlan.Name.PadRight(45));
                Log.LogInformation("==            Date: {SourcePlanStartDate}=============================".PadLeft(indent), sourcePlan.StartDate.ToShortDateString().PadRight(45));
                Log.LogInformation("==          Suites: {SourcePlanRootSuiteEntriesCount}=============================".PadLeft(indent), sourcePlan.RootSuite.Entries.Count.ToString().PadRight(45));
                Log.LogInformation("===============================================================================================".PadLeft(indent));
                Log.LogInformation("===============================================================================================".PadLeft(indent));
                Log.LogInformation("===============================================================================================".PadLeft(indent));
            }
            Log.LogInformation("== {LogTags} - planid[{SourcePlanId}] | {message}".PadLeft(indent), GetLogTags(), sourcePlan.Id.ToString().PadRight(6), message);
            if (header) Log.LogInformation("===============================================================================================".PadLeft(indent));
        }

        private void InnerLog(ITestSuiteBase sourceTestSuite, string message = "", int indent = 0, bool header = false)
        {
            indent = indent + 5;
            if (header)
            {
                Log.LogInformation("===============================================================================================".PadLeft(indent));
                Log.LogInformation("==      Suite Title: {SourceTestSuiteTitle}=============================".PadLeft(indent), sourceTestSuite.Title.PadRight(45));
                Log.LogInformation("===============================================================================================".PadLeft(indent));
            }
            Log.LogInformation("== {Tags} - suiteid[{SourceTestSuiteId}] | {message}".PadLeft(indent), GetLogTags(), sourceTestSuite.Id.ToString().PadRight(6), message);
            if (header) Log.LogInformation("===============================================================================================".PadLeft(indent));
        }

        private void ValidateAndFixTestSuiteQuery(ITestSuiteBase source, IDynamicTestSuite targetSuiteChild,
            TestManagementContext targetTestStore)
        {
            try
            {
                // Verifying that the query is valid
                targetSuiteChild.Query.Execute();
            }
            catch (Exception e)
            {
                FixIterationNotFound(e, source, targetSuiteChild, targetTestStore);
            }
            finally
            {
                targetSuiteChild.Repopulate();
            }
        }
    }
}