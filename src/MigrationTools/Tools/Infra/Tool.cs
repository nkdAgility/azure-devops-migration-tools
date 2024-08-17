using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MigrationTools.Tools.Infra
{
    public abstract class Tool<ToolOptions> : ITool where ToolOptions : class
    {
        protected ITelemetryLogger Telemetry { get; }
        protected IServiceProvider Services { get; }
        protected ILogger<ITool> Log { get; }

        protected ToolOptions Options { get; }

        public Tool(IOptions<ToolOptions> options, IServiceProvider services, ILogger<ITool> logger, ITelemetryLogger telemetry)
        {
            Options = options.Value;
            Services = services;
            Log = logger;
            Telemetry = telemetry;
        }
    }
}
