using System;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.DataContracts.Pipelines
{
    [ApiPath("distributedtask/taskgroups")]
    [ApiName("Task Groups")]
    public partial class TaskGroup : RestApiDefinition
    {
        public TaskElement[] Tasks { get; set; }

        public string[] RunsOn { get; set; }

        public long Revision { get; set; }

        public EdBy CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public EdBy ModifiedBy { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }

        public Version Version { get; set; }

        public string IconUrl { get; set; }

        public string FriendlyName { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string DefinitionType { get; set; }

        public string Author { get; set; }

        public object[] Demands { get; set; }

        public object[] Groups { get; set; }

        public Input[] Inputs { get; set; }

        public object[] Satisfies { get; set; }

        public object[] SourceDefinitions { get; set; }

        public object[] DataSourceBindings { get; set; }

        public string InstanceNameFormat { get; set; }

        public Execution PreJobExecution { get; set; }

        public Execution Execution { get; set; }

        public Execution PostJobExecution { get; set; }

        public string Comment { get; set; }

        public string[] Visibility { get; set; }

        public string? Owner { get; set; }

        public bool? ContentsUploaded { get; set; }

        public string PackageType { get; set; }

        public string PackageLocation { get; set; }

        public string SourceLocation { get; set; }

        public string MinimumAgentVersion { get; set; }

        public string HelpMarkDown { get; set; }

        public override bool HasTaskGroups()
        {
            throw new NotImplementedException("we currently not support taskgroup nesting.");
        }

        public override RestApiDefinition ResetObject()
        {
            return this;
        }
    }

    public partial class EdBy
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }

        public string UniqueName { get; set; }
    }

    public partial class Execution
    {
    }

    public partial class Input
    {
        public object[] Aliases { get; set; }

        public Execution Options { get; set; }

        public Properties Properties { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public string DefaultValue { get; set; }

        public bool InputRequired { get; set; }

        public string Type { get; set; }

        public string HelpMarkDown { get; set; }

        public string GroupName { get; set; }
    }

    public partial class Properties
    {
        public string EditableOptions { get; set; }
    }

    public partial class TaskElement
    {
        public Execution Environment { get; set; }

        public string DisplayName { get; set; }

        public bool AlwaysRun { get; set; }

        public bool ContinueOnError { get; set; }

        public string? Condition { get; set; }

        public bool Enabled { get; set; }

        public long TimeoutInMinutes { get; set; }

        public ExpandoObject Inputs { get; set; }

        public TaskTask Task { get; set; }
    }

    public partial class TaskTask
    {
        public string Id { get; set; }

        public string VersionSpec { get; set; }

        public string DefinitionType { get; set; }
    }

    public partial class Version
    {
        public long Major { get; set; }

        public long Minor { get; set; }

        public long Patch { get; set; }

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