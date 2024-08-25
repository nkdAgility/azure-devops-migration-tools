using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;



namespace MigrationTools.Processors
{
    public class ExportTeamListProcessor : TfsProcessor
    {
        public ExportTeamListProcessor(IOptions<ProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;


        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            // Retrieve the project URI. Needed to enumerate teams.
            var css4 = Target.GetService<ICommonStructureService4>();
            ProjectInfo projectInfo = css4.GetProjectFromName(Target.Options.Project);
            // Retrieve a list of all teams on the project.
            TfsTeamService teamService = Target.GetService<TfsTeamService>();
            TfsConnection connection = (TfsConnection)Target.InternalCollection;

            foreach (ProjectInfo p in css4.ListAllProjects())
            {
                var allTeams = teamService.QueryTeams(p.Uri);

                foreach (TeamFoundationTeam team in allTeams)
                {
                    Log.LogInformation("Team name: {0}", team.Name);
                    Log.LogInformation("Team ID: {0}", team.Identity.TeamFoundationId.ToString());
                    Log.LogInformation("Description: {0}", team.Description, p.Name);
                    var members = team.GetMembers(connection, MembershipQuery.Direct);
                    Log.LogInformation("Team Accounts: {0}", String.Join(";", (from member in team.GetMembers(connection, MembershipQuery.Direct) select member.UniqueName)));
                    Log.LogInformation("Team names: {0}", String.Join(";", (from member in team.GetMembers(connection, MembershipQuery.Direct) select member.DisplayName)));
                }
            }

            //////////////////////////////////////////////////
            stopwatch.Stop();

            Log.LogInformation("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }
    }
}