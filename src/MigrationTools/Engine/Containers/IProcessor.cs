using MigrationTools.Configuration;

namespace MigrationTools.Engine.Containers
{
    public interface IProcessor
    {
        string Name { get; }
        ProcessingStatus Status { get; }

        void Execute();

        void Configure(IProcessorConfig config);
    }
}