using System;

namespace MigrationTools.DataContracts
{
    public class ApiPathAttribute : Attribute
    {
        public ApiPathAttribute(string Path, bool IncludeProject = true, bool IncludeTrailingSlash = true, string ApiVersion = "", bool IncludeIdOnUpdate = true, HttpVerbs UpdateVerb = HttpVerbs.Put)
        {
            this.Path = Path;
            this.IncludeProject = IncludeProject;
            this.IncludeTrailingSlash = IncludeTrailingSlash;
            this.ApiVersion = ApiVersion;
            this.IncludeIdOnUpdate = IncludeIdOnUpdate;
            this.UpdateVerb = UpdateVerb;
        }

        public string Path { get; }
        public bool IncludeProject { get; }
        public bool IncludeTrailingSlash { get; }
        public string ApiVersion { get; }
        public string Expands { get; set; }
        public bool IncludeIdOnUpdate { get; set; }
        public HttpVerbs UpdateVerb { get; set; }
    }
    public enum HttpVerbs
    {
        Get,
        Options,
        Delete,
        Put,
        Patch,
    }

    public class ApiNameAttribute : Attribute
    {
        public ApiNameAttribute(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; }
    }
}