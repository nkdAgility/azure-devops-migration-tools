using MigrationTools._Enginev1.Containers;
using MigrationTools.Configuration;
using MigrationTools.Processors;

namespace MigrationTools.Engine.Containers.Tests
{
    public class SimpleProcessorMock : IProcessor
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