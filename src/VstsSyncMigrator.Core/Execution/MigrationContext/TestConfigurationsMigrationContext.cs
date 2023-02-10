using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.TestManagement.Client;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Processors;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// Migrates Test configurations  
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Suites &amp; Plans</processingtarget>
    public class TestConfigurationsMigrationContext : MigrationProcessorBase
    {
        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/

        public TestConfigurationsMigrationContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<TestConfigurationsMigrationContext> logger) : base(engine, services, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "TestConfigurationsMigrationContext";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
        }

        protected override void InternalExecute()
        {
            TestManagementContext SourceTmc = new TestManagementContext(Engine.Source);
            TestManagementContext targetTmc = new TestManagementContext(Engine.Target);

            ITestConfigurationCollection tc = SourceTmc.Project.TestConfigurations.Query("Select * From TestConfiguration");
            Log.LogDebug("Plan to copy {TestCaseCount} Configurations", tc.Count);

            foreach (var sourceTestConf in tc)
            {
                Log.LogDebug("{sourceTestConfName} - Copy Configuration", sourceTestConf.Name);
                ITestConfiguration targetTc = GetCon(targetTmc.Project.TestConfigurations, sourceTestConf.Name);
                if (targetTc != null)
                {
                    Log.LogDebug("{sourceTestConfName} - Found", sourceTestConf.Name);
                    // Move on
                }
                else
                {
                    Log.LogDebug("{sourceTestConfName} - Create new", sourceTestConf.Name);
                    targetTc = targetTmc.Project.TestConfigurations.Create();
                    targetTc.AreaPath = sourceTestConf.AreaPath.Replace(Engine.Source.Config.AsTeamProjectConfig().Project, Engine.Target.Config.AsTeamProjectConfig().Project);
                    targetTc.Description = sourceTestConf.Description;
                    targetTc.IsDefault = sourceTestConf.IsDefault;
                    targetTc.Name = sourceTestConf.Name;

                    foreach (var val in sourceTestConf.Values)
                    {
                        if (!targetTc.Values.ContainsKey(val.Key))
                        {
                            targetTc.Values.Add(val);
                        }
                    }

                    targetTc.State = sourceTestConf.State;
                    targetTc.Save();
                    Log.LogDebug("{sourceTestConfName} - Saved as {targetTcName}", sourceTestConf.Name, targetTc.Name);
                }
            }
        }

        private ITestConfiguration GetCon(ITestConfigurationHelper tch, string configToFind)
        {
            // Test configurations are case insensitive in VSTS so need ignore case in comparison
            return tch.Query("Select * From TestConfiguration").FirstOrDefault(variable => string.Equals(variable.Name, configToFind, StringComparison.OrdinalIgnoreCase));
        }
    }
}