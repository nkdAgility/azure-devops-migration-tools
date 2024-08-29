using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure.Shadows
{
    public class MockComplexProcessor : Processor
    {
        public MockComplexProcessor(IOptions<IProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        protected override void InternalExecute()
        {
           // Do Nothing! 
        }
    }
}
