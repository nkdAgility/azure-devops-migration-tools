﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;
using MigrationTools.Processors.Infrastructure;
using Microsoft.Extensions.Options;
using MigrationTools.Enrichers;


namespace MigrationTools.Processors
{
    /// <summary>
    /// Creates folders in Sared Queries for each Team
    /// </summary>
    /// <status>alpha</status>
    /// <processingtarget>Shared Queries</processingtarget>
    public class CreateTeamFolders : TfsProcessor
    {
        public CreateTeamFolders(IOptions<EmptyProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;


        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            TfsTeamService teamService = Target.GetService<TfsTeamService>();
            QueryHierarchy qh = ((TfsWorkItemMigrationClient)Target.WorkItems).Store.Projects[Target.Options.Project].QueryHierarchy;
            List<TeamFoundationTeam> teamList = teamService.QueryTeams(Target.Options.Project).ToList();

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

                CreateFolderHierarchy(bits, qh["Shared Queries"]);

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

        private void CreateFolderHierarchy(string[] toCreate, QueryItem currentItem, int focus = 0)
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
                    CreateFolderHierarchy(toCreate, currentFolder[toCreate[focus]], focus + 1);
                }
            }
        }
    }
}