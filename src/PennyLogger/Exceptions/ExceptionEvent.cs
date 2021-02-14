// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;

namespace PennyLogger.Exceptions
{
    /// <summary>
    /// PennyLogger event logged by <see cref="PennyLoggerExceptionExtensions.Exception(IPennyLogger, Exception)"/>
    /// </summary>
    [PennyEventRawLogging(Level = LogLevel.Warning)]
    [PennyEventAggregateLogging]
    public class ExceptionEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ex">Exception to log</param>
        public ExceptionEvent(Exception ex)
        {
            Type = ex.GetType().FullName;
            Message = ex.Message;
            StackTrace = ex.StackTrace;
        }

        /// <summary>
        /// The fully-qualified name of the exception, including its namespace but not assembly
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Exception's message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Exception's stack trace
        /// </summary>
        public string StackTrace { get; private set; }
    }
}
