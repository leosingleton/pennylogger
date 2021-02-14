// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using PennyLogger.Configuration;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Raw logging configuration options for a PennyLogger event
    /// </summary>
    public class PennyEventRawLoggingOptions : OptionsBase<PennyEventRawLoggingOptions>
    {
        /// <inheritdoc cref="PennyEventRawLoggingConfig.Level"/>
        public LogLevel? Level { get; set; }

        /// <inheritdoc cref="PennyEventRawLoggingConfig.Max"/>
        public int? Max { get; set; }

        /// <inheritdoc cref="PennyEventRawLoggingConfig.Interval"/>
        public int? Interval { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Level, Max, Interval);
    }
}
