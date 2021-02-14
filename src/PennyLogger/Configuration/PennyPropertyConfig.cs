// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger
{
    /// <summary>
    /// Combines configuration from <see cref="PennyPropertyAttribute"/> and <see cref="PennyPropertyOptions"/> to hold
    /// the final merged configuration of a property
    /// </summary>
    internal class PennyPropertyConfig
    {
        /// <summary>
        /// Enables or disables logging of this property. Defaults to enabled (true).
        /// </summary>
        public bool Enabled { get; private set; }

        public const bool DefaultEnabled = true;

        public string Name { get; private set; }

        public const string DefaultName = null;

        public bool IgnoreNull { get; private set; }

        public const bool DefaultIgnoreNull = true;

        public bool IgnoreEmpty { get; private set; }

        public const bool DefaultIgnoreEmpty = true;

        public int MaxLength { get; private set; }

        public const int DefaultMaxLength = 1024;

        public PennyPropertyEnumerableConfig Enumerable { get; private set; }

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
        /// Configuration from a <see cref="PennyPropertyAttribute"/> on the object. May be null if the object does not
        /// have this attribute.
        /// </param>
        /// <param name="enumerableAttribute">
        /// Configuration from a <see cref="PennyPropertyEnumerableAttribute"/> on the object. May be null if the object
        /// does not have this attribute.
        /// </param>
        /// <param name="propertyType">
        /// Type of the property. Default configuration values vary depending on the type specified.
        /// </param>
        /// <returns>Resulting merged configuration</returns>
        public static PennyPropertyConfig Create(PennyPropertyOptions optionsHigh, PennyPropertyOptions optionsLow,
            PennyPropertyAttribute attribute, PennyPropertyEnumerableAttribute enumerableAttribute, Type propertyType)
        {
            var isNumeric = propertyType?.FullName switch
            {
                "System.Int32" => true,
                "System.Int64" => true,
                "System.UInt32" => true,
                "System.UInt64" => true,
                "System.Single" => true,
                "System.Double" => true,
                "System.Decimal" => true,
                _ => false
            };

            // Calculate the Enumerable property. Its default value depends on the property type.
            var enumerable = PennyPropertyEnumerableConfig.Create(optionsHigh?.Enumerable, optionsLow?.Enumerable,
                enumerableAttribute);
            if (!isNumeric)
            {
                enumerable ??= PennyPropertyEnumerableConfig.Defaults;
            }

            return new PennyPropertyConfig
            {
                Enabled = optionsHigh?.Enabled ?? optionsLow?.Enabled ?? attribute?.Enabled ?? DefaultEnabled,
                Name = optionsHigh?.Name ?? optionsLow?.Name ?? attribute?.Name ?? DefaultName,
                IgnoreNull = optionsHigh?.IgnoreNull ?? optionsLow?.IgnoreNull ?? attribute?.IgnoreNull ??
                    DefaultIgnoreNull,
                IgnoreEmpty = optionsHigh?.IgnoreEmpty ?? optionsLow?.IgnoreEmpty ?? attribute?.IgnoreEmpty ??
                    DefaultIgnoreEmpty,
                MaxLength = optionsHigh?.MaxLength ?? optionsLow?.MaxLength ?? attribute?.MaxLength ??
                    DefaultMaxLength,
                Enumerable = enumerable
            };
        }
    }
}
