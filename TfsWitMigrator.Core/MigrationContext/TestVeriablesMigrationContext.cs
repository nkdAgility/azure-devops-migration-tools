using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using _VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Linq;
using System.Collections.Generic;

namespace _VSTS.DataBulkEditor.Engine
{
    public class TestVeriablesMigrationContext : MigrationContextBase
    {
        public override string Name
        {
            get
            {
                return "TestVeriablesMigrationContext";
            }
        }


        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/


        public TestVeriablesMigrationContext(MigrationEngine me) : base(me)
        {

        }

        internal override void InternalExecute()
        {
            WorkItemStoreContext sourceWisc = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TestManagementContext SourceTmc = new TestManagementContext(me.Source);

            WorkItemStoreContext targetWisc = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            TestManagementContext targetTmc = new TestManagementContext(me.Target);

            List<ITestVariable> sourceVars = SourceTmc.Project.TestVariables.Query().ToList();
            Trace.WriteLine(string.Format("Plan to copy {0} Veriables?", sourceVars.Count));

            foreach (var sourceVar in sourceVars)
            {
                Trace.WriteLine(string.Format("Copy: {0}", sourceVar.Name));
                ITestVariable targetVar= GetVar(targetTmc.Project.TestVariables, sourceVar.Name);
                if (targetVar != null)
                {
                    Trace.WriteLine(string.Format("    Exists: {0}", sourceVar.Name));
                    // match values
                    foreach (var sourceVal in sourceVar.AllowedValues)
                    {
                        Trace.WriteLine(string.Format("    Seeking: {0}", sourceVal.Value));
                        ITestVariableValue targetVal = GetVal(targetVar, sourceVal.Value);
                        if (targetVal != null)
                        {
                            Trace.WriteLine(string.Format("    Exists: {0}", targetVal.Value));

                        } else
                        {
                            Trace.WriteLine(string.Format("    Need to create: {0}", targetVal.Value));
                            throw new NotImplementedException();
                        }
                    }
                }
                else
                {
                    Trace.WriteLine(string.Format("    Need to create: {0}", sourceVar.Name));
                    throw new NotImplementedException();
                }


            }
        }


        internal ITestVariable GetVar(ITestVariableHelper tvh, string variableToFind)
        {
            return (from tv in tvh.Query() where tv.Name == variableToFind select tv).SingleOrDefault();
        }

        internal ITestVariableValue GetVal(ITestVariable targetVar, string valueToFind)
        {
            return (from tv in targetVar.AllowedValues where tv.Value == valueToFind select tv).SingleOrDefault();
        }

    }

    }
