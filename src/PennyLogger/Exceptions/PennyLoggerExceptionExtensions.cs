// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger.Exceptions
{
    /// <summary>
    /// Extension methods to <see cref="IPennyLogger"/> to add exception support
    /// </summary>
    public static class PennyLoggerExceptionExtensions
    {
        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="logger">PennyLogger service instance</param>
        /// <param name="ex">Exception</param>
        public static void Exception(this IPennyLogger logger, Exception ex) => logger.Event(new ExceptionEvent(ex));
    }
}
