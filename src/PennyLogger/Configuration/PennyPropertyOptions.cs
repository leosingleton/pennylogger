// PennyLogger: Log event aggregation and filtering library
// See LICENSE in the project root for license information.

using PennyLogger.Configuration;
using System.Runtime.CompilerServices;

namespace PennyLogger
{
    /// <summary>
    /// Per-property PennyLogger configuration
    /// </summary>
    public class PennyPropertyOptions : OptionsBase<PennyPropertyOptions>
    {
        /// <inheritdoc cref="PennyPropertyConfig.Enabled"/>
        public bool? Enabled { get; set; }

        /// <inheritdoc cref="PennyPropertyConfig.Name"/>
        public string Name { get; set; }

        /// <inheritdoc cref="PennyPropertyConfig.IgnoreNull"/>
        public bool? IgnoreNull { get; set; }

        /// <inheritdoc cref="PennyPropertyConfig.IgnoreEmpty"/>
        public bool? IgnoreEmpty { get; set; }

        /// <inheritdoc cref="PennyPropertyConfig.MaxLength"/>
        public int? MaxLength { get; set; }

        /// <inheritdoc cref="PennyPropertyConfig.Enumerable"/>
        public PennyPropertyEnumerableOptions Enumerable { get; set; }

        /// <inheritdoc/>
        protected override ITuple ToTuple() => (Enabled, Name, IgnoreNull, IgnoreEmpty, MaxLength, Enumerable);
    }
}
