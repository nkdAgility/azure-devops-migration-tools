using MigrationTools.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

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
