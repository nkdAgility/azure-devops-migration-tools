using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using MigrationTools.Core.Configuration;
using Microsoft.Extensions.Hosting;
using MigrationTools;

namespace VstsSyncMigrator.Engine
{
    public class CreateTeamFolders : ProcessingContextBase
    {


        public CreateTeamFolders(IServiceProvider services, MigrationEngine me, ITelemetryLogger telemetry) : base(services, me, telemetry)
        {
         
        }

        public override string Name
        {
            get
            {
                return "CreateTeamFolders";
            }
        }

        public override void Configure(ITfsProcessingConfig config)
        {
           
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules, Telemetry);

            TfsQueryContext tfsqc = new TfsQueryContext(targetStore, Telemetry);

            TfsTeamService teamService = me.Target.Collection.GetService<TfsTeamService>();
            QueryHierarchy qh = targetStore.Store.Projects[me.Target.Config.Project].QueryHierarchy;
            List<TeamFoundationTeam> teamList = teamService.QueryTeams(me.Target.Config.Project).ToList();

            Trace.WriteLine(string.Format("Found {0} teams?", teamList.Count));
            //////////////////////////////////////////////////
            int current = teamList.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (TeamFoundationTeam team in teamList)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();

				Trace.Write(string.Format("Processing team {0}", team.Name));
                Regex r = new Regex(@"^Project - ([a-zA-Z ]*)");
                string path;
                if (r.IsMatch(team.Name))
                {
                    Trace.Write(string.Format(" is a Project"));
                    path = string.Format(@"Projects\{0}", r.Match(team.Name).Groups[1].Value.Replace(" ", "-"));

                }
                else
                {
                    Trace.Write(string.Format(" is a Team"));
                    path = string.Format(@"Teams\{0}", team.Name.Replace(" ", "-"));
                }
                Trace.Write(string.Format(" and new path is {0}", path));
                //me.AddFieldMap("*", new RegexFieldMap("KM.Simulation.Team", "System.AreaPath", @"^Project - ([a-zA-Z ]*)", @"Nemo\Projects\$1"));

                string[] bits = path.Split(char.Parse(@"\"));

                CreateFolderHyerarchy(bits, qh["Shared Queries"]);

                //_me.ApplyFieldMappings(workitem);
                qh.Save();


                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine("");
                //Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }


        void CreateFolderHyerarchy(string[] toCreate, QueryItem currentItem, int focus = 0)
        {
            if (currentItem is QueryFolder)
            {
                QueryFolder currentFolder = (QueryFolder)currentItem;
                
                if (!currentFolder.Contains(toCreate[focus]))
                {
                    currentFolder.Add(new QueryFolder(toCreate[focus]));
                    Trace.WriteLine(string.Format("  Created: {0}", toCreate[focus]));
                }
                if (toCreate.Length != focus+1)
                {
                    CreateFolderHyerarchy(toCreate, currentFolder[toCreate[focus]], focus + 1);
                }
            }
        }
    }
}
