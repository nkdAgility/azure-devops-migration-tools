using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class ExportUsersForMappingConfig : IProcessorConfig
    {
        public string LocalExportJsonFile { get; set; }
        public bool Enabled { get; set; }
        public string WIQLQuery { get; set; }
        public List<string> IdentityFieldsToCheck { get; set; }

        public string Processor
        {
            get { return "ExportUsersForMapping"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}