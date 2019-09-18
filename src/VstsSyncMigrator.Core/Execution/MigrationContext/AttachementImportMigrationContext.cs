using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    [Obsolete()]
    public class AttachementImportMigrationContext : AttachementMigrationContextBase
    {
        public override string Name
        {
            get
            {
                return "AttachementImportMigrationContext";
            }
        }
        public AttachementImportMigrationContext(MigrationEngine me, AttachementImportMigrationConfig config) : base(me, config)
        {

        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project destProject = targetStore.GetProject();

            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name));

            List<string> files = System.IO.Directory.EnumerateFiles(exportPath).ToList<string>();
            WorkItem targetWI = null;
            int current = files.Count;
            int failures = 0;
            int skipped = 0;
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                try
                {
                    var fileNameParts = fileName.Split('#');
                    if (fileNameParts.Length != 2
                        || !int.TryParse(fileNameParts[0], out var sourceReflectedID))
                        continue;

                    var targetFileName = fileNameParts[1];
                    var renamedFilePath = Path.Combine(Path.GetDirectoryName(file), targetFileName);
                    if (File.Exists(renamedFilePath))
                        File.Delete(renamedFilePath);

                    File.Move(file, renamedFilePath);
                    targetWI = targetStore.FindReflectedWorkItemByReflectedWorkItemId(sourceReflectedID, true);
                    skipped = ImportAttachemnt(files, targetWI, current, skipped, fileName, targetFileName, renamedFilePath);
                }
                catch (FileAttachmentException ex)
                {
                    // Probably due to attachment being over size limit
                    Trace.WriteLine(ex.Message);
                    failures++;
                }
                current--;
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Trace.WriteLine(string.Format(@"IMPORT DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds - {4} Files, {1} Files imported, {2} Failures, {3} Skipped", stopwatch.Elapsed, (files.Count - failures - skipped), failures, skipped, files.Count));
        }

        private static int ImportAttachemnt(List<string> files, WorkItem targetWI, int current, int skipped, string fileName, string targetFileName, string renamedFilePath)
        {
            if (targetWI != null)
            {
                Trace.WriteLine(string.Format("{0} of {1} - Import {2} to {3}", current, files.Count, fileName, targetWI.Id));
                var attachments = targetWI.Attachments.Cast<Attachment>();
                var attachment = attachments.Where(a => a.Name == targetFileName).FirstOrDefault();
                if (attachment == null)
                {
                    Attachment a = new Attachment(renamedFilePath);
                    targetWI.Attachments.Add(a);
                    targetWI.Save();
                }
                else
                {
                    Trace.WriteLine(string.Format(" [SKIP] WorkItem {0} already contains attachment {1}", targetWI.Id, fileName));
                    skipped++;
                }
            }
            else
            {
                Trace.WriteLine(string.Format("{0} of {1} - Skipping {2} to {3}", current, files.Count, fileName, 0));
                skipped++;
            }
            File.Delete(renamedFilePath);
            return skipped;
        }
    }
}