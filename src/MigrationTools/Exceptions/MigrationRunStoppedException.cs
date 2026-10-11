using System;

namespace MigrationTools.Exceptions
{
    /// <summary>
    /// Thrown when a validation check or a fatal condition means the migration run cannot continue.
    /// It replaces direct calls to <c>Environment.Exit</c>, which the generic host overrides with exit code 0,
    /// so that the failure propagates to the processor, the engine and the process exit code.
    /// </summary>
    public class MigrationRunStoppedException : Exception
    {
        public MigrationRunStoppedException(string message) : base(message)
        {
        }

        public MigrationRunStoppedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
