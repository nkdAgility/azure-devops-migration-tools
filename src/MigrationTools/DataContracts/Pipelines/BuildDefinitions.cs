using System;
using System.Dynamic;

namespace MigrationTools.DataContracts.Pipelines
{
    public partial class BuildDefinitions
    {
        public long Count { get; set; }

        public BuildDefinition[] Value { get; set; }
    }

    public partial class BuildDefinition : PipelineDefinition
    {
        public Option[] Options { get; set; }

        public ExpandoObject[] Triggers { get; set; }

        public Variables Variables { get; set; }

        public RetentionRule[] RetentionRules { get; set; }

        public ProcessParameters Properties { get; set; }

        public object[] Tags { get; set; }

        public PipelineLinks Links { get; set; }

        public string JobAuthorizationScope { get; set; }

        public long JobTimeoutInMinutes { get; set; }

        public long JobCancelTimeoutInMinutes { get; set; }

        public Process Process { get; set; }

        public Repository Repository { get; set; }

        public ProcessParameters ProcessParameters { get; set; }

        public string Quality { get; set; }

        public AuthoredBy AuthoredBy { get; set; }

        public object[] Drafts { get; set; }

        public Queue Queue { get; set; }

        public long Id { get; set; }

        public Uri Url { get; set; }

        public string Uri { get; set; }

        public string Path { get; set; }

        public string Type { get; set; }

        public string QueueStatus { get; set; }

        public long Revision { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public Project Project { get; set; }
    }

    public partial class AuthoredBy
    {
        public string DisplayName { get; set; }

        public Uri Url { get; set; }

        public AuthoredByLinks Links { get; set; }

        public string Id { get; set; }

        public string UniqueName { get; set; }

        public Uri ImageUrl { get; set; }

        public string Descriptor { get; set; }
    }

    public partial class AuthoredByLinks
    {
        public Badge Avatar { get; set; }
    }

    public partial class Badge
    {
        public Uri Href { get; set; }
    }

    public partial class PipelineLinks
    {
        public Badge Self { get; set; }

        public Badge Web { get; set; }

        public Badge Editor { get; set; }

        public Badge Badge { get; set; }
    }

    public partial class Option
    {
        public bool Enabled { get; set; }

        public Definition Definition { get; set; }

        public OptionInputs Inputs { get; set; }
    }

    public partial class Definition
    {
        public string Id { get; set; }
    }

    public partial class OptionInputs
    {
        public string BranchFilters { get; set; }

        public string AdditionalFields { get; set; }

        public string WorkItemType { get; set; }

        public bool? AssignToRequestor { get; set; }
    }

    public partial class Process
    {
        public Phase[] Phases { get; set; }

        public int Type { get; set; }
    }

    public partial class Phase
    {
        public Step[] Steps { get; set; }

        public string Name { get; set; }

        public string RefName { get; set; }

        public string Condition { get; set; }

        public Target Target { get; set; }

        public string JobAuthorizationScope { get; set; }
    }

    public partial class Step
    {
        public ProcessParameters Environment { get; set; }

        public bool Enabled { get; set; }

        public bool ContinueOnError { get; set; }

        public bool AlwaysRun { get; set; }

        public string DisplayName { get; set; }

        public long TimeoutInMinutes { get; set; }

        public string Condition { get; set; }

        public string RefName { get; set; }

        public Task Task { get; set; }

        public ExpandoObject Inputs { get; set; }
    }

    public partial class ProcessParameters
    {
    }

    public partial class Task
    {
        public string Id { get; set; }

        public string VersionSpec { get; set; }

        public string DefinitionType { get; set; }
    }

    public partial class Target
    {
        public ExecutionOptions ExecutionOptions { get; set; }

        public bool AllowScriptsAuthAccessOption { get; set; }

        public int Type { get; set; }
    }

    public partial class ExecutionOptions
    {
        public int Type { get; set; }
    }

    public partial class Project
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public string State { get; set; }

        public long Revision { get; set; }

        public string Visibility { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }

    public partial class Queue
    {
        public QueueLinks Links { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public Pool Pool { get; set; }
    }

    public partial class QueueLinks
    {
        public Badge Self { get; set; }
    }

    public partial class Pool
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public partial class Repository
    {
        public Properties Properties { get; set; }

        public string Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public string DefaultBranch { get; set; }

        public bool Clean { get; set; }

        public bool CheckoutSubmodules { get; set; }
    }

    public partial class Properties
    {
        public string CleanOptions { get; set; }

        public string LabelSources { get; set; }

        public string LabelSourcesFormat { get; set; }

        public bool ReportBuildStatus { get; set; }

        public bool GitLfsSupport { get; set; }

        public bool SkipSyncSource { get; set; }

        public bool CheckoutNestedSubmodules { get; set; }

        public long FetchDepth { get; set; }
    }

    public partial class RetentionRule
    {
        public string[] Branches { get; set; }

        public object[] Artifacts { get; set; }

        public string[] ArtifactTypesToDelete { get; set; }

        public long DaysToKeep { get; set; }

        public long MinimumToKeep { get; set; }

        public bool DeleteBuildRecord { get; set; }

        public bool DeleteTestResults { get; set; }
    }

    public partial class Trigger
    {
        public string[] BranchFilters { get; set; }

        public object[] PathFilters { get; set; }

        public bool BatchChanges { get; set; }

        public long MaxConcurrentBuildsPerBranch { get; set; }

        public long PollingInterval { get; set; }

        public string TriggerType { get; set; }
    }

    public partial class Variables
    {
        public SystemDebug SystemDebug { get; set; }
    }

    public partial class SystemDebug
    {
        public bool Value { get; set; }

        public bool AllowOverride { get; set; }
    }
}