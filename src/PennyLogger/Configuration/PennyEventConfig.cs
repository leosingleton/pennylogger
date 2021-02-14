// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger
{
    /// <summary>
    /// Combines configuration from <see cref="PennyEventAttribute"/> and <see cref="PennyEventOptions"/> to hold the
    /// final merged configuration of an event
    /// </summary>
    internal class PennyEventConfig
    {
        /// <summary>
        /// Enables or disables logging of this event. Defaults to enabled (true).
        /// </summary>
        public bool Enabled { get; private set; }

        public const bool DefaultEnabled = true;

        /// <summary>
        /// Unique Event ID used for logging. This value should only be set if the object does not already include a
        /// property named &quot;Event&quot;. If no &quot;Event&quot; property exists nor an ID is specified in the
        /// options, then the class name is used as the ID (excluding any -Event suffix on the name).
        /// </summary>
        public string Id { get; private set; }

        public const string DefaultId = null;

        /// <summary>
        /// Enables aggregate logging of this event and specifies configuration options. If null, aggregate logging is
        /// disabled.
        /// </summary>
        public PennyEventAggregateLoggingConfig AggregateLogging { get; private set; }

        /// <summary>
        /// Enables raw logging of this event and specifies configuration options. If null, raw logging is disabled.
        /// </summary>
        public PennyEventRawLoggingConfig RawLogging { get; private set; }

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
        /// Configuration from a <see cref="PennyEventAttribute"/> on the object. May be null if the object does not
        /// have this attribute.
        /// </param>
        /// <param name="aggregateAttribute">
        /// Configuration from a <see cref="PennyEventAggregateLoggingAttribute"/> on the object. May be null if the
        /// object does not have this attribute.
        /// </param>
        /// <param name="rawAttribute">
        /// Configuration from a <see cref="PennyEventRawLoggingAttribute"/> on the object. May be null if the object
        /// does not have this attribute.
        /// </param>
        /// <returns>Resulting merged configuration</returns>
        public static PennyEventConfig Create(PennyEventOptions optionsHigh, PennyEventOptions optionsLow,
            PennyEventAttribute attribute, PennyEventAggregateLoggingAttribute aggregateAttribute = null,
            PennyEventRawLoggingAttribute rawAttribute = null)
        {
            // Calculate the logging configuration. If no logging is enabled, default to aggregate logging.
            var aggregateLogging = PennyEventAggregateLoggingConfig.Create(optionsHigh?.AggregateLogging,
                optionsLow?.AggregateLogging, aggregateAttribute);
            var rawLogging = PennyEventRawLoggingConfig.Create(optionsHigh?.RawLogging, optionsLow?.RawLogging,
                rawAttribute);
            if (aggregateLogging == null && rawLogging == null)
            {
                aggregateLogging = PennyEventAggregateLoggingConfig.Defaults;
            }

            return new PennyEventConfig
            {
                Enabled = optionsHigh?.Enabled ?? optionsLow?.Enabled ?? attribute?.Enabled ?? DefaultEnabled,
                Id = optionsHigh?.Id ?? optionsLow?.Id ?? attribute?.Id ?? DefaultId,
                AggregateLogging = aggregateLogging,
                RawLogging = rawLogging
            };
        }
    }
}
