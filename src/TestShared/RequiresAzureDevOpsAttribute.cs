using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Tests
{
    /// <summary>
    /// Marks a test (or class) that talks to the live Azure DevOps test orgs. It is skipped, not failed, when no
    /// test credentials are configured, so forks and local runs without access still get a green build.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RequiresAzureDevOpsAttribute : ConditionBaseAttribute
    {
        public RequiresAzureDevOpsAttribute() : base(ConditionMode.Include)
        {
            IgnoreMessage = TestingConstants.NotConfiguredMessage;
        }

        public override string GroupName => nameof(RequiresAzureDevOpsAttribute);

        public override bool IsConditionMet => TestingConstants.IsLiveConfigured;
    }
}
