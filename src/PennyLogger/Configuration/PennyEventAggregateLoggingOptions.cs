// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using PennyLogger.Configuration;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Aggregate logging configuration options for a PennyLogger event
    /// </summary>
    public class PennyEventAggregateLoggingOptions : OptionsBase<PennyEventAggregateLoggingOptions>
    {
        /// <inheritdoc cref="PennyEventAggregateLoggingConfig.Level"/>
        public LogLevel? Level { get; set; }

        /// <inheritdoc cref="PennyEventAggregateLoggingConfig.Interval"/>
        public int? Interval { get; set; }

        /// <inheritdoc cref="PennyEventAggregateLoggingConfig.LogIfZero"/>
        public bool? LogIfZero { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Level, Interval, LogIfZero);
    }
}
