﻿using System;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Services;

namespace MigrationTools.Clients
{
    public class TfsTeamProjectEndpoint : Endpoint<TfsTeamProjectEndpointOptions>, IMigrationClient // TODO: Rename IMigrationClient to ITfsTeamProjectEndpoint
    {
        private TfsTeamProjectCollection _collection;
        private VssCredentials _vssCredentials;
        private IWorkItemMigrationClient _workItemClient;
        private ITestPlanMigrationClient _testPlanClient;

        public TfsTeamProjectEndpoint(
            IOptions<TfsTeamProjectEndpointOptions> options,
            EndpointEnricherContainer endpointEnrichers,
            ITelemetryLogger telemetry,
            ILogger<TfsTeamProjectEndpoint> logger,
            IServiceProvider Services
            ) : base(options, endpointEnrichers, Services, telemetry, logger)
        {
            _testPlanClient = new TfsTestPlanMigrationClient(options);

            _workItemClient = ActivatorUtilities.CreateInstance<TfsWorkItemMigrationClient>(Services, this, options);
            //networkCredentials IOptions<NetworkCredentialsOptions> networkCredentials,
        }

        [Obsolete("Dont know what this is for")]
        public override int Count => 0;

        public IWorkItemMigrationClient WorkItems
        {
            get
            {
                return _workItemClient;
            }
        }

        public ITestPlanMigrationClient TestPlans
        {
            get
            {
                return _testPlanClient;
            }
        }

        public object InternalCollection
        {
            get
            {
                EnsureCollection();
                return _collection;
            }
        }

        private void EnsureCollection()
        {
            if (_collection == null)
            {
                _collection = GetDependantTfsCollection();
            }
        }

        private TfsTeamProjectCollection GetDependantTfsCollection()
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetDependantTfsCollection", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("url.full", Options.Collection);
                activity?.SetTag("server.address", Options.Collection);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                TfsTeamProjectCollection y = null;
                try
                {
                    Log.LogDebug("TfsMigrationClient::GetDependantTfsCollection:AuthenticationMode({0})", Options.Authentication.AuthenticationMode.ToString());
                    switch (Options.Authentication.AuthenticationMode)
                    {
                        case AuthenticationMode.AccessToken:
                            Log.LogInformation("Connecting with AccessToken ");
                            if (string.IsNullOrEmpty(Options.Authentication.AccessToken))
                            {
                                Log.LogCritical("You must provide a PAT to use 'AccessToken' as the authentication mode. You can set this through the config at 'MigrationTools:Endpoints:{name}:Authentication:AccessToken', or you can set an environemnt variable of 'MigrationTools__Endpoints__{name}__Authentication__AccessToken'. Check the docs on https://devopsmigration.io/Reference/Endpoints/TfsTeamProjectEndpoint/");
                                Environment.Exit(-1);
                            }
                            var pat = Options.Authentication.AccessToken;
                            _vssCredentials = new VssBasicCredential(string.Empty, pat);
                            y = new TfsTeamProjectCollection(Options.Collection, _vssCredentials);
                            break;
                        case AuthenticationMode.Windows:
                            Log.LogInformation("Connecting with NetworkCredential ");
                            if (Options.Authentication.NetworkCredentials == null)
                            {
                                Log.LogCritical("You must set NetworkCredential to use 'Windows' as the authentication mode. You can set this through the config at 'MigrationTools:Endpoints:{name}:Authentication:AccessToken', or you can set an environemnt variable of 'MigrationTools__Endpoints__{name}__Authentication__AccessToken'. Check the docs on https://devopsmigration.io/Reference/Endpoints/TfsTeamProjectEndpoint/");
                                Environment.Exit(-1);
                            }
                            var cred = new NetworkCredential(Options.Authentication.NetworkCredentials.UserName, Options.Authentication.NetworkCredentials.Password, Options.Authentication.NetworkCredentials.Domain);
                            _vssCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(cred));
                            y = new TfsTeamProjectCollection(Options.Collection, _vssCredentials);
                            break;
                        case AuthenticationMode.Prompt:
                            Log.LogInformation("Prompting for credentials ");
                            _vssCredentials = new VssClientCredentials();
                            _vssCredentials.PromptType = CredentialPromptType.PromptIfNeeded;
                            y = new TfsTeamProjectCollection(Options.Collection, _vssCredentials);
                            break;

                        default:
                            Log.LogInformation("Setting _vssCredentials to Null ");
                            y = new TfsTeamProjectCollection(Options.Collection);
                            break;
                    }
                    Log.LogDebug("MigrationClient: Connecting to {CollectionUrl} ", Options.Collection);
                    Log.LogTrace("MigrationClient: validating security for {@AuthorizedIdentity} ", y.AuthorizedIdentity);
                    y.EnsureAuthenticated();
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    activity?.SetTag("http.response.status_code", "200");
                    Log.LogInformation("Access granted to {CollectionUrl} for {Name} ({Account})", Options.Collection, y.AuthorizedIdentity.DisplayName, y.AuthorizedIdentity.UniqueName);
                }
                catch (TeamFoundationServerUnauthorizedException ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    activity?.SetTag("http.response.status_code", "401");
                    Log.LogError(ex, "Unable to configure store: Check persmissions and credentials for {AuthenticationMode}!", Options.Authentication.AuthenticationMode);
                    Environment.Exit(-1);
                }
                catch (Exception ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    activity?.SetTag("http.response.status_code", "500");
                    Telemetry.TrackException(ex, activity?.Tags);
                    Log.LogError("Unable to configure store: Check persmissions and credentials for {AuthenticationMode}: " + ex.Message, Options.Authentication.AuthenticationMode);
                    switch (Options.Authentication.AuthenticationMode)
                    {
                        case AuthenticationMode.AccessToken:
                            Log.LogError("The PAT MUST be 'full access' for it to work with the Object Model API.");
                            break;
                        default:
                            break;
                    }
                    Environment.Exit(-1);
                }
                return y;
            }
        }

        public T GetService<T>()
        {
            EnsureCollection();
            return _collection.GetService<T>();
        }

        public T GetClient<T>() where T : IVssHttpClient
        {
            EnsureCollection();
            return _collection.GetClient<T>();
        }
    }
}
