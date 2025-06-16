using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds.Users;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Provides mapping functionality for transforming work item types from source to target systems during migration, allowing different work item type names to be used in the target.
    /// </summary>
    public class WorkItemTypeMappingTool : Tool<WorkItemTypeMappingToolOptions>, IWorkItemTypeMappingTool
    {
        /// <summary>
        /// Gets the dictionary of work item type mappings from source to target types.
        /// </summary>
        public Dictionary<string, string> Mappings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the WorkItemTypeMappingTool class.
        /// </summary>
        /// <param name="options">Configuration options for work item type mappings</param>
        /// <param name="services">Service provider for dependency injection</param>
        /// <param name="logger">Logger for the tool operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public WorkItemTypeMappingTool(IOptions<WorkItemTypeMappingToolOptions> options, IServiceProvider services, ILogger<WorkItemTypeMappingTool> logger, ITelemetryLogger telemetryLogger)
           : base(options, services, logger, telemetryLogger)
        {
            Mappings = Options.Mappings;
        }

    }

}

