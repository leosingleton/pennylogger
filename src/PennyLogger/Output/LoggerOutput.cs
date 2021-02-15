// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using PennyLogger.Internals;
using System;
using System.Text.Json;

namespace PennyLogger.Output
{
    /// <summary>
    /// PennyLogger support for <see cref="ILogger"/> output
    /// </summary>
    public class LoggerOutput : IPennyLoggerOutput
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Microsoft logging extensions instance</param>
        public LoggerOutput(ILogger logger)
        {
            Logger = logger;
        }

        private readonly ILogger Logger;

        /// <inheritdoc/>
        public void Log(LogLevel level, Action<Utf8JsonWriter> writeLambda)
        {
            if (Logger.IsEnabled(level))
            {
                string message = Utf8JsonSerializer.Write(writeLambda);
                Logger.Log(level, message);
            }
        }
    }
}
