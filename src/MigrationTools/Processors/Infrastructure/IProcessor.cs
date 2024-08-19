using System;
using System.Reflection;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors;

namespace MigrationTools.Processors.Infrastructure
{
    public interface IOldProcessor
    {
        string Name { get; }
        ProcessingStatus Status { get; }
        ProcessorType Type { get; }

        void Execute();
    }
}