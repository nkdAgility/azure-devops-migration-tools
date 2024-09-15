using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Enrichers;
using MigrationTools.Exceptions;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class TfsProcessor : Processor
    {
        protected TfsProcessor(IOptions<ProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
  
        }

        new public TfsTeamProjectEndpoint Source
        {
            get
            {
                var endpoint = base.Source as TfsTeamProjectEndpoint;
                if (endpoint == null)
                {
                    throw new ConfigurationValidationException(Options, ValidateOptionsResult.Fail($"The Endpoint '{Options.SourceName}' specified for `{this.GetType().Name}` is of the wrong type! {nameof(TfsTeamProjectEndpoint)} was expected."));
                }
                return endpoint;
            }
        }

        new public TfsTeamProjectEndpoint Target
        {
            get
            {
                var endpoint = base.Target as TfsTeamProjectEndpoint;
                if (endpoint == null)
                {
                    throw new ConfigurationValidationException(Options, ValidateOptionsResult.Fail($"The Endpoint '{Options.TargetName}' specified for `{this.GetType().Name}` is of the wrong type! {nameof(TfsTeamProjectEndpoint)} was expected."));
                }
                return endpoint;
            }
        }

        new public TfsCommonTools CommonTools => (TfsCommonTools)base.CommonTools;

    }
}
