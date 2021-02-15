// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace PennyLogger.Mocks.UnitTests
{
    class MockLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string s = formatter(state, exception);
            LogHistory.Add(s);
        }

        public readonly List<string> LogHistory = new List<string>();
    }
}
