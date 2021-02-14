// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace PennyLogger.Internals
{
    /// <summary>
    /// Creates timers used for logging
    /// </summary>
    /// <remarks>
    /// Right now, this class is a thin wrapper around <see cref="Timer"/>, but it is likely to become a performance
    /// bottleneck at large scale. Documentation suggests the limit to timers is around 10k per process. We should
    /// probably reimplement this class to have a single timer that executes the callbacks. It could also be used to
    /// help spread the logging load more evenly.
    /// </remarks>
    internal class TimerManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TimerManager()
        {
        }

        /// <summary>
        /// Starts a new timer
        /// </summary>
        /// <param name="interval">Timer interval, in seconds</param>
        /// <param name="callback">Callback to execute when the timer is fired</param>
        /// <returns>An <see cref="IDisposable"/> object that when disposed, stops the timer</returns>
        public IDisposable Start(int interval, Action callback)
        {
            var time = TimeSpan.FromSeconds(interval);
            return new Timer(_ => callback(), null, time, time);
        }
    }
}
