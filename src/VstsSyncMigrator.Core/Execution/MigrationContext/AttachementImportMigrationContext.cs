using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project destProject = targetStore.GetProject();

            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name));

            List<string> files = System.IO.Directory.EnumerateFiles(exportPath).ToList<string>();
            WorkItem targetWI = null;
            int current = files.Count;
            int failures = 0;
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                try
                {
                    string reflectedID = fileName.Split('#')[0].Replace('+', ':').Replace("--", "/");
                    targetWI = targetStore.FindReflectedWorkItemByReflectedWorkItemId(reflectedID, me.ReflectedWorkItemIdFieldName);
                    if (targetWI != null)
                    {
                        Trace.WriteLine(string.Format("{0} of {1} - Import {2} to {3}", current, files.Count, fileName, targetWI.Id));
                        Attachment a = new Attachment(file);
                        targetWI.Attachments.Add(a);

                        targetWI.Save();
                        System.IO.File.Delete(file);
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("{0} of {1} - Skipping {2} to {3}", current, files.Count, fileName, 0));
                    }
                } catch (FileAttachmentException ex)
                {
                    // Probably due to attachment being over size limit
                    Trace.WriteLine(ex.Message) ;
                    failures++;
                }
                current--;
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"IMPORT DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds - {1} Files imported, {2} Failures", stopwatch.Elapsed, (files.Count- failures), failures);
        }

    }
}