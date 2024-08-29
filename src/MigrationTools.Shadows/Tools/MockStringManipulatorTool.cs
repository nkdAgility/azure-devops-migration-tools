using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.DataContracts;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools.Shadows
{
    public class MockStringManipulatorTool : IStringManipulatorTool
    {
        public void ProcessorExecutionWithFieldItem(IProcessor processor, FieldItem fieldItem)
        {
            throw new NotImplementedException();
        }
    }
}
