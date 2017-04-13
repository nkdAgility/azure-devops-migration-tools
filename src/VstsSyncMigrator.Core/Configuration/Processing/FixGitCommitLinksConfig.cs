using System;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class FixGitCommitLinksConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }

        public string TargetRepository { get; set; }

        public Type Processor
        {
            get
            {
                return typeof(FixGitCommitLinks);
            }
        }

    }
    }

