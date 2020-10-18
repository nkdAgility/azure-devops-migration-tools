using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools.Clients;
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
            QueryHierarchy qh = ((TfsWorkItemMigrationClient)Engine.Target.WorkItems).Store.Projects[Engine.Target.Config.AsTeamProjectConfig().Project].QueryHierarchy;
            List<TeamFoundationTeam> teamList = teamService.QueryTeams(Engine.Target.Config.AsTeamProjectConfig().Project).ToList();

            Log.LogInformation("Found {0} teams?", teamList.Count);
            //////////////////////////////////////////////////
            int current = teamList.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (TeamFoundationTeam team in teamList)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();

                Log.LogTrace("Processing team {0}", team.Name);
                Regex r = new Regex(@"^Project - ([a-zA-Z ]*)");
                string path;
                if (r.IsMatch(team.Name))
                {
                    Log.LogInformation("{0} is a Project", team.Name);
                    path = string.Format(@"Projects\{0}", r.Match(team.Name).Groups[1].Value.Replace(" ", "-"));
                }
                else
                {
                    Log.LogInformation("{0} is a Team", team.Name);
                    path = string.Format(@"Teams\{0}", team.Name.Replace(" ", "-"));
                }
                Log.LogInformation(" and new path is {0}", path);
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
                Log.LogInformation("Average time of {average} per work item and {remaining} estimated to completion", average.ToString("c"), remaining.ToString("c"));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        private void CreateFolderHyerarchy(string[] toCreate, QueryItem currentItem, int focus = 0)
        {
            if (currentItem is QueryFolder)
            {
                QueryFolder currentFolder = (QueryFolder)currentItem;

                if (!currentFolder.Contains(toCreate[focus]))
                {
                    currentFolder.Add(new QueryFolder(toCreate[focus]));
                    Log.LogInformation("  Created: {0}", toCreate[focus]);
                }
                if (toCreate.Length != focus + 1)
                {
                    CreateFolderHyerarchy(toCreate, currentFolder[toCreate[focus]], focus + 1);
                }
            }
        }
    }
}