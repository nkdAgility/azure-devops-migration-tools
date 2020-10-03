using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;
using MigrationTools;
using MigrationTools.Configuration;
using MigrationTools.Engine.Processors;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine
{
    public class TestConfigurationsMigrationContext : MigrationProcessorBase
    {
        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/

        public override string Name
        {
            get
            {
                return "TestConfigurationsMigrationContext";
            }
        }

        public TestConfigurationsMigrationContext(IMigrationEngine me, IServiceProvider services, ITelemetryLogger telemetry) : base(me, services, telemetry)
        {
        }

        protected override void InternalExecute()
        {
            TestManagementContext SourceTmc = new TestManagementContext(Engine.Source);
            TestManagementContext targetTmc = new TestManagementContext(Engine.Target);

            ITestConfigurationCollection tc = SourceTmc.Project.TestConfigurations.Query("Select * From TestConfiguration");
            Trace.WriteLine($"Plan to copy {tc.Count} Configurations", Name);

            foreach (var sourceTestConf in tc)
            {
                Trace.WriteLine($"{sourceTestConf.Name} - Copy Configuration", Name);
                ITestConfiguration targetTc = GetCon(targetTmc.Project.TestConfigurations, sourceTestConf.Name);
                if (targetTc != null)
                {
                    Trace.WriteLine($"{sourceTestConf.Name} - Found", Name);
                    // Move on
                }
                else
                {
                    Trace.WriteLine($"{sourceTestConf.Name} - Create new", Name);
                    targetTc = targetTmc.Project.TestConfigurations.Create();
                    targetTc.AreaPath = sourceTestConf.AreaPath.Replace(Engine.Source.Config.Project, Engine.Target.Config.Project);
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
                    Trace.WriteLine($"{sourceTestConf.Name} - Saved as {targetTc.Name}", Name);
                }
            }
        }

        private ITestConfiguration GetCon(ITestConfigurationHelper tch, string configToFind)
        {
            // Test configurations are case insensitive in VSTS so need ignore case in comparison
            return tch.Query("Select * From TestConfiguration").FirstOrDefault(variable => string.Equals(variable.Name, configToFind, StringComparison.OrdinalIgnoreCase));
        }

        public override void Configure(IProcessorConfig config)
        {
        }
    }
}