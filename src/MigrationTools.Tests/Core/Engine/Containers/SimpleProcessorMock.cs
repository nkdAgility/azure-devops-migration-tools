using MigrationTools.Core.Configuration;

namespace MigrationTools.Core.Engine.Containers.Tests
{
    public class SimpleProcessorMock : IProcessor
    {
        public string Name => "TestSimpleContext" ;

        public ProcessingStatus Status => ProcessingStatus.None;

        public void Configure(IProcessorConfig config)
        {
            
        }

        public void Execute()
        {
            
        }
    }
}