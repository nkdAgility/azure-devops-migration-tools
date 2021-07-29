using System;
using System.Collections.Generic;

namespace MigrationTools.DataContracts.Pipelines
{
    public class ArtifactPackage : RestApiDefinition
    {
        public AzureFeed Feed { get; set; }
        public bool IsCached { get; set; }
        public string NormalizedName { get; set; }
        public string ProtocolType { get; set; }
        public int StarCount { get; set; }
        public List<PackageVersion> Versions { get; set; }

        public override bool HasTaskGroups()
        {
            return false;
        }

        public override bool HasVariableGroups()
        {
            return false;
        }

        public override void ResetObject()
        {
        }
    }

    public class PackageVersion
    {
        public string Id { get; set; }
        public string NormalizedVersion { get; set; }
        public string Version { get; set; }
        public bool IsLatest { get; set; }
        public bool IsListed { get; set; }
        public string StorageId { get; set; }
        public List<View> Views { get; set; }
        public DateTime PublishDate { get; set; }
        public string Href { get; set; }
    }

    public class View
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
    }
}