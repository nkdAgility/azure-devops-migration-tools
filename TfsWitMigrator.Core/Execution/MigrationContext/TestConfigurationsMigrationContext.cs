using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Linq;
using VSTS.DataBulkEditor.Engine.Configuration.Processing;

namespace VSTS.DataBulkEditor.Engine
{
    public class TestConfigurationsMigrationContext : MigrationContextBase
    {

        // http://blogs.microsoft.co.il/shair/2015/02/02/tfs-api-part-56-test-configurations/

        public override string Name
        {
            get
            {
                return "TestConfigurationsMigrationContext";
            }
        }
        public TestConfigurationsMigrationContext(MigrationEngine me, TestConfigurationsMigrationConfig config) : base(me, config)
        {

        }

        internal override void InternalExecute()
        {
            WorkItemStoreContext sourceWisc = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TestManagementContext SourceTmc = new TestManagementContext(me.Source);

            WorkItemStoreContext targetWisc = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            TestManagementContext targetTmc = new TestManagementContext(me.Target);


            ITestConfigurationCollection tc = SourceTmc.Project.TestConfigurations.Query("Select * From TestConfiguration");
            Trace.WriteLine(string.Format("Plan to copy {0} Configurations?", tc.Count));

            foreach (var sourceTestConf in tc)
            {
                Trace.WriteLine("Copy Configuration {0} - ", sourceTestConf.Name);
                ITestConfiguration targetTc = GetCon(targetTmc.Project.TestConfigurations, sourceTestConf.Name);
                if (targetTc != null)
                {
                    Trace.WriteLine("    Found {0} - ", sourceTestConf.Name);  
                    // Move on
                } else
                {
                    Trace.WriteLine("    Create new {0} - ", sourceTestConf.Name);
                    targetTc = targetTmc.Project.TestConfigurations.Create();
                    targetTc.AreaPath = sourceTestConf.AreaPath.Replace(me.Source.Name, me.Target.Name);
                    targetTc.Description = sourceTestConf.Description;
                    targetTc.IsDefault = sourceTestConf.IsDefault;
                    targetTc.Name = sourceTestConf.Name;
                    foreach (var val in targetTc.Values)
                    {
                        if (!targetTc.Values.ContainsKey(val.Key)) {
                            targetTc.Values.Add(val);
                        }
                    }
                    targetTc.State = sourceTestConf.State;
                    targetTc.Save();
                    Trace.WriteLine(string.Format("    Saved {0} - ", targetTc.Name));
                }
            }

        }
        internal ITestConfiguration GetCon(ITestConfigurationHelper tch, string configToFind)
        {
            return (from tv in tch.Query("Select * From TestConfiguration") where tv.Name == configToFind select tv).SingleOrDefault();
        }

    }
}