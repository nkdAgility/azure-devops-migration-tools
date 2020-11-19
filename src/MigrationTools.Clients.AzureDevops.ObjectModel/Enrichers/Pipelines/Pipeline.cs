using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Enrichers.Pipelines
{
    public partial class Pipeline
    {
        [JsonProperty("_links")]
        public PipelineLinks Links { get; set; }

        [JsonProperty("configuration")]
        public Configuration Configuration { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("folder")]
        public string Folder { get; set; }
    }

    public partial class Configuration
    {
        [JsonProperty("designerJson")]
        public DesignerJson DesignerJson { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class DesignerJson
    {
        [JsonProperty("options")]
        public Option[] Options { get; set; }

        [JsonProperty("triggers")]
        public Trigger[] Triggers { get; set; }

        [JsonProperty("variables")]
        public Variables Variables { get; set; }

        [JsonProperty("retentionRules")]
        public RetentionRule[] RetentionRules { get; set; }

        [JsonProperty("properties")]
        public OptionsClass Properties { get; set; }

        [JsonProperty("tags")]
        public object[] Tags { get; set; }

        [JsonProperty("_links")]
        public DesignerJsonLinks Links { get; set; }

        [JsonProperty("buildNumberFormat")]
        public string BuildNumberFormat { get; set; }

        [JsonProperty("jobAuthorizationScope")]
        public string JobAuthorizationScope { get; set; }

        [JsonProperty("jobTimeoutInMinutes")]
        public long JobTimeoutInMinutes { get; set; }

        [JsonProperty("jobCancelTimeoutInMinutes")]
        public long JobCancelTimeoutInMinutes { get; set; }

        [JsonProperty("badgeEnabled")]
        public bool BadgeEnabled { get; set; }

        [JsonProperty("process")]
        public Process Process { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("processParameters")]
        public ProcessParameters ProcessParameters { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("authoredBy")]
        public AuthoredBy AuthoredBy { get; set; }

        [JsonProperty("drafts")]
        public object[] Drafts { get; set; }

        [JsonProperty("queue")]
        public Queue Queue { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("queueStatus")]
        public string QueueStatus { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }
    }

    public partial class AuthoredBy
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public AuthoredByLinks Links { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }
    }

    public partial class AuthoredByLinks
    {
        [JsonProperty("avatar")]
        public Self Avatar { get; set; }
    }

    public partial class Self
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public partial class DesignerJsonLinks
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("web")]
        public Self Web { get; set; }

        [JsonProperty("editor")]
        public Self Editor { get; set; }

        [JsonProperty("badge")]
        public Self Badge { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("definition")]
        public Definition Definition { get; set; }

        [JsonProperty("inputs")]
        public Inputs Inputs { get; set; }
    }

    public partial class Definition
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
    }

    public partial class Inputs
    {
        [JsonProperty("branchFilters", NullValueHandling = NullValueHandling.Ignore)]
        public string BranchFilters { get; set; }

        [JsonProperty("additionalFields")]
        public string AdditionalFields { get; set; }

        [JsonProperty("workItemType", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkItemType { get; set; }

        [JsonProperty("assignToRequestor", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool? AssignToRequestor { get; set; }
    }

    public partial class Process
    {
        [JsonProperty("phases")]
        public Phase[] Phases { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }
    }

    public partial class Phase
    {
        [JsonProperty("steps")]
        public Step[] Steps { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("refName")]
        public string RefName { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("target")]
        public Target Target { get; set; }

        [JsonProperty("jobAuthorizationScope")]
        public string JobAuthorizationScope { get; set; }

        [JsonProperty("jobCancelTimeoutInMinutes")]
        public long JobCancelTimeoutInMinutes { get; set; }
    }

    public partial class Step
    {
        [JsonProperty("environment")]
        public OptionsClass Environment { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("continueOnError")]
        public bool ContinueOnError { get; set; }

        [JsonProperty("alwaysRun")]
        public bool AlwaysRun { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("timeoutInMinutes")]
        public long TimeoutInMinutes { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("task")]
        public Task Task { get; set; }

        [JsonProperty("inputs")]
        public Dictionary<string, string> Inputs { get; set; }

        [JsonProperty("refName", NullValueHandling = NullValueHandling.Ignore)]
        public string RefName { get; set; }
    }

    public partial class OptionsClass
    {
    }

    public partial class Task
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("versionSpec")]
        public string VersionSpec { get; set; }

        [JsonProperty("definitionType")]
        public string DefinitionType { get; set; }
    }

    public partial class Target
    {
        [JsonProperty("demands")]
        public string[] Demands { get; set; }

        [JsonProperty("executionOptions")]
        public ExecutionOptions ExecutionOptions { get; set; }

        [JsonProperty("allowScriptsAuthAccessOption")]
        public bool AllowScriptsAuthAccessOption { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }
    }

    public partial class ExecutionOptions
    {
        [JsonProperty("type")]
        public long Type { get; set; }
    }

    public partial class ProcessParameters
    {
        [JsonProperty("inputs")]
        public Input[] Inputs { get; set; }
    }

    public partial class Input
    {
        [JsonProperty("aliases")]
        public object[] Aliases { get; set; }

        [JsonProperty("options")]
        public OptionsClass Options { get; set; }

        [JsonProperty("properties")]
        public OptionsClass Properties { get; set; }

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

        [JsonProperty("visibleRule")]
        public string VisibleRule { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }
    }

    public partial class Project
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("lastUpdateTime")]
        public DateTimeOffset LastUpdateTime { get; set; }
    }

    public partial class Queue
    {
        [JsonProperty("_links")]
        public QueueLinks Links { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("pool")]
        public Pool Pool { get; set; }
    }

    public partial class QueueLinks
    {
        [JsonProperty("self")]
        public Self Self { get; set; }
    }

    public partial class Pool
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Repository
    {
        [JsonProperty("properties")]
        public RepositoryProperties Properties { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("defaultBranch")]
        public string DefaultBranch { get; set; }

        [JsonProperty("clean")]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool Clean { get; set; }

        [JsonProperty("checkoutSubmodules")]
        public bool CheckoutSubmodules { get; set; }
    }

    public partial class RepositoryProperties
    {
        [JsonProperty("cleanOptions")]
        [JsonConverter(typeof(FluffyParseStringConverter))]
        public long CleanOptions { get; set; }

        [JsonProperty("labelSources")]
        [JsonConverter(typeof(FluffyParseStringConverter))]
        public long LabelSources { get; set; }

        [JsonProperty("labelSourcesFormat")]
        public string LabelSourcesFormat { get; set; }

        [JsonProperty("reportBuildStatus")]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool ReportBuildStatus { get; set; }

        [JsonProperty("gitLfsSupport")]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool GitLfsSupport { get; set; }

        [JsonProperty("skipSyncSource")]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool SkipSyncSource { get; set; }

        [JsonProperty("checkoutNestedSubmodules")]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool CheckoutNestedSubmodules { get; set; }

        [JsonProperty("fetchDepth")]
        [JsonConverter(typeof(FluffyParseStringConverter))]
        public long FetchDepth { get; set; }
    }

    public partial class RetentionRule
    {
        [JsonProperty("branches")]
        public string[] Branches { get; set; }

        [JsonProperty("artifacts")]
        public object[] Artifacts { get; set; }

        [JsonProperty("artifactTypesToDelete")]
        public string[] ArtifactTypesToDelete { get; set; }

        [JsonProperty("daysToKeep")]
        public long DaysToKeep { get; set; }

        [JsonProperty("minimumToKeep")]
        public long MinimumToKeep { get; set; }

        [JsonProperty("deleteBuildRecord")]
        public bool DeleteBuildRecord { get; set; }

        [JsonProperty("deleteTestResults")]
        public bool DeleteTestResults { get; set; }
    }

    public partial class Trigger
    {
        [JsonProperty("branchFilters")]
        public string[] BranchFilters { get; set; }

        [JsonProperty("pathFilters")]
        public object[] PathFilters { get; set; }

        [JsonProperty("batchChanges")]
        public bool BatchChanges { get; set; }

        [JsonProperty("maxConcurrentBuildsPerBranch")]
        public long MaxConcurrentBuildsPerBranch { get; set; }

        [JsonProperty("pollingInterval")]
        public long PollingInterval { get; set; }

        [JsonProperty("triggerType")]
        public string TriggerType { get; set; }
    }

    public partial class Variables
    {
        [JsonProperty("buildVersionSuffix")]
        public BuildVersionSuffix BuildVersionSuffix { get; set; }

        [JsonProperty("system.debug")]
        public SystemDebug SystemDebug { get; set; }
    }

    public partial class BuildVersionSuffix
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class SystemDebug
    {
        [JsonProperty("value")]
        [JsonConverter(typeof(PurpleParseStringConverter))]
        public bool Value { get; set; }

        [JsonProperty("allowOverride")]
        public bool AllowOverride { get; set; }
    }

    public partial class PipelineLinks
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("web")]
        public Self Web { get; set; }
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

    internal class PurpleParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(bool) || t == typeof(bool?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            bool b;
            if (Boolean.TryParse(value, out b))
            {
                return b;
            }
            throw new Exception("Cannot unmarshal type bool");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (bool)untypedValue;
            var boolString = value ? "true" : "false";
            serializer.Serialize(writer, boolString);
            return;
        }

        public static readonly PurpleParseStringConverter Singleton = new PurpleParseStringConverter();
    }

    internal class FluffyParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly FluffyParseStringConverter Singleton = new FluffyParseStringConverter();
    }
}