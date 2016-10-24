using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VSTS.DataBulkEditor.Engine.Configuration.Processing;

namespace VSTS.DataBulkEditor.Engine
{
    public class TestPlansAndSuitsMigrationContext : MigrationContextBase
    {
        MigrationEngine engine;

        WorkItemStoreContext sourceWitStore;
        TestManagementContext sourceTestStore;

        WorkItemStoreContext targetWitStore;
        TestManagementContext targetTestStore;
        ITestConfigurationCollection targetTestConfigs;

        TestPlansAndSuitsMigrationConfig config;

        public override string Name
        {
            get
            {
                return "TestPlansAndSuitsMigrationContext";
            }
        }

        public TestPlansAndSuitsMigrationContext(MigrationEngine me, TestPlansAndSuitsMigrationConfig config) : base(me, config)
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
                string newPlanName = string.Format("{0}-{1}", sourceWitStore.GetProject().Name, sourcePlan.Name);
                Trace.WriteLine(string.Format("Process Plan {0} - ", newPlanName), "TestPlansAndSuites");
                ITestPlan targetPlan = FindTestPlan(targetTestStore, newPlanName);
                if (targetPlan == null)
                {
                    Trace.WriteLine(string.Format("    Plan missing...creating"), "TestPlansAndSuites");
                    targetPlan = CreateNewTestPlanFromSource(sourcePlan, newPlanName);
                    targetPlan.Save();
                }
                else
                {
                    Trace.WriteLine(string.Format("    Plan found"), "TestPlansAndSuites");
                }
                if (HasChildSuits(sourcePlan.RootSuite))
                {
                    Trace.WriteLine(string.Format("    Source Plan has {0} Suites", sourcePlan.RootSuite.Entries.Count), "TestPlansAndSuites");
                    foreach (ITestSuiteBase sourcerSuiteChild in sourcePlan.RootSuite.SubSuites)
                    {
                        ProcessStaticSuite(sourcerSuiteChild, targetPlan.RootSuite, targetPlan);
                    }
                    // Add Test Cases
                    ProcessChildTestCases(sourcePlan.RootSuite, targetPlan.RootSuite, targetPlan);

                }
            }

        }

        private void ProcessStaticSuite(ITestSuiteBase sourceSuit, ITestSuiteBase targetParent, ITestPlan targetPlan)
        {
            Trace.WriteLine(string.Format("    Processing {0} : {1} - {2} ", sourceSuit.TestSuiteType.ToString(), sourceSuit.Id, sourceSuit.Title), "TestPlansAndSuites");
            ITestSuiteBase targetSuitChild = FindSuiteEntry((IStaticTestSuite)targetParent, sourceSuit.Title);
            if (targetSuitChild == null)
            {
                // Should create
                switch (sourceSuit.TestSuiteType)
                {
                    case TestSuiteType.None:
                        throw new NotImplementedException();
                        //break;
                    case TestSuiteType.DynamicTestSuite:
                        targetSuitChild = CreateNewDynamicTestSuite(sourceSuit);
                        break;
                    case TestSuiteType.StaticTestSuite:
                        targetSuitChild = CreateNewStaticTestSuit(sourceSuit);
                        break;
                    case TestSuiteType.RequirementTestSuite:
                        int sourceRid = ((IRequirementTestSuite)sourceSuit).RequirementId;
                        WorkItem sourceReq = sourceWitStore.Store.GetWorkItem(sourceRid);
                        WorkItem targetReq = targetWitStore.FindReflectedWorkItemByReflectedWorkItemId(sourceReq, me.ReflectedWorkItemIdFieldName);
                        targetSuitChild = CreateNewRequirementTestSuite(sourceSuit, targetReq);
                        break;
                    default:
                        throw new NotImplementedException();
                        //break;
                }
                if (targetSuitChild == null) { return; }
                // Add to tareget and Save
                ApplyConfigurations(sourceSuit.TestSuiteEntry, targetSuitChild.TestSuiteEntry);
                SaveNewTestSuitToPlan(targetPlan, (IStaticTestSuite)targetParent, targetSuitChild);
            }
            else
            {
                // found
                Trace.WriteLine(string.Format("            Suite Exists"), "TestPlansAndSuites");
                ApplyConfigurations(sourceSuit.TestSuiteEntry, targetSuitChild.TestSuiteEntry);
                if (targetSuitChild.IsDirty)
                {
                    targetPlan.Save();
                }
            }
            // Recurse if Static Suite
            if (sourceSuit.TestSuiteType == TestSuiteType.StaticTestSuite && HasChildSuits(sourceSuit))
            {
                Trace.WriteLine(string.Format("            Suite has {0} children", ((IStaticTestSuite)sourceSuit).Entries.Count), "TestPlansAndSuites");
                foreach (ITestSuiteBase sourceSuitChild in ((IStaticTestSuite)sourceSuit).SubSuites)
                {
                    ProcessStaticSuite(sourceSuitChild, targetSuitChild, targetPlan);

                }
            }
            // Add Test Cases
            ProcessChildTestCases(sourceSuit, targetSuitChild, targetPlan);
        }

        private void ProcessChildTestCases(ITestSuiteBase source, ITestSuiteBase target, ITestPlan targetPlan)
        {
            if (source.TestSuiteType == TestSuiteType.StaticTestSuite && HasChildTestCases(source))
            {
                Trace.WriteLine(string.Format("            Suite has {0} test cases", ((IStaticTestSuite)source).TestCases.Count), "TestPlansAndSuites");
                List<ITestCase> tcs = new List<ITestCase>();
                foreach (ITestSuiteEntry sourceTestCaseEntry in source.TestCases)
                {
                    Trace.WriteLine(string.Format("    Processing {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                    WorkItem wi = targetWitStore.FindReflectedWorkItem(sourceTestCaseEntry.TestCase.WorkItem, me.ReflectedWorkItemIdFieldName, false);
                    if (wi == null)
                    {
                        Trace.WriteLine(string.Format("    ERROR NOT FOUND {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                        break;
                    }
                    var exists = (from tc in target.TestCases
                                  where tc.TestCase.WorkItem.Id == wi.Id
                                  select tc).SingleOrDefault();

                    if (exists != null)
                    {
                        Trace.WriteLine(string.Format("    EXISTS {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                        ApplyConfigurations(sourceTestCaseEntry, exists);
                    }
                    else
                    {
                        ITestCase targetTestCase = targetTestStore.Project.TestCases.Find(wi.Id);
                        ApplyConfigurations(sourceTestCaseEntry, targetTestCase.TestSuiteEntry);
                        tcs.Add(targetTestCase);
                        Trace.WriteLine(string.Format("    ADDING {0} : {1} - {2} ", sourceTestCaseEntry.EntryType.ToString(), sourceTestCaseEntry.Id, sourceTestCaseEntry.Title), "TestPlansAndSuites");
                    }
                }
                target.TestCases.AddCases(tcs);
                targetPlan.Save();
                Trace.WriteLine(string.Format("    SAVED {0} : {1} - {2} ", target.TestSuiteType.ToString(), target.Id, target.Title), "TestPlansAndSuites");

            }
        }

        private void ApplyConfigurations(ITestSuiteBase source, ITestSuiteBase target)
        {
            if (source.DefaultConfigurations != null)
            {
                //if (source.DefaultConfigurations != null && source.DefaultConfigurations.Count != target.DefaultConfigurations.Count)
                //{
                    Trace.WriteLine(string.Format("   CONFIG MNISSMATCH FOUND --- FIX AATTEMPTING"), "TestPlansAndSuites");
                    target.ClearDefaultConfigurations();
                    IList<IdAndName> targetConfigs = new List<IdAndName>();
                    foreach (var config in source.DefaultConfigurations)
                    {
                        var targetFound = (from tc in targetTestConfigs
                                           where tc.Name == config.Name
                                           select tc).SingleOrDefault();
                        if (!(targetFound == null))
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

               // }

            }
        }

        private void ApplyConfigurations(ITestSuiteEntry sourceEntry, ITestSuiteEntry targetEntry)
        {
            if (sourceEntry.Configurations != null)
            {
                if (sourceEntry.Configurations.Count != targetEntry.Configurations.Count)
                {
                    Trace.WriteLine(string.Format("   CONFIG MNISSMATCH FOUND --- FIX AATTEMPTING"), "TestPlansAndSuites");
                    targetEntry.Configurations.Clear();
                    IList<IdAndName> targetConfigs = new List<IdAndName>();
                    foreach (var config in sourceEntry.Configurations)
                    {
                        var targetFound = (from tc in targetTestConfigs
                                           where tc.Name == config.Name
                                           select tc).SingleOrDefault();
                        if (!(targetFound == null))
                        {

                            targetConfigs.Add(new IdAndName(targetFound.Id, targetFound.Name));
                        }
                    }
                    try
                    {
                        targetEntry.SetConfigurations(targetConfigs);
                    }
                    catch (Exception)
                    {
                        // SOmetimes this will error out for no reason.
                    }
                    
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
            if (source.TestSuiteEntry.Configurations != null)
            {
                ApplyConfigurations(source, targetSuitChild);
            }
            targetSuitChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            targetSuitChild.Query = ((IDynamicTestSuite)source).Query;
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

            if (source.TestSuiteEntry.Configurations != null)
            {
                ApplyConfigurations(source, targetSuitChild);
            }
            targetSuitChild.Title = source.Title;
            return targetSuitChild;
        }

        private void SaveNewTestSuitToPlan(ITestPlan testPlan, IStaticTestSuite parent, ITestSuiteBase newTestSuite)
        {
            Trace.WriteLine(string.Format("       Saving {0} : {1} - {2} ", newTestSuite.TestSuiteType.ToString(), newTestSuite.Id, newTestSuite.Title), "TestPlansAndSuites");
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

        private ITestSuiteBase CreateNewStaticTestSuit(ITestSuiteBase source)
        {
            ITestSuiteBase targetSuitChild = targetTestStore.Project.TestSuites.CreateStatic();
            if (source.TestSuiteEntry.Configurations != null)
            {
                ApplyConfigurations(source,targetSuitChild);
            }
            targetSuitChild.TestSuiteEntry.Title = source.TestSuiteEntry.Title;
            return targetSuitChild;
        }

    

        private ITestSuiteBase FindSuiteEntry(IStaticTestSuite staticSuit, string titleToFind)
        {
            return (from s in staticSuit.SubSuites where s.Title == titleToFind select s).SingleOrDefault();
        }

        private bool HasChildSuits(ITestSuiteBase sourceSuit)
        {
            bool hasChildren = false;
            if (sourceSuit != null && sourceSuit.TestSuiteType == TestSuiteType.StaticTestSuite)
            {
                hasChildren = (((IStaticTestSuite)sourceSuit).Entries.Count > 0);
            }
            return hasChildren;
        }

        private ITestPlan CreateNewTestPlanFromSource(ITestPlan sourcePlan,  string newPlanName)
        {
            ITestPlan targetPlan;
            targetPlan = targetTestStore.CreateTestPlan();
            targetPlan.CopyPropertiesFrom(sourcePlan);
            targetPlan.Name = newPlanName;
            targetPlan.StartDate = sourcePlan.StartDate;
            targetPlan.EndDate = sourcePlan.EndDate;
            if (config.PrefixProjectToNodes)
            {
                targetPlan.AreaPath = string.Format(@"{0}\{1}", engine.Target.Name, sourcePlan.AreaPath);
                targetPlan.Iteration = string.Format(@"{0}\{1}", engine.Target.Name, sourcePlan.Iteration);
            }
            else
            {
                var regex = new Regex(Regex.Escape(engine.Source.Name));
                targetPlan.AreaPath = regex.Replace(sourcePlan.AreaPath, engine.Target.Name, 1);
                targetPlan.Iteration = regex.Replace(sourcePlan.Iteration, engine.Target.Name, 1);
            }
            targetPlan.ManualTestSettingsId = 0;
            return targetPlan;
        }

        private ITestPlan FindTestPlan(TestManagementContext tmc, string name)
        {
            return (from p in tmc.Project.TestPlans.Query("Select * From TestPlan") where p.Name == name select p).SingleOrDefault();
        }



    }
}