using System;

namespace MigrationTools.Exceptions
{
    public class UnknownLinkTypeException : Exception
    {
        public UnknownLinkTypeException(string message) : base(message)
        {
        }
    }
}