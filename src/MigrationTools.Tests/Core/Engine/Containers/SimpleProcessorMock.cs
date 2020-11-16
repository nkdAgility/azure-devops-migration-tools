using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors;

namespace MigrationTools.Engine.Containers.Tests
{
    public class SimpleProcessorMock : _EngineV1.Containers.IProcessor
    {
        public string Name => "TestSimpleContext";

        public ProcessingStatus Status => ProcessingStatus.None;

        public void Configure(IProcessorConfig config)
        {
        }

        public void Execute()
        {
        }
    }
}