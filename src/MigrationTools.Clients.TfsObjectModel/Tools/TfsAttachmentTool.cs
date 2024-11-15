using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;
using Serilog;

namespace MigrationTools.Tools
{
    public class TfsAttachmentTool : Tool<TfsAttachmentToolOptions>
    {
        private string _exportWiPath;
        private WorkItemServer _workItemServer;


        public TfsAttachmentTool(IOptions<TfsAttachmentToolOptions> options, IServiceProvider services, ILogger<TfsAttachmentTool> logger, ITelemetryLogger telemetryLogger) : base(options,services, logger, telemetryLogger)
        {

        }

        public void ProcessAttachemnts(TfsProcessor processer, WorkItemData source, WorkItemData target, bool save = true)
        {
            SetupWorkItemServer(processer);
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            Log.LogInformation("AttachmentMigrationEnricher: Migrating  {AttachmentCount} attachemnts from {SourceWorkItemID} to {TargetWorkItemID}", source.ToWorkItem().Attachments.Count, source.Id, target.Id);
            _exportWiPath = Path.Combine(Options.ExportBasePath, source.ToWorkItem().Id.ToString());
            if (Directory.Exists(_exportWiPath))
            {
                Directory.Delete(_exportWiPath, true);
            }
            Directory.CreateDirectory(_exportWiPath);


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
                    Directory.CreateDirectory(Path.Combine(_exportWiPath, wia.Id.ToString()));
                    filepath = ExportAttachment(source.ToWorkItem(), wia, _exportWiPath);
                    Log.LogDebug("AttachmentMigrationEnricher: Exported {Filename} to disk", Path.GetFileName(filepath));
                    if (filepath != null)
                    {
                        ImportAttachment(target.ToWorkItem(), wia, filepath, save);
                        Log.LogDebug("AttachmentMigrationEnricher: Imported {Filename} from disk", Path.GetFileName(filepath));
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "AttachmentMigrationEnricher:Unable to process atachment from source wi {SourceWorkItemId} called {AttachmentName}", source.ToWorkItem().Id, wia.Name);
                    Telemetry.TrackException(ex, null);
                }
            }
            if (save)
            {
                target.SaveToAzureDevOps();
                Log.LogInformation("Work iTem now has {AttachmentCount} attachemnts", source.ToWorkItem().Attachments.Count);
                CleanUpAfterSave();
            }
        }

        public void CleanUpAfterSave()
        {
            if (_exportWiPath != null && Directory.Exists(_exportWiPath))
            {
                try
                {
                    Directory.Delete(_exportWiPath, true);
                    _exportWiPath = null;
                }
                catch (Exception)
                {
                    Log.LogWarning(" ERROR: Unable to delete folder {0}! Should be cleaned up at the end.", _exportWiPath);
                }
            }
        }

        private string ExportAttachment(WorkItem wi, Attachment wia, string exportpath)
        {
            string fname = GetSafeFilename(wia.Name);
            Log.LogDebug(fname);

            string fpath = Path.Combine(exportpath, wia.Id.ToString(), fname);

            if (!File.Exists(fpath))
            {
                Log.LogDebug(string.Format("...downloading {0} to {1}", fname, exportpath));
                try
                {
                    var fileLocation = _workItemServer.DownloadFile(wia.Id);
                    File.Copy(fileLocation, fpath, true);
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "Exception downloading attachements");
                    Telemetry.TrackException(ex, null);
                    return null;
                }
            }
            else
            {
                Log.LogDebug("...already downloaded");
            }
            return fpath;
        }

        private void ImportAttachment(WorkItem targetWorkItem, Attachment wia, string filepath, bool save = true)
        {
            var filename = Path.GetFileName(filepath);
            FileInfo fi = new FileInfo(filepath);
            if (Options.MaxAttachmentSize > fi.Length)
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
                    Log.LogDebug(" [SKIP] WorkItem {0} already contains attachment {1}", targetWorkItem.Id, filepath);
                }
            }
            else
            {
                Log.LogWarning(" [SKIP] Attachment {filename} on Work Item {targetWorkItemId} is bigger than the limit of {maxAttachmentSize} bites for Azure DevOps.", filename, targetWorkItem.Id, Options.MaxAttachmentSize);
            }
        }

        public string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private void SetupWorkItemServer(TfsProcessor processer)
        {
            if (_workItemServer == null)
            {
                IMigrationEngine engine = Services.GetRequiredService<IMigrationEngine>();
                _workItemServer = processer.Source.GetService<WorkItemServer>();
            }
        }

    }
}
