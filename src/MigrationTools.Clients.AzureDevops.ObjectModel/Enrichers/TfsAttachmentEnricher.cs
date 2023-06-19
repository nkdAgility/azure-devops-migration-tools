using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Enrichers
{
    public class TfsAttachmentEnricher : IAttachmentMigrationEnricher
    {
        private WorkItemServer _server;
        private string _exportBasePath;
        private string _exportWiPath;
        private int _maxAttachmentSize;

        public TfsAttachmentEnricher(WorkItemServer workItemServer, string exportBasePath, int maxAttachmentSize = 480000000)
        {
            _server = workItemServer;
            _exportBasePath = exportBasePath;
            _maxAttachmentSize = maxAttachmentSize;
        }

        public void ProcessAttachemnts(WorkItemData source, WorkItemData target, bool save = true)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Log.Information("AttachmentMigrationEnricher: Migrating  {AttachmentCount} attachemnts from {SourceWorkItemID} to {TargetWorkItemID}", source.ToWorkItem().Attachments.Count, source.Id, target.Id);
            _exportWiPath = Path.Combine(_exportBasePath, source.ToWorkItem().Id.ToString());
            if (System.IO.Directory.Exists(_exportWiPath))
            {
                System.IO.Directory.Delete(_exportWiPath, true);
            }
            System.IO.Directory.CreateDirectory(_exportWiPath);


            int count = 0;
            foreach (Attachment wia in source.ToWorkItem().Attachments) // TODO#1 Limit to 100 attachements
            {
                count++;
                if (count > 100)
                {
                    break; 
                }

                try
                {
                    string filepath = null;
                    System.IO.Directory.CreateDirectory(Path.Combine(_exportWiPath, wia.Id.ToString()));
                    filepath = ExportAttachment(source.ToWorkItem(), wia, _exportWiPath);
                    Log.Debug("AttachmentMigrationEnricher: Exported {Filename} to disk", System.IO.Path.GetFileName(filepath));
                    if (filepath != null)
                    {
                        ImportAttachment(target.ToWorkItem(), wia, filepath, save);
                        Log.Debug("AttachmentMigrationEnricher: Imported {Filename} from disk", System.IO.Path.GetFileName(filepath));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "AttachmentMigrationEnricher:Unable to process atachment from source wi {SourceWorkItemId} called {AttachmentName}", source.ToWorkItem().Id, wia.Name);
                }
            }
            if (save)
            {
                target.SaveToAzureDevOps();
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
                    Log.Warning(" ERROR: Unable to delete folder {0}", _exportWiPath);
                }
            }
        }

        private string ExportAttachment(WorkItem wi, Attachment wia, string exportpath)
        {
            string fname = GetSafeFilename(wia.Name);
            Log.Debug(fname);
            
            string fpath = Path.Combine(exportpath, wia.Id.ToString(), fname);
            
            if (!File.Exists(fpath))
            {
                Log.Debug(string.Format("...downloading {0} to {1}", fname, exportpath));
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
                Log.Debug("...already downloaded");
            }
            return fpath;
        }

        private void ImportAttachment(WorkItem targetWorkItem, Attachment wia, string filepath, bool save = true)
        {
            var filename = System.IO.Path.GetFileName(filepath);
            FileInfo fi = new FileInfo(filepath);
            if (_maxAttachmentSize > fi.Length)
            {
                string originalId = "[originalId:" + wia.Id + "]";
                var attachments = targetWorkItem.Attachments.Cast<Attachment>();
                var attachment = attachments.Where(a => a.Name == wia.Name && a.Length == wia.Length && a.Comment.Contains(originalId)).FirstOrDefault();
                if (attachment == null)
                {
                    Attachment a = new Attachment(filepath);
                    a.Comment = originalId;
                    if (wia.Comment != "")
                    {
                        string originalComment = wia.Comment;
                        string regexPatternOriginalId = @"(\[originalId:\d+\])";
                        MatchCollection matches = Regex.Matches(originalComment, regexPatternOriginalId);
                        foreach (Match match in matches)
                        {
                            originalComment = originalComment.Replace(match.Value, "");
                        }
                        originalComment = originalComment.Trim();
                        a.Comment = originalComment + " " + originalId;
                    }
                    targetWorkItem.Attachments.Add(a);
                }
                else
                {
                    Log.Debug(" [SKIP] WorkItem {0} already contains attachment {1}", targetWorkItem.Id, filepath);
                }
            }
            else
            {
                Log.Warning(" [SKIP] Attachment {filename} on Work Item {targetWorkItemId} is bigger than the limit of {maxAttachmentSize} bites for Azure DevOps.", filename, targetWorkItem.Id, _maxAttachmentSize);
            }
        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}