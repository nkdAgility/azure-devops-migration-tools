using System;
using Microsoft.VisualStudio.Services.Client;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TfsUrlParser;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Configuration options for connecting to a TFS Team Project endpoint. Inherits from TfsEndpointOptions to provide team project-specific connection settings.
    /// </summary>
    public class TfsTeamProjectEndpointOptions : TfsEndpointOptions
    {

    }
}