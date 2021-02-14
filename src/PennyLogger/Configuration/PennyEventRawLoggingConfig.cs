// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace PennyLogger
{
    /// <summary>
    /// Combines configuration from <see cref="PennyEventRawLoggingAttribute"/> and
    /// <see cref="PennyEventRawLoggingOptions"/> to hold the final merged logging configuration of an event
    /// </summary>
    internal class PennyEventRawLoggingConfig
    {
        /// <summary>
        /// Log level to use when logging this event as a raw event. Defaults to <see cref="LogLevel.Trace"/>.
        /// </summary>
        public LogLevel Level { get; private set; }

        public const LogLevel DefaultLevel = LogLevel.Trace;

        /// <summary>
        /// Raw logging of this event is rate-limited to this number of log entries to log within an interval. Defaults
        /// to 10.
        /// </summary>
        public int Max { get; private set; }

        public const int DefaultMax = 10;

        /// <summary>
        /// Interval, in seconds, at which the rate-limiting resets. Defaults to 5 minutes (300 seconds).
        /// </summary>
        public int Interval { get; private set; }

        public const int DefaultInterval = 300;

        /// <summary>
        /// Merges configuration from multiple sources, including defaults, to calculate the resulting event
        /// configuration
        /// </summary>
        /// <param name="optionsHigh">
        /// Higher-priority options for appsettings or an external configuration source. May be null.
        /// </param>
        /// <param name="optionsLow">
        /// Lower-priority options from the method parameters. May be null.
        /// </param>
        /// <param name="attribute">
        /// Configuration from a <see cref="PennyEventRawLoggingAttribute"/> on the object. May be null if the object
        /// does not have this attribute.
        /// </param>
        /// <returns>
        /// Resulting merged configuration. May return null if all parameters are null. Use the <see cref="Defaults"/>
        /// property to get the default values.
        /// </returns>
        public static PennyEventRawLoggingConfig Create(PennyEventRawLoggingOptions optionsHigh,
            PennyEventRawLoggingOptions optionsLow, PennyEventRawLoggingAttribute attribute)
        {
            if (optionsHigh == null && optionsLow == null && attribute == null)
            {
                return null;
            }

            return new PennyEventRawLoggingConfig
            {
                Level = optionsHigh?.Level ?? optionsLow?.Level ?? attribute?.Level ?? DefaultLevel,
                Max = optionsHigh?.Max ?? optionsLow?.Max ?? attribute?.Max ?? DefaultMax,
                Interval = optionsHigh?.Interval ?? optionsLow?.Interval ?? attribute?.Interval ?? DefaultInterval
            };
        }

        public static PennyEventRawLoggingConfig Defaults => new PennyEventRawLoggingConfig
        {
            Level = DefaultLevel,
            Max = DefaultMax,
            Interval = DefaultInterval
        };
    }
}
