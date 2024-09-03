using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;

using Microsoft.Extensions.Options;
using MigrationTools.Tools;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Clients;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Downloads corporate images and updates TFS/Azure DevOps profiles
    /// </summary>
    /// <status>alpha</status>
    /// <processingtarget>Profiles</processingtarget>
    public class TfsExportProfilePictureFromADProcessor : Processor
    {
        private IIdentityManagementService2 ims2;

        public TfsExportProfilePictureFromADProcessor(IOptions<TfsExportProfilePictureFromADProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
            //http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C
            ims2 = Target.GetService<IIdentityManagementService2>();
        }

        new TfsExportProfilePictureFromADProcessorOptions Options => (TfsExportProfilePictureFromADProcessorOptions)base.Options;

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            string exportPath;
            string assPath = Assembly.GetEntryAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "export-pic");
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            TeamFoundationIdentity SIDS = ims2.ReadIdentity(IdentitySearchFactor.AccountName, "Team Foundation Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);

            Log.LogInformation("Found {0}", SIDS.Members.Count());
            var itypes = (from IdentityDescriptor id in SIDS.Members select id.IdentityType).Distinct();

            foreach (string item in itypes)
            {
                var infolks = (from IdentityDescriptor id in SIDS.Members where id.IdentityType == item select id);
                Log.LogInformation("Found {0} of {1}", infolks.Count(), item);
            }
            var folks = (from IdentityDescriptor id in SIDS.Members where id.IdentityType == "System.Security.Principal.WindowsIdentity" select id);

            DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, Options.Domain, Options.Username, Options.Password);
            Domain objDomain = Domain.GetDomain(objContext);
            string ldapName = string.Format("LDAP://{0}", objDomain.Name);

            int current = folks.Count();
            foreach (IdentityDescriptor id in folks)
            {
                try
                {
                    TeamFoundationIdentity i = ims2.ReadIdentity(IdentitySearchFactor.Identifier, id.Identifier, MembershipQuery.Direct, ReadIdentityOptions.None);
                    if (!(i == null) && i.IsContainer == false)
                    {
                        DirectoryEntry d = new DirectoryEntry(ldapName, Options.Username, Options.Password);
                        DirectorySearcher dssearch = new DirectorySearcher(d)
                        {
                            Filter = string.Format("(sAMAccountName={0})", i.UniqueName.Split(char.Parse(@"\"))[1])
                        };
                        SearchResult sresult = dssearch.FindOne();
                        WebClient webClient = new WebClient
                        {
                            Credentials = CredentialCache.DefaultNetworkCredentials
                        };
                        if (sresult != null)
                        {
                            string newImage = Path.Combine(exportPath, string.Format("{0}.jpg", i.UniqueName.Replace(@"\", "-")));
                            if (!File.Exists(newImage))
                            {
                                DirectoryEntry deUser = new DirectoryEntry(sresult.Path, Options.Username, Options.Password);
                                Log.LogInformation("{0} [PROCESS] {1}: {2}", current, deUser.Name, newImage);
                                string empPic = string.Format(Options.PictureEmpIDFormat, deUser.Properties["employeeNumber"].Value);
                                try
                                {
                                    webClient.DownloadFile(empPic, newImage);
                                }
                                catch (Exception ex)
                                {
                                    Log.LogError(ex, "      [ERROR] {0}", ex.ToString());
                                }
                            }
                            else
                            {
                                Log.LogWarning("{0} [SKIP] Exists {1}", current, newImage);
                            }
                        }
                        webClient.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "      [ERROR] {0}", ex.ToString());
                }

                current--;
            }

            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed}", stopwatch.Elapsed.ToString("c"));
        }
    }
}