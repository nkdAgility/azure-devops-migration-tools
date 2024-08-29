using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Processors.Infrastructure
{
    public class InvalidProcessorException : Exception
    {

        public InvalidProcessorException() : base()
        {
        }
        public InvalidProcessorException(string message) : base(message)
        {
        }
        public InvalidProcessorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
