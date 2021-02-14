// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace PennyLogger.Mocks.UnitTests
{
    /// <summary>
    /// Mock implementation of <see cref="OptionsMonitor{TOptions}"/> for unit testing
    /// </summary>
    public class MockOptionsMonitor : IOptionsMonitor<PennyLoggerOptions>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialValue">Initial options values</param>
        public MockOptionsMonitor(PennyLoggerOptions initialValue)
        {
            _CurrentValue = initialValue;
            Listeners = new List<Action<PennyLoggerOptions, string>>();
        }

        /// <inheritdoc/>
        public PennyLoggerOptions CurrentValue
        {
            get => _CurrentValue;
            set
            {
                _CurrentValue = value;
                foreach (var listener in Listeners)
                {
                    listener.Invoke(value, null);
                }
            }
        }
        private PennyLoggerOptions _CurrentValue;

        /// <inheritdoc/>
        public PennyLoggerOptions Get(string name) => CurrentValue;

        /// <inheritdoc/>
        public IDisposable OnChange(Action<PennyLoggerOptions, string> listener)
        {
            Listeners.Add(listener);
            return new RemoveListener(this, listener);
        }

        private readonly List<Action<PennyLoggerOptions, string>> Listeners;

        private class RemoveListener : IDisposable
        {
            public RemoveListener(MockOptionsMonitor monitor, Action<PennyLoggerOptions, string> listener)
            {
                Monitor = monitor;
                Listener = listener;
            }

            private readonly MockOptionsMonitor Monitor;
            private readonly Action<PennyLoggerOptions, string> Listener;

            public void Dispose()
            {
                Monitor.Listeners.Remove(Listener);
            }
        }
    }
}
