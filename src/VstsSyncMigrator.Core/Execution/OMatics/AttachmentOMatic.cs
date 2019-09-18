using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core.Execution.OMatics
{
    public class AttachmentOMatic
    {
        private WorkItemServer _server;
        private string _exportBasePath;

        public AttachmentOMatic(WorkItemServer workItemServer,string exportBasePath)
        {
            _server = workItemServer;
            _exportBasePath = exportBasePath;
        }

        public void ProcessAttachemnts(WorkItem sourceWorkItem, WorkItem targetWorkItem)
        {
            string exportpath = Path.Combine(_exportBasePath, sourceWorkItem.Id.ToString());
            foreach (Attachment wia in sourceWorkItem.Attachments)
            {
                string filepath = null;
                filepath = ExportAttachment(sourceWorkItem, wia, exportpath);
                if (filepath != null)
                { 
                ImportAttachemnt(targetWorkItem, filepath);
                } else
                {

                }
                Trace.WriteLine("...done");
            }
        }

        private string ExportAttachment(WorkItem wi, Attachment wia, string exportpath)
        {
            string fname = GetSafeFilename(wia.Name);
            Trace.Write("-");
            Trace.Write(fname);

            string fpath = Path.Combine(exportpath, fname);
            if (!File.Exists(fpath))
            {
                Trace.Write(string.Format("...downloading {0} to {1}", fname, exportpath));
                try
                {
                    var fileLocation = _server.DownloadFile(wia.Id);
                    File.Copy(fileLocation, fpath, true);
                    Trace.Write("...done");
                }
                catch (Exception ex)
                {
                    Telemetry.Current.TrackException(ex);
                    Trace.Write($"\r\nException downloading attachements {ex.Message}");
                    return null;
                }

            }
            else
            {
                Trace.Write("...already downloaded");
            }
            return fpath;
        }

        private void  ImportAttachemnt( WorkItem targetWorkItem,string filepath)
        {
                var attachments = targetWorkItem.Attachments.Cast<Attachment>();
                var attachment = attachments.Where(a => a.Name == filepath).FirstOrDefault();
                if (attachment == null)
                {
                    Attachment a = new Attachment(filepath);
                    targetWorkItem.Attachments.Add(a);
                    targetWorkItem.Save();
                }
                else
                {
                    Trace.WriteLine(string.Format(" [SKIP] WorkItem {0} already contains attachment {1}", targetWorkItem.Id, filepath));
                }
            File.Delete(filepath);
        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
