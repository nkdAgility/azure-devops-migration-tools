using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    [Obsolete()]
    public class AttachementExportMigrationContext : AttachementMigrationContextBase
    {

        AttachementExportMigrationConfig _config;

        public override string Name
        {
            get
            {
                return "AttachementExportMigrationContext";
            }
        }
        public AttachementExportMigrationContext(MigrationEngine me, AttachementExportMigrationConfig config) : base(me, config)
        {
            this._config = config;
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////



			WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Config.Name);
            tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", _config.QueryBit);
            WorkItemCollection sourceWIS = tfsqc.Execute();

            int current = sourceWIS.Count;
            var workItemServer = me.Source.Collection.GetService<WorkItemServer>();

            var invalidFileNameChars = Path.GetInvalidFileNameChars();

            foreach (WorkItem wi in sourceWIS)
            {
                WorkItemAttachmentExport( workItemServer, wi);
                current++;
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"EXPORT DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

        private void WorkItemAttachmentExport(WorkItemServer workItemServer, WorkItem wi)
        {
            foreach (Attachment wia in wi.Attachments)
            {
                string fname = string.Format("{0}#{1}", wi.Id, wia.Name);
                fname = GetSafeFilename(fname);

                Trace.Write("-");
                Trace.Write(fname);

                string fpath = Path.Combine(exportPath, fname);
                if (!System.IO.File.Exists(fpath))
                {
                    Trace.Write("...downloading");
                    try
                    {
                        var fileLocation = workItemServer.DownloadFile(wia.Id);
                        File.Copy(fileLocation, fpath, true);
                        Trace.Write("...done");
                    }
                    catch (Exception ex)
                    {
                        Telemetry.Current.TrackException(ex);
                        Trace.Write($"\r\nException downloading attachements {ex.Message}");
                    }

                }
                else
                {
                    Trace.Write("...skipping");
                }
                Trace.WriteLine("...done");
            }
        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}