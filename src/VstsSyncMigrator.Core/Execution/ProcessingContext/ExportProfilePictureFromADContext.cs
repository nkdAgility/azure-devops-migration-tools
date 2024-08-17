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
using VstsSyncMigrator._EngineV1.Processors;
using VstsSyncMigrator.Core.Execution;
using Microsoft.Extensions.Options;
using MigrationTools.Tools;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// Downloads corporate images and updates TFS/Azure DevOps profiles
    /// </summary>
    /// <status>alpha</status>
    /// <processingtarget>Profiles</processingtarget>
    public class ExportProfilePictureFromADContext : TfsStaticProcessorBase
    {
        private IIdentityManagementService2 ims2;
        private ExportProfilePictureFromADConfig _config;

        public ExportProfilePictureFromADContext(IOptions<ExportProfilePictureFromADConfig> options, TfsStaticTools tfsStaticEnrichers, StaticTools staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<StaticProcessorBase> logger) : base(tfsStaticEnrichers, staticEnrichers, services, me, telemetry, logger)
        {
            _config = options.Value;
            //http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C
            ims2 = Engine.Target.GetService<IIdentityManagementService2>();
        }

        public override string Name
        {
            get
            {
                return "ExportProfilePictureFromADContext";
            }
        }

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

            DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, _config.Domain, _config.Username, _config.Password);
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
                        DirectoryEntry d = new DirectoryEntry(ldapName, _config.Username, _config.Password);
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
                                DirectoryEntry deUser = new DirectoryEntry(sresult.Path, _config.Username, _config.Password);
                                Log.LogInformation("{0} [PROCESS] {1}: {2}", current, deUser.Name, newImage);
                                string empPic = string.Format(_config.PictureEmpIDFormat, deUser.Properties["employeeNumber"].Value);
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