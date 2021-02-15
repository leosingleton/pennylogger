// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace PennyLogger
{
    /// <summary>
    /// Interface used by PennyLogger to write to an output log provider
    /// </summary>
    public interface IPennyLoggerOutput
    {
        /// <summary>
        /// Writes an event to the output log
        /// </summary>
        /// <param name="level">Log level of the output message</param>
        /// <param name="writeLambda">
        /// If the output logger wishes to receive a message of the specified log level, it should construct a
        /// <see cref="Utf8JsonWriter"/> and call this lambda function to serialize the log event to JSON.
        /// </param>
        public void Log(LogLevel level, Action<Utf8JsonWriter> writeLambda);
    }
}
