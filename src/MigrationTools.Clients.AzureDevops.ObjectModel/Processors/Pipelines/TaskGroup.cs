using System;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Enrichers.Pipelines
{
    public partial class TaskGroup
    {
        [JsonProperty("tasks")]
        public TaskElement[] Tasks { get; set; }

        [JsonProperty("runsOn")]
        public string[] RunsOn { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("createdBy")]
        public EdBy CreatedBy { get; set; }

        [JsonProperty("createdOn")]
        public DateTimeOffset CreatedOn { get; set; }

        [JsonProperty("modifiedBy")]
        public EdBy ModifiedBy { get; set; }

        [JsonProperty("modifiedOn")]
        public DateTimeOffset ModifiedOn { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public Version Version { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("definitionType")]
        public string DefinitionType { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("demands")]
        public object[] Demands { get; set; }

        [JsonProperty("groups")]
        public object[] Groups { get; set; }

        [JsonProperty("inputs")]
        public Input[] Inputs { get; set; }

        [JsonProperty("satisfies")]
        public object[] Satisfies { get; set; }

        [JsonProperty("sourceDefinitions")]
        public object[] SourceDefinitions { get; set; }

        [JsonProperty("dataSourceBindings")]
        public object[] DataSourceBindings { get; set; }

        [JsonProperty("instanceNameFormat")]
        public string InstanceNameFormat { get; set; }

        [JsonProperty("preJobExecution")]
        public Execution PreJobExecution { get; set; }

        [JsonProperty("execution")]
        public Execution Execution { get; set; }

        [JsonProperty("postJobExecution")]
        public Execution PostJobExecution { get; set; }

        [JsonProperty("comment", NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Visibility { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public string? Owner { get; set; }

        [JsonProperty("contentsUploaded", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ContentsUploaded { get; set; }

        [JsonProperty("packageType", NullValueHandling = NullValueHandling.Ignore)]
        public string PackageType { get; set; }

        [JsonProperty("packageLocation", NullValueHandling = NullValueHandling.Ignore)]
        public string PackageLocation { get; set; }

        [JsonProperty("sourceLocation", NullValueHandling = NullValueHandling.Ignore)]
        public string SourceLocation { get; set; }

        [JsonProperty("minimumAgentVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string MinimumAgentVersion { get; set; }

        [JsonProperty("helpMarkDown", NullValueHandling = NullValueHandling.Ignore)]
        public string HelpMarkDown { get; set; }
    }

    public partial class EdBy
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }
    }

    public partial class Execution
    {
    }

    public partial class Input
    {
        [JsonProperty("aliases", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Aliases { get; set; }

        [JsonProperty("options")]
        public Execution Options { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty("required")]
        public bool InputRequired { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("helpMarkDown")]
        public string HelpMarkDown { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("EditableOptions", NullValueHandling = NullValueHandling.Ignore)]
        public string EditableOptions { get; set; }
    }

    public partial class TaskElement
    {
        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public Execution Environment { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("alwaysRun")]
        public bool AlwaysRun { get; set; }

        [JsonProperty("continueOnError")]
        public bool ContinueOnError { get; set; }

        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public string? Condition { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("timeoutInMinutes")]
        public long TimeoutInMinutes { get; set; }

        [JsonProperty("inputs")]
        public ExpandoObject Inputs { get; set; }

        [JsonProperty("task")]
        public TaskTask Task { get; set; }
    }

    public partial class TaskTask
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("versionSpec")]
        public string VersionSpec { get; set; }

        [JsonProperty("definitionType")]
        public string DefinitionType { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("major")]
        public long Major { get; set; }

        [JsonProperty("minor")]
        public long Minor { get; set; }

        [JsonProperty("patch")]
        public long Patch { get; set; }

        [JsonProperty("isTest")]
        public bool IsTest { get; set; }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
