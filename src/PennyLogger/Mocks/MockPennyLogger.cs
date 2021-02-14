// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger.Mocks
{
    /// <summary>
    /// Mock implementation of PennyLogger that treats all events as no-ops. Used to allow unit testing of components
    /// that require PennyLogger.
    /// </summary>
    public class MockPennyLogger : IPennyLogger
    {
        /// <inheritdoc/>
        public void Event(object eventObject, Type eventType, PennyEventOptions options = null)
        {
        }

        /// <inheritdoc/>
        public IDisposable Sample(Func<object> samplerLambda, Type samplerType, PennySamplerOptions options = null)
        {
            return new MockDisposable();
        }

        private class MockDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
