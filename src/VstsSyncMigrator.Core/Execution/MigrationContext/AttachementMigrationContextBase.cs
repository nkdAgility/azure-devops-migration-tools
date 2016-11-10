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
    public abstract class AttachementMigrationContextBase : MigrationContextBase
    {

        internal string exportPath;

        public AttachementMigrationContextBase(MigrationEngine me, ITfsProcessingConfig config) : base(me, config)
        {
            EnsureExportPath();
        }

        private void EnsureExportPath()
        {
            if (exportPath == null)
            { 
                string assPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                this.exportPath = Path.Combine(Path.GetDirectoryName(assPath), "export");
                Directory.CreateDirectory(exportPath);
            }
        }
    }
}