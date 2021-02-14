// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace PennyLogger
{
    /// <summary>
    /// Combines configuration from <see cref="PennySamplerAttribute"/> and <see cref="PennySamplerOptions"/> to hold
    /// the final merged configuration of a sampler
    /// </summary>
    internal class PennySamplerConfig
    {
        /// <summary>
        /// Enables or disables logging of this sampler. Defaults to enabled (true).
        /// </summary>
        public bool Enabled { get; private set; }

        public const bool DefaultEnabled = true;

        /// <summary>
        /// Unique Event ID used for logging. This value should only be set if the sampler object does not already
        /// include a property named &quot;Event&quot;. If no &quot;Event&quot; property exists nor an ID is specified
        /// in the options, then the class name is used as the ID (excluding any -Sampler suffix on the name).
        /// </summary>
        public string Id { get; private set; }

        public const string DefaultId = null;

        /// <summary>
        /// Log level to use when logging the values of this sampler
        /// </summary>
        public LogLevel Level { get; private set; }

        public const LogLevel DefaultLevel = LogLevel.Information;

        /// <summary>
        /// Sampling interval, in seconds
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
        /// Configuration from a <see cref="PennySamplerAttribute"/> on the object. May be null if the object does not
        /// have this attribute.
        /// </param>
        /// <returns>Resulting merged configuration</returns>
        public static PennySamplerConfig Create(PennySamplerOptions optionsHigh, PennySamplerOptions optionsLow,
            PennySamplerAttribute attribute)
        {
            return new PennySamplerConfig
            {
                Enabled = optionsHigh?.Enabled ?? optionsLow?.Enabled ?? attribute?.Enabled ?? DefaultEnabled,
                Id = optionsHigh?.Id ?? optionsLow?.Id ?? attribute?.Id ?? DefaultId,
                Level = optionsHigh?.Level ?? optionsLow?.Level ?? attribute?.Level ?? DefaultLevel,
                Interval = optionsHigh?.Interval ?? optionsLow?.Interval ?? attribute?.Interval ?? DefaultInterval,
            };
        }
    }
}
