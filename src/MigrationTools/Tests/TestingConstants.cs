using System;

namespace MigrationTools.Tests
{
    public static class TestingConstants
    {
        // Live integration tests read their target from the environment (or dotnet user-secrets surfaced as env):
        //   MigrationTools__Tests__AuthenticationMode  AccessToken | EntraId (default: AccessToken when a token is set, else EntraId when enabled)
        //   MigrationTools__Tests__AccessToken         PAT or Entra token (alias: AZDEVOPS_SYSTEM_TEST_PAT)
        //   MigrationTools__Tests__UseEntraId          true to use the ambient Entra identity (az login, OIDC, managed identity)
        //   MigrationTools__Tests__SourceCollection    e.g. https://dev.azure.com/my-source-org/
        //   MigrationTools__Tests__TargetCollection    defaults to SourceCollection
        // See CONTRIBUTING.md "Running the integration tests" for how to get access.
        private const string Prefix = "MigrationTools__Tests__";
        private const string DefaultCollection = "https://dev.azure.com/nkdagility-preview/";

        private static string Env(string name) => Environment.GetEnvironmentVariable(name) is { Length: > 0 } v ? v : null;

        public static Uri SourceCollection => new Uri(Env(Prefix + "SourceCollection") ?? DefaultCollection);
        public static Uri TargetCollection => new Uri(Env(Prefix + "TargetCollection") ?? SourceCollection.ToString());

        /// <summary>Target-side test projects (migrationTarget*) live in the target org; everything else in the source org.</summary>
        public static Uri CollectionFor(string project)
            => project != null && project.StartsWith("migrationTarget", StringComparison.OrdinalIgnoreCase) ? TargetCollection : SourceCollection;

        public static string AccessToken => Env(Prefix + "AccessToken") ?? Env("AZDEVOPS_SYSTEM_TEST_PAT") ?? LegacyAccessToken;

        public static bool UseEntraId => string.Equals(Env(Prefix + "UseEntraId"), "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Env(Prefix + "AuthenticationMode"), nameof(MigrationTools.Endpoints.AuthenticationMode.EntraId), StringComparison.OrdinalIgnoreCase);

        public static MigrationTools.Endpoints.AuthenticationMode AuthenticationMode
            => UseEntraId ? MigrationTools.Endpoints.AuthenticationMode.EntraId : MigrationTools.Endpoints.AuthenticationMode.AccessToken;

        /// <summary>The token to send to Azure DevOps for whichever mode is configured.</summary>
        public static string ResolveAccessToken() => UseEntraId ? MigrationTools.Endpoints.EntraIdCredentials.GetAccessToken() : AccessToken;

        public static bool IsLiveConfigured => UseEntraId || !string.IsNullOrEmpty(AccessToken);

        public const string NotConfiguredMessage =
            "Skipped: no Azure DevOps test credentials. Set MigrationTools__Tests__UseEntraId=true (after 'az login' with an identity that has access to the test orgs) " +
            "or MigrationTools__Tests__AccessToken to a PAT, plus MigrationTools__Tests__SourceCollection/TargetCollection for your own orgs. " +
            "See https://github.com/nkdAgility/azure-devops-migration-tools/blob/main/CONTRIBUTING.md#running-the-integration-tests";

        // TEMPORARY: the previously committed token, kept only until the Entra-based test orgs are validated (#3193). Removed in the final commit.
        private static string LegacyAccessToken => AccessTokenRaw.Replace(@"fake", "");
        public static readonly string AccessTokenRaw = "5JuxdNPwL6LJ4unvKHK7wLS6ISE7ns3FRuvd4YjiyLvYz3MfzA4aJQQJ99CJfakeACAAAAAjtRX1AAASAZDOWMPe";
    }
}