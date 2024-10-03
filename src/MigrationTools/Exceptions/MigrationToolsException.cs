using System;
using System.Collections.Generic;
using System.Text;
using Elmah.Io.Client;
using static MigrationTools.Exceptions.MigrationToolsException;

namespace MigrationTools.Exceptions
{

    public static class MigrationToolsExceptionExtensions
    {
        public static MigrationToolsException AsMigrationToolsException(this Exception ex, ExceptionSource errorSource)
        {
            return new MigrationToolsException(ex, errorSource);
        }
    }

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
