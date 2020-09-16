using MigrationTools.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Engine
{
   public interface ITfsProcessingContext
    {
        string Name { get; }
        ProcessingStatus Status { get; }
        void Execute();
        void Configure(ITfsProcessingConfig config);
    }
}
