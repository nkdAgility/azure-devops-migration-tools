using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;
using MigrationTools.Clients.AzureDevops.ObjectModel;
using Serilog;
using VstsSyncMigrator.Engine.ComponentContext;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;
using MigrationTools;
using MigrationTools.Engine.Processors;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class TestPlandsAndSuitesMigrationContext : MigrationProcessorBase
    {
        TestManagementContext sourceTestStore;
        ITestConfigurationCollection sourceTestConfigs;

        TestManagementContext targetTestStore;
        ITestConfigurationCollection targetTestConfigs;

        TestPlansAndSuitesMigrationConfig config;

        IIdentityManagementService sourceIdentityManagementService;
        IIdentityManagementService targetIdentityManagementService;

        int _currentPlan = 0;
        int _totalPlans = 0;
        long _elapsedms = 0;
        int __currentSuite = 0;
        int __totalSuites = 0;
        int _currentTestCases = 0;
        int _totalTestCases = 0;

        public override string Name
        {
            get
            {
                return "TestPlansAndSuitesMigrationContext";
            }
        }


        public TestPlandsAndSuitesMigrationContext(IMigrationEngine me, IServiceProvider services, ITelemetryLogger telemetry) : base(me, services, telemetry)
        {
        }

        public override void Configure(IProcessorConfig configx)
        {
            config = (TestPlansAndSuitesMigrationConfig)configx;
        }

        private void TraceWriteLine(ITestPlan sourcePlan, string message = "", int indent = 0, bool header = false)
        {
            if (header)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
                Trace.WriteLine($"==      Suite Name: {sourcePlan.Name.PadRight(45)}=============================".PadLeft(indent));
                Trace.WriteLine($"==            Date: {sourcePlan.StartDate.ToShortDateString().PadRight(45)}=============================".PadLeft(indent));
                Trace.WriteLine($"==          Suites: {sourcePlan.RootSuite.Entries.Count.ToString().PadRight(45)}=============================".PadLeft(indent));
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Trace.WriteLine($"== {GetLogTags()} - planid[{sourcePlan.Id.ToString().PadRight(6)}] | {message}".PadLeft(indent));
            if (header) Trace.WriteLine("===============================================================================================".PadLeft(indent));
            Console.ForegroundColor = ConsoleColor.White;
        }

        private string GetLogTags()
        {
            return $"{GetLogTag("Plan", _currentPlan, _totalPlans)} {GetLogTag("Suite", __currentSuite, __totalSuites)} {GetLogTag("Cases", _currentTestCases, _totalTestCases)} ";
        }

        private string GetLogTag(string name, int current, int total)
        {
            var currentString = current.ToString();
            var totalString = total.ToString();
            return $"{name}[{currentString.PadLeft(totalString.Length)}/{totalString}]";
        }

        private void TraceWriteLine(ITestSuiteBase sourceTestSuite, string message = "", int indent = 0, bool header = false)
        {
            indent = indent + 5;
            if (header)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
                Trace.WriteLine($"==      Suite Title: {sourceTestSuite.Title.PadRight(45)}=============================".PadLeft(indent));
                Trace.WriteLine("===============================================================================================".PadLeft(indent));
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Trace.WriteLine($"== {GetLogTags()} - suiteid[{sourceTestSuite.Id.ToString().PadRight(6)}] | {message}".PadLeft(indent));
            if (header) Trace.WriteLine("===============================================================================================".PadLeft(indent));
            Console.ForegroundColor = ConsoleColor.White;
        }

        protected override void InternalExecute()
        {
            sourceTestStore = new TestManagementContext(Engine.Source, config.TestPlanQueryBit);
            targetTestStore = new TestManagementContext(Engine.Target);
            sourceTestConfigs = sourceTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            targetTestConfigs = targetTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            sourceIdentityManagementService = Engine.Source.GetService<IIdentityManagementService>();
            targetIdentityManagementService = Engine.Target.GetService<IIdentityManagementService>();

            bool filterByCompleted = false;

            var stopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            ITestPlanCollection sourcePlans = sourceTestStore.GetTestPlans();
            List<ITestPlan> toProcess;
            if (filterByCompleted)
            {
                var targetPlanNames = (from ITestPlan tp in targetTestStore.GetTestPlans() select tp.Name).ToList();
                toProcess = (from ITestPlan tp in sourcePlans where !targetPlanNames.Contains(tp.Name) select tp).ToList();
            }
            else
            {
                toProcess = sourcePlans.ToList();
            }

            Trace.WriteLine(string.Format("Plan to copy {0} Plans?", toProcess.Count()), "TestPlansAndSuites");
            _currentPlan = 0;
            _totalPlans = toProcess.Count();

            foreach (ITestPlan sourcePlan in toProcess)
            {
                _currentPlan++;
                if (CanSkipElementBecauseOfTags(sourcePlan.Id))
                    continue;

                ProcessTestPlan(sourcePlan);

            }
            _currentPlan = 0;
            _totalPlans = 0;
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

        private void ProcessTestPlan(ITestPlan sourcePlan)
        {
            var stopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            var metrics = new Dictionary<string, double>();
            var parameters = new Dictionary<string, string>();
            AddParameter("PlanId", parameters, sourcePlan.Id.ToString());
            ////////////////////////////////////
            var newPlanName = config.PrefixProjectToNodes
                ? $"{Engine.Source.WorkItems.GetProject().Name}-{sourcePlan.Name}"
                : $"{sourcePlan.Name}";
            TraceWriteLine(sourcePlan, $"Process Plan {newPlanName}", 0, true);
            var targetPlan = FindTestPlan(targetTestStore, newPlanName);
            if (targetPlan != null && TargetPlanContansTag(targetPlan.Id))
            {
                return;
            }
            if (targetPlan == null)
            {
                TraceWriteLine(sourcePlan, $" Creating Plan {newPlanName}", 5);
                targetPlan = CreateNewTestPlanFromSource(sourcePlan, newPlanName);

                RemoveInvalidLinks(targetPlan);

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
                TraceWriteLine(sourcePlan, $"Found Plan {newPlanName}", 5); ;
            }
            if (HasChildSuites(sourcePlan.RootSuite))
            {
                __currentSuite = 0;
                __totalSuites = sourcePlan.RootSuite.Entries.Count;
                TraceWriteLine(sourcePlan, $"Source Plan has {__totalSuites} Suites", 5);
                metrics.Add("SubSuites", __totalSuites);
                foreach (var sourceSuiteChild in sourcePlan.RootSuite.SubSuites)
                {
                    __currentSuite++;
                    TraceWriteLine(sourceSuiteChild, $"", 5, true);
                    ProcessTestSuite(sourceSuiteChild, targetPlan.RootSuite, targetPlan);

                }
                __currentSuite = 0;
                __totalSuites = 0;
            }

            targetPlan.Save();
            // Load the plan again, because somehow it doesn't let me set configurations on the already loaded plan
            TraceWriteLine(sourcePlan, $"ApplyConfigurationsAndAssignTesters {targetPlan.Name}", 5); ;
            ITestPlan targetPlan2 = FindTestPlan(targetTestStore, targetPlan.Name);
            ApplyConfigurationsAndAssignTesters(sourcePlan.RootSuite, targetPlan2.RootSuite);
            //////////////////////////////
            TagCompletedTargetPlan(targetPlan.Id);
            ///////////////////////////////////////////////
            metrics.Add("ElapsedMS", stopwatch.ElapsedMilliseconds);
            Telemetry.TrackEvent("MigrateTestPlan", parameters, metrics);
            Telemetry.TrackRequest("MigrateTestPlan", starttime, stopwatch.Elapsed, "200", true);
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
                catch (UriFormatException e)
                {
                    linksToRemove.Add(link);
                }
            }

            if (linksToRemove.Any())
            {
                if (!config.RemoveInvalidTestSuiteLinks)
                {
                    TraceWriteLine(targetPlan, "We have detected test suite links that probably can't be migrated. You might receive an error 'The URL specified has a potentially unsafe URL protocol' when migrating to VSTS.", 5);
                    TraceWriteLine(targetPlan, "Please see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178 for more details.", 5);
                }
                else
                {
                    TraceWriteLine(targetPlan, $"Link count before removal of invalid: [{targetPlan.Links.Count}]", 5);
                    foreach (var link in linksToRemove)
                    {
                        TraceWriteLine(targetPlan,
                            $"Link with Description [{link.Description}] could not be migrated, as the URI is invalid. (We can't display the URI because of limitations in the TFS/VSTS/DevOps API.) Removing link.", 0);
                        targetPlan.Links.Remove(link);
                    }

                    TraceWriteLine(targetPlan, $"Link count after removal of invalid: [{targetPlan.Links.Count}]", 0);
                }
            }
        }

        private void AssignReflectedWorkItemId(int sourceWIId, int targetWIId)
        {
            var sourceWI = Engine.Source.WorkItems.GetWorkItem(sourceWIId.ToString());
            var targetWI = Engine.Target.WorkItems.GetWorkItem(targetWIId.ToString());
            targetWI.ToWorkItem().Fields[Engine.Target.Config.ReflectedWorkItemIDFieldName].Value = Engine.Source.WorkItems.CreateReflectedWorkItemId(sourceWI);
            targetWI.ToWorkItem().Save();
        }

        private void ApplyFieldMappings(int sourceWIId, int targetWIId)
        {
            var sourceWI = Engine.Source.WorkItems.GetWorkItem(sourceWIId.ToString());
            var targetWI = Engine.Target.WorkItems.GetWorkItem(targetWIId.ToString());

            if (config.PrefixProjectToNodes)
            {
                targetWI.ToWorkItem().AreaPath = string.Format(@"{0}\{1}", Engine.Target.Config.Project, sourceWI.ToWorkItem().AreaPath);
                targetWI.ToWorkItem().IterationPath = string.Format(@"{0}\{1}", Engine.Target.Config.Project, sourceWI.ToWorkItem().IterationPath);
            }
            else
            {
                var regex = new Regex(Regex.Escape(Engine.Source.Config.Project));
                targetWI.ToWorkItem().AreaPath = regex.Replace(sourceWI.ToWorkItem().AreaPath, Engine.Target.Config.Project, 1);
                targetWI.ToWorkItem().IterationPath = regex.Replace(sourceWI.ToWorkItem().IterationPath, Engine.Target.Config.Project, 1);
            }

            Engine.FieldMaps.ApplyFieldMappings(sourceWI, targetWI);

            //validate if save operation will work and report issues if found
            ArrayList validationIssues = targetWI.ToWorkItem().Validate();

            if (validationIssues.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("error(s) during validation in work item fields before saving:");

                foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.Field issue in validationIssues)
                {
                    sb.AppendLine(issue.ReferenceName + " - value = " + issue.Value);
                }

                throw new Exception(sb.ToString());
            }

            targetWI.ToWorkItem().Save();
        }

        private void TagCompletedTargetPlan(int workItemId)
        {
            var targetPlanWorkItem = Engine.Target.WorkItems.GetWorkItem(workItemId.ToString()) ;
            targetPlanWorkItem.ToWorkItem().Tags = targetPlanWorkItem.ToWorkItem().Tags + ";migrated";
            this.SaveWorkItem(targetPlanWorkItem);
        }

        private bool TargetPlanContansTag(int workItemId)
        {
            var targetPlanWorkItem = Engine.Target.WorkItems.GetWorkItem(workItemId.ToString());
            return targetPlanWorkItem.ToWorkItem().Tags.Contains("migrated");
        }


        private bool CanSkipElementBecauseOfTags(int workItemId)
        {
            if (config.OnlyElementsWithTag == null)
            {
                return false;
            }
            var sourcePlanWorkItem = Engine.Source.WorkItems.GetWorkItem(workItemId.ToString());
            var tagWhichMustBePresent = config.OnlyElementsWithTag;
            return !sourcePlanWorkItem.ToWorkItem().Tags.Contains(tagWhichMustBePresent);
        }

        private void ProcessTestSuite(ITestSuiteBase sourceSuite, ITestSuiteBase targetParent, ITestPlan targetPlan)
        {
            if (CanSkipElementBecauseOfTags(sourceSuite.Id))
                return;
            //////////////////////////////////////////
            var stopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            var metrics = new Dictionary<string, double>();
            var parameters = new Dictionary<string, string>();
            AddParameter("SuiteId", parameters, sourceSuite.Id.ToString());
            AddParameter("TestSuiteType", parameters, sourceSuite.TestSuiteType.ToString());
            ////////////////////////////////////

            TraceWriteLine(sourceSuite, $"    Processing {sourceSuite.TestSuiteType} : {sourceSuite.Id} - {sourceSuite.Title} ", 5);
            var targetSuiteChild = FindSuiteEntry((IStaticTestSuite)targetParent, sourceSuite.Title);

            // Target suite is not found in target system. We should create it.
            if (targetSuiteChild == null)
            {
                switch (sourceSuite.TestSuiteType)
                {
                    case TestSuiteType.None:
                        throw new NotImplementedException();
                    //break;
                    case TestSuiteType.DynamicTestSuite:
                        targetSuiteChild = CreateNewDynamicTestSuite(sourceSuite);
                        break;
                    case TestSuiteType.StaticTestSuite:
                        targetSuiteChild = CreateNewStaticTestSuite(sourceSuite);
                        break;
                    case TestSuiteType.RequirementTestSuite:
                        int sourceRid = ((IRequirementTestSuite)sourceSuite).RequirementId;
                        WorkItemData sourceReq = null;
                        WorkItemData targetReq = null;
                        try
                        {
                            sourceReq = Engine.Source.WorkItems.GetWorkItem(sourceRid.ToString()) ;
                            if (sourceReq == null)
                            {
                                TraceWriteLine(sourceSuite, "            Source work item not found", 5);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            TraceWriteLine(sourceSuite, "            Source work item cannot be loaded", 5);
                            break;
                        }
                        try
                        {
                            targetReq = Engine.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceReq);

                            if (targetReq == null)
                            {
                                TraceWriteLine(sourceSuite, "            Target work item not found", 5);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            TraceWriteLine(sourceSuite, "            Source work item not migrated to target, cannot be found", 5);
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
                TraceWriteLine(sourceSuite, "            Suite Exists", 5);
                ApplyDefaultConfigurations(sourceSuite, targetSuiteChild);
                if (targetSuiteChild.IsDirty)
                {
                    targetPlan.Save();
                }
            }

            // Recurse if Static Suite
            if (sourceSuite.TestSuiteType == TestSuiteType.StaticTestSuite && targetSuiteChild != null)
            {
                // Add Test Cases 
                AddChildTestCases(sourceSuite, targetSuiteChild, targetPlan);

                if (HasChildSuites(sourceSuite))
                {
                    TraceWriteLine(sourceSuite, $"            Suite has {((IStaticTestSuite)sourceSuite).Entries.Count} children", 5);
                    foreach (var sourceSuitChild in ((IStaticTestSuite)sourceSuite).SubSuites)
                    {
                        ProcessTestSuite(sourceSuitChild, targetSuiteChild, targetPlan);
                    }
                }
            }
            ///////////////////////////////////////////////

            metrics.Add("ElapsedMS", stopwatch.ElapsedMilliseconds);
            Telemetry.TrackEvent("MigrateTestSuite", parameters, metrics);
            Telemetry.TrackRequest("MigrateTestSuite", starttime, stopwatch.Elapsed, "200", true);
        }

        /// <summary>
        /// Fix work item ID's in query based suites
        /// </summary>
        private void FixWorkItemIdInQuery(ITestSuiteBase targetSuitChild)
        {
            var targetPlan = targetSuitChild.Plan;
            if (targetSuitChild.TestSuiteType == TestSuiteType.DynamicTestSuite)
            {
                var dynamic = (IDynamicTestSuite)targetSuitChild;

                if (
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(dynamic.Query.QueryText, "[System.Id]",
                        CompareOptions.IgnoreCase) >= 0)
                {
                    string regExSearchForSystemId = @"(\[System.Id\]\s*=\s*[\d]*)";
                    string regExSearchForSystemId2 = @"(\[System.Id\]\s*IN\s*)";

                    MatchCollection matches = Regex.Matches(dynamic.Query.QueryText, regExSearchForSystemId, RegexOptions.IgnoreCase);

                    foreach (Match match in matches)
                    {
                        var qid = match.Value.Split('=')[1].Trim();
                        var targetWi = Engine.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(Convert.ToInt32(qid), false);

                        if (targetWi == null)
                        {
                            TraceWriteLine(targetSuitChild, "Target WI does not exist. We are skipping this item. Please fix it manually.", 10);
                        }
                        else
                        {
                            TraceWriteLine(targetSuitChild, "Fixing [System.Id] in query in test suite '" + dynamic.Title + "' from " + qid + " to " + targetWi.Id, 10);
                            if (targetPlan != null)
                            {
                                dynamic.Refresh();
                                dynamic.Repopulate();
                            }
                            dynamic.Query = targetTestStore.Project.CreateTestQuery(dynamic.Query.QueryText.Replace(match.Value, string.Format("[System.Id] = {0}", targetWi.Id)));
                            if (targetPlan != null)
                            {
                                targetPlan.Save();
                            }
                        }
                    }
                }
            }
        }

        private void FixAssignedToValue(int sourceWIId, int targetWIId)
        {
            var sourceWI = Engine.Source.WorkItems.GetWorkItem(sourceWIId.ToString());
            var targetWI = Engine.Target.WorkItems.GetWorkItem(targetWIId.ToString());
            targetWI.ToWorkItem().Fields["System.AssignedTo"].Value = sourceWI.ToWorkItem().Fields["System.AssignedTo"].Value;
            targetWI.ToWorkItem().Save();
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
                return;


            _totalTestCases = source.TestCases.Count;
            _currentTestCases = 0;
            AddMetric("TestCaseCount", metrics, _totalTestCases);
            TraceWriteLine(source, string.Format("            Suite has {0} test cases", _totalTestCases), 15);
            List<ITestCase> tcs = new List<ITestCase>();
            foreach (ITestSuiteEntry sourceTestCaseEntry in source.TestCases)
            {
                _currentTestCases++;
                TraceWriteLine(source, $"Work item: {sourceTestCaseEntry.Id}", 15);

                if (CanSkipElementBecauseOfTags(sourceTestCaseEntry.Id))
                    return;

                TraceWriteLine(source, string.Format("    Processing {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                WorkItemData wi = Engine.Target.WorkItems.FindReflectedWorkItem(sourceTestCaseEntry.TestCase.WorkItem.ToWorkItemData(), false, Engine.Source.Config.ReflectedWorkItemIDFieldName);
                if (wi == null)
                {
                    TraceWriteLine(source, string.Format("    Can't find work item for Test Case. Has it been migrated? {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                    continue;
                }
                var exists = (from tc in target.TestCases
                              where tc.TestCase.WorkItem.Id.ToString() == wi.Id
                              select tc).SingleOrDefault();

                if (exists != null)
                {
                    TraceWriteLine(source, string.Format("    Test case already in suite {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                }
                else
                {
                    ITestCase targetTestCase = targetTestStore.Project.TestCases.Find(int.Parse(wi.Id));
                    if (targetTestCase == null)
                    {
                        TraceWriteLine(source, string.Format("    ERROR: Test case not found {0} : {1} - {2} ", sourceTestCaseEntry.EntryType, sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                    }
                    else
                    {
                        tcs.Add(targetTestCase);
                        TraceWriteLine(source, string.Format("    Adding {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), 15);
                    }
                }
            }

            target.TestCases.AddCases(tcs);

            targetPlan.Save();
            TraceWriteLine(source, string.Format("    SAVED {0} : {1} - {2} ", target.TestSuiteType.ToString(), target.Id, target.Title), 15);

            metrics.Add("ElapsedMS", stopwatch.ElapsedMilliseconds);
            Telemetry.TrackEvent("MigrateTestCases", parameters, metrics);
            Telemetry.TrackRequest("MigrateTestCases", starttime, stopwatch.Elapsed, "200", true);
            stopwatch.Stop();
            _totalTestCases = 0;
            _currentTestCases = 0;
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
                TraceWriteLine(source, $"   Setting default configurations for suite {target.Title}", 15);
                IList<IdAndName> targetConfigs = new List<IdAndName>();
                foreach (var config in source.DefaultConfigurations)
                {
                    var targetFound = (from tc in targetTestConfigs
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


        private void ApplyConfigurationsAndAssignTesters(ITestSuiteBase sourceSuite, ITestSuiteBase targetSuite)
        {

            _totalTestCases = sourceSuite.TestCases.Count();
            TraceWriteLine(sourceSuite, $"Applying configurations for test cases in source suite {sourceSuite.Title}", 5);
            foreach (ITestSuiteEntry sourceTce in sourceSuite.TestCases)
            {
                var stopwatch = Stopwatch.StartNew();
                var starttime = DateTime.Now;
                _currentTestCases++;
                WorkItemData wi = Engine.Target.WorkItems.FindReflectedWorkItem(sourceTce.TestCase.WorkItem.ToWorkItemData(), false);
                ITestSuiteEntry targetTce;
                if (wi != null)
                {
                    targetTce = (from tc in targetSuite.TestCases
                                 where tc.TestCase.WorkItem.Id.ToString() == wi.Id
                                 select tc).SingleOrDefault();
                    if (targetTce != null)
                    {
                        TraceWriteLine(sourceSuite, $"ApplyConfigurations Test Case ${sourceTce.Title} ", 5);
                        ApplyConfigurations(sourceTce, targetTce);
                    }
                    else
                    {
                        TraceWriteLine(sourceSuite, $"Test Case ${sourceTce.Title} from source is not included in target. Cannot apply configuration for it.", 5);
                    }
                }
                else
                {
                    TraceWriteLine(sourceSuite, $"Work Item for Test Case {sourceTce.Title} cannot be found in target. Has it been migrated?", 5);
                }
                Telemetry.TrackRequest("ApplyConfigurationsAndAssignTesters", starttime, stopwatch.Elapsed, "200", true);

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
                    WorkItemData sourceSuiteWi = Engine.Source.WorkItems.GetWorkItem(sourceSuiteChild.Id.ToString());
                    WorkItemData targetSuiteWi = Engine.Target.WorkItems.FindReflectedWorkItem(sourceSuiteWi, false);
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
                            TraceWriteLine(sourceSuite, $"Test Suite {sourceSuiteChild.Title} from source cannot be found in target. Has it been migrated?", 5);
                        }
                    }
                    else
                    {
                        TraceWriteLine(sourceSuite, $"Test Suite {sourceSuiteChild.Title} from source cannot be found in target. Has it been migrated?", 5);
                    }
                }
            }
        }

        private void AssignTesters(ITestSuiteBase sourceSuite, ITestSuiteBase targetSuite)
        {
            if (targetSuite == null)
            {
                Trace.TraceError($"Target Suite is NULL");
            }

            List<ITestPointAssignment> assignmentsToAdd = new List<ITestPointAssignment>();
            //loop over all source test case entries
            _totalTestCases = sourceSuite.TestCases.Count();
            _currentTestCases = 0;
            foreach (ITestSuiteEntry sourceTce in sourceSuite.TestCases)
            {
                _currentTestCases++;
                // find target testcase id for this source tce
                WorkItemData targetTc = Engine.Target.WorkItems.FindReflectedWorkItem(sourceTce.TestCase.WorkItem.ToWorkItemData(), false);

                if (targetTc == null)
                {
                    Trace.TraceError($"Target Reflected Work Item Not found for source WorkItem ID: {sourceTce.TestCase.WorkItem.Id}");
                }
                else
                {
                    TraceWriteLine(sourceSuite, $"Test Point Assignement for wi{targetTc.Id}", 15);
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
                                sourceIdentityManagementService.RefreshIdentity(tpa.AssignedTo.Descriptor);
                            }

                            targetIdentity = GetTargetIdentity(tpa.AssignedTo.Descriptor);
                        }

                        // translate source configuration id to target configuration id and name
                        //// Get source configuration name
                        string sourceConfigName = (from tc in sourceTestConfigs
                                                   where tc.Id == sourceConfigurationId
                                                   select tc.Name).FirstOrDefault();

                        //// Find source configuration name in target and get the id for it
                        int targetConfigId = (from tc in targetTestConfigs
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
                            var newAssignment = targetSuite.CreateTestPointAssignment(
                                int.Parse(targetTc.Id),
                                targetConfiguration,
                                targetUserId);

                            // add the test point assignment to the list
                            assignmentsToAdd.Add(newAssignment);
                        }
                        else
                        {
                            Trace.WriteLine($"Cannot find configuration with name [{sourceConfigName}] in target. Cannot assign tester to it.", "TestPlansAndSuites");
                        }
                    }
                }
            }

            // assign the list to the suite
            targetSuite.AssignTestPoints(assignmentsToAdd);
        }

        /// <summary>
        /// Retrieve the target identity for a given source descriptor
        /// </summary>
        /// <param name="sourceIdentityDescriptor">Source identity Descriptor</param>
        /// <returns>Target Identity</returns>
        private TeamFoundationIdentity GetTargetIdentity(IdentityDescriptor sourceIdentityDescriptor)
        {
            var sourceIdentity = sourceIdentityManagementService.ReadIdentity(
                sourceIdentityDescriptor,
                MembershipQuery.Direct,
                ReadIdentityOptions.ExtendedProperties);
            string sourceIdentityMail = sourceIdentity.GetProperty("Mail") as string;

            // Try refresh the Identity if we are missing the Mail property
            if (string.IsNullOrEmpty(sourceIdentityMail))
            {
                sourceIdentity = sourceIdentityManagementService.ReadIdentity(
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
                    targetIdentity = targetIdentityManagementService.ReadIdentity(
                        IdentitySearchFactor.MailAddress,
                        sourceIdentityMail,
                        MembershipQuery.Direct,
                        ReadIdentityOptions.None);
                }
                catch (MultipleIdentitiesFoundException)
                {
                    Trace.WriteLine($"Multiple identities found with email [{sourceIdentityMail}] in target system. Trying Account Name.", "TestPlansAndSuites");
                }

                if (targetIdentity == null)
                {
                    targetIdentity = targetIdentityManagementService.ReadIdentity(
                        IdentitySearchFactor.AccountName,
                        sourceIdentityMail,
                        MembershipQuery.Direct,
                        ReadIdentityOptions.None);
                }

                if (targetIdentity == null)
                {
                    Trace.WriteLine($"Cannot find tester with e-mail [{sourceIdentityMail}] in target system. Cannot assign.", "TestPlansAndSuites");
                    return null;
                }

                return targetIdentity;
            }
            else
            {
                Trace.WriteLine($"No e-mail address known in source system for [{sourceIdentity.DisplayName}]. Cannot translate to target.",
                    "TestPlansAndSuites");
                return null;
            }
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
                Trace.WriteLine(string.Format("   CONFIG MISMATCH FOUND --- FIX ATTEMPTING"), "TestPlansAndSuites");
                IList<IdAndName> targetConfigs = new List<IdAndName>();
                foreach (var config in sourceEntry.Configurations)
                {
                    var targetFound = (from tc in targetTestConfigs
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
                    Log.Error(ex, "Applying Configurations");
                }
            }

        }

        private bool HasChildTestCases(ITestSuiteBase sourceSuite)
        {
            return sourceSuite.TestCaseCount > 0;
        }

        private ITestSuiteBase CreateNewDynamicTestSuite(ITestSuiteBase source)
        {
            IDynamicTestSuite targetSuiteChild = targetTestStore.Project.TestSuites.CreateDynamic();
            targetSuiteChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            ApplyTestSuiteQuery(source, targetSuiteChild, targetTestStore);

            return targetSuiteChild;
        }

        private ITestSuiteBase CreateNewRequirementTestSuite(ITestSuiteBase source, WorkItemData requirement)
        {
            IRequirementTestSuite targetSuiteChild;
            try
            {
                targetSuiteChild = targetTestStore.Project.TestSuites.CreateRequirement(requirement.ToWorkItem());
            }
            catch (TestManagementValidationException ex)
            {
                TraceWriteLine(source, string.Format("            Unable to Create Requirement based Test Suite: {0}", ex.Message), 10);
                return null;
            }
            targetSuiteChild.Title = source.Title;
            return targetSuiteChild;
        }

        private void SaveNewTestSuiteToPlan(ITestPlan testPlan, IStaticTestSuite parent, ITestSuiteBase newTestSuite)
        {
            TraceWriteLine(newTestSuite,
                $"       Saving {newTestSuite.TestSuiteType} : {newTestSuite.Id} - {newTestSuite.Title} ", 10);
            try
            {
                ((IStaticTestSuite)parent).Entries.Add(newTestSuite);
            }
            catch (TestManagementServerException ex)
            {
                Log.Error(ex, " FAILED {TestSuiteType} : {Id} - {Title}",
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", Engine.Target.Config.Project},
                          { "Target Collection", Engine.Target.Config.Collection.ToString() },
                          { "Source Project", Engine.Source.Config.Project},
                          { "Source Collection", Engine.Source.Config.Collection.ToString() },
                          { "Status", Status.ToString() },
                          { "Task", "SaveNewTestSuitToPlan" },
                          { "Id", newTestSuite.Id.ToString()},
                          { "Title", newTestSuite.Title},
                          { "TestSuiteType", newTestSuite.TestSuiteType.ToString()}
                      });
                ITestSuiteBase ErrorSuiteChild = targetTestStore.Project.TestSuites.CreateStatic();
                ErrorSuiteChild.TestSuiteEntry.Title = string.Format(@"BROKEN: {0} | {1}", newTestSuite.Title, ex.Message);
                ((IStaticTestSuite)parent).Entries.Add(ErrorSuiteChild);
            }

            testPlan.Save();
        }

        private ITestSuiteBase CreateNewStaticTestSuite(ITestSuiteBase source)
        {
            ITestSuiteBase targetSuiteChild = targetTestStore.Project.TestSuites.CreateStatic();
            targetSuiteChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            return targetSuiteChild;
        }

        private ITestSuiteBase FindSuiteEntry(IStaticTestSuite staticSuite, string titleToFind)
        {
            return (from s in staticSuite.SubSuites where s.Title == titleToFind select s).SingleOrDefault();
        }

        private bool HasChildSuites(ITestSuiteBase sourceSuite)
        {
            bool hasChildren = false;
            if (sourceSuite != null && sourceSuite.TestSuiteType == TestSuiteType.StaticTestSuite)
            {
                hasChildren = (((IStaticTestSuite)sourceSuite).Entries.Count > 0);
            }
            return hasChildren;
        }

        private ITestPlan CreateNewTestPlanFromSource(ITestPlan sourcePlan, string newPlanName)
        {
            ITestPlan targetPlan;
            targetPlan = targetTestStore.CreateTestPlan();
            targetPlan.CopyPropertiesFrom(sourcePlan);
            targetPlan.Name = newPlanName;
            targetPlan.StartDate = sourcePlan.StartDate;
            targetPlan.EndDate = sourcePlan.EndDate;
            targetPlan.Description = sourcePlan.Description;

            // Set area and iteration to root of the target project. 
            // We will set the correct values later, when we actually have a work item available
            targetPlan.Iteration = Engine.Target.Config.Project;
            targetPlan.AreaPath = Engine.Target.Config.Project;

            // Remove testsettings reference because VSTS Sync doesn't support migrating these artifacts
            if (targetPlan.ManualTestSettingsId != 0)
            {
                targetPlan.ManualTestSettingsId = 0;
                targetPlan.AutomatedTestSettingsId = 0;
                Trace.WriteLine("Ignoring migration of Testsettings. Azure DevOps Migration Tools don't support migration of this artifact type.");
            }

            // Remove reference to build uri because VSTS Sync doesn't support migrating these artifacts
            if (targetPlan.BuildUri != null)
            {
                targetPlan.BuildUri = null;
                Trace.WriteLine(string.Format("Ignoring migration of assigned Build artifact {0}. Azure DevOps Migration Tools don't support migration of this artifact type.", sourcePlan.BuildUri));
            }
            return targetPlan;
        }

        private ITestPlan FindTestPlan(TestManagementContext tmc, string name)
        {
            return (from p in tmc.Project.TestPlans.Query("Select * From TestPlan") where p.Name == name select p).SingleOrDefault();
        }

        private void ApplyTestSuiteQuery(ITestSuiteBase source, IDynamicTestSuite targetSuiteChild, TestManagementContext targetTestStore)
        {
            targetSuiteChild.Query = ((IDynamicTestSuite)source).Query;

            // Postprocessing common errors
            FixQueryForTeamProjectNameChange(source, targetSuiteChild, targetTestStore);
            FixWorkItemIdInQuery(targetSuiteChild);
        }

        private void FixQueryForTeamProjectNameChange(ITestSuiteBase source, IDynamicTestSuite targetSuiteChild, TestManagementContext targetTestStore)
        {
            // Replacing old projectname in queries with new projectname
            // The target team project name is only available via target test store because the dyn. testsuite isnt saved at this point in time
            if (!source.Plan.Project.TeamProjectName.Equals(targetTestStore.Project.TeamProjectName))
            {
                Trace.WriteLine(string.Format(@"Team Project names dont match. We need to fix the query in dynamic test suite {0} - {1}.", source.Id, source.Title));
                Trace.WriteLine(string.Format(@"Replacing old project name {1} in query {0} with new team project name {2}", targetSuiteChild.Query.QueryText, source.Plan.Project.TeamProjectName, targetTestStore.Project.TeamProjectName));
                // First need to check is prefix project nodes has been applied for the migration
                if (config.PrefixProjectToNodes)
                {
                    // if prefix project nodes has been applied we need to take the original area/iteration value and prefix
                    targetSuiteChild.Query =
                        targetSuiteChild.Project.CreateTestQuery(targetSuiteChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", source.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}\{1}", targetTestStore.Project.TeamProjectName, source.Plan.Project.TeamProjectName)));
                }
                else
                {
                    // If we are not profixing project nodes then we just need to take the old value for the project and replace it with the new project value
                    targetSuiteChild.Query = targetSuiteChild.Project.CreateTestQuery(targetSuiteChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", source.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}", targetTestStore.Project.TeamProjectName)));
                }
                Trace.WriteLine(string.Format("New query is now {0}", targetSuiteChild.Query.QueryText));
            }
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

        private void FixIterationNotFound(Exception exception, ITestSuiteBase source, IDynamicTestSuite targetSuiteChild, TestManagementContext targetTestStore)
        {
            if (exception.Message.Contains("The specified iteration path does not exist."))
            {
                Regex regEx = new Regex(@"'(.*?)'");

                var missingIterationPath = regEx.Match(exception.Message).Groups[0].Value;
                missingIterationPath = missingIterationPath.Substring(missingIterationPath.IndexOf(@"\") + 1, missingIterationPath.Length - missingIterationPath.IndexOf(@"\") - 2);

                Trace.WriteLine("Found a orphaned iteration path in test suite query.");
                Trace.WriteLine(string.Format("Invalid iteration path {0}:", missingIterationPath));
                Trace.WriteLine("Replacing the orphaned iteration path from query with root iteration path. Please fix the query after the migration.");

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
    }
}
