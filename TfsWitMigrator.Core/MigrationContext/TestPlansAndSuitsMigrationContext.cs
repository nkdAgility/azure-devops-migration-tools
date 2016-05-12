using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using TfsWitMigrator.Core.ComponentContext;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace TfsWitMigrator.Core
{
    public class TestPlansAndSuitsMigrationContext : MigrationContextBase
    {

        WorkItemStoreContext sourceWitStore;
        TestManagementContext sourceTestStore;

        WorkItemStoreContext targetWitStore;
        TestManagementContext targetTestStore;

        string targetAreaIterationPath;

        public override string Name
        {
            get
            {
                return "TestPlansAndSuitsMigrationContext";
            }
        }

        public TestPlansAndSuitsMigrationContext(MigrationEngine me, string areaIterationPath) : base(me)
        {
            sourceWitStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            sourceTestStore = new TestManagementContext(me.Source);
            targetWitStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            targetTestStore = new TestManagementContext(me.Target);
            this.targetAreaIterationPath = areaIterationPath;
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
                        WorkItem targetReq = targetWitStore.FindReflectedWorkItemByReflectedWorkItemId(sourceReq);
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
                    WorkItem wi = targetWitStore.FindReflectedWorkItem(sourceTestCaseEntry.TestCase.WorkItem);
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

        private void ApplyConfigurations(ITestSuiteEntry sourceEntry, ITestSuiteEntry targetEntry)
        {
            if (sourceEntry.Configurations != null)
            {
                if (sourceEntry.Configurations.Count != targetEntry.Configurations.Count)
                {
                    Trace.WriteLine(string.Format("   CONFIG MNISSMATCH FOUND --- FIX AATTEMPTING"), "TestPlansAndSuites");
                    targetEntry.Configurations.Clear();
                    IList<IdAndName> targetConfigs = new List<IdAndName>();

                    ITestConfigurationCollection tcc = targetTestStore.Project.TestConfigurations.Query("Select * From TestConfiguration");
                    foreach (var config in sourceEntry.Configurations)
                    {
                        var found = (from tc in tcc
                                     where tc.Name == config.Name
                                     select tc).SingleOrDefault();
                        if (!(found == null))
                        {
                            targetConfigs.Add(new IdAndName(found.Id, found.Name));
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
                // TODO targetSuitChild.TestSuiteEntry.SetConfigurations(source.TestSuiteEntry.Configurations);
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
                // TODO targetSuitChild.TestSuiteEntry.SetConfigurations(source.TestSuiteEntry.Configurations);
            }
            targetSuitChild.Title = source.Title;
            return targetSuitChild;
        }

        private void SaveNewTestSuitToPlan(ITestPlan testPlan, IStaticTestSuite parent, ITestSuiteBase newTestSuite)
        {
            Trace.WriteLine(string.Format("       Saving {0} : {1} - {2} ", newTestSuite.TestSuiteType.ToString(), newTestSuite.Id, newTestSuite.Title), "TestPlansAndSuites");
            ((IStaticTestSuite)parent).Entries.Add(newTestSuite);
            testPlan.Save();
        }

        private ITestSuiteBase CreateNewStaticTestSuit(ITestSuiteBase source)
        {
            ITestSuiteBase targetSuitChild = targetTestStore.Project.TestSuites.CreateStatic();
            if (source.TestSuiteEntry.Configurations != null)
            {
                targetSuitChild.TestSuiteEntry.SetConfigurations(source.TestSuiteEntry.Configurations);
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
            targetPlan.AreaPath = this.targetAreaIterationPath ;
            targetPlan.Iteration = this.targetAreaIterationPath;
            targetPlan.ManualTestSettingsId = 0;
            return targetPlan;
        }

        private ITestPlan FindTestPlan(TestManagementContext tmc, string name)
        {
            return (from p in tmc.Project.TestPlans.Query("Select * From TestPlan") where p.Name == name select p).SingleOrDefault();
        }



    }
}