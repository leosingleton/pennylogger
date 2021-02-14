// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

namespace PennyLogger
{
    /// <summary>
    /// Combines configuration from <see cref="PennyPropertyEnumerableAttribute"/> and
    /// <see cref="PennyPropertyEnumerableOptions"/> to hold the final merged configuration of an enumerable property
    /// </summary>
    internal class PennyPropertyEnumerableConfig
    {
        /// <summary>
        /// Enumerable properties log the Top-N values. This configuration option controls N, which defaults to 5.
        /// </summary>
        public int Top { get; private set; }

        public const int DefaultTop = 5;

        /// <summary>
        /// If enabled (default), probabilistic data structures are used to estimate the Top-N values. This is usually
        /// desirable, as it prevents memory consumption from growing linearly with the number of unique input values.
        /// If disabled, a traditional hash table is used instead, which results in a more accurate Top-N calculation.
        /// Be careful in disabling this setting, and only do so if there is a limited set of possible values for this
        /// property.
        /// </summary>
        public bool Approximate { get; private set; }

        public const bool DefaultApproximate = true;

        /// <summary>
        /// If Approximate is enabled (default), controls the maximum bytes of memory that may be used to compute the
        /// Top-N values of this property. This value is ignored if Approximate is disabled. The default is 128 KB.
        /// </summary>
        public int MaxMemory { get; private set; }

        public const int DefaultMaxMemory = 128 * 1024;

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
        /// Configuration from a <see cref="PennyPropertyEnumerableAttribute"/> on the object. May be null if the object
        /// does not have this attribute.
        /// </param>
        /// <returns>
        /// Resulting merged configuration. May return null if all parameters are null. Use the <see cref="Defaults"/>
        /// property to get the default values.
        /// </returns>
        public static PennyPropertyEnumerableConfig Create(PennyPropertyEnumerableOptions optionsHigh,
            PennyPropertyEnumerableOptions optionsLow, PennyPropertyEnumerableAttribute attribute)
        {
            if (optionsHigh == null && optionsLow == null && attribute == null)
            {
                return null;
            }

            return new PennyPropertyEnumerableConfig
            {
                Top = optionsHigh?.Top ?? optionsLow?.Top ?? attribute?.Top ?? DefaultTop,
                Approximate = optionsHigh?.Approximate ?? optionsLow?.Approximate ?? attribute?.Approximate ??
                    DefaultApproximate,
                MaxMemory = optionsHigh?.MaxMemory ?? optionsLow?.MaxMemory ?? attribute?.MaxMemory ?? DefaultMaxMemory
            };
        }

        public static PennyPropertyEnumerableConfig Defaults => new PennyPropertyEnumerableConfig
        {
            Top = DefaultTop,
            Approximate = DefaultApproximate,
            MaxMemory = DefaultMaxMemory
        };
    }
}
