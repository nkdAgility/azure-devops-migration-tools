﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.Client;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;
using MigrationTools.Processors.Infrastructure;
using Microsoft.Extensions.Options;
using MigrationTools.Enrichers;
using MigrationTools._EngineV1.Clients;


namespace MigrationTools.Processors
{
    /// <summary>
    /// This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`. 
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Suites &amp; Plans</processingtarget>
    public class TestVariablesMigrationProcessor : Processor
    {
        public TestVariablesMigrationProcessor(IOptions<TestVariablesMigrationProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        new TestVariablesMigrationProcessorOptions Options => (TestVariablesMigrationProcessorOptions)base.Options;

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        internal ITestVariableValue GetVal(ITestVariable targetVar, string valueToFind)
        {
            // Test Variable values are case insensitive in VSTS so need ignore case in comparison
            return targetVar.AllowedValues.FirstOrDefault(
                variable => string.Equals(variable.Value, valueToFind, StringComparison.OrdinalIgnoreCase));
        }

        internal ITestVariable GetVar(ITestVariableHelper tvh, string variableToFind)
        {
            // Test Variables are case insensitive in VSTS so need ignore case in comparison
            return tvh.Query()
                .FirstOrDefault(variable => string.Equals(variable.Name, variableToFind,
                    StringComparison.OrdinalIgnoreCase));
        }

        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/
        protected override void InternalExecute()
        {
            TestManagementContext SourceTmc = new TestManagementContext(Source);
            TestManagementContext targetTmc = new TestManagementContext(Target);
            List<ITestVariable> sourceVars = SourceTmc.Project.TestVariables.Query().ToList();
            Log.LogInformation("Plan to copy {0} Veriables?", sourceVars.Count);

            foreach (var sourceVar in sourceVars)
            {
                Log.LogInformation("Copy: {0}", sourceVar.Name);
                ITestVariable targetVar = GetVar(targetTmc.Project.TestVariables, sourceVar.Name);
                bool isDirty = false;
                if (targetVar == null)
                {
                    Log.LogInformation("    Need to create: {0}", sourceVar.Name);
                    targetVar = targetTmc.Project.TestVariables.Create();
                    targetVar.Name = sourceVar.Name;
                    isDirty = true;
                }
                else
                {
                    Log.LogInformation("    Exists: {0}", sourceVar.Name);
                }
                // match values
                foreach (var sourceVal in sourceVar.AllowedValues)
                {
                    Log.LogInformation("    Seeking: {0}", sourceVal.Value);
                    ITestVariableValue targetVal = GetVal(targetVar, sourceVal.Value);
                    if (targetVal == null)
                    {
                        Log.LogInformation("    Need to create: {0}", sourceVal.Value);
                        targetVal = targetTmc.Project.TestVariables.CreateVariableValue(sourceVal.Value);
                        targetVar.AllowedValues.Add(targetVal);
                        isDirty = true;
                    }
                    else
                    {
                        Log.LogInformation("    Exists: {0}", targetVal.Value);
                    }
                }
                if (isDirty)
                {
                    targetVar.Save();
                }
            }
        }
    }
}