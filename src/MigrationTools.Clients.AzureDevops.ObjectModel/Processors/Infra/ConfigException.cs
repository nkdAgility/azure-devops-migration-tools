﻿using System;
using System.Runtime.Serialization;

namespace MigrationTools.Processors.Infrastructure
{
    [Serializable]
    internal class ConfigException : Exception
    {
        public ConfigException()
        {
        }

        public ConfigException(string message) : base(message)
        {
        }

        public ConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}