using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsChangeSetMappingTool : Tool<TfsChangeSetMappingToolOptions>
    {

        private Dictionary<int, string> _ChangeSetMappings = new Dictionary<int, string>();
        public ReadOnlyDictionary<int, string> Items { get { return new ReadOnlyDictionary<int, string>(_ChangeSetMappings); } }
        public int Count { get { return _ChangeSetMappings.Count; } }

        public TfsChangeSetMappingTool(IOptions<TfsChangeSetMappingToolOptions> options, IServiceProvider services, ILogger<TfsChangeSetMappingTool> logger, ITelemetryLogger telemetry) : base(options, services, logger, telemetry)
        {
            ApplyLegacyFileSetting();
            if (Options.ChangeSetMappingFile != null)
            {
                if (System.IO.File.Exists(Options.ChangeSetMappingFile))
                {
                    ImportMappings(_ChangeSetMappings);
                }
                else
                {
                    Log.LogWarning("TfsChangeSetMappingTool: ChangeSetMappingFile '{ChangeSetMappingFile}' was not found, so no changeset mappings were loaded.", Options.ChangeSetMappingFile);
                }
            }
        }

        /// <summary>
        /// Older samples used "File" rather than "ChangeSetMappingFile". Honour it, with a warning, so those
        /// configurations are not silently ignored.
        /// </summary>
        private void ApplyLegacyFileSetting()
        {
            if (!string.IsNullOrWhiteSpace(Options.ChangeSetMappingFile))
            {
                return;
            }
            var configuration = Services?.GetService(typeof(IConfiguration)) as IConfiguration;
            var legacyFile = configuration?.GetValue<string>($"{Options.ConfigurationMetadata.PathToInstance}:File");
            if (string.IsNullOrWhiteSpace(legacyFile))
            {
                return;
            }
            Log.LogWarning("TfsChangeSetMappingTool: 'File' is not a supported setting; use 'ChangeSetMappingFile' instead. Using '{ChangeSetMappingFile}' for this run. Run the 'upgrade' command to update your configuration.", legacyFile);
            Options.ChangeSetMappingFile = legacyFile;
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
