using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors;

namespace MigrationTools._EngineV1.Containers
{
    public interface IProcessor
    {
        string Name { get; }
        ProcessingStatus Status { get; }

        void Execute();

        void Configure(IProcessorConfig config);
    }
}