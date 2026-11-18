using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Host.Commands;

namespace MigrationTools.Host.Tests
{
    [TestClass]
    public class CommandFailureLogTests
    {
        private class CapturingLogger : ILogger
        {
            public List<(LogLevel Level, string Message, Exception Exception)> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Entries.Add((logLevel, formatter(state, exception), exception));
            }
        }

        [TestMethod, TestCategory("L0")]
        public void LogCommandFailure_WritesOneErrorLineWithoutTheException_AndTheExceptionAtDebug()
        {
            var logger = new CapturingLogger();
            var ex = new InvalidOperationException("Sequence contains more than one matching element");

            logger.LogCommandFailure("execute", ex);

            var errors = logger.Entries.FindAll(e => e.Level >= LogLevel.Error);
            Assert.AreEqual(1, errors.Count);
            Assert.IsNull(errors[0].Exception);
            StringAssert.StartsWith(errors[0].Message, "execute failed: Sequence contains more than one matching element");
            Assert.IsFalse(errors[0].Message.Contains(Environment.NewLine));
            var debug = logger.Entries.Find(e => e.Level == LogLevel.Debug);
            Assert.AreSame(ex, debug.Exception);
        }

        [TestMethod, TestCategory("L0")]
        public void Describe_WrappedException_GivesWhatFailedThenTheCause()
        {
            var ex = new InvalidDataException("Failed to load configuration from file 'x.json'.", new FormatException("Bad JSON at line 3."));

            Assert.AreEqual("Failed to load configuration from file 'x.json'. Bad JSON at line 3.", CommandFailureLog.Describe(ex));
        }
    }
}
