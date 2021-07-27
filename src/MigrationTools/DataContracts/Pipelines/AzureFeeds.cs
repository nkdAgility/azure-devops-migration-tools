using System.Collections.Generic;

namespace MigrationTools.DataContracts.Pipelines
{
    public partial class AzureFeed : RestApiDefinition
    {
        public object Description { get; set; }
        public string Url { get; set; }
        public Links Links { get; set; }
        public bool HideDeletedPackageVersions { get; set; }
        public string DefaultViewId { get; set; }
        public object ViewId { get; set; }
        public object ViewName { get; set; }
        public string FullyQualifiedName { get; set; }
        public string FullyQualifiedId { get; set; }
        public List<UpstreamSource> UpstreamSources { get; set; }
        public string Capabilities { get; set; }
        public bool? UpstreamEnabled { get; set; }

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
            Links = null;
            Url = null;
        }
    }

    public partial class Packages
    {
        public string Href { get; set; }
    }

    public partial class Permissions
    {
        public string Href { get; set; }
    }

    public partial class Links
    {
        public Self Self { get; set; }
        public Packages Packages { get; set; }
        public Permissions Permissions { get; set; }
    }

    public partial class UpstreamSource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Protocol { get; set; }
        public string Location { get; set; }
        public string UpstreamSourceType { get; set; }
    }
}