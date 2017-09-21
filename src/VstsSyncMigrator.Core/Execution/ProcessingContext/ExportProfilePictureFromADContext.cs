using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Net;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class ExportProfilePictureFromADContext : ProcessingContextBase
    {

        //private readonly TfsTeamService teamService;
        //private readonly ProjectInfo projectInfo;
        private readonly IIdentityManagementService2 ims2;
        ExportProfilePictureFromADConfig config;

        public override string Name
        {
            get
            {
                return "ExportProfilePictureFromADContext";
            }
        }

        public ExportProfilePictureFromADContext(MigrationEngine me, ExportProfilePictureFromADConfig config) : base(me, config)
        {
            //http://www.codeproject.com/Articles/18102/Howto-Almost-Everything-In-Active-Directory-via-C
            ims2 = (IIdentityManagementService2)me.Target.Collection.GetService(typeof(IIdentityManagementService2));
            this.config = config;
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            string exportPath;
            string assPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "export-pic");
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }


            TeamFoundationIdentity SIDS = ims2.ReadIdentity(IdentitySearchFactor.AccountName, "Team Foundation Valid Users", MembershipQuery.Expanded, ReadIdentityOptions.None);

            Trace.WriteLine(string.Format("Found {0}", SIDS.Members.Count()));
            var itypes = (from IdentityDescriptor id in SIDS.Members select id.IdentityType).Distinct();

            foreach (string item in itypes)
            {
                var infolks = (from IdentityDescriptor id in SIDS.Members where id.IdentityType == item select id);
                Trace.WriteLine(string.Format("Found {0} of {1}", infolks.Count(), item));
            }
            var folks = (from IdentityDescriptor id in SIDS.Members where id.IdentityType == "System.Security.Principal.WindowsIdentity" select id);

            DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, config.Domain, config.Username, config.Password);
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
                        DirectoryEntry d = new DirectoryEntry(ldapName, config.Username, config.Password);
                        DirectorySearcher dssearch = new DirectorySearcher(d);
                        dssearch.Filter = string.Format("(sAMAccountName={0})", i.UniqueName.Split(char.Parse(@"\"))[1]);
                        SearchResult sresult = dssearch.FindOne();
                        WebClient webClient = new WebClient();
                        webClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                        if (sresult != null)
                        {
                            string newImage = Path.Combine(exportPath, string.Format("{0}.jpg", i.UniqueName.Replace(@"\", "-")));
                            if (!File.Exists(newImage))
                            {
                                DirectoryEntry deUser = new DirectoryEntry(sresult.Path, config.Username, config.Password);
                                Trace.WriteLine(string.Format("{0} [PROCESS] {1}: {2}", current, deUser.Name, newImage));
                                string empPic = string.Format(config.PictureEmpIDFormat, deUser.Properties["employeeNumber"].Value);
                                try
                                {

                                    webClient.DownloadFile(empPic, newImage);
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine(string.Format("      [ERROR] {0}", ex.ToString()));

                                }
                            }
                            else
                            {
                                Trace.WriteLine(string.Format("{0} [SKIP] Exists {1}", current, newImage));
                            }
                        }
                        webClient.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("      [ERROR] {0}", ex.ToString()));
                }

                current--;
            }



            //////////////////////////////////////////////////
            stopwatch.Stop();
            Trace.WriteLine(string.Format(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed));
        }
    }
}