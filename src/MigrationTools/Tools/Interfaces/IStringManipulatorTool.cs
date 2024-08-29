using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Tools.Interfaces
{
    public interface IStringManipulatorTool
    {
        void ProcessorExecutionWithFieldItem(IProcessor processor, FieldItem fieldItem);
    }
}
