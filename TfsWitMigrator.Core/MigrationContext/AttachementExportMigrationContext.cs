using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace TfsWitMigrator.Core
{
    public class AttachementExportMigrationContext : AttachementMigrationContextBase
    {
        public override string Name
        {
            get
            {
                return "AttachementExportMigrationContext";
            }
        }
        public AttachementExportMigrationContext(MigrationEngine me) : base(me)
        {
     
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
          


            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Name);
            tfsqc.Query = @"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject AND [System.AttachedFileCount] > 0 AND  [Microsoft.VSTS.Common.ClosedDate] = '' ORDER BY [System.ChangedDate] desc ";
            WorkItemCollection sourceWIS = tfsqc.Execute();

            WebClient webClient = new WebClient();
            webClient.Credentials = CredentialCache.DefaultNetworkCredentials;
            int current = sourceWIS.Count;
            foreach (WorkItem wi in sourceWIS)
            {
                Trace.Write(string.Format("Attachement Export: {0} of {1} - {2}", current, sourceWIS.Count, wi.Id));
                foreach (Attachment wia in wi.Attachments)
                {
                    string reflectedId = sourceStore.CreateReflectedWorkItemId(wi);
                    string fname = string.Format("{0}#{1}", reflectedId.Replace("/", "-").Replace(":", "+"), wia.Name);
                    Trace.Write("-");
                    Trace.Write(fname);
                    string fpath = Path.Combine(exportPath, fname);
                    if (!File.Exists(fpath))
                    {
                        Trace.Write("...downloading");
                        try
                        {
                            webClient.DownloadFile(wia.Uri, fpath);
                            Trace.Write("...done");
                        }
                        catch (Exception ex)
                        {

                            Trace.Write("...failed");
                        }
                     
                    }
                    else
                    {
                        Trace.Write("...skipping");
                    }
                    Trace.WriteLine("...done");
                }
                current--;
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"EXPORT DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

    }
}