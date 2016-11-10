using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Execution.Exceptions
{
    public class UnknownLinkTypeException : Exception
    {
        public UnknownLinkTypeException(string message) : base(message)
        {
        }
    }
}
