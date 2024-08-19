using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Engine.Containers.Tests
{
    public class SimpleProcessorMock : IOldProcessor, IProcessor
    {
        public string Name => "TestSimpleContext";

        public ProcessingStatus Status => ProcessingStatus.None;

        public ProcessorType Type => ProcessorType.Legacy;

        public IEndpoint Source => throw new System.NotImplementedException();

        public IEndpoint Target => throw new System.NotImplementedException();

        public bool SupportsProcessorEnrichers => throw new System.NotImplementedException();

        public ProcessorEnricherContainer ProcessorEnrichers => throw new System.NotImplementedException();

        public void Configure(IProcessorConfig config)
        {
        }

        public void Execute()
        {
        }
    }
}