// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger.Events
{
    /// <summary>
    /// Base class for an event that is logged to PennyLogger on <see cref="Dispose"/>. Can be used to build up the
    /// event properties inside a using block, and ensures the event is always logged, even if partially complete
    /// because an exception occurred.
    /// </summary>
    public abstract class DisposableLogEvent : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">PennyLogger instance</param>
        protected DisposableLogEvent(IPennyLogger logger)
        {
            Logger = logger;
        }

        private readonly IPennyLogger Logger;
        private bool Written;

        /// <summary>
        /// Writes the event to PennyLogger. Events are only written once, so if this method is called, the event is no
        /// longer logged on <see cref="Dispose"/>.
        /// </summary>
        public virtual void Write()
        {
            if (!Written)
            {
                Written = true;
                Logger.Event(this, GetType());
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Write();
            GC.SuppressFinalize(this);
        }
    }
}
