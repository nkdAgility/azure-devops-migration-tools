using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Tools.Infra;

namespace MigrationTools.Tools
{
    public class TfsChangeSetMappingTool : Tool<TfsChangeSetMappingToolOptions>
    {

        private Dictionary<int, string> _ChangeSetMappings = new Dictionary<int, string>();
        public ReadOnlyDictionary<int, string> Items { get { return new ReadOnlyDictionary<int, string>(_ChangeSetMappings); } }
        public int Count { get { return _ChangeSetMappings.Count; } }

        public TfsChangeSetMappingTool(IOptions<TfsChangeSetMappingToolOptions> options, IServiceProvider services, ILogger<TfsChangeSetMappingTool> logger, ITelemetryLogger telemetry) : base(options, services, logger, telemetry)
        {
            if (Options.ChangeSetMappingFile != null)
            {
                if (System.IO.File.Exists(Options.ChangeSetMappingFile))
                {
                    ImportMappings(_ChangeSetMappings);
                }
            }
        }

        public void ImportMappings(Dictionary<int, string> changesetMappingStore)
        {
            if (!string.IsNullOrWhiteSpace(Options.ChangeSetMappingFile))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Options.ChangeSetMappingFile))
                {
                    string line = string.Empty;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        var split = line.Split('-');
                        if (split == null
                            || split.Length != 2
                            || !int.TryParse(split[0], out int changesetId))
                        {
                            continue;
                        }

                        changesetMappingStore.Add(changesetId, split[1]);
                    }
                }
            }
        }

    }
}
