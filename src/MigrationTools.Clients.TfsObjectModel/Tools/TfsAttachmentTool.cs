using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Tool for processing and migrating work item attachments between Team Foundation Server instances, handling file downloads, uploads, and attachment metadata.
    /// </summary>
    public class TfsAttachmentTool : Tool<TfsAttachmentToolOptions>
    {
        private string _exportWiPath;
        private WorkItemServer _workItemServer;


        /// <summary>
        /// Initializes a new instance of the TfsAttachmentTool class.
        /// </summary>
        /// <param name="options">Configuration options for the attachment tool</param>
        /// <param name="services">Service provider for dependency injection</param>
        /// <param name="logger">Logger for the tool operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public TfsAttachmentTool(
            IOptions<TfsAttachmentToolOptions> options,
            IServiceProvider services,
            ILogger<TfsAttachmentTool> logger,
            ITelemetryLogger telemetryLogger)
            : base(options, services, logger, telemetryLogger)
        {
        }

        /// <summary>
        /// Processes and migrates attachments from a source work item to a target work item.
        /// </summary>
        /// <param name="processer">The TFS processor performing the migration</param>
        /// <param name="source">The source work item containing attachments to migrate</param>
        /// <param name="target">The target work item to receive the attachments</param>
        /// <param name="save">Whether to save the target work item after processing attachments</param>
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
                    filepath = ExportAttachment(wia, _exportWiPath);
                    Log.LogDebug("AttachmentMigrationEnricher: Exported {Filename} to disk", Path.GetFileName(filepath));
                    if (filepath != null)
                    {
                        ImportAttachment(target.ToWorkItem(), wia, filepath);
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

        private string ExportAttachment(Attachment wia, string exportpath)
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

        private void ImportAttachment(WorkItem targetWorkItem, Attachment wia, string filepath)
        {
            const int MaxCommentLength = 255;

            var filename = Path.GetFileName(filepath);
            FileInfo fi = new FileInfo(filepath);
            if (Options.MaxAttachmentSize > fi.Length)
            {
                string originalId = "[originalId:" + wia.Id + "]";
                var attachments = targetWorkItem.Attachments.Cast<Attachment>();
                var attachment = attachments.Where(a => a.Name == wia.Name && a.Length == wia.Length && a.Comment.Contains(originalId)).FirstOrDefault();
                if (attachment is null)
                {
                    attachment = attachments.Where(a => a.Name.Equals(wia.Name, StringComparison.OrdinalIgnoreCase)
                        && a.Comment.Equals(wia.Comment, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();
                }
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
                        string newComment = originalComment + " " + originalId;
                        a.Comment = newComment.Length > MaxCommentLength ? originalComment : newComment;
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
