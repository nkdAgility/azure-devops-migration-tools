using System;
using System.Dynamic;
using Newtonsoft.Json;

namespace MigrationTools.DataContracts.Pipelines
{
    public partial class ReleaseDefinitions
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public ReleaseDefinition[] Value { get; set; }
    }

    public partial class ReleaseDefinition : ReleaseBuildDefinitionAbstract
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("variables")]
        public ExpandoObject Variables { get; set; }

        [JsonProperty("variableGroups")]
        public object[] VariableGroups { get; set; }

        [JsonProperty("environments")]
        public Environment[] Environments { get; set; }

        [JsonProperty("artifacts")]
        public Artifact[] Artifacts { get; set; }

        [JsonProperty("triggers")]
        public object[] Triggers { get; set; }

        [JsonProperty("releaseNameFormat")]
        public string ReleaseNameFormat { get; set; }

        [JsonProperty("tags")]
        public object[] Tags { get; set; }

        [JsonProperty("pipelineProcess")]
        public PipelineProcess PipelineProcess { get; set; }

        [JsonProperty("properties")]
        public ExpandoObject Properties { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("projectReference")]
        public object ProjectReference { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public TaskGroupLinks Links { get; set; }
    }

    public partial class Artifact
    {
        [JsonProperty("sourceId")]
        public string SourceId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("definitionReference")]
        public DefinitionReference DefinitionReference { get; set; }

        [JsonProperty("isPrimary")]
        public bool IsPrimary { get; set; }

        [JsonProperty("isRetained")]
        public bool IsRetained { get; set; }
    }

    public partial class DefinitionReference
    {
        [JsonProperty("artifactSourceDefinitionUrl")]
        public ArtifactSourceDefinitionUrl ArtifactSourceDefinitionUrl { get; set; }

        [JsonProperty("defaultVersionBranch")]
        public ArtifactSourceDefinitionUrl DefaultVersionBranch { get; set; }

        [JsonProperty("defaultVersionSpecific")]
        public ArtifactSourceDefinitionUrl DefaultVersionSpecific { get; set; }

        [JsonProperty("defaultVersionTags")]
        public ArtifactSourceDefinitionUrl DefaultVersionTags { get; set; }

        [JsonProperty("defaultVersionType")]
        public ArtifactSourceDefinitionUrl DefaultVersionType { get; set; }

        [JsonProperty("definition")]
        public ArtifactSourceDefinitionUrl Definition { get; set; }

        [JsonProperty("project")]
        public ArtifactSourceDefinitionUrl Project { get; set; }
    }

    public partial class ArtifactSourceDefinitionUrl
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class CreatedBy
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public CreatedByLinks Links { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }
    }

    public partial class CreatedByLinks
    {
        [JsonProperty("avatar")]
        public Self Avatar { get; set; }
    }

    public partial class Self
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public partial class Environment
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("owner")]
        public CreatedBy Owner { get; set; }

        [JsonProperty("variables")]
        public EnvironmentVariables Variables { get; set; }

        [JsonProperty("variableGroups")]
        public object[] VariableGroups { get; set; }

        [JsonProperty("preDeployApprovals")]
        public DeployApprovals PreDeployApprovals { get; set; }

        [JsonProperty("deployStep")]
        public DeployStep DeployStep { get; set; }

        [JsonProperty("postDeployApprovals")]
        public DeployApprovals PostDeployApprovals { get; set; }

        [JsonProperty("deployPhases")]
        public DeployPhase[] DeployPhases { get; set; }

        [JsonProperty("environmentOptions")]
        public EnvironmentOptions EnvironmentOptions { get; set; }

        [JsonProperty("demands")]
        public object[] Demands { get; set; }

        [JsonProperty("conditions")]
        public ConditionElement[] Conditions { get; set; }

        [JsonProperty("executionPolicy")]
        public ExecutionPolicy ExecutionPolicy { get; set; }

        [JsonProperty("schedules")]
        public object[] Schedules { get; set; }

        [JsonProperty("currentRelease")]
        public CurrentRelease CurrentRelease { get; set; }

        [JsonProperty("retentionPolicy")]
        public RetentionPolicy RetentionPolicy { get; set; }

        [JsonProperty("processParameters")]
        public ProcessParameters ProcessParameters { get; set; }

        [JsonProperty("properties")]
        public ProcessParameters Properties { get; set; }

        [JsonProperty("preDeploymentGates")]
        public DeploymentGates PreDeploymentGates { get; set; }

        [JsonProperty("postDeploymentGates")]
        public DeploymentGates PostDeploymentGates { get; set; }

        [JsonProperty("environmentTriggers")]
        public object[] EnvironmentTriggers { get; set; }

        [JsonProperty("badgeUrl")]
        public Uri BadgeUrl { get; set; }
    }

    public partial class ConditionElement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("conditionType")]
        public string ConditionType { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class CurrentRelease
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public ProcessParameters Links { get; set; }
    }

    public partial class ProcessParameters
    {
    }

    public partial class DeployPhase
    {
        [JsonProperty("deploymentInput")]
        public DeploymentInput DeploymentInput { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("phaseType")]
        public string PhaseType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("refName")]
        public object RefName { get; set; }

        [JsonProperty("workflowTasks")]
        public WorkflowTask[] WorkflowTasks { get; set; }
    }

    public partial class DeploymentInput
    {
        [JsonProperty("parallelExecution")]
        public ParallelExecution ParallelExecution { get; set; }

        [JsonProperty("agentSpecification")]
        public object AgentSpecification { get; set; }

        [JsonProperty("skipArtifactsDownload")]
        public bool SkipArtifactsDownload { get; set; }

        [JsonProperty("artifactsDownloadInput")]
        public ArtifactsDownloadInput ArtifactsDownloadInput { get; set; }

        [JsonProperty("demands")]
        public string[] Demands { get; set; }

        [JsonProperty("enableAccessToken")]
        public bool EnableAccessToken { get; set; }

        [JsonProperty("timeoutInMinutes")]
        public long TimeoutInMinutes { get; set; }

        [JsonProperty("jobCancelTimeoutInMinutes")]
        public long JobCancelTimeoutInMinutes { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("overrideInputs")]
        public ProcessParameters OverrideInputs { get; set; }
    }

    public partial class ArtifactsDownloadInput
    {
        [JsonProperty("downloadInputs")]
        public object[] DownloadInputs { get; set; }
    }

    public partial class ParallelExecution
    {
        [JsonProperty("parallelExecutionType")]
        public string ParallelExecutionType { get; set; }
    }

    public partial class WorkflowTask
    {
        [JsonProperty("taskId")]
        public Guid TaskId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("refName")]
        public string RefName { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("alwaysRun")]
        public bool AlwaysRun { get; set; }

        [JsonProperty("continueOnError")]
        public bool ContinueOnError { get; set; }

        [JsonProperty("timeoutInMinutes")]
        public long TimeoutInMinutes { get; set; }

        [JsonProperty("definitionType")]
        public string? DefinitionType { get; set; }

        [JsonProperty("overrideInputs")]
        public ProcessParameters OverrideInputs { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("inputs")]
        public ExpandoObject Inputs { get; set; }
    }

    public partial class DeployStep
    {
        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public partial class EnvironmentOptions
    {
        [JsonProperty("emailNotificationType")]
        public string EmailNotificationType { get; set; }

        [JsonProperty("emailRecipients")]
        public string EmailRecipients { get; set; }

        [JsonProperty("skipArtifactsDownload")]
        public bool SkipArtifactsDownload { get; set; }

        [JsonProperty("timeoutInMinutes")]
        public long TimeoutInMinutes { get; set; }

        [JsonProperty("enableAccessToken")]
        public bool EnableAccessToken { get; set; }

        [JsonProperty("publishDeploymentStatus")]
        public bool PublishDeploymentStatus { get; set; }

        [JsonProperty("badgeEnabled")]
        public bool BadgeEnabled { get; set; }

        [JsonProperty("autoLinkWorkItems")]
        public bool AutoLinkWorkItems { get; set; }

        [JsonProperty("pullRequestDeploymentEnabled")]
        public bool PullRequestDeploymentEnabled { get; set; }
    }

    public partial class ExecutionPolicy
    {
        [JsonProperty("concurrencyCount")]
        public long ConcurrencyCount { get; set; }

        [JsonProperty("queueDepthCount")]
        public long QueueDepthCount { get; set; }
    }

    public partial class DeployApprovals
    {
        [JsonProperty("approvals")]
        public Approval[] Approvals { get; set; }

        [JsonProperty("approvalOptions")]
        public ApprovalOptions ApprovalOptions { get; set; }
    }

    public partial class ApprovalOptions
    {
        [JsonProperty("requiredApproverCount")]
        public object RequiredApproverCount { get; set; }

        [JsonProperty("releaseCreatorCanBeApprover")]
        public bool ReleaseCreatorCanBeApprover { get; set; }

        [JsonProperty("autoTriggeredAndPreviousEnvironmentApprovedCanBeSkipped")]
        public bool AutoTriggeredAndPreviousEnvironmentApprovedCanBeSkipped { get; set; }

        [JsonProperty("enforceIdentityRevalidation")]
        public bool EnforceIdentityRevalidation { get; set; }

        [JsonProperty("timeoutInMinutes")]
        public long TimeoutInMinutes { get; set; }

        [JsonProperty("executionOrder")]
        public string ExecutionOrder { get; set; }
    }

    public partial class Approval
    {
        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("isAutomated")]
        public bool IsAutomated { get; } = true;

        [JsonProperty("isNotificationOn")]
        public bool IsNotificationOn { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public partial class DeploymentGates
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("gatesOptions")]
        public object GatesOptions { get; set; }

        [JsonProperty("gates")]
        public object[] Gates { get; set; }
    }

    public partial class RetentionPolicy
    {
        [JsonProperty("daysToKeep")]
        public long DaysToKeep { get; set; }

        [JsonProperty("releasesToKeep")]
        public long ReleasesToKeep { get; set; }

        [JsonProperty("retainBuild")]
        public bool RetainBuild { get; set; }
    }

    public partial class EnvironmentVariables
    {
        [JsonProperty("Password")]
        public Password Password { get; set; }

        [JsonProperty("TargetSCUrl")]
        public DemandsAppName TargetScUrl { get; set; }

        [JsonProperty("UserAccount")]
        public DemandsAppName UserAccount { get; set; }
    }

    public partial class Password
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("isSecret")]
        public bool IsSecret { get; set; }
    }

    public partial class DemandsAppName
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class TaskGroupLinks
    {
        [JsonProperty("self")]
        public Self Self { get; set; }

        [JsonProperty("web")]
        public Self Web { get; set; }
    }

    public partial class PipelineProcess
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("System.EnvironmentRankLogicVersion")]
        public SystemEnvironmentRankLogicVersion SystemEnvironmentRankLogicVersion { get; set; }
    }

    public partial class SystemEnvironmentRankLogicVersion
    {
        [JsonProperty("$type")]
        public string Type { get; set; }

        [JsonProperty("$value")]
        [JsonConverter(typeof(FluffyParseStringConverter))]
        public long Value { get; set; }
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