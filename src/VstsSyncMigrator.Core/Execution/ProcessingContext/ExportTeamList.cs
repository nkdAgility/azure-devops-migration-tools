using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Common;
using MigrationTools.Configuration.Processing;
using MigrationTools.Configuration;
using Microsoft.Extensions.Hosting;
using MigrationTools;
using MigrationTools;

namespace VstsSyncMigrator.Engine
{
    public class ExportTeamList : StaticProcessorBase
    {


        public ExportTeamList(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry) : base(services, me, telemetry)
        {

        }

        public override string Name
        {
            get
            {
                return "ExportTeamList";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
         
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			// Retrieve the project URI. Needed to enumerate teams.     
			var css4 = Engine.Target.GetService<ICommonStructureService4>();
            ProjectInfo projectInfo = css4.GetProjectFromName(Engine.Target.Config.Project);
            // Retrieve a list of all teams on the project.     
            TfsTeamService teamService = Engine.Target.GetService<TfsTeamService>();
            TfsConnection connection = (TfsConnection)Engine.Target.InternalCollection;


            foreach (ProjectInfo p in css4.ListAllProjects())
            {
                var allTeams = teamService.QueryTeams(p.Uri);

                foreach (TeamFoundationTeam team in allTeams)
                {
                    Trace.WriteLine(string.Format("Team name: {0}", team.Name), p.Name);
                    Trace.WriteLine(string.Format("Team ID: {0}", team.Identity.TeamFoundationId.ToString()), p.Name);
                    Trace.WriteLine(string.Format("Description: {0}", team.Description), p.Name);
                    var members =  team.GetMembers(connection, MembershipQuery.Direct);
                    Trace.WriteLine(string.Format("Team Accounts: {0}", String.Join(";", (from member in team.GetMembers(connection, MembershipQuery.Direct) select member.UniqueName))), p.Name);
                    Trace.WriteLine(string.Format("Team names: {0}", String.Join(";", (from member in team.GetMembers(connection, MembershipQuery.Direct) select member.DisplayName))), p.Name);
                }
            }

           
            //////////////////////////////////////////////////
            stopwatch.Stop();

            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

    }
}