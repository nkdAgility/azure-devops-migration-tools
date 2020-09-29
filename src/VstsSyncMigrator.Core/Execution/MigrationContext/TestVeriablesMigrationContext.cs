using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using VstsSyncMigrator.Engine.ComponentContext;
using System.Linq;
using System.Collections.Generic;
using MigrationTools.Core.Configuration.Processing;
using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using MigrationTools;

namespace VstsSyncMigrator.Engine
{
    public class TestVeriablesMigrationContext : MigrationProcessorBase
    {
        public override string Name
        {
            get { return "TestVeriablesMigrationContext"; }
        }


        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/

        public TestVeriablesMigrationContext(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {

        }

        protected override void InternalExecute()
        {
            WorkItemStoreContext sourceWisc = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None, Telemetry);
            TestManagementContext SourceTmc = new TestManagementContext(me.Source);

            WorkItemStoreContext targetWisc = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules, Telemetry);
            TestManagementContext targetTmc = new TestManagementContext(me.Target);

            List<ITestVariable> sourceVars = SourceTmc.Project.TestVariables.Query().ToList();
            Trace.WriteLine(string.Format("Plan to copy {0} Veriables?", sourceVars.Count));

            foreach (var sourceVar in sourceVars)
            {
                Trace.WriteLine(string.Format("Copy: {0}", sourceVar.Name));
                ITestVariable targetVar = GetVar(targetTmc.Project.TestVariables, sourceVar.Name);
                if (targetVar == null)
                {
                    Trace.WriteLine(string.Format("    Need to create: {0}", sourceVar.Name));
                    targetVar = targetTmc.Project.TestVariables.Create();
                    targetVar.Name = sourceVar.Name;
                    targetVar.Save();
                }
                else
                {
                    Trace.WriteLine(string.Format("    Exists: {0}", sourceVar.Name));
                }
                // match values
                foreach (var sourceVal in sourceVar.AllowedValues)
                {
                    Trace.WriteLine(string.Format("    Seeking: {0}", sourceVal.Value));
                    ITestVariableValue targetVal = GetVal(targetVar, sourceVal.Value);
                    if (targetVal == null)
                    {
                        Trace.WriteLine(string.Format("    Need to create: {0}", sourceVal.Value));
                        targetVal = targetTmc.Project.TestVariables.CreateVariableValue(sourceVal.Value);
                        targetVar.AllowedValues.Add(targetVal);
                        targetVar.Save();
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("    Exists: {0}", targetVal.Value));
                    }
                }

            }
        }


        internal ITestVariable GetVar(ITestVariableHelper tvh, string variableToFind)
        {
            // Test Variables are case insensitive in VSTS so need ignore case in comparison
            return tvh.Query()
                .FirstOrDefault(variable => string.Equals(variable.Name, variableToFind,
                    StringComparison.OrdinalIgnoreCase));
        }

        internal ITestVariableValue GetVal(ITestVariable targetVar, string valueToFind)
        {
            // Test Variable values are case insensitive in VSTS so need ignore case in comparison
            return targetVar.AllowedValues.FirstOrDefault(
                variable => string.Equals(variable.Value, valueToFind, StringComparison.OrdinalIgnoreCase));
        }

        public override void Configure(IProcessorConfig config)
        {
            
        }
    }
}
