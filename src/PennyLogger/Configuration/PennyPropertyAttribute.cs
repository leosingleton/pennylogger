// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using System;

namespace PennyLogger
{
    /// <summary>
    /// Custom attribute applied to a string in a PennyLogger event to configure additional settings
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PennyPropertyAttribute : Attribute
    {
        /// <inheritdoc cref="PennyPropertyConfig.Enabled"/>
        public bool Enabled { get; set; } = PennyPropertyConfig.DefaultEnabled;

        /// <inheritdoc cref="PennyPropertyConfig.Name"/>
        public string Name { get; set; } = PennyPropertyConfig.DefaultName;

        /// <inheritdoc cref="PennyPropertyConfig.IgnoreNull"/>
        public bool IgnoreNull { get; set; } = PennyPropertyConfig.DefaultIgnoreNull;

        /// <inheritdoc cref="PennyPropertyConfig.IgnoreEmpty"/>
        public bool IgnoreEmpty { get; set; } = PennyPropertyConfig.DefaultIgnoreEmpty;

        /// <inheritdoc cref="PennyPropertyConfig.MaxLength"/>
        public int MaxLength { get; set; } = PennyPropertyConfig.DefaultMaxLength;
    }
}
