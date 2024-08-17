﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds.Users;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
    /// </summary>
    public class GitRepoMappingTool : Infra.Tool<GitRepoMappingToolOptions>
    {
        public ReadOnlyDictionary<string, string> Mappings { get; private set; }

        public GitRepoMappingTool(IOptions<GitRepoMappingToolOptions> options, IServiceProvider services, ILogger<GitRepoMappingTool> logger, ITelemetryLogger telemetryLogger)
           : base(options, services, logger, telemetryLogger)
        {

            Mappings = new ReadOnlyDictionary<string, string>(Options.Mappings);
        }

    }

}
