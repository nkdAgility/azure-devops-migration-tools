using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors.Infrastructure.Shadows
{
    public class MockSimpleProcessor : IProcessor
    {
        public MockSimpleProcessor(IOptions<IProcessorOptions> options, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger)
        {
        }

        public IEndpoint Source => throw new NotImplementedException();

        public IEndpoint Target => throw new NotImplementedException();

        public bool SupportsProcessorEnrichers => throw new NotImplementedException();

        public ProcessorEnricherContainer ProcessorEnrichers => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public ProcessingStatus Status => throw new NotImplementedException();

        public ProcessorType Type => throw new NotImplementedException();

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}