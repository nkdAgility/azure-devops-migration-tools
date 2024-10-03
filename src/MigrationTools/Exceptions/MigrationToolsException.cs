using System;
using System.Collections.Generic;
using System.Text;
using Elmah.Io.Client;

namespace MigrationTools.Exceptions
{
    

    public class MigrationToolsException : Exception
    {
        public MigrationToolsException(Exception ex, ExceptionSource errorSource) : base(ex.Message)
        {
            this.ErrorSource = errorSource;
        }

        public ExceptionSource ErrorSource { get; }

        public enum ExceptionSource
        {
            Configuration,
            Internal
        }
    }
}
