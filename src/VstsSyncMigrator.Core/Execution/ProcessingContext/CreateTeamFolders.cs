using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;
using MigrationTools.Configuration;

namespace VstsSyncMigrator.Engine
{
    public class CreateTeamFolders : StaticProcessorBase
    {


        public CreateTeamFolders(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<CreateTeamFolders> logger) : base(services, me, telemetry, logger)
        {

        }

        public override string Name
        {
            get
            {
                return "CreateTeamFolders";
            }
        }

        public override void Configure(IProcessorConfig config)
        {

        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            TfsTeamService teamService = Engine.Target.GetService<TfsTeamService>();
            QueryHierarchy qh = ((WorkItemMigrationClient)Engine.Target.WorkItems).Store.Projects[Engine.Target.Config.Project].QueryHierarchy;
            List<TeamFoundationTeam> teamList = teamService.QueryTeams(Engine.Target.Config.Project).ToList();

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
                if (toCreate.Length != focus + 1)
                {
                    CreateFolderHyerarchy(toCreate, currentFolder[toCreate[focus]], focus + 1);
                }
            }
        }
    }
}
