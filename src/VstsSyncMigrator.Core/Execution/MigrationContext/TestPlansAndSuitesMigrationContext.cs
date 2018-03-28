using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using VstsSyncMigrator.Engine.ComponentContext;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using VstsSyncMigrator.Engine.Configuration.Processing;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace VstsSyncMigrator.Engine
{
    public class TestPlandsAndSuitesMigrationContext : MigrationContextBase
    {
        MigrationEngine engine;

        WorkItemStoreContext sourceWitStore;
        TestManagementContext sourceTestStore;

        WorkItemStoreContext targetWitStore;
        TestManagementContext targetTestStore;
        ITestConfigurationCollection targetTestConfigs;

        TestPlansAndSuitesMigrationConfig config;

        public override string Name
        {
            get
            {
                return "TestPlansAndSuitesMigrationContext";
            }
        }

        public TestPlandsAndSuitesMigrationContext(MigrationEngine me, TestPlansAndSuitesMigrationConfig config) : base(me, config)
        {
            this.engine = me;
            sourceWitStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            sourceTestStore = new TestManagementContext(me.Source);
            targetWitStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            targetTestStore = new TestManagementContext(me.Target);
            targetTestConfigs = targetTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
            this.config = config;
        }

        internal override void InternalExecute()
        {
            ITestPlanCollection sourcePlans = sourceTestStore.GetTestPlans();
            Trace.WriteLine(string.Format("Plan to copy {0} Plans?", sourcePlans.Count), "TestPlansAndSuites");
            foreach (ITestPlan sourcePlan in sourcePlans)
            {
                if (CanSkipElementBecauseOfTags(sourcePlan.Id))
                    continue;

                var newPlanName = config.PrefixProjectToNodes
                    ? $"{sourceWitStore.GetProject().Name}-{sourcePlan.Name}"
                    : $"{sourcePlan.Name}";

                Trace.WriteLine($"Process Plan {newPlanName}", Name);
                var targetPlan = FindTestPlan(targetTestStore, newPlanName);
                if (targetPlan == null)
                {
                    Trace.WriteLine("    Plan missing... creating", Name);
                    targetPlan = CreateNewTestPlanFromSource(sourcePlan, newPlanName);
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
                    Trace.WriteLine("    Plan already found, not creating", Name);
                }
                if (HasChildSuites(sourcePlan.RootSuite))
                {
                    Trace.WriteLine($"    Source Plan has {sourcePlan.RootSuite.Entries.Count} Suites", Name);
                    foreach (var sourceSuiteChild in sourcePlan.RootSuite.SubSuites)
                    {
                        ProcessTestSuite(sourceSuiteChild, targetPlan.RootSuite, targetPlan);
                    }
                }

                targetPlan.Save();
                ITestPlan targetPlan2 = FindTestPlan(targetTestStore, targetPlan.Name);
                ApplyConfigurations(sourcePlan.RootSuite, targetPlan2.RootSuite);

            }
        }

        private void AssignReflectedWorkItemId(int sourceWIId, int targetWIId)
        {
            var sourceWI = sourceWitStore.Store.GetWorkItem(sourceWIId);
            var targetWI = targetWitStore.Store.GetWorkItem(targetWIId);
            targetWI.Fields[me.ReflectedWorkItemIdFieldName].Value = sourceWitStore.CreateReflectedWorkItemId(sourceWI);
            targetWI.Save();
        }

        private void ApplyFieldMappings(int sourceWIId, int targetWIId)
        {
            var sourceWI = sourceWitStore.Store.GetWorkItem(sourceWIId);
            var targetWI = targetWitStore.Store.GetWorkItem(targetWIId);

            if (config.PrefixProjectToNodes)
            {
                targetWI.AreaPath = string.Format(@"{0}\{1}", engine.Target.Name, sourceWI.AreaPath);
                targetWI.IterationPath = string.Format(@"{0}\{1}", engine.Target.Name, sourceWI.IterationPath);
            }
            else
            {
                var regex = new Regex(Regex.Escape(engine.Source.Name));
                targetWI.AreaPath = regex.Replace(sourceWI.AreaPath, engine.Target.Name, 1);
                targetWI.IterationPath = regex.Replace(sourceWI.IterationPath, engine.Target.Name, 1);
            }

            me.ApplyFieldMappings(sourceWI, targetWI);
            targetWI.Save();
        }

        private bool CanSkipElementBecauseOfTags(int workItemId)
        {
            if (config.OnlyElementsWithTag == null)
            {
                return false;
            }
            var sourcePlanWorkItem = sourceWitStore.Store.GetWorkItem(workItemId);
            var tagWhichMustBePresent = config.OnlyElementsWithTag;
            return !sourcePlanWorkItem.Tags.Contains(tagWhichMustBePresent);
        }

        private void ProcessTestSuite(ITestSuiteBase sourceSuite, ITestSuiteBase targetParent, ITestPlan targetPlan)
        {
            if (CanSkipElementBecauseOfTags(sourceSuite.Id))
                return;

            Trace.WriteLine($"    Processing {sourceSuite.TestSuiteType} : {sourceSuite.Id} - {sourceSuite.Title} ", Name);
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
                        WorkItem sourceReq = null;
                        WorkItem targetReq = null;
                        try
                        {
                            sourceReq = sourceWitStore.Store.GetWorkItem(sourceRid);
                            if (sourceReq == null)
                            {
                                Trace.WriteLine("            Source work item not found", Name);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine("            Source work item cannot be loaded", Name);
                            break;
                        }
                        try
                        {
                            targetReq = targetWitStore.FindReflectedWorkItemByReflectedWorkItemId(sourceReq,
                                me.ReflectedWorkItemIdFieldName);

                            if (targetReq == null)
                            {
                                Trace.WriteLine("            Target work item not found", Name);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine("            Source work item not migrated to target, cannot be found", Name);
                            break;
                        }
                        targetSuiteChild = CreateNewRequirementTestSuite(sourceSuite, targetReq);
                        break;
                    default:
                        throw new NotImplementedException();
                        //break;
                }
                if (targetSuiteChild == null) { return; }
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
            else
            {
                // The target suite already exists, take it from here
                Trace.WriteLine("            Suite Exists", Name);
                ApplyDefaultConfigurations(sourceSuite, targetSuiteChild);
                if (targetSuiteChild.IsDirty)
                {
                    targetPlan.Save();
                }
            }

            // Recurse if Static Suite
            if (sourceSuite.TestSuiteType == TestSuiteType.StaticTestSuite)
            {
                // Add Test Cases 
                AddChildTestCases(sourceSuite, targetSuiteChild, targetPlan);

                if (HasChildSuites(sourceSuite))
                {
                    Trace.WriteLine($"            Suite has {((IStaticTestSuite)sourceSuite).Entries.Count} children", Name);
                    foreach (var sourceSuitChild in ((IStaticTestSuite)sourceSuite).SubSuites)
                    {
                        ProcessTestSuite(sourceSuitChild, targetSuiteChild, targetPlan);
                    }
                }
            }
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
                        var targetWi = targetWitStore.FindReflectedWorkItemByReflectedWorkItemId(qid,
                            me.ReflectedWorkItemIdFieldName);

                        if (targetWi == null)
                        {
                            Trace.WriteLine("Target WI does not exist. We are skipping this item. Please fix it manually.");
                        }
                        else
                        {
                            Trace.WriteLine("Fixing [System.Id] in query in test suite '" + dynamic.Title + "' from " + qid + " to " + targetWi.Id, Name);
                            dynamic.Refresh();
                            dynamic.Repopulate();
                            dynamic.Query = targetTestStore.Project.CreateTestQuery(dynamic.Query.QueryText.Replace(match.Value, string.Format("[System.Id] = {0}", targetWi.Id)));
                            targetPlan.Save();
                        }
                    }
                }
            }
        }

        private void FixAssignedToValue(int sourceWIId, int targetWIId)
        {
            var sourceWI = sourceWitStore.Store.GetWorkItem(sourceWIId);
            var targetWI = targetWitStore.Store.GetWorkItem(targetWIId);
            targetWI.Fields["System.AssignedTo"].Value = sourceWI.Fields["System.AssignedTo"].Value;
            targetWI.Save();
        }

        private void AddChildTestCases(ITestSuiteBase source, ITestSuiteBase target, ITestPlan targetPlan)
        {
            if (CanSkipElementBecauseOfTags(source.Id))
                return;

            Trace.WriteLine(string.Format("            Suite has {0} test cases", source.TestCases.Count), "TestPlansAndSuites");
            List<ITestCase> tcs = new List<ITestCase>();
            foreach (ITestSuiteEntry sourceTestCaseEntry in source.TestCases)
            {
                if (CanSkipElementBecauseOfTags(sourceTestCaseEntry.Id))
                    return;

                Trace.WriteLine(string.Format("    Processing {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                WorkItem wi = targetWitStore.FindReflectedWorkItem(sourceTestCaseEntry.TestCase.WorkItem, me.ReflectedWorkItemIdFieldName, false);
                if (wi == null)
                {
                    Trace.WriteLine(string.Format("    Can't find work item for Test Case. Has it been migrated? {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                    break;
                }
                var exists = (from tc in target.TestCases
                              where tc.TestCase.WorkItem.Id == wi.Id
                              select tc).SingleOrDefault();

                if (exists != null)
                {
                    Trace.WriteLine(string.Format("    Test case already in suite {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                }
                else
                {
                    ITestCase targetTestCase = targetTestStore.Project.TestCases.Find(wi.Id);
                    if (targetTestCase == null)
                    {
                        Trace.WriteLine(string.Format("    ERROR: Test case not found {0} : {1} - {2} ", sourceTestCaseEntry.EntryType, sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                    }
                    else
                    {
                        tcs.Add(targetTestCase);
                        Trace.WriteLine(string.Format("    Adding {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                    }
                }
            }

            target.TestCases.AddCases(tcs);

            targetPlan.Save();
            Trace.WriteLine(string.Format("    SAVED {0} : {1} - {2} ", target.TestSuiteType.ToString(), target.Id, target.Title), "TestPlansAndSuites");

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
                Trace.WriteLine($"   Setting default configurations for suite {target.Title}", "TestPlansAndSuites");
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
            } else
            {
                target.ClearDefaultConfigurations();
            }
        }

        private void ApplyConfigurations(ITestSuiteBase sourceSuite, ITestSuiteBase targetSuite)
        {
            Trace.Write($"Applying configurations for test cases in source suite {sourceSuite.Title}");
            foreach (ITestSuiteEntry sourceTce in sourceSuite.TestCases)
            {
                WorkItem wi = targetWitStore.FindReflectedWorkItem(sourceTce.TestCase.WorkItem, me.ReflectedWorkItemIdFieldName, false);
                ITestSuiteEntry targetTce = (from tc in targetSuite.TestCases
                                where tc.TestCase.WorkItem.Id == wi.Id
                                select tc).SingleOrDefault();
                ApplyConfigurations(sourceTce, targetTce);
            }

            if(HasChildSuites(sourceSuite))
            {
                foreach(ITestSuiteEntry sourceSuiteChild in ((IStaticTestSuite)sourceSuite).Entries.Where(
                    e => e.EntryType == TestSuiteEntryType.DynamicTestSuite
                    || e.EntryType == TestSuiteEntryType.RequirementTestSuite
                    || e.EntryType == TestSuiteEntryType.StaticTestSuite))
                {
                    //Find migrated suite in target
                    WorkItem sourceSuiteWi = sourceWitStore.Store.GetWorkItem(sourceSuiteChild.Id);
                    WorkItem targetSuiteWi = targetWitStore.FindReflectedWorkItem(sourceSuiteWi, me.ReflectedWorkItemIdFieldName, false);
                    ITestSuiteEntry targetSuiteChild = (from tc in ((IStaticTestSuite)targetSuite).Entries
                                                        where tc.Id == targetSuiteWi.Id
                                                        select tc).FirstOrDefault();
                    if(targetSuiteChild != null)
                    {
                        ApplyConfigurations(sourceSuiteChild.TestSuite, targetSuiteChild.TestSuite);
                    }
                }
            }
        }

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
                    Telemetry.Current.TrackException(ex);
                }
            }

        }

        private bool HasChildTestCases(ITestSuiteBase sourceSuit)
        {
            return sourceSuit.TestCaseCount > 0;
        }

        private ITestSuiteBase CreateNewDynamicTestSuite(ITestSuiteBase source)
        {
            IDynamicTestSuite targetSuitChild = targetTestStore.Project.TestSuites.CreateDynamic();
            targetSuitChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            ApplyTestSuiteQuery(source, targetSuitChild, targetTestStore);

            return targetSuitChild;
        }

        private ITestSuiteBase CreateNewRequirementTestSuite(ITestSuiteBase source, WorkItem requirement)
        {
            IRequirementTestSuite targetSuitChild;
            try
            {
                targetSuitChild = targetTestStore.Project.TestSuites.CreateRequirement(requirement);
            }
            catch (TestManagementValidationException ex)
            {
                Trace.WriteLine(string.Format("            Unable to Create Requirement based Test Suit: {0}", ex.Message), "TestPlansAndSuites");
                return null;
            }
            targetSuitChild.Title = source.Title;
            return targetSuitChild;
        }

        private void SaveNewTestSuiteToPlan(ITestPlan testPlan, IStaticTestSuite parent, ITestSuiteBase newTestSuite)
        {
            Trace.WriteLine(
                $"       Saving {newTestSuite.TestSuiteType} : {newTestSuite.Id} - {newTestSuite.Title} ", "TestPlansAndSuites");
            try
            {
                ((IStaticTestSuite)parent).Entries.Add(newTestSuite);
            }
            catch (TestManagementServerException ex)
            {
                Telemetry.Current.TrackException(ex,
                      new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", me.Target.Name},
                          { "Target Collection", me.Target.Collection.Name },
                          { "Source Project", me.Source.Name},
                          { "Source Collection", me.Source.Collection.Name },
                          { "Status", Status.ToString() },
                          { "Task", "SaveNewTestSuitToPlan" },
                          { "Id", newTestSuite.Id.ToString()},
                          { "Title", newTestSuite.Title},
                          { "TestSuiteType", newTestSuite.TestSuiteType.ToString()}
                      });
                Trace.WriteLine(string.Format("       FAILED {0} : {1} - {2} | {3}", newTestSuite.TestSuiteType.ToString(), newTestSuite.Id, newTestSuite.Title, ex.Message), "TestPlansAndSuites");
                ITestSuiteBase ErrorSuitChild = targetTestStore.Project.TestSuites.CreateStatic();
                ErrorSuitChild.TestSuiteEntry.Title = string.Format(@"BROKEN: {0} | {1}", newTestSuite.Title, ex.Message);
                ((IStaticTestSuite)parent).Entries.Add(ErrorSuitChild);
            }

            testPlan.Save();
        }

        private ITestSuiteBase CreateNewStaticTestSuite(ITestSuiteBase source)
        {
            ITestSuiteBase targetSuitChild = targetTestStore.Project.TestSuites.CreateStatic();
            targetSuitChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            return targetSuitChild;
        }

        private ITestSuiteBase FindSuiteEntry(IStaticTestSuite staticSuite, string titleToFind)
        {
            return (from s in staticSuite.SubSuites where s.Title == titleToFind select s).SingleOrDefault();
        }

        private bool HasChildSuites(ITestSuiteBase sourceSuit)
        {
            bool hasChildren = false;
            if (sourceSuit != null && sourceSuit.TestSuiteType == TestSuiteType.StaticTestSuite)
            {
                hasChildren = (((IStaticTestSuite)sourceSuit).Entries.Count > 0);
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
            targetPlan.Iteration = engine.Target.Name;
            targetPlan.AreaPath = engine.Target.Name;

            // Remove testsettings reference because VSTS Sync doesnt support migrating these artifacts
            if (targetPlan.ManualTestSettingsId != 0)
            {
                targetPlan.ManualTestSettingsId = 0;
                targetPlan.AutomatedTestSettingsId = 0;
                Trace.WriteLine("Ignoring migration of Testsettings. VSTS Sync Migration Tools dont support migration of this artifact type.");
            }

            // Remove reference to build uri because VSTS Sync doesnt support migrating these artifacts
            if (targetPlan.BuildUri != null)
            {
                targetPlan.BuildUri = null;
                Trace.WriteLine(string.Format("Ignoring migration of assigned Build artifact {0}. VSTS Sync Migration Tools dont support migration of this artifact type.", sourcePlan.BuildUri));
            }
            return targetPlan;
        }

        private ITestPlan FindTestPlan(TestManagementContext tmc, string name)
        {
            return (from p in tmc.Project.TestPlans.Query("Select * From TestPlan") where p.Name == name select p).SingleOrDefault();
        }

        private void ApplyTestSuiteQuery(ITestSuiteBase source, IDynamicTestSuite targetSuitChild, TestManagementContext targetTestStore)
        {
            targetSuitChild.Query = ((IDynamicTestSuite)source).Query;

            // Postprocessing common errors
            FixQueryForTeamProjectNameChange(source, targetSuitChild, targetTestStore);
            FixWorkItemIdInQuery(targetSuitChild);
        }

        private void FixQueryForTeamProjectNameChange(ITestSuiteBase source, IDynamicTestSuite targetSuitChild, TestManagementContext targetTestStore)
        {
            // Replacing old projectname in queries with new projectname
            // The target team project name is only available via target test store because the dyn. testsuite isnt saved at this point in time
            if (!source.Plan.Project.TeamProjectName.Equals(targetTestStore.Project.TeamProjectName))
            {
                Trace.WriteLine(string.Format(@"Team Project names dont match. We need to fix the query in dynamic test suite {0} - {1}.", source.Id, source.Title));
                Trace.WriteLine(string.Format(@"Replacing old project name {1} in query {0} with new team project name {2}", targetSuitChild.Query.QueryText, source.Plan.Project.TeamProjectName, targetTestStore.Project.TeamProjectName));
                // First need to check is prefix project nodes has been applied for the migration
                if (config.PrefixProjectToNodes)
                {
                    // if prefix project nodes has been applied we need to take the original area/iteration value and prefix
                    targetSuitChild.Query =
                        targetSuitChild.Project.CreateTestQuery(targetSuitChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", source.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}\{1}", targetTestStore.Project.TeamProjectName, source.Plan.Project.TeamProjectName)));
                }
                else
                {
                    // If we are not profixing project nodes then we just need to take the old value for the project and replace it with the new project value
                    targetSuitChild.Query = targetSuitChild.Project.CreateTestQuery(targetSuitChild.Query.QueryText.Replace(
                            string.Format(@"'{0}", source.Plan.Project.TeamProjectName),
                            string.Format(@"'{0}", targetTestStore.Project.TeamProjectName)));
                }
                Trace.WriteLine(string.Format("New query is now {0}", targetSuitChild.Query.QueryText));
            }
        }

        private void ValidateAndFixTestSuiteQuery(ITestSuiteBase source, IDynamicTestSuite targetSuitChild,
            TestManagementContext targetTestStore)
        {
            try
            {
                // Verifying that the query is valid 
                targetSuitChild.Query.Execute();

            }
            catch (Exception e)
            {
                FixIterationNotFound(e, source, targetSuitChild, targetTestStore);
            }
            finally
            {
                targetSuitChild.Repopulate();
            }
        }

        private void FixIterationNotFound(Exception exception, ITestSuiteBase source, IDynamicTestSuite targetSuitChild, TestManagementContext targetTestStore)
        {
            if (exception.Message.Contains("The specified iteration path does not exist."))
            {
                Regex regEx = new Regex(@"'(.*?)'");

                var missingIterationPath = regEx.Match(exception.Message).Groups[0].Value;
                missingIterationPath = missingIterationPath.Substring(missingIterationPath.IndexOf(@"\") + 1, missingIterationPath.Length - missingIterationPath.IndexOf(@"\") - 2);

                Trace.WriteLine("Found a orphaned iteration path in test suite query.");
                Trace.WriteLine(string.Format("Invalid iteration path {0}:", missingIterationPath));
                Trace.WriteLine("Replacing the orphaned iteration path from query with root iteration path. Please fix the query after the migration.");

                targetSuitChild.Query = targetSuitChild.Project.CreateTestQuery(
                    targetSuitChild.Query.QueryText.Replace(
                        string.Format(@"'{0}\{1}'", source.Plan.Project.TeamProjectName, missingIterationPath),
                        string.Format(@"'{0}'", targetTestStore.Project.TeamProjectName)
                    ));

                targetSuitChild.Query = targetSuitChild.Project.CreateTestQuery(
                    targetSuitChild.Query.QueryText.Replace(
                        string.Format(@"'{0}\{1}'", targetTestStore.Project.TeamProjectName, missingIterationPath),
                        string.Format(@"'{0}'", targetTestStore.Project.TeamProjectName)
                    ));
            }
        }
    }
}