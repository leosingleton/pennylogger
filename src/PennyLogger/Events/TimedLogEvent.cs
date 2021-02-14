// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System.Diagnostics;

namespace PennyLogger.Events
{
    /// <summary>
    /// Base class for an event that is logged to PennyLogger on Dispose and automatically includes the execution time
    /// as the <see cref="Time"/> property.
    /// </summary>
    public abstract class TimedLogEvent : DisposableLogEvent
    {
        /// <inheritdoc/>
        protected TimedLogEvent(IPennyLogger logger) : base(logger)
        {
            _ElapsedTime.Start();
        }

        /// <summary>
        /// Elapsed time, in milliseconds
        /// </summary>
        public long Time => _ElapsedTime.ElapsedMilliseconds;

        private readonly Stopwatch _ElapsedTime = new Stopwatch();
    }
}
