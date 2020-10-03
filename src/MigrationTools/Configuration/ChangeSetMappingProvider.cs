using System.Collections.Generic;

namespace MigrationTools.Configuration
{
    public class ChangeSetMappingProvider : IChangeSetMappingProvider
    {
        private readonly string _MappingFilePath;
        public ChangeSetMappingProvider(string mappingFilePath)
        {
            _MappingFilePath = mappingFilePath;
        }

        public void ImportMappings(Dictionary<int, string> changesetMappingStore)
        {
            if (!string.IsNullOrWhiteSpace(_MappingFilePath))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(_MappingFilePath))
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
