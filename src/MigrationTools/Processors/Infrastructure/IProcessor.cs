using System;
using System.Reflection;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors;

namespace MigrationTools._EngineV1.Containers
{
    public interface IProcessor
    {
        string Name { get; }
        ProcessingStatus Status { get; }
        ProcessorType Type { get; }

        void Execute();
    }
}