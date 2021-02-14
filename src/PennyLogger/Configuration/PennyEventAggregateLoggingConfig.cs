// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace PennyLogger
{
    /// <summary>
    /// Combines configuration from <see cref="PennyEventAggregateLoggingAttribute"/> and
    /// <see cref="PennyEventAggregateLoggingOptions"/> to hold the final merged logging configuration of an event
    /// </summary>
    internal class PennyEventAggregateLoggingConfig
    {
        /// <summary>
        /// Log level to use when logging this event as a aggregate event. Defaults to
        /// <see cref="LogLevel.Information"/>.
        /// </summary>
        public LogLevel Level { get; private set; }

        public const LogLevel DefaultLevel = LogLevel.Information;

        /// <summary>
        /// Interval, in seconds, which to log an aggregate event. Defaults to 5 minutes (300 seconds).
        /// </summary>
        public int Interval { get; private set; }

        public const int DefaultInterval = 300;

        /// <summary>
        /// Logs an aggregate event every interval, even if no events were logged. Defaults to false.
        /// </summary>
        public bool LogIfZero { get; private set; }

        public const bool DefaultLogIfZero = false;

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
        /// Configuration from a <see cref="PennyEventAggregateLoggingAttribute"/> on the object. May be null if the
        /// object does not have this attribute.
        /// </param>
        /// <returns>
        /// Resulting merged configuration. May return null if all parameters are null. Use the <see cref="Defaults"/>
        /// property to get the default values.
        /// </returns>
        public static PennyEventAggregateLoggingConfig Create(PennyEventAggregateLoggingOptions optionsHigh,
            PennyEventAggregateLoggingOptions optionsLow, PennyEventAggregateLoggingAttribute attribute)
        {
            if (optionsHigh == null && optionsLow == null && attribute == null)
            {
                return null;
            }

            return new PennyEventAggregateLoggingConfig
            {
                Level = optionsHigh?.Level ?? optionsLow?.Level ?? attribute?.Level ?? DefaultLevel,
                Interval = optionsHigh?.Interval ?? optionsLow?.Interval ?? attribute?.Interval ?? DefaultInterval,
                LogIfZero = optionsHigh?.LogIfZero ?? optionsLow?.LogIfZero ?? attribute?.LogIfZero ?? DefaultLogIfZero
            };
        }

        public static PennyEventAggregateLoggingConfig Defaults => new PennyEventAggregateLoggingConfig
        {
            Level = DefaultLevel,
            Interval = DefaultInterval,
            LogIfZero = DefaultLogIfZero
        };
    }
}
