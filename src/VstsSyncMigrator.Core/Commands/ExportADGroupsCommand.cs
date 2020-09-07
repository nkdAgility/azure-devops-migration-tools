using AzureDevOpsMigrationTools.CommandLine;
using CsvHelper;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Commands
{
    public class ExportADGroupsCommand : CommandBase
    {

        public static object Run(ExportADGroupsOptions opts, string logPath)
        {
            Telemetry.Current.TrackEvent("Run-ExportADGroupsCommand");
            string exportPath = CreateExportPath(logPath, "ExportADGroups");
            Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(exportPath, "ExportADGroups.log"), "ExportADGroupsCommand"));
			Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            StreamWriter sw = File.CreateText(Path.Combine(exportPath, "AzureADGroups.csv"));
            sw.AutoFlush = true;
            using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader<AzureAdGroupItem>();

                TfsTeamProjectCollection sourceCollection = new TfsTeamProjectCollection(opts.CollectionURL);
                sourceCollection.EnsureAuthenticated();
                IIdentityManagementService2 sourceIMS2 = (IIdentityManagementService2)sourceCollection.GetService(typeof(IIdentityManagementService2));
                List<CatalogNode> sourceTeamProjects = sourceCollection.CatalogNode.QueryChildren(new[] { CatalogResourceTypes.TeamProject }, false, CatalogQueryOptions.None).ToList();
                if (opts.TeamProject != null)
                {
                    sourceTeamProjects = sourceTeamProjects.Where(x => x.Resource.DisplayName == opts.TeamProject).ToList();
                }
                int current = sourceTeamProjects.Count();
                foreach (CatalogNode sourceTeamProject in sourceTeamProjects)
                {
                    Trace.WriteLine(string.Format("---------------{0}\\{1}", current, sourceTeamProjects.Count()));
                    Trace.WriteLine(string.Format("{0}, {1}", sourceTeamProject.Resource.DisplayName, sourceTeamProject.Resource.Identifier));
                    string projectUri = sourceTeamProject.Resource.Properties["ProjectUri"];
                    TeamFoundationIdentity[] appGroups = sourceIMS2.ListApplicationGroups(projectUri, ReadIdentityOptions.None);
                    foreach (TeamFoundationIdentity appGroup in appGroups.Where(x => !x.DisplayName.EndsWith("\\Project Valid Users")))
                    {
                        Trace.WriteLine(string.Format("    {0}", appGroup.DisplayName));
                        TeamFoundationIdentity sourceAppGroup = sourceIMS2.ReadIdentity(appGroup.Descriptor, MembershipQuery.Expanded, ReadIdentityOptions.None);
                        foreach (IdentityDescriptor child in sourceAppGroup.Members.Where(x => x.IdentityType == "Microsoft.TeamFoundation.Identity"))
                        {

                            TeamFoundationIdentity sourceChildIdentity = sourceIMS2.ReadIdentity(IdentitySearchFactor.Identifier, child.Identifier, MembershipQuery.None, ReadIdentityOptions.ExtendedProperties);

                            if ((string)sourceChildIdentity.GetProperty("SpecialType") == "AzureActiveDirectoryApplicationGroup")
                            {
                                Trace.WriteLine(string.Format("     Suspected AD Group {0}", sourceChildIdentity.DisplayName));
                                csv.WriteRecord<AzureAdGroupItem>(new AzureAdGroupItem
                                {
                                    TeamProject = sourceTeamProject.Resource.DisplayName,
                                    ApplciationGroup = sourceTeamProject.Resource.DisplayName,
                                    Account = (string)sourceChildIdentity.GetProperty("Account"),
                                    Mail = (string)sourceChildIdentity.GetProperty("Mail"),
                                    DirectoryAlias = (string)sourceChildIdentity.GetProperty("DirectoryAlias")
                                });
                            }
                        }
                    }
                    current--;
                    sw.Flush();
                }
            }
            sw.Close();
            //    current--;
            //}



            //////////////////////////////////////////////////
            stopwatch.Stop();
            Trace.WriteLine(string.Format(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed));
            Trace.Listeners.Remove("ExportADGroupsCommand");
            return 0;
        }
    }
}
