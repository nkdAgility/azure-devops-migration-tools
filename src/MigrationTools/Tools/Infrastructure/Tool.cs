using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class Tool<TToolOptions> : ITool where TToolOptions : class, IToolOptions, new()
    {
        protected ITelemetryLogger Telemetry { get; }
        protected IServiceProvider Services { get; }
        protected ILogger<ITool> Log { get; }
        protected Serilog.ILogger ContextLog {get;}

        protected TToolOptions Options { get; }

        public bool Enabled => Options.Enabled;

        public Tool(IOptions<TToolOptions> options, IServiceProvider services, ILogger<ITool> logger, ITelemetryLogger telemetry)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            Options = options.Value;
            Services = services;
            Log = logger;
            ContextLog = Serilog.Log.ForContext<Tool<TToolOptions>>();
            Telemetry = telemetry;
        }
    }
}
