using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Exceptions;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass]
    public class EntraIdOptionsTests
    {
        private const string Tenant = "11111111-1111-1111-1111-111111111111";
        private const string Client = "22222222-2222-2222-2222-222222222222";

        [TestMethod, TestCategory("L0")]
        public void Validate_AcceptsEveryCredentialWithItsRequiredSettings()
        {
            var valid = new[]
            {
                new EntraIdOptions(),
                new EntraIdOptions { Credential = EntraIdCredentialType.AzureCli, TenantId = Tenant },
                new EntraIdOptions { Credential = EntraIdCredentialType.Interactive, LoginHint = "user@contoso.com" },
                new EntraIdOptions { Credential = EntraIdCredentialType.DeviceCode, TenantId = "contoso.onmicrosoft.com" },
                new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client, CertificateThumbprint = "ABCDEF" },
                new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client, CertificatePath = "c.pfx", CertificatePassword = "p" },
                new EntraIdOptions { Credential = EntraIdCredentialType.ClientSecret, TenantId = Tenant, ClientId = Client, ClientSecret = "s" },
                new EntraIdOptions { Credential = EntraIdCredentialType.ManagedIdentity },
                new EntraIdOptions { Credential = EntraIdCredentialType.ManagedIdentity, ClientId = Client },
            };
            foreach (var options in valid)
            {
                CollectionAssert.AreEqual(new string[0], options.Validate("Source").ToArray(), options.Credential.ToString());
            }
        }

        [TestMethod, TestCategory("L0")]
        public void Validate_NamesTheMissingSettingAndWhereToSetIt()
        {
            var errors = new EntraIdOptions { Credential = EntraIdCredentialType.ClientSecret, TenantId = Tenant, ClientSecret = "s" }.Validate("Target");

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Endpoint 'Target': the ClientSecret Entra ID credential needs ClientId. Set MigrationTools:Endpoints:Target:Authentication:EntraId:ClientId.", errors[0]);
        }

        [TestMethod, TestCategory("L0")]
        public void Validate_RejectsASettingTheCredentialDoesNotUse()
        {
            var errors = new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client, CertificateThumbprint = "AB", ClientSecret = "s" }.Validate("Target");

            Assert.AreEqual(1, errors.Count);
            StringAssert.Contains(errors[0], "does not use ClientSecret");
        }

        [TestMethod, TestCategory("L0")]
        public void Validate_CertificateNeedsExactlyOneSource()
        {
            var neither = new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client }.Validate("Target");
            var both = new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client, CertificateThumbprint = "AB", CertificatePath = "c.pfx" }.Validate("Target");

            StringAssert.Contains(neither.Single(), "exactly one of CertificateThumbprint or CertificatePath");
            StringAssert.Contains(both.Single(), "exactly one of CertificateThumbprint or CertificatePath");
        }

        [TestMethod, TestCategory("L0")]
        public void Validate_RejectsIdsThatAreNotGuids()
        {
            var errors = new EntraIdOptions { Credential = EntraIdCredentialType.ClientSecret, TenantId = "not a tenant", ClientId = "app", ClientSecret = "s" }.Validate("Target");

            Assert.AreEqual(2, errors.Count);
            StringAssert.Contains(errors[0], "TenantId 'not a tenant'");
            StringAssert.Contains(errors[1], "ClientId 'app' is not a GUID");
        }

        [TestMethod, TestCategory("L0")]
        public void Validate_RejectsUserSettingsOnAnAppCredential()
        {
            var errors = new EntraIdOptions { Credential = EntraIdCredentialType.ManagedIdentity, LoginHint = "user@contoso.com" }.Validate("Target");

            StringAssert.Contains(errors.Single(), "does not use LoginHint");
        }

        [TestMethod, TestCategory("L0")]
        public void Factory_BuildsTheAzureIdentityCredentialForEachType()
        {
            Assert.IsInstanceOfType(EntraIdCredentialFactory.Create(new EntraIdOptions(), "Source"), typeof(DefaultAzureCredential));
            Assert.IsInstanceOfType(EntraIdCredentialFactory.Create(new EntraIdOptions { Credential = EntraIdCredentialType.AzureCli }, "Source"), typeof(AzureCliCredential));
            Assert.IsInstanceOfType(EntraIdCredentialFactory.Create(new EntraIdOptions { Credential = EntraIdCredentialType.ClientSecret, TenantId = Tenant, ClientId = Client, ClientSecret = "s" }, "Source"), typeof(ClientSecretCredential));
            Assert.IsInstanceOfType(EntraIdCredentialFactory.Create(new EntraIdOptions { Credential = EntraIdCredentialType.ManagedIdentity }, "Source"), typeof(ManagedIdentityCredential));
        }

        [TestMethod, TestCategory("L0")]
        public void Factory_StopsTheRunWhenTheCertificateIsNotInTheStore()
        {
            var ex = Assert.ThrowsExactly<MigrationRunStoppedException>(() => EntraIdCredentialFactory.Create(
                new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client, CertificateThumbprint = "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01" }, "Target"));

            StringAssert.StartsWith(ex.Message, "Endpoint 'Target': no certificate with thumbprint 0000000000000000000000000000000000000001");
        }

        [TestMethod, TestCategory("L0")]
        public void Factory_StopsTheRunWhenTheCertificateFileIsMissing()
        {
            var ex = Assert.ThrowsExactly<MigrationRunStoppedException>(() => EntraIdCredentialFactory.Create(
                new EntraIdOptions { Credential = EntraIdCredentialType.Certificate, TenantId = Tenant, ClientId = Client, CertificatePath = "does-not-exist.pfx" }, "Target"));

            StringAssert.Contains(ex.Message, "'does-not-exist.pfx' does not exist");
        }

        [TestMethod, TestCategory("L0")]
        public void AuthenticationRecordPath_IsPerEndpointCredentialAndTenant()
        {
            var source = EntraIdCredentialFactory.AuthenticationRecordPath(new EntraIdOptions { Credential = EntraIdCredentialType.Interactive }, "Source");
            var target = EntraIdCredentialFactory.AuthenticationRecordPath(new EntraIdOptions { Credential = EntraIdCredentialType.DeviceCode, TenantId = Tenant }, "Target");

            StringAssert.EndsWith(source, System.IO.Path.Combine("MigrationTools", "auth", "Source-Interactive-organizations.json"));
            StringAssert.EndsWith(target, $"Target-DeviceCode-{Tenant}.json");
            Assert.AreEqual("x.json", EntraIdCredentialFactory.AuthenticationRecordPath(new EntraIdOptions { AuthenticationRecordPath = "x.json" }, "Source"));
        }

        private sealed class Counting : TokenCredential
        {
            private readonly string _prefix;
            public Counting(string prefix) { _prefix = prefix; }
            public int Calls;
            public override AccessToken GetToken(TokenRequestContext c, CancellationToken t) => new AccessToken(_prefix + Interlocked.Increment(ref Calls), DateTimeOffset.UtcNow.AddHours(1));
            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext c, CancellationToken t) => new ValueTask<AccessToken>(GetToken(c, t));
        }

        [TestMethod, TestCategory("L0")]
        public void Instances_KeepSeparateCredentialsAndTokensPerEndpoint()
        {
            var sourceCredential = new Counting("source-");
            var targetCredential = new Counting("target-");
            var source = new EntraIdCredentials(new EntraIdOptions { Credential = EntraIdCredentialType.AzureCli }, "Source", (o, n) => sourceCredential);
            var target = new EntraIdCredentials(new EntraIdOptions { Credential = EntraIdCredentialType.Certificate }, "Target", (o, n) => targetCredential);

            Assert.AreEqual("source-1", source.GetAccessToken());
            Assert.AreEqual("target-1", target.GetAccessToken());
            target.Reset();
            Assert.AreEqual("source-1", source.GetAccessToken(), "Resetting Target must not touch Source.");
            Assert.AreEqual("target-2", target.GetAccessToken());
        }

        [TestMethod, TestCategory("L0")]
        public void Instance_IsNamedOnceByTheFirstCallerThatKnowsTheEndpoint()
        {
            var credentials = new EntraIdCredentials(null, null);
            credentials.NameEndpoint("Source").NameEndpoint("Target");

            Assert.AreEqual("Source", credentials.EndpointName);
        }

        private sealed class Throwing : TokenCredential
        {
            private readonly Exception _ex;
            public Throwing(Exception ex) { _ex = ex; }
            public override AccessToken GetToken(TokenRequestContext c, CancellationToken t) => throw _ex;
            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext c, CancellationToken t) => throw _ex;
        }

        [TestMethod, TestCategory("L0")]
        public void SignInFailures_StopTheRunWithOneMessagePerCredentialType()
        {
            var cases = new (EntraIdCredentialType Type, Exception Error, string Expected)[]
            {
                (EntraIdCredentialType.AzureCli, new CredentialUnavailableException("Please run 'az login'."), "Run 'az login'"),
                (EntraIdCredentialType.Interactive, new AuthenticationRequiredException("needs sign in", new TokenRequestContext(new[] { "x" })), "has expired or been revoked"),
                (EntraIdCredentialType.DeviceCode, new AuthenticationFailedException("User cancelled."), "User cancelled."),
                (EntraIdCredentialType.Certificate, new AuthenticationFailedException("AADSTS700027: Client assertion failed.\nTrace ID: 1"), "AADSTS700027: Client assertion failed. Check TenantId, ClientId and that this certificate"),
                (EntraIdCredentialType.ClientSecret, new AuthenticationFailedException("AADSTS7000215: Invalid client secret provided."), "that this secret is registered"),
                (EntraIdCredentialType.ManagedIdentity, new CredentialUnavailableException("No managed identity endpoint found."), "no managed identity is available on this machine"),
            };
            foreach (var c in cases)
            {
                var credentials = new EntraIdCredentials(new EntraIdOptions { Credential = c.Type }, "Target", (o, n) => new Throwing(c.Error));

                var ex = Assert.ThrowsExactly<MigrationRunStoppedException>(() => credentials.GetAccessToken(), c.Type.ToString());

                StringAssert.StartsWith(ex.Message, $"Endpoint 'Target': could not get a Microsoft Entra ID token with {c.Type}", c.Type.ToString());
                StringAssert.Contains(ex.Message, c.Expected, c.Type.ToString());
                Assert.IsFalse(ex.Message.Contains("Trace ID"), "Only the first line of the reason is shown.");
            }
        }
    }
}
