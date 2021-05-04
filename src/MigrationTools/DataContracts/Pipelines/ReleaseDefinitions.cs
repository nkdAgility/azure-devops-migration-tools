using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;

namespace MigrationTools.DataContracts.Pipelines
{
    [ApiPath("release/definitions")]
    [ApiName("Release Piplines")]
    public partial class ReleaseDefinition : RestApiDefinition
    {
        public string Source { get; set; }

        public long Revision { get; set; }

        public object Description { get; set; }

        public bool IsDeleted { get; set; }

        public ExpandoObject Variables { get; set; }

        public int[] VariableGroups { get; set; }

        public Environment[] Environments { get; set; }

        public Artifact[] Artifacts { get; set; }

        public object[] Triggers { get; set; }

        public string ReleaseNameFormat { get; set; }

        public object[] Tags { get; set; }

        public PipelineProcess PipelineProcess { get; set; }

        public ExpandoObject Properties { get; set; }

        public string Path { get; set; }

        public object ProjectReference { get; set; }

        public Uri Url { get; set; }

        public TaskGroupLinks Links { get; set; }

        ///<inheritdoc/>
        public override void ResetObject()
        {
            SetSourceId(Id);
            Source = "restApi";
            Revision = 1;
            Links = null;
            Artifacts = null;
            Url = null;
            Links = null;
            Id = "0";
            Triggers = null;
            PipelineProcess = null;

            foreach (var env in Environments)
            {
                env.ResetObject();
            }
        }

        public override bool HasTaskGroups()
        {
            return Environments.Any(e => e.DeployPhases.Any(d => d.WorkflowTasks.Any(w => w.DefinitionType == "metaTask")));
        }

        public override bool HasVariableGroups()
        {
            return Environments.Any(e => e.VariableGroups != null);
        }
    }

    public partial class Artifact
    {
        public string SourceId { get; set; }

        public string Type { get; set; }

        public string Alias { get; set; }

        public DefinitionReference DefinitionReference { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsRetained { get; set; }
    }

    public partial class DefinitionReference
    {
        public ArtifactSourceDefinitionUrl ArtifactSourceDefinitionUrl { get; set; }

        public ArtifactSourceDefinitionUrl DefaultVersionBranch { get; set; }

        public ArtifactSourceDefinitionUrl DefaultVersionSpecific { get; set; }

        public ArtifactSourceDefinitionUrl DefaultVersionTags { get; set; }

        public ArtifactSourceDefinitionUrl DefaultVersionType { get; set; }

        public ArtifactSourceDefinitionUrl Definition { get; set; }

        public ArtifactSourceDefinitionUrl Project { get; set; }
    }

    public partial class ArtifactSourceDefinitionUrl
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public partial class CreatedBy
    {
        public string DisplayName { get; set; }

        public Uri Url { get; set; }

        public CreatedByLinks Links { get; set; }

        public string Id { get; set; }

        public string UniqueName { get; set; }

        public Uri ImageUrl { get; set; }

        public string Descriptor { get; set; }
    }

    public partial class CreatedByLinks
    {
        public Self Avatar { get; set; }
    }

    public partial class Self
    {
        public Uri Href { get; set; }
    }

    public partial class Environment
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long Rank { get; set; }

        public CreatedBy Owner { get; set; }

        public ExpandoObject Variables { get; set; }

        public int[] VariableGroups { get; set; }

        public DeployApprovals PreDeployApprovals { get; set; }

        public DeployStep DeployStep { get; set; }

        public DeployApprovals PostDeployApprovals { get; set; }

        public DeployPhase[] DeployPhases { get; set; }

        public EnvironmentOptions EnvironmentOptions { get; set; }

        public object[] Demands { get; set; }

        public ConditionElement[] Conditions { get; set; }

        public ExecutionPolicy ExecutionPolicy { get; set; }

        public object[] Schedules { get; set; }

        public CurrentRelease CurrentRelease { get; set; }

        public RetentionPolicy RetentionPolicy { get; set; }

        public ProcessParameters ProcessParameters { get; set; }

        public ProcessParameters Properties { get; set; }

        public DeploymentGates PreDeploymentGates { get; set; }

        public DeploymentGates PostDeploymentGates { get; set; }

        public object[] EnvironmentTriggers { get; set; }

        public Uri BadgeUrl { get; set; }

        public void ResetObject()
        {
            Id = 0;
            DeployStep = null;
            Owner = null;
            BadgeUrl = null;
            CurrentRelease = null;

            foreach (var deployPhase in DeployPhases)
            {
                deployPhase.ResetObject();
            }
            PreDeployApprovals.ResetObject();
            PostDeployApprovals.ResetObject();
        }
    }

    public partial class ConditionElement
    {
        public string Name { get; set; }

        public string ConditionType { get; set; }

        public string Value { get; set; }
    }

    public partial class CurrentRelease
    {
        public long Id { get; set; }

        public Uri Url { get; set; }

        public ProcessParameters Links { get; set; }
    }

    public partial class ProcessParameters
    {
    }

    public partial class DeployPhase
    {
        public DeploymentInput DeploymentInput { get; set; }

        public long Rank { get; set; }

        public string PhaseType { get; set; }

        public string Name { get; set; }

        public object RefName { get; set; }

        public WorkflowTask[] WorkflowTasks { get; set; }

        public void ResetObject()
        {
            foreach (var workflowTask in WorkflowTasks)
            {
                workflowTask.ResetObject();
            }
        }
    }

    public partial class DeploymentInput
    {
        public ParallelExecution ParallelExecution { get; set; }

        public string[] Tags { get; set; }
        public int QueueId { get; set; }
        public bool SkipArtifactsDownload { get; set; }

        public ArtifactsDownloadInput ArtifactsDownloadInput { get; set; }

        public string[] Demands { get; set; }

        public bool EnableAccessToken { get; set; }

        public long TimeoutInMinutes { get; set; }

        public long JobCancelTimeoutInMinutes { get; set; }

        public string Condition { get; set; }

        public ProcessParameters OverrideInputs { get; set; }
    }

    public partial class ArtifactsDownloadInput
    {
        public object[] DownloadInputs { get; set; }
    }

    public partial class ParallelExecution
    {
        public string ParallelExecutionType { get; set; }
    }

    public partial class WorkflowTask
    {
        public Guid TaskId { get; set; }

        public string Version { get; set; }

        public string Name { get; set; }

        public string RefName { get; set; }

        public bool Enabled { get; set; }

        public bool AlwaysRun { get; set; }

        public bool ContinueOnError { get; set; }

        public long TimeoutInMinutes { get; set; }

        public string? DefinitionType { get; set; }

        public ProcessParameters OverrideInputs { get; set; }

        public string Condition { get; set; }

        public ExpandoObject Inputs { get; set; }

        public void ResetObject()
        {
            var secureFiles = Inputs.Where(i => i.Key == "secureFile");
            for (int i = 0; i < secureFiles.Count(); i++)
            {
                var secureFile = secureFiles.ElementAt(i);
                ((ICollection<KeyValuePair<string, object>>)Inputs).Remove(secureFile);
            }
        }
    }

    public partial class DeployStep
    {
        public long Id { get; set; }
    }

    public partial class EnvironmentOptions
    {
        public string EmailNotificationType { get; set; }

        public string EmailRecipients { get; set; }

        public bool SkipArtifactsDownload { get; set; }

        public long TimeoutInMinutes { get; set; }

        public bool EnableAccessToken { get; set; }

        public bool PublishDeploymentStatus { get; set; }

        public bool BadgeEnabled { get; set; }

        public bool AutoLinkWorkItems { get; set; }

        public bool PullRequestDeploymentEnabled { get; set; }
    }

    public partial class ExecutionPolicy
    {
        public long ConcurrencyCount { get; set; }

        public long QueueDepthCount { get; set; }
    }

    public partial class DeployApprovals
    {
        public Approval[] Approvals { get; set; }

        public ApprovalOptions ApprovalOptions { get; set; }

        public void ResetObject()
        {
            foreach (var approval in Approvals)
            {
                approval.ResetObject();
            }
        }
    }

    public partial class ApprovalOptions
    {
        public object RequiredApproverCount { get; set; }

        public bool ReleaseCreatorCanBeApprover { get; set; }

        public bool AutoTriggeredAndPreviousEnvironmentApprovedCanBeSkipped { get; set; }

        public bool EnforceIdentityRevalidation { get; set; }

        public long TimeoutInMinutes { get; set; }

        public string ExecutionOrder { get; set; }
    }

    public partial class Approval
    {
        public long Rank { get; set; }

        public bool IsAutomated { get; } = true;

        public bool IsNotificationOn { get; set; }

        public long Id { get; set; }

        public void ResetObject()
        {
            Id = 0;
        }
    }

    public partial class DeploymentGates
    {
        public long Id { get; set; }

        public object GatesOptions { get; set; }

        public object[] Gates { get; set; }
    }

    public partial class RetentionPolicy
    {
        public long DaysToKeep { get; set; }

        public long ReleasesToKeep { get; set; }

        public bool RetainBuild { get; set; }
    }

    public partial class EnvironmentVariables
    {
        public Password Password { get; set; }

        public DemandsAppName TargetScUrl { get; set; }

        public DemandsAppName UserAccount { get; set; }
    }

    public partial class Password
    {
        public object Value { get; set; }

        public bool IsSecret { get; set; }
    }

    public partial class DemandsAppName
    {
        public string Value { get; set; }
    }

    public partial class TaskGroupLinks
    {
        public Self Self { get; set; }

        public Self Web { get; set; }
    }

    public partial class PipelineProcess
    {
        public string Type { get; set; }
    }

    public partial class Properties
    {
        public SystemEnvironmentRankLogicVersion SystemEnvironmentRankLogicVersion { get; set; }
    }

    public partial class SystemEnvironmentRankLogicVersion
    {
        public string Type { get; set; }

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