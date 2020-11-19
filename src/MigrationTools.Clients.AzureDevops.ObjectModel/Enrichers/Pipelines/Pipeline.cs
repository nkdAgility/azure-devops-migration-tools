using System;
using System.Collections.Generic;
using System.Dynamic;
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
        [JsonProperty("options")]
        public Option[] Options { get; set; }

        [JsonProperty("triggers")]
        public Trigger[] Triggers { get; set; }

        [JsonProperty("variables")]
        public Variables Variables { get; set; }

        [JsonProperty("retentionRules")]
        public RetentionRule[] RetentionRules { get; set; }

        [JsonProperty("properties")]
        public ProcessParameters Properties { get; set; }

        [JsonProperty("tags")]
        public object[] Tags { get; set; }

        [JsonProperty("_links")]
        public PipelineLinks Links { get; set; }

        [JsonProperty("jobAuthorizationScope")]
        public string JobAuthorizationScope { get; set; }

        [JsonProperty("jobTimeoutInMinutes")]
        public long JobTimeoutInMinutes { get; set; }

        [JsonProperty("jobCancelTimeoutInMinutes")]
        public long JobCancelTimeoutInMinutes { get; set; }

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
        public string Id { get; set; }

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
        public Badge Avatar { get; set; }
    }

    public partial class Badge
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public partial class PipelineLinks
    {
        [JsonProperty("self")]
        public Badge Self { get; set; }

        [JsonProperty("web")]
        public Badge Web { get; set; }

        [JsonProperty("editor")]
        public Badge Editor { get; set; }

        [JsonProperty("badge")]
        public Badge Badge { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("definition")]
        public Definition Definition { get; set; }

        [JsonProperty("inputs")]
        public OptionInputs Inputs { get; set; }
    }

    public partial class Definition
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class OptionInputs
    {
        [JsonProperty("branchFilters", NullValueHandling = NullValueHandling.Ignore)]
        public string BranchFilters { get; set; }

        [JsonProperty("additionalFields")]
        public string AdditionalFields { get; set; }

        [JsonProperty("workItemType", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkItemType { get; set; }

        [JsonProperty("assignToRequestor", NullValueHandling = NullValueHandling.Ignore)]

        public bool? AssignToRequestor { get; set; }
    }

    public partial class Process
    {
        [JsonProperty("phases")]
        public Phase[] Phases { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
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
    }

    public partial class Step
    {
        [JsonProperty("environment")]
        public ProcessParameters Environment { get; set; }

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

        [JsonProperty("refName")]
        public string RefName { get; set; }

        [JsonProperty("task")]
        public Task Task { get; set; }

        [JsonProperty("inputs")]
        public ExpandoObject Inputs { get; set; }
    }

    public partial class ProcessParameters
    {
    }


    public partial class Task
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("versionSpec")]
        public string VersionSpec { get; set; }

        [JsonProperty("definitionType")]
        public string DefinitionType { get; set; }
    }

    public partial class Target
    {
        [JsonProperty("executionOptions")]
        public ExecutionOptions ExecutionOptions { get; set; }

        [JsonProperty("allowScriptsAuthAccessOption")]
        public bool AllowScriptsAuthAccessOption { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public partial class ExecutionOptions
    {
        [JsonProperty("type")]
        public int Type { get; set; }
    }

    public partial class Project
    {
        [JsonProperty("id")]
        public string Id { get; set; }

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
        public Badge Self { get; set; }
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
        public Properties Properties { get; set; }

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

        public bool Clean { get; set; }

        [JsonProperty("checkoutSubmodules")]
        public bool CheckoutSubmodules { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("cleanOptions")]

        public string CleanOptions { get; set; }

        [JsonProperty("labelSources")]

        public string LabelSources { get; set; }

        [JsonProperty("labelSourcesFormat")]
        public string LabelSourcesFormat { get; set; }

        [JsonProperty("reportBuildStatus")]

        public bool ReportBuildStatus { get; set; }

        [JsonProperty("gitLfsSupport")]

        public bool GitLfsSupport { get; set; }

        [JsonProperty("skipSyncSource")]

        public bool SkipSyncSource { get; set; }

        [JsonProperty("checkoutNestedSubmodules")]

        public bool CheckoutNestedSubmodules { get; set; }

        [JsonProperty("fetchDepth")]

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
        [JsonProperty("system.debug")]
        public SystemDebug SystemDebug { get; set; }
    }

    public partial class SystemDebug
    {
        [JsonProperty("value")]
        public bool Value { get; set; }

        [JsonProperty("allowOverride")]
        public bool AllowOverride { get; set; }
    }
}

