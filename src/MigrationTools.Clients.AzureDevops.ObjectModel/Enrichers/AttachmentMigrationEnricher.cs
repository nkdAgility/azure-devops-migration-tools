using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers
{
    public class AttachmentMigrationEnricher : IAttachmentMigrationEnricher
    {
        private WorkItemServer _server;
        private string _exportBasePath;
        private string _exportWiPath;
        private int _maxAttachmentSize;

        public AttachmentMigrationEnricher(WorkItemServer workItemServer, string exportBasePath, int maxAttachmentSize = 480000000)
        {
            _server = workItemServer;
            _exportBasePath = exportBasePath;
            _maxAttachmentSize = maxAttachmentSize;
        }

        public void ProcessAttachemnts(WorkItemData source, WorkItemData target, bool save = true)
        {

            _exportWiPath = Path.Combine(_exportBasePath, source.ToWorkItem().Id.ToString());
            if (System.IO.Directory.Exists(_exportWiPath))
            {
                System.IO.Directory.Delete(_exportWiPath, true);
            }
            System.IO.Directory.CreateDirectory(_exportWiPath);
            foreach (Attachment wia in source.ToWorkItem().Attachments)
            {
                try
                {
                    string filepath = null;
                    filepath = ExportAttachment(source.ToWorkItem(), wia, _exportWiPath);
                    Log.Information("Exported {Filename} to disk", System.IO.Path.GetFileName(filepath));
                    if (filepath != null)
                    {
                        ImportAttachemnt(target.ToWorkItem(), filepath, save);
                        Log.Information("Imported {Filename} from disk", System.IO.Path.GetFileName(filepath));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR: Unable to process atachment from source wi {SourceWorkItemId} called {AttachmentName}", source.ToWorkItem().Id, wia.Name, ex);
                }

            }
            if (save)
            {
                this.SaveMigratedWorkItem(target);
                Log.Information("Work iTem now has {AttachmentCount} attachemnts", source.ToWorkItem().Attachments.Count);
                CleanUpAfterSave();
            }

        }

        public void CleanUpAfterSave()
        {
            if (_exportWiPath != null && System.IO.Directory.Exists(_exportWiPath))
            {
                try
                {
                    System.IO.Directory.Delete(_exportWiPath, true);
                    _exportWiPath = null;
                }
                catch (Exception)
                {
                    Trace.WriteLine(string.Format(" ERROR: Unable to delete folder {0}", _exportWiPath));
                }
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
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception downloading attachements");
                    return null;
                }

            }
            else
            {
                Log.Information("...already downloaded");
            }
            return fpath;
        }

        private void ImportAttachemnt(WorkItem targetWorkItem, string filepath, bool save = true)
        {
            var filename = System.IO.Path.GetFileName(filepath);
            FileInfo fi = new FileInfo(filepath);
            if (_maxAttachmentSize > fi.Length)
            {
                var attachments = targetWorkItem.Attachments.Cast<Attachment>();
                var attachment = attachments.Where(a => a.Name == filename).FirstOrDefault();
                if (attachment == null)
                {
                    Attachment a = new Attachment(filepath);
                    targetWorkItem.Attachments.Add(a);
                }
                else
                {
                    Log.Information(" [SKIP] WorkItem {0} already contains attachment {1}", targetWorkItem.Id, filepath);
                }
            }
            else
            {
                Log.Information(" [SKIP] Attachemnt {filename} on Work Item {targetWorkItemId} is bigger than the limit of {maxAttachmentSize} bites for Azure DevOps.", filename, targetWorkItem.Id, _maxAttachmentSize);

            }


        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
